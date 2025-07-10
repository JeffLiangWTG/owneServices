#if !NET8_0_OR_GREATER // Should be fixed in WI00669071: Remove usages of AppDomains
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Controls.Internal;
using Enterprise.Core.Environment.Semaphores.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.Initialisation;
using Enterprise.MasterFiles.Business;
using Enterprise.RemoteDesktopServices;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.RemoteDesktopServices.Testing;
using Enterprise.Semaphores.Common;
using Enterprise.Startup.RemoteDesktopServices.Testing;
using Enterprise.URLHandler;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ActivityLogging;
using Enterprise.ZArchitecture.Business.Test;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Startup.Testing
{
	[UseSnapshotProtection]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI012:UnmaintainableProductName", Justification = "Test code")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1015:ApplicationOpenForms", Justification = "Test code")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1157:Do not use System.AppDomain.", Justification = "Pending migration")] // WI00669071 - Do not use System.AppDomain.
	sealed class ApplicationStartupDirectorNoTransactionTest : TestCase
	{
		[GuiTest]
		public void TestApplicationThreadExistShouldStopTracking()
		{
			Assert(GCTracker.IsTrackingForTest);
			LoginDirector.Instance.HideUI = false;
			var appDomain = new AppDomainWrappers.Net.AppDomainWrapper("TestApplicationThreadExistShouldStopTracking", true);

			var appDomainData = new Dictionary<string, object> {
				{ "serverName",Db.ServerName },
				{ "databaseName",Db.DatabaseName }
			};
			try
			{
				appDomain.RunActionInAppDomain(() =>
				{
					using (var handle = new EventWaitHandle(false, EventResetMode.AutoReset))
					{
						TestingState.Setup();
						ApplicationStartupDirector.MainFormShownActionForTest = (sender, e) =>
						{
							Assert(GCTracker.IsTrackingForTest);
							var mainForm = sender as MainForm;
							System.Threading.Tasks.Task.Run(() =>
							{
								Thread.Sleep(1000);
								mainForm.Invoke(new MethodInvoker(() =>
								{
									mainForm.Close();
									handle.Set();
								}));
							});
						};
						var serverName = (string)AppDomain.CurrentDomain.GetData("serverName");
						var databaseName = (string)AppDomain.CurrentDomain.GetData("databaseName");
						ApplicationStartupDirector.Main(new string[] { serverName, databaseName });
						handle.WaitOne();
					}

					Assert(!GCTracker.IsTrackingForTest);
				}, appDomainData);
			}
			finally
			{
				appDomain.Dispose();
			}
			AssertNull(ApplicationStartupDirector.MainFormShownActionForTest);
			Assert(GCTracker.IsTrackingForTest);
		}

		#region TestStartupDirectorWithInvalidArgument

		[ExpectNoExceptions]
		public void TestStartupDirectorWithInvalidArgument()
		{
			AppDomain appDomain = AppDomain.CreateDomain("Xerxes");
			try
			{
				CrossAppDomainDelegate crossAppDomainDelegate = delegate
				{
					NUnit.Framework.TestingState.Setup();
					ApplicationStartupDirectorTest.TestApplicationStartupDirector director = new ApplicationStartupDirectorTest.TestApplicationStartupDirector();
					director.StartEnterprise(new string[] { "-WheresWally" });

					AssertEquals("IsEnterpriseRun", false, director.IsEnterpriseRun);
					AssertEquals("HadError", true, Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert(Enterprise.ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("Invalid argument"));
				};
				appDomain.DoCallBack(crossAppDomainDelegate);
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}
		#endregion

		#region TestStartupDirector

		[ExpectNoExceptions]
		public void TestStartupDirector()
		{
			var appDomain = GetAppDomain();
			try
			{
				CrossAppDomainDelegate crossAppDomainDelegate = delegate
				{
					NUnit.Framework.TestingState.Setup();
					AppDomain currentDomain = AppDomain.CurrentDomain;
					string currentUserName = (string)currentDomain.GetData("currentUserName");
					string currentBranchCode = (string)currentDomain.GetData("currentBranchCode");
					Guid currentBranchPK = (Guid)currentDomain.GetData("currentBranch");
					Guid currentDepartmentPK = (Guid)currentDomain.GetData("currentDepartment");
					string serverName = (string)currentDomain.GetData("serverName");
					string databaseName = (string)currentDomain.GetData("databaseName");
					string serverDirectory = (string)currentDomain.GetData("serverDirectory");

					ApplicationStartupDirectorTest.TestApplicationStartupDirector director = new ApplicationStartupDirectorTest.TestApplicationStartupDirector();
					director.StartEnterprise(new string[] { serverName, databaseName, "-Branch:" + currentBranchCode });

					AssertEquals("IsEnterpriseRun", true, director.IsEnterpriseRun);

					Thread.Sleep(TimeSpan.FromSeconds(2)); // give the progres form a chance to dispose
				};
				appDomain.DoCallBack(crossAppDomainDelegate);
			}
			finally
			{
				AppDomain.Unload(appDomain);
			}
		}

		static AppDomain GetAppDomain()
		{
			var appDomain = AppDomain.CreateDomain("Xerxes");
			appDomain.SetData("currentUserName", Env.CurrentUser.LoginName);
			appDomain.SetData("currentBranch", Env.CurrentBranch.PK);
			appDomain.SetData("currentBranchCode", Env.CurrentBranch.Code);
			appDomain.SetData("currentDepartment", Env.CurrentDepartment.PK);
			appDomain.SetData("serverName", Db.ServerName);
			appDomain.SetData("databaseName", Db.DatabaseName);
			appDomain.SetData("serverDirectory", Env.TempPath);
			return appDomain;
		}

		public void TestShowErrorWhenRunEnterpriseStartupTasksUnhandledException()
		{
			var appDomain = GetAppDomain();
			var isThrown = false;
			try
			{
				CrossAppDomainDelegate appDomainDelegate = delegate
				{
					Globals.IsTest_ForTest.Value = false;
					UnitTestUserNotification.Instance.AddOKAnswer();

					AppDomain currentDomain = AppDomain.CurrentDomain;
					string currentUserName = (string)currentDomain.GetData("currentUserName");
					string currentBranchCode = (string)currentDomain.GetData("currentBranchCode");
					Guid currentBranchPK = (Guid)currentDomain.GetData("currentBranch");
					Guid currentDepartmentPK = (Guid)currentDomain.GetData("currentDepartment");
					string serverName = (string)currentDomain.GetData("serverName");
					string databaseName = (string)currentDomain.GetData("databaseName");
					string serverDirectory = (string)currentDomain.GetData("serverDirectory");

					ApplicationStartupDirectorTest.TestApplicationStartupDirector director = new ApplicationStartupDirectorTest.TestApplicationStartupDirector();

					var exceptionHandler = new ApplicationStartupDirector.EndStartupErrorHandler();
					var task = new ApplicationStartupDirectorTest.TestStartupTask(true, ApplicationStartupDirectorTest.TestStartupTask.Result.Throw);
					director.OverrideStartupTasks = new IApplicationStartupTask[] { exceptionHandler, task };
					director.StartEnterprise(new string[] { serverName, databaseName, "-Branch:" + currentBranchCode });
				};
				appDomain.DoCallBack(appDomainDelegate);
			}
			catch (Exception e)
			{
				isThrown = true;
				AssertStartsWith("Show Error", "Failed to start", e.Message);
			}
			finally
			{
				Globals.IsTest_ForTest.Value = true;
				if (!isThrown)
				{
					AppDomain.Unload(appDomain);
				}
			}
		}

		#endregion

		#region TestUrlAuthentication

		[ExpectNoExceptions, GuiTest]
		public void TestUrlAuthentication()
		{
			UrlAuthenticationTestRunner.Run(r => r.TestUrlAuthentication);
		}

		[ExpectNoExceptions, GuiTest]
		public void TestRdpUrlAuthentication()
		{
			UrlAuthenticationTestRunner.Run(r => r.TestRdpUrlAuthentication);
		}

		[Serializable]
		class UrlAuthenticationTestRunner
		{
			UrlAuthenticationTestRunner(string url)
			{
				this.url = Argument.NotNullOrEmpty(url, nameof(url));
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", Justification = "Unit Test Fix.")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2219:DoNotRaiseExceptionsInExceptionClauses", Justification = "Unit Test Fix.")]
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Unit Test Fix.")]
			internal static void Run(Func<UrlAuthenticationTestRunner, CrossAppDomainDelegate> testToRun)
			{
				var domain = AppDomain.CreateDomain("UrlAuthenticationTestRunner");
				domain.SetData("BaseSourcePath", BaseSourcePath);
				domain.SetData("ServerName", Db.ServerName);
				domain.SetData("DatabaseName", Db.DatabaseName);

				try
				{
					var factory = new BusinessObjectFactory();
					var otherUser = factory.NewWithValidTestData<GlbStaff>();
					otherUser.GS_Code = "OTH";
					otherUser.GS_LoginName = "OtherUser";
					otherUser.GS_PasswordNeverChanges = true;
					otherUser.GS_ChangePasswordAtNextLogin = false;

					var security1 = factory.NewWithValidTestData<GlbSecurity>();
					security1.GU_GS = otherUser.PK;
					security1.GU_SecurityItemIsAllowed = true;
					security1.GU_SecurityRight = "Login";

					var dummy = factory.New<DummyBusinessObject>();
					factory.Save();

					var urlAuthenticationKey = Guid.NewGuid().ToString();
					var otherUserProvider = (ISemaphoreProvider)new SemaphoreProviderWithMockUserIdForTesting(otherUser.PK.ToGuid());
					using ((IDisposable)otherUserProvider)
					using (otherUserProvider.CreateSemaphoreHandle(new UrlAuthenticationSemaphore(urlAuthenticationKey)))
					using (UrlAuthenticationMutex.Obtain())
					{
						var url = ShowEditFormUrlHandler.Instance.Create(DummyControllerIDs.Dummy, dummy.PK)
											+ "&Branch=SYD&Department=TE&UrlAuthenticationKey=" + urlAuthenticationKey;

						var runner = new UrlAuthenticationTestRunner(url);
						domain.DoCallBack(() =>
						{
							var currentDomain = AppDomain.CurrentDomain;
							BaseSourcePath = (string)currentDomain.GetData("BaseSourcePath");
							Db.InitializeDatabaseDetails((string)currentDomain.GetData("ServerName"), (string)currentDomain.GetData("DatabaseName"));
							Initialiser.InitialiseWinForms();
						});
						domain.DoCallBack(testToRun(runner));
						AssertNull(domain.GetData("Exception"));
					}
				}
				finally
				{
					for (var i = 0; i < 5; i++)
					{
						try
						{
							System.Threading.Thread.Sleep(1000);
							AppDomain.Unload(domain);
						}
						catch (AppDomainUnloadedException)
						{
							// if we are here means AppDomain is unloaded.
							break;
						}
					}
				}
			}

			#region TestUrlAuthentication

			internal void TestUrlAuthentication()
			{
				try
				{
					TestingState.Setup();
					using (var registration = new URLHandlerServiceRegistrationForTest())
					{
						AssertUrlAuthentication(() =>
						{
							var isRemotingServerRegistered = CheckWithTimeout(() => registration.IsRemotingServerRegistered, 5000);
							Assert("Url handler service should be registered before url authentication", isRemotingServerRegistered);
							AssertNull("No specific licence key registered just yet", registration.RegisteredLicenseKeyIdentifier);
							AssertEquals("Should register all url handlers", 12, EnterpriseUrlHandlerService.UrlHandlers.Length);
							return EnterpriseUrlHandlerService.Instance.ExecuteUrl(url, true);
						});
						AssertEquals("Should register ediLoad location for specific license key", "EDIEDIDAT", registration.RegisteredLicenseKeyIdentifier);
					}
				}
				catch (Exception ex)
				{
					AppDomain.CurrentDomain.SetData("Exception", ex);
				}
			}

			static void AssertUrlAuthentication(Func<bool> executeUrl)
			{
				IUserContext userContextFromThread = null;

				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						ApplicationStartupDirector.Main(new[] { Db.ServerName, Db.DatabaseName, "-ShowLogin", "-Branch:SYD" });
					}
				});

				try
				{
					thread.SetApartmentState(ApartmentState.STA);
					thread.Start();

					Thread.Sleep(2000);
					Assert("Still waits till url authentication is completed", !CheckLoginCompleted());

					Assert("Should login using url authentication", executeUrl());
					Assert("Url authentication is completed successfully", CheckLoginCompleted());
					userContextFromThread = ApplicationDispatcher.Current.Invoke(() => Env.CurrentUserContext);
					AssertNotNull("userContextFromThread should not be null", userContextFromThread);
					AssertNotNull("userContextFromThread.User should not be null", userContextFromThread.User);
					AssertEquals("Should log in as other user as specified by url authentication semaphore", "OtherUser", userContextFromThread.User.LoginName);
					AssertEquals("Should log in to appropriate branch", "SYD", userContextFromThread.Branch.Code);
					AssertEquals("Should log in to appropriate department", "TE", userContextFromThread.Department.Code);
					AssertNull("Should hide main form for url authentication", StartupOpenMainFormTask.MainFormInstance);
					AssertEquals("Url should be executed", true, CheckDummyFormOpen());

					AssertEquals("Application dispatcher should be captured", thread, ApplicationDispatcher.MainThread);
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					ApplicationDispatcher.Current.Invoke(Application.Exit);
					thread.Join(TimeSpan.FromSeconds(1));
				}
			}

			static bool CheckLoginCompleted()
			{
				return LoginDirector.Instance.LoggedInLocation || LoginDirector.Instance.AuthenticatedUser.LoginValidated;
			}

			static bool CheckDummyFormOpen()
			{
				var form = Application.OpenForms.OfType<ZDummyForm>().FirstOrDefault();
				var isDummyFormOpen = form != null;
				if (isDummyFormOpen)
				{
					DisposeForm(form);
				}
				return isDummyFormOpen;
			}

			static void DisposeForm(Form form)
			{
				if (form != null)
				{
					if (form.InvokeRequired)
					{
						form.Invoke(new Action(form.Dispose));
					}
					else
					{
						form.Dispose();
					}
				}
			}

			static bool CheckWithTimeout(Func<bool> condition, int timeout = 2000)
			{
				var startTime = DateTime.Now;
				bool result;
				while (!(result = condition()) && (DateTime.Now - startTime).TotalMilliseconds < timeout)
				{
					Thread.Sleep(500);
				}
				return result;
			}

			#endregion

			#region TestRdpUrlAuthentication

			internal void TestRdpUrlAuthentication()
			{
				using (var provider = new NullEnvProvider())
				{
					provider.Enable();
					if (!string.IsNullOrEmpty(url))
					{
						new RdpUrlAuthenticationTest { Url = url, Name = "Run" }.RunBare();
					}
					else
					{
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "url is empty or null. value = {0}", url));
					}
				}
			}

			class RdpUrlAuthenticationTest : RemoteDesktopServicesTest
			{
				[GuiTest]
				public void Run()
				{
					try
					{
						TestingState.Setup();
						ObjectFactory.Substitute<TerminalService>(new ZTerminalServiceForTest());
						EnvProxy.SetHostedLocationForTest("SYD");

						AssertUrlAuthentication(() =>
						{
							CheckWithTimeout(() => EnterpriseChannel.Instance != null && MessageHandlers.RegisteredMessageTypes.Contains(EnterpriseChannelMessageTypes.EdiEntUrl), 60000);
							RemoteDesktopServicesInitializationTaskTest.AssertBasicRemoteDesktopServicesInitialized("Basic configuration should be initialized");
							var urlHandlerClient = new EnterpriseUrlHandlerClient();
							var urlHandlerService = urlHandlerClient.GetUrlHandlerService(ClientProcessId);
							return urlHandlerService.ExecuteUrl(Url, true);
						});

						RemoteDesktopServicesInitializationTaskTest.AssertAllRemoteDesktopServicesInitialized();
					}
					catch (Exception ex)
					{
						AppDomain.CurrentDomain.SetData("Exception", ex);
					}
				}

				protected override void InitializeServer(bool waitForInitialization = false)
				{
				}

				internal string Url { private get; set; }
			}

			#endregion

			readonly string url;
		}

		#endregion

		#region Startup Sequence for Branding

		static readonly IBranding CargoWiseNextBranding = new CargoWiseNextBranding();
		static readonly IBranding CargoWiseOneBranding = new CargoWiseOneBranding();
		static readonly IBranding ProductivityWiseBranding = new ProductivityWiseBranding();

		public void TestStartup_WhenProductivityWiseModeDisabled_ShouldUseCargoWiseOneBranding()
		{
			StartInNewAppDomainAndAssertBranding(CargoWiseOneBranding.ProductName, CargoWiseOneBranding.ProductBrandingName, CargoWiseOneBranding.CompanyName, CargoWiseOneBranding.ProductWebSite, false);
		}

		public void TestStartup_WhenProductivityWiseModeEnabled_ShouldUseProductivityWiseBranding()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			StartInNewAppDomainAndAssertBranding(ProductivityWiseBranding.ProductName, ProductivityWiseBranding.ProductBrandingName, ProductivityWiseBranding.CompanyName, ProductivityWiseBranding.ProductWebSite, false);
		}

		public void TestStartup_UsingPWParameter_ShouldUseProductivityWiseBranding()
		{
			StartInNewAppDomainAndAssertBranding(ProductivityWiseBranding.ProductName, ProductivityWiseBranding.ProductBrandingName, ProductivityWiseBranding.CompanyName, ProductivityWiseBranding.ProductWebSite, true);
		}

		public void TestStartup_UsingPWParameterAndProductivityWiseModeEnabled_ShouldUseProductivityWiseBranding()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			StartInNewAppDomainAndAssertBranding(ProductivityWiseBranding.ProductName, ProductivityWiseBranding.ProductBrandingName, ProductivityWiseBranding.CompanyName, ProductivityWiseBranding.ProductWebSite, true);
		}

		public void TestStartup_UsingPWParameterAndProductivityWiseModeDisabled_ShouldUseCargoWiseOneBranding()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			StartInNewAppDomainAndAssertBranding(CargoWiseOneBranding.ProductName, CargoWiseOneBranding.ProductBrandingName, CargoWiseOneBranding.CompanyName, CargoWiseOneBranding.ProductWebSite, true);
		}

		public void TestStartup_UsingPWParameterAndProductivityWiseModeDisabled_ShouldUseCargoWiseNextBranding()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = false;

			StartInNewAppDomainAndAssertBranding(CargoWiseNextBranding.ProductName, CargoWiseNextBranding.ProductBrandingName, CargoWiseNextBranding.CompanyName, CargoWiseNextBranding.ProductWebSite, pw: true, isCwNext: true);
		}

		static void StartInNewAppDomainAndAssertBranding(string expectedProductName, string expectedProductBrandName, string expectedCompanyName, string expectedProductWebSite, bool pw, bool isCwNext = false)
		{
			using (CWNextFeatureTestHelper.SetIsCWNextEnabled(isCwNext))
			using (var appDomain = new AppDomainWrappers.Net.AppDomainWrapper(nameof(TestStartup_WhenProductivityWiseModeDisabled_ShouldUseCargoWiseOneBranding), true))
			{
				var appDomainData = new Dictionary<string, object>
			{
				{ "currentUserName", Env.CurrentUser.LoginName },
				{ "currentBranch", Env.CurrentBranch.PK },
				{ "currentBranchCode", Env.CurrentBranch.Code },
				{ "currentDepartment", Env.CurrentDepartment.PK },
				{ "serverName", Db.ServerName },
				{ "databaseName", Db.DatabaseName },
				{ "serverDirectory", Env.TempPath },
				{ "taskTypeName", typeof(SetupDbConnection).FullName },
				{ "pw", pw ? "-PW" : "" }
			};
				var appDomainReturnData = new Dictionary<string, object>()
			{
				{ "FirstThing.SafeTopLevelCaptionFormat",null },
				{ "FirstThing.CompanyName",null },
				{ "FirstThing.CompanyBrandingName",null },
				{ "FirstThing.ProductName",null },
				{ "FirstThing.ProductBrandingName",null },
				{ "FirstThing.ProductWebSite",null },
				{ "SecondThing.SafeTopLevelCaptionFormat",null },
				{ "SecondThing.CompanyName",null },
				{ "SecondThing.CompanyBrandingName",null },
				{ "SecondThing.ProductName",null },
				{ "SecondThing.ProductBrandingName",null },
				{ "SecondThing.ProductWebSite",null }
			};

				var appDomainResultData = appDomain.RunActionInAppDomain(() =>
				{
					var currentBranchCode = (string)AppDomain.CurrentDomain.GetData("currentBranchCode");
					var serverName = (string)AppDomain.CurrentDomain.GetData("serverName");
					var databaseName = (string)AppDomain.CurrentDomain.GetData("databaseName");
					var taskTypeName = (string)AppDomain.CurrentDomain.GetData("taskTypeName");
					var serverDirectory = (string)AppDomain.CurrentDomain.GetData("serverDirectory");
					var pwArg = (string)AppDomain.CurrentDomain.GetData("pw");

					var director = new ApplicationStartupDirectorTest.TestApplicationStartupDirector();
					var tasks = director.GetEnterpriseStartupTasksForTest();
					var args = new[] { serverName, databaseName, @"-SDir:" + serverDirectory, "-Branch:" + currentBranchCode, pwArg };
					director.OverrideStartupTasks = tasks.TakeUntil(t => t.GetType().FullName == taskTypeName).ToArray();

					try
					{
						director.StartEnterprise(args);

						void ExportThings(string thingNamePrefix)
						{
							AppDomain.CurrentDomain.SetData(thingNamePrefix + ".SafeTopLevelCaptionFormat", Application.SafeTopLevelCaptionFormat);
							AppDomain.CurrentDomain.SetData(thingNamePrefix + ".CompanyName", BrandingFactory.Instance.CompanyName);
							AppDomain.CurrentDomain.SetData(thingNamePrefix + ".CompanyBrandingName", BrandingFactory.Instance.CompanyBrandingName);
							AppDomain.CurrentDomain.SetData(thingNamePrefix + ".ProductName", BrandingFactory.Instance.ProductName);
							AppDomain.CurrentDomain.SetData(thingNamePrefix + ".ProductBrandingName", BrandingFactory.Instance.ProductBrandingName);
							AppDomain.CurrentDomain.SetData(thingNamePrefix + ".ProductWebSite", BrandingFactory.Instance.ProductWebSite);
						}

						ExportThings("FirstThing");

						director.OverrideStartupTasks = tasks.Skip(director.OverrideStartupTasks.Length).Take(1).ToArray();
						director.StartEnterprise(args.Append(ApplicationArguments.OptionNoSplash).ToArray());

						ExportThings("SecondThing");
						args[4] = "";
						DataRegistry.Instance.ProductivityWiseModeEnabled = false;
						director.StartEnterprise(args.Append(ApplicationArguments.OptionNoSplash).ToArray());
					}
					finally
					{
						var openForm = Application.OpenForms.OfType<StartupSplashForm>().SingleOrDefault();
						try
						{
							openForm?.Invoke(new Action(() => openForm.Dispose()));
						}
						catch (Exception ex) when (ex is ObjectDisposedException || ex is InvalidAsynchronousStateException)
						{
							// The splash form isn't relevant to this test, but we should dispose it if indeed it was shown.
						}
					}
				}, appDomainData, appDomainReturnData);

				CombineAssertions("Config extracted from the new app domain should be correct", () =>
				{
					AssertEquals("Before product determined: 'SafeTopLevelCaptionFormat'", pw ? ProductivityWiseBranding.ProductName : CargoWiseNextBranding.ProductName, (string)appDomainResultData["FirstThing.SafeTopLevelCaptionFormat"]);
					AssertEquals("Before product determined: BrandingFactory.Instance.CompanyName", expectedCompanyName, (string)appDomainResultData["FirstThing.CompanyName"]);
					AssertEquals("Before product determined: BrandingFactory.Instance.CompanyBrandingName", expectedCompanyName, (string)appDomainResultData["FirstThing.CompanyBrandingName"]);
					AssertEquals("Before product determined: BrandingFactory.Instance.ProductName", pw ? ProductivityWiseBranding.ProductName : CargoWiseNextBranding.ProductName, (string)appDomainResultData["FirstThing.ProductName"]);
					AssertEquals("Before product determined: BrandingFactory.Instance.ProductWebSite", pw ? ProductivityWiseBranding.ProductWebSite : CargoWiseNextBranding.ProductWebSite, (string)appDomainResultData["FirstThing.ProductWebSite"]);

					AssertEquals("After product determined: 'SafeTopLevelCaptionFormat'", expectedProductName, (string)appDomainResultData["SecondThing.SafeTopLevelCaptionFormat"]);
					AssertEquals("After product determined: BrandingFactory.Instance.CompanyName", expectedCompanyName, (string)appDomainResultData["SecondThing.CompanyName"]);
					AssertEquals("After product determined: BrandingFactory.Instance.CompanyBrandingName", expectedCompanyName, (string)appDomainResultData["SecondThing.CompanyBrandingName"]);
					AssertEquals("After product determined: BrandingFactory.Instance.ProductName", expectedProductName, (string)appDomainResultData["SecondThing.ProductName"]);
					AssertEquals("After product determined: BrandingFactory.Instance.ProductBrandingName", expectedProductBrandName, (string)appDomainResultData["SecondThing.ProductBrandingName"]);
					AssertEquals("After product determined: BrandingFactory.Instance.ProductWebSite", expectedProductWebSite, (string)appDomainResultData["SecondThing.ProductWebSite"]);
				});
			}
		}

		#endregion

		public void TestApplication_ThreadExit_WhenDatabaseUpgradedExceptionHasBeenThrown_DoesNotLogout()
		{
			var env = ((WinFormsEnvironment)Env.Instance);
			var initialLoginController = env.LoginController;
			var actualDbEnv = DbEnv.Instance;
			var originalIsTracking = GCTracker.IsTrackingForTest;
			var mockController = new Mock<IUserLoginController>();

			var bumpedSchemaVersion = ObjectFactory.Get<IDatabaseAspectVersions>().SchemaVersion.Major + 10;
			var versionMock = Mock.Of<IDatabaseAspectVersions>(x => x.SchemaVersion == new VersionLabel(bumpedSchemaVersion, 0));

			var mockGuidPlugin = new Mock<IDbConnectionGuiPlugin>();
			var mockDbEnv = new Mock<BaseDbEnvironment>();
			mockDbEnv.Setup(m => m.ConnectionGuiPlugin).Returns(mockGuidPlugin.Object);
			mockGuidPlugin.Setup(m => m.HandleDatabaseUpgradeException(It.IsAny<DatabaseUpgradeException>()));

			DbEnv.SetDbEnvironment(mockDbEnv.Object);
			env.SetLoginControllerForTesting(mockController.Object);
			try
			{
				using (ObjectFactory.Substitute(versionMock))
				{
					Db.Connection.CloseConnection();
					AssertExceptionThrown<DatabaseUpgradedException>("PRE", () => Db.Connection.EnsureIsOpen());
					AssertEquals("PRE", true, Db.DatabaseUpgradedExceptionHasBeenThrownInConnection);
					GCTracker.StartTracking();

					var director = new ApplicationStartupDirectorTest.TestApplicationStartupDirector();
					director.Application_ThreadExit_Exposed(this, EventArgs.Empty);

					mockController.Verify(x => x.Logout(), Times.Never);
					AssertEquals("ConnectionState", ConnectionState.Closed, Db.Connection.State);
					AssertEquals("GCTracker.IsTracking", false, GCTracker.IsTrackingForTest);
				}
			}
			finally
			{
				env.SetLoginControllerForTesting(initialLoginController);
				DbEnv.SetDbEnvironment(actualDbEnv);
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = false;
				if (originalIsTracking != GCTracker.IsTrackingForTest)
				{
					if (originalIsTracking)
					{
						GCTracker.StartTracking();
					}
					else
					{
						GCTracker.StopTracking();
					}
				}
			}
		}

		protected override void TearDown()
		{
			ZFormActivityLogger.Instance.DisableActivityLogger();
			base.TearDown();
		}
	}
}
#endif
