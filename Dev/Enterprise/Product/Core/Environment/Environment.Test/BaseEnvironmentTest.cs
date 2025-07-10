using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.Semaphores.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Moq;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	sealed class BaseEnvironmentTest : TransactionedTestCase
	{
		class TestBaseEnvironment : BaseEnvironment
		{
			public TestBaseEnvironment()
				: base(new MultiThreadUserContextManager())
			{
			}

			public override void ExitApplication()
			{
			}

			public override string ApplicationStartupPath
			{
				get { return null; }
			}

			public override IUserLoginController LoginController
			{
				get { return null; }
			}

			protected override ISemaphoreProvider EnvironmentSpecificSemaphoreProvider
			{
				get { throw new NotImplementedException("TeStBaSeEnViRoNmEnT dOeS nOt PrOvIdEs SeMaPhOrEs"); }
			}

			protected override void Dispose(bool isDisposing)
			{
				Security.UnHookClientHookChanged();
				base.Dispose(isDisposing);
			}
		}

		public void TestClearEnvironmentResetsFieldsInitialisedByConstructor()
		{
			using (BaseEnvironment env = new TestBaseEnvironment())
			{
				Dictionary<FieldInfo, object> originalValues = new Dictionary<FieldInfo, object>();
				foreach (FieldInfo field in typeof(BaseEnvironment).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
				{
					originalValues[field] = field.GetValue(env);
				}
				env.ClearUserContext();
				foreach (FieldInfo field in originalValues.Keys)
				{
					if (originalValues[field] == null)
					{
						AssertNull("Field " + field.Name + " was null after construction, so it should still be null after ClearEnvironment()", field.GetValue(env));
					}
					else
					{
						AssertNotNull("Field " + field.Name + " was not null after construction, so it should not be null after ClearEnvironment()", field.GetValue(env));
					}
				}
			}
		}

		public void TestEnvironmentIsCollectable()
		{
			CreateEnvironmentWeakRef(out WeakReference testWeakRef);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			Assert("Environment should not be alive", !testWeakRef.IsAlive);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "EDI005:WeakReferenceTargetRaceConditionRule", Justification = "Testing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void CreateEnvironmentWeakRef(out WeakReference testWeakRef)
		{
			var testEnv = new TestBaseEnvironment();
			testEnv.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
			testWeakRef = new WeakReference(testEnv);
			SecurityCore security = ((BaseEnvironment)testWeakRef.Target).Security;
			((BaseEnvironment)testWeakRef.Target).Dispose();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestSetupWillKeepSecurityInstanceTheSame()
		{
			//Security instance must not be recreated, otherwise all the checkpoints already remembered by the Sections on the moduletree will reference the incorrect security class.

			using (var testEnv = new TestBaseEnvironment())
			{
				testEnv.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				SecurityCore security1 = testEnv.Security;

				testEnv.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
				SecurityCore security2 = testEnv.Security;

				AssertEquals("Security", security1, security2);
			}
		}

		public void TestSetTemporaryUserContext()
		{
			ICompany oldCurrentCompany = Env.Instance.CurrentCompany;
			IBranch oldCurrentBranch = Env.Instance.CurrentBranch;
			IDepartment oldCurrentDepartment = Env.Instance.CurrentDepartment;
			IUser oldCurrentUser = Env.Instance.CurrentUser;
			DataRegistry oldRegistry = Env.Instance.Registry;
			var oldRecentItemManagerInstance = RecentItemManager.Instance;
			var glowUserDataManagerMock = new Mock<IGlowUserDataManager>(MockBehavior.Strict);
			var disposable = new Mock<IDisposable>(MockBehavior.Strict);
			disposable.Setup(m => m.Dispose()).Verifiable();
			glowUserDataManagerMock.Setup(m => m.IncreaseTempUserCount()).Returns(disposable.Object).Verifiable(); 
			ObjectFactory.Substitute("IGlowServiceClientFactory", glowUserDataManagerMock.Object);

			using (Env.Instance.SetTemporaryUserContext(new UserContext("Test", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertNotEquals("Temporary Environment CurrentCompany", oldCurrentCompany, Env.Instance.CurrentCompany);
				AssertNotEquals("Temporary Environment CurrentBranch", oldCurrentBranch, Env.Instance.CurrentBranch);
				AssertNotEquals("Temporary Environment CurrentDepartment", oldCurrentDepartment, Env.Instance.CurrentDepartment);
				AssertNotEquals("Temporary Environment CurrentUser", oldCurrentUser, Env.Instance.CurrentUser);
				AssertNotEquals("Temporary Environment Registry", oldCurrentUser, Env.Instance.Registry); // Should be oldRegistryProbably...
				AssertNotEquals("Temporary Recent Item Manager Instance", oldRecentItemManagerInstance, RecentItemManager.Instance);
				glowUserDataManagerMock.Verify(m => m.IncreaseTempUserCount(), Times.Once);
			}

			AssertEquals("CurrentCompany was restored", oldCurrentCompany, Env.Instance.CurrentCompany);
			AssertEquals("CurrentBranch was restored", oldCurrentBranch, Env.Instance.CurrentBranch);
			AssertEquals("CurrentDepartment was restored", oldCurrentDepartment, Env.Instance.CurrentDepartment);
			AssertEquals("CurrentUser was restored", oldCurrentUser, Env.Instance.CurrentUser);
			AssertEquals("CurrentUser was restored", oldRegistry, Env.Instance.Registry);
			AssertEquals("Recent Item Manager Instance was restored", oldRecentItemManagerInstance, RecentItemManager.Instance);
			disposable.Verify(m => m.Dispose(), Times.Once);
		}

		public void TestSetTemporaryUserContext_NestedCalls()
		{
			ICompany oldCurrentCompany = Env.Instance.CurrentCompany;
			IBranch oldCurrentBranch = Env.Instance.CurrentBranch;
			IDepartment oldCurrentDepartment = Env.Instance.CurrentDepartment;
			IUser oldCurrentUser = Env.Instance.CurrentUser;
			DataRegistry oldRegistry = Env.Instance.Registry;
			var oldRecentItemManagerInstance = RecentItemManager.Instance;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("Test1", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (Env.Instance.SetTemporaryUserContext(new UserContext("Test2", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertNotEquals("Temporary Environment CurrentCompany", oldCurrentCompany, Env.Instance.CurrentCompany);
				AssertNotEquals("Temporary Environment CurrentBranch", oldCurrentBranch, Env.Instance.CurrentBranch);
				AssertNotEquals("Temporary Environment CurrentDepartment", oldCurrentDepartment, Env.Instance.CurrentDepartment);
				AssertNotEquals("Temporary Environment CurrentUser", oldCurrentUser, Env.Instance.CurrentUser);
				AssertNotEquals("Temporary Environment Registry", oldCurrentUser, Env.Instance.Registry); // Should be oldRegistryProbably...
				AssertNotEquals("Temporary Recent Item Manager Instance", oldRecentItemManagerInstance, RecentItemManager.Instance);
			}

			AssertEquals("CurrentCompany was restored", oldCurrentCompany, Env.Instance.CurrentCompany);
			AssertEquals("CurrentBranch was restored", oldCurrentBranch, Env.Instance.CurrentBranch);
			AssertEquals("CurrentDepartment was restored", oldCurrentDepartment, Env.Instance.CurrentDepartment);
			AssertEquals("CurrentUser was restored", oldCurrentUser, Env.Instance.CurrentUser);
			AssertEquals("CurrentUser was restored", oldRegistry, Env.Instance.Registry);
			AssertEquals("Recent Item Manager Instance was restored", oldRecentItemManagerInstance, RecentItemManager.Instance);
		}

		public void TestSetTemporaryUserContext_ShouldErrorReport_WhenSwitchingFromWebUser()
		{
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertEquals(@"Unexpected context switch for the web user. The new context:
User: CWSupport
Branch: BNE
Department: BRN

If you are sure the context switching is correct, wrap your code with context switching with SuppressSwitchContextCheck.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		public void TestSetTemporaryUserContext_ShouldBeThreadLocal()
		{
			var firstThreadSuppressedTheCheck = new AutoResetEvent(false);
			var secondThreadSuppressedTheCheck = new AutoResetEvent(false);
			var firstThreadCompleted = new AutoResetEvent(false);

			using (var env = new TestBaseEnvironment())
			{
				var thread = new Thread(new ThreadStart(() =>
				{
					using (Db.DisposableActionForDbConnection())
					using (env.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
					using (env.SuppressSwitchContextCheck())
					{
						secondThreadSuppressedTheCheck.Set();
						firstThreadSuppressedTheCheck.WaitOne(); // both threads suppressed the check now
						firstThreadCompleted.WaitOne();
						using (env.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK))) // that's the place where an error report could happen
						{
						}
					}
				}));
				thread.Start();

				using (env.SuppressSwitchContextCheck())
				{
					firstThreadSuppressedTheCheck.Set();
					secondThreadSuppressedTheCheck.WaitOne();
				}
				firstThreadCompleted.Set();
			}

			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestSetTemporaryUserContext_ShouldNotErrorReport_WhenSwitchingFromWebUser_ButCheckIsSuppressed()
		{
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (Env.Instance.SuppressSwitchContextCheck())
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
			}

			AssertEquals("Should not report when the check is suppressed", string.Empty, ErrorReporter.LastMessageReported);

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWWeb", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertContains("Should report when the check is not suppressed", @"Unexpected context switch for the web user. The new context:", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}

			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:Use Env.SetUserContext", Justification = "Testing")]
		public void TestEnsureUserContextIsRestoredAfterThis_ShouldThrow_WhenUserContextIsNotRestored()
		{
			var contextSwitched = false;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertExceptionThrown<UserContextLostException>("Should throw when the initial context is not restored", () =>
				{
					using (Env.Instance.EnsureUserContextIsRestoredAfterThis())
					{
						Env.Instance.SetUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
						contextSwitched = true;
					}
				});
			}
			Assert(contextSwitched);
		}

		public void TestEnsureUserContextIsRestoredAfterThis_ShouldThrow_WhenUserContextIsNotRestored_DueToExceptionWhileRestoringInitialContext()
		{
			var contextSwitched = false;
			bool fired = false;

			try
			{
				using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					AssertExceptionThrown<UserContextLostException>("Should throw when the initial context is not restored", () =>
					{
						using (Env.Instance.EnsureUserContextIsRestoredAfterThis())
						{
							Env.Instance.UserContextChanging += OnUserContextChanging;

							AssertExceptionThrown<UserContextLostException>("SetTemporaryUserContext should throw on reverting", () =>
							{
								using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
								{
									contextSwitched = true;
								}
							});
						}
					});
				}
				Assert(contextSwitched);
				Env.Instance.UserContextManagerForTesting.ClearCurrentThread();
			}
			finally
			{
				Env.Instance.UserContextChanging -= OnUserContextChanging;
			}

			void OnUserContextChanging(object sender, IUserContextChangingEventArgs userContextChangingDetails)
			{
				if (userContextChangingDetails.IsRevert && !fired)
				{
					fired = true;
					var error = SqlExceptionBuilder.CreateSqlError(942, 1, 1, "", "Test SQL Exception should have been caught", "", 1);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
					throw sqlException;
				}
			}
		}

		public void TestEnsureUserContextIsRestoredAfterThis_ShouldNotThrow_WhenUserContextIsRestored()
		{
			var contextSwitched = false;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertNoExceptionThrown("Should not throw when the initial context is successfully restored", () =>
				{
					using (Env.Instance.EnsureUserContextIsRestoredAfterThis())
					{
						using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
						{
							contextSwitched = true;
						}
					}
				});
			}
			Assert(contextSwitched);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:Use Env.SetUserContext", Justification = "Testing")]
		public void TestSuppressSwitchContextCheck_ShouldThrow_WhenUnsuppressing_AndUserContextIsNotRestored()
		{
			var contextSwitched = false;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertExceptionThrown<UserContextLostException>("Should throw when the initial context is not restored", () =>
				{
					using (Env.Instance.SuppressSwitchContextCheck())
					{
						Env.Instance.SetUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
						contextSwitched = true;
					}
				});
			}
			Assert(contextSwitched);
		}

		public void TestSuppressSwitchContextCheck_ShouldThrow_WhenUnsuppressing_AndUserContextIsNotRestored_DueToExceptionWhileRestoringInitialContext()
		{
			var contextSwitched = false;
			var fired = false;

			try
			{
				using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
				{
					AssertExceptionThrown<UserContextLostException>("Should throw when the initial context is not restored", () =>
					{
						using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: true))
						{
							Env.Instance.UserContextChanging += OnUserContextChanging;

							AssertExceptionThrown<UserContextLostException>("SetTemporaryUserContext should throw on reverting", () =>
							{
								using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
								{
									contextSwitched = true;
								}
							});
						}
					});
				}
				Assert(contextSwitched);
				Env.Instance.UserContextManagerForTesting.ClearCurrentThread();
			}
			finally
			{
				Env.Instance.UserContextChanging -= OnUserContextChanging;
			}

			void OnUserContextChanging(object sender, IUserContextChangingEventArgs userContextChangingDetails)
			{
				if (userContextChangingDetails.IsRevert && !fired)
				{
					fired = true;
					var error = SqlExceptionBuilder.CreateSqlError(942, 1, 1, "", "Databaize gawn", "", 1);
					var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
					var sqlException = SqlExceptionBuilder.CreateSqlException(errors);
					throw sqlException;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:Use Env.SetUserContext", Justification = "Testing")]
		public void TestSuppressSwitchContextCheck_ShouldNotThrow_WhenUnsuppressing_AndUserContextIsNotRestored_ButContextRestorationCheckIsDisabled()
		{
			var contextSwitched = false;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertNoExceptionThrown("Should not throw when context restoration check is disabled", () =>
				{
					using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
					{
						Env.Instance.SetUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK));
						contextSwitched = true;
					}
				});
			}
			Assert(contextSwitched);
		}

		public void TestSuppressSwitchContextCheck_ShouldNotThrow_WhenUserContextIsRestoredAfterSuppression()
		{
			var contextSwitched = false;

			using (Env.Instance.SetTemporaryUserContext(new UserContext("CWService", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertNoExceptionThrown("Should not throw when the initial context is successfully restored", () =>
				{
					using (Env.Instance.SetTemporaryUserContext(new UserContext("CWSupport", Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
					{
						contextSwitched = true;
					}
				});
			}
			Assert(contextSwitched);
		}

		#region TestGlobalFormTopCaption

		public void TestGlobalFormTopCaptionNullProperties()
		{
			using (TestBaseEnvironment testEnv = new TestBaseEnvironment())
			{
				AssertNull("CurrentBranch should be null", testEnv.CurrentBranch);
				AssertNull("CurrentCompany should be null", testEnv.CurrentCompany);
				AssertNull("CurrentDepartment should be null", testEnv.CurrentDepartment);
				AssertNull("CurrentUser should be null", testEnv.CurrentUser);

				string databaseName = CargoWise.Data.Db.DatabaseName;

				testEnv.Registry.GlobalFormTopCaption = "";
				testEnv.Registry.ShowDatabaseName = false;
				testEnv.Registry.ShowBranchName = false;
				testEnv.Registry.ShowCompanyName = false;
				testEnv.Registry.ShowDepartmentName = false;
				testEnv.Registry.ShowUserName = false;
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);

				testEnv.Registry.GlobalFormTopCaption = "FormTopCaption";
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.GlobalFormTopCaption = "";
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowDatabaseName = true;
				AssertEquals("GlobalFormTopCaption", "DB: " + databaseName, testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowDatabaseName = false;

				testEnv.Registry.ShowBranchName = true;
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowBranchName = false;

				testEnv.Registry.ShowCompanyName = true;
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowCompanyName = false;

				testEnv.Registry.ShowDepartmentName = true;
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowDepartmentName = false;

				testEnv.Registry.ShowUserName = true;
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowUserName = false;

				testEnv.Registry.GlobalFormTopCaption = "FormTopCaption";
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowCompanyName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowBranchName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowUserName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowDatabaseName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption - DB: " + databaseName, testEnv.GlobalFormTopCaption);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestGlobalFormTopCaptionNotNullProperties()
		{
			using (var testEnv = new TestBaseEnvironment())
			{
				testEnv.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				AssertNotNullOrEmpty("CurrentBranch should have a Name", testEnv.CurrentBranch.Name);
				AssertNotNullOrEmpty("CurrentCompany should have a Name", testEnv.CurrentCompany.Name);
				AssertNotNullOrEmpty("CurrentDepartment should have a Description", testEnv.CurrentDepartment.Description);
				AssertNotNullOrEmpty("CurrentUser should have a FullName", testEnv.CurrentUser.FullName);

				string databaseName = Db.DatabaseName;

				testEnv.Registry.GlobalFormTopCaption = "";
				testEnv.Registry.ShowDatabaseName = false;
				testEnv.Registry.ShowBranchName = false;
				testEnv.Registry.ShowCompanyName = false;
				testEnv.Registry.ShowDepartmentName = false;
				testEnv.Registry.ShowUserName = false;
				AssertEquals("GlobalFormTopCaption", "", testEnv.GlobalFormTopCaption);

				testEnv.Registry.GlobalFormTopCaption = "FormTopCaption";
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.GlobalFormTopCaption = "";

				testEnv.Registry.ShowDatabaseName = true;
				AssertEquals("GlobalFormTopCaption", "DB: " + databaseName, testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowDatabaseName = false;

				testEnv.Registry.ShowBranchName = true;
				AssertEquals("GlobalFormTopCaption", "Branch: " + testEnv.CurrentBranch.Name, testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowBranchName = false;

				testEnv.Registry.ShowCompanyName = true;
				AssertEquals("GlobalFormTopCaption", "Company: " + testEnv.CurrentCompany.Name, testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowCompanyName = false;

				testEnv.Registry.ShowDepartmentName = true;
				AssertEquals("GlobalFormTopCaption", "Department: " + testEnv.CurrentDepartment.Description, testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowDepartmentName = false;

				testEnv.Registry.ShowUserName = true;
				AssertEquals("GlobalFormTopCaption", "User: " + testEnv.CurrentUser.FullName, testEnv.GlobalFormTopCaption);
				testEnv.Registry.ShowUserName = false;

				testEnv.Registry.GlobalFormTopCaption = "FormTopCaption";
				AssertEquals("GlobalFormTopCaption", "FormTopCaption", testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowDepartmentName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption - Department: " + testEnv.CurrentDepartment.Description, testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowCompanyName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption - Company: " + testEnv.CurrentCompany.Name + " - Department: " +
					testEnv.CurrentDepartment.Description, testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowBranchName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption - Branch: " + testEnv.CurrentBranch.Name + " - Company: " + testEnv.CurrentCompany.Name +
					" - Department: " + testEnv.CurrentDepartment.Description, testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowDatabaseName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption - DB: " + databaseName + " - Branch: " + testEnv.CurrentBranch.Name +
					" - Company: " + testEnv.CurrentCompany.Name + " - Department: " + testEnv.CurrentDepartment.Description, testEnv.GlobalFormTopCaption);

				testEnv.Registry.ShowUserName = true;
				AssertEquals("GlobalFormTopCaption", "FormTopCaption - DB: " + databaseName + " - Branch: " + testEnv.CurrentBranch.Name +
					" - Company: " + testEnv.CurrentCompany.Name + " - Department: " + testEnv.CurrentDepartment.Description + " - User: " + testEnv.CurrentUser.FullName, testEnv.GlobalFormTopCaption);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestGlobalFormTopCaptionNoErrorReportOnBranchAccess()
		{
			using (var testEnv = new TestBaseEnvironment())
			{
				testEnv.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				using (testEnv.TemporaryServiceTaskContext("TST", canRunInAnyBranch: true)) // To disallow access to CurrentBranch
				{
					testEnv.Registry.GlobalFormTopCaption = "";
					testEnv.Registry.ShowDatabaseName = false;
					testEnv.Registry.ShowBranchName = false;
					testEnv.Registry.ShowCompanyName = false;
					testEnv.Registry.ShowDepartmentName = false;
					testEnv.Registry.ShowUserName = false;

					testEnv.Registry.ShowBranchName = true;
					AssertEquals("GlobalFormTopCaption", "Branch: " + Env.CurrentBranch.Name, testEnv.GlobalFormTopCaption);

					AssertEquals("Error should not be reported when CurrentBranch via GlobalFormTopCaption", 0, ErrorReporter.TotalErrorCount);

					AssertEquals("CurrentBranch", Env.CurrentBranch.Name, testEnv.CurrentBranch.Name);
					AssertContains("Service Task: TST accesses environment current branch without setting the environment first.", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestCurrentBranchReportsIssueOnUnexpectedAccess()
		{
			// Arrange
			using (var testEnv = new TestBaseEnvironment())
			{
				testEnv.SetUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK));

				var errorReporterMock = new Mock<IErrorReporter>();
				using (testEnv.TemporaryServiceTaskContext("TST", canRunInAnyBranch: true)) // To disallow access to CurrentBranch
				using (new DisposableAction(ErrorReporter.Clear))
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					// Act
					_ = testEnv.CurrentBranch.Name;
				}

				// Assert
				AssertNoExceptionThrown(() =>
				{
					errorReporterMock.Verify(
						reporter => reporter.ReportDeveloperExceptionOrHandleSilently(
							null,
							@"Service Task: TST accesses environment current branch without setting the environment first.
Direct access to Env.CurrentBranch is not allowed from service tasks, please use Enterprise.Environment.DisposableEnvironment to set your service task's running environment before the task execution.
",
							It.IsNotNull<BranchAccessedWithoutConfiguredEnvironmentException>()),
						Times.Once);
					errorReporterMock.VerifyNoOtherCalls();
				});
			}
		}

		public void TestBranchAccessProtectionWhenMultipleThreads()
		{
			ErrorReporter.Clear();
			using (var testEnv = new TestBaseEnvironment())
			{
				using (var backgroundContextReadyEvent = new AutoResetEvent(false))
				using (var foregroundContextReadyEvent = new AutoResetEvent(false))
				using (testEnv.TemporaryServiceTaskContext("BLA", true))
				{
					var eventTimeout = TimeSpan.FromSeconds(5);

					var backgroundTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (testEnv.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
						{
							backgroundContextReadyEvent.Set();
							Assert(foregroundContextReadyEvent.WaitOne(eventTimeout));
						}
					});

					Assert(backgroundContextReadyEvent.WaitOne(eventTimeout));
					using (testEnv.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
					{
						foregroundContextReadyEvent.Set();
						Assert(backgroundTask.Wait(eventTimeout));
						AssertNotNull(testEnv.CurrentBranch);
						AssertEquals("Error should not be reported, we are in a user context", 0, ErrorReporter.TotalErrorCount);
					}

					AssertNull(testEnv.CurrentBranch);
					AssertEquals("Error should be reported, we are no longer in a user context", 1, ErrorReporter.TotalErrorCount);
					AssertContains("accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestSuspendBranchAccessProtectionWhenMultipleThreads()
		{
			ErrorReporter.Clear();
			using (var testEnv = new TestBaseEnvironment())
			using (testEnv.TemporaryServiceTaskContext("BLA", true))
			{
				int threads = 5;
				int threadsCompletedSuspensions = 0;
				int threadsToCheckCurrentBranch = threads;
				Parallel.For(1, threads + 1, (i) =>
				{
					try
					{
						using (testEnv.SuspendBranchAccessError())
						{
							var suspends = new List<IDisposable>();
							for (int j = 0; j < 10000; j++)
							{
								suspends.Add(testEnv.SuspendBranchAccessError());
							}
							foreach (var suspension in suspends)
							{
								suspension.Dispose();
							}

							AssertNull(testEnv.CurrentBranch);
							AssertEquals("Error should not be reported, access is suspended", 0, ErrorReporter.TotalErrorCount);
						}
					}
					finally
					{
						Interlocked.Increment(ref threadsCompletedSuspensions);
					}

					for (int k = 0; k < 500 && threadsCompletedSuspensions < threads; ++k)
					{
						Thread.Sleep(10);
					}

					for (int k = 0; k < 500 && threadsToCheckCurrentBranch != i; ++k)
					{
						Thread.Sleep(10);
					}
					AssertEquals("Error should not be reported, haven't accessed yet", 0, ErrorReporter.TotalErrorCount);
					AssertNull(testEnv.CurrentBranch);
					AssertEquals("Error should be reported, we are no longer in a user context", 1, ErrorReporter.TotalErrorCount);
					AssertContains("accesses environment current branch without setting the environment first", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
					Interlocked.Decrement(ref threadsToCheckCurrentBranch);
				});
			}
		}

		public void TestExceptionDuringContext()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New(ObjectFactory.GetType("IGlbStaff"));
			staff.FillWithValidTestData();
			factory.Save();

			void Instance_UserContextChanging(object sender, IUserContextChangingEventArgs e) => throw new NotImplementedException("Explode for the test.");
			var firstContext = new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			var secondContext = new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK);
			using (var testEnv = new TestBaseEnvironment())
			using (testEnv.SetTemporaryUserContext(firstContext))
			{
				testEnv.UserContextChanging += Instance_UserContextChanging;
				try
				{
					AssertExceptionThrown<NotImplementedException>(() => testEnv.SetTemporaryUserContext(secondContext));
					AssertEquals("", Env.CurrentUserPK, testEnv.CurrentUserPK);
				}
				finally
				{
					testEnv.UserContextChanging -= Instance_UserContextChanging;
				}
			}

			ErrorReporter.Clear();
		}

		public void TestNestedSetTemporaryUserContext()
		{
			ErrorReporter.Clear();
			using (var testEnv = new TestBaseEnvironment())
			using (testEnv.TemporaryServiceTaskContext("BLA", true))
			{
				using (testEnv.SetTemporaryUserContext(new UserContext()))
				{
					AssertNull(testEnv.CurrentBranch);
					AssertEquals("Error should not be reported, temp context has been applied", 0, ErrorReporter.TotalErrorCount);

					using (testEnv.SetTemporaryUserContext(new UserContext()))
					{
						AssertNull(testEnv.CurrentBranch);
						AssertEquals("Error should not be reported, temp context has been applied", 0, ErrorReporter.TotalErrorCount);
					}

					AssertNull(testEnv.CurrentBranch);
					AssertEquals("Error should not be reported, temp context has been applied", 0, ErrorReporter.TotalErrorCount);
				}
			}
		}

		public void TestUserContextTraceLoggerLogsUserContextParameters()
		{
			ErrorReporter.Clear();
			using (var testEnv = new TestBaseEnvironment())
			using (testEnv.TemporaryServiceTaskContext("BLA", true))
			{
				Mock<IUserContext> context1Mock = new Mock<IUserContext>();
				context1Mock.Setup(m => m.Equals(It.IsAny<IUserContext>())).Returns<IUserContext>(x => x == context1Mock.Object);

				Mock<IUserContext> context2Mock = new Mock<IUserContext>();
				context2Mock.Setup(m => m.Equals(It.IsAny<IUserContext>())).Returns<IUserContext>(x => x == context2Mock.Object);

				var user = new Mock<IUser>();
				user.Setup(m => m.LoginName).Returns("Joe");

				var branch = new Mock<IBranch>();
				branch.Setup(m => m.Code).Returns("BranchCode");

				var company = new Mock<ICompany>();
				company.Setup(m => m.Code).Returns("CompanyCode");

				var department = new Mock<IDepartment>();
				department.Setup(m => m.Code).Returns("DepartmentCode");

				context1Mock.Setup(m => m.User).Returns(user.Object);
				context1Mock.Setup(m => m.Branch).Returns(branch.Object);
				context1Mock.Setup(m => m.Company).Returns(company.Object);
				context1Mock.Setup(m => m.Department).Returns(department.Object);

				context2Mock.Setup(m => m.User).Returns(user.Object);
				context2Mock.Setup(m => m.Branch).Returns(branch.Object);
				context2Mock.Setup(m => m.Company).Returns(company.Object);
				context2Mock.Setup(m => m.Department).Returns(department.Object);

				var logger = new UserContextSwitchLogger();

				using (Env.StartContextSwitchTrace(logger))
				{
					using (Env.SetTemporaryUserContext(context1Mock.Object))
					{
						using (Env.SetTemporaryUserContext(context2Mock.Object))
						{
						}
					}
				}

				var logs = logger.Logs.ToArray();
				AssertEquals("Joe", logs[2].NewUserContext.Value.UserName);
				AssertEquals("BranchCode", logs[2].NewUserContext.Value.BranchCode);
				AssertEquals("CompanyCode", logs[2].NewUserContext.Value.CompanyCode);
			}
		}

		public void TestIsNotWebServiceEnvironment()
		{
			using (var testEnv = new TestBaseEnvironment())
			{
				AssertEquals(nameof(testEnv.IsWebService), false, testEnv.IsWebService);
			}
		}

		public void TestGlowUserDataManagerShouldCallClearUserDataWhenSetTempUser()
		{
			var glowUserDataManagerMock = new Mock<IGlowUserDataManager>(MockBehavior.Strict);
			var disposable = new Mock<IDisposable>(MockBehavior.Strict);
			disposable.Setup(m => m.Dispose()).Callback(() => { }).Verifiable();
			glowUserDataManagerMock.Setup(m => m.IncreaseTempUserCount()).Returns(disposable.Object);
			glowUserDataManagerMock.Setup(m => m.ClearUserData()).Verifiable();
			ObjectFactory.Substitute("IGlowServiceClientFactory", glowUserDataManagerMock.Object);

			using var testEnv = new TestBaseEnvironment();
			var currentUserPk = Env.CurrentUser.PK;
			var currentBranchPk = Env.CurrentBranch.PK;
			var currentDepartmentPk = Env.CurrentDepartment.PK;
			var userContext = new UserContext(currentUserPk, currentBranchPk, currentDepartmentPk);
			testEnv.SetUserContext(userContext);
			glowUserDataManagerMock.Verify(m => m.ClearUserData(), Times.Once);

			using (var tempUserContext = testEnv.SetTemporaryUserContext(userContext))
			{
				glowUserDataManagerMock.Verify(m => m.ClearUserData(), Times.Once);
			}
			glowUserDataManagerMock.Verify(m => m.ClearUserData(), Times.Once);

			using (var tempUserContext = testEnv.SetTemporaryMasterUserContext(userContext))
			{
				glowUserDataManagerMock.Verify(m => m.ClearUserData(), Times.Exactly(2));
			}
			glowUserDataManagerMock.Verify(m => m.ClearUserData(), Times.Exactly(3));
			Assert(true);
		}

		public class IsAuthenticatedTest : TransactionedTestCase
		{
			public void TestFalseIfUserIsNull()
			{
				// Arrange
				using (var baseEnvironment = new TestBaseEnvironment())
				{
					var userContextMock = new Mock<IUserContext>();
					userContextMock.Setup(context => context.User).Returns((IUser)null);

					using (baseEnvironment.SetTemporaryUserContext(userContextMock.Object))
					{
						// Act
						var result = baseEnvironment.IsAuthenticated;

						// Assert
						AssertEquals(false, result);
						userContextMock.Verify(context => context.User, Times.AtLeastOnce);
					}
				}
			}

			public void TestDoesNotCareAboutTwoFactorAuthentication()
			{
				// Arrange
				using (var baseEnvironment = new TestBaseEnvironment())
				{
					var userContextMock = new Mock<IUserContext>();
					var userMock = new Mock<IUser>();
					userContextMock.Setup(context => context.User).Returns(userMock.Object);
					userContextMock.Setup(context => context.LoginAuthenticationInfo).Returns(LoginAuthenticationInfo.NewSuccessfulLogin(userMock.Object));

					using (baseEnvironment.SetTemporaryUserContext(userContextMock.Object))
					{
						// Act
						var result = baseEnvironment.IsAuthenticated;

						// Assert
						AssertEquals(true, result);
						userMock.Verify(user => user.IsTwoFactorAuthenticationEnabled, Times.Never);
					}
				}
			}

			public void TestFalseIfLoginAuthenticationInfoIsNull()
			{
				// Arrange
				using (var baseEnvironment = new TestBaseEnvironment())
				{
					var userContextMock = new Mock<IUserContext>();
					var userMock = new Mock<IUser>();
					userContextMock.Setup(context => context.User).Returns(userMock.Object);
					userContextMock.Setup(context => context.LoginAuthenticationInfo).Returns((LoginAuthenticationInfo)null);

					using (baseEnvironment.SetTemporaryUserContext(userContextMock.Object))
					{
						// Act
						var result = baseEnvironment.IsAuthenticated;

						// Assert
						AssertEquals(false, result);
						userContextMock.Verify(context => context.User, Times.AtLeastOnce);
					}
				}
			}

			public void TestFalseIfLoginAuthenticationInfoIsNotOk()
			{
				// Arrange
				using (var baseEnvironment = new TestBaseEnvironment())
				{
					var userContextMock = new Mock<IUserContext>();
					var userMock = new Mock<IUser>();
					userContextMock.Setup(context => context.User).Returns(userMock.Object);
					var loginAuthenticationInfo = LoginAuthenticationInfo.NewFailedLogin(LoginAuthenticationInfo.Status.DepartmentNotFound, ":(");
					userContextMock.Setup(context => context.LoginAuthenticationInfo).Returns(loginAuthenticationInfo);

					using (baseEnvironment.SetTemporaryUserContext(userContextMock.Object))
					{
						// Act
						var result = baseEnvironment.IsAuthenticated;

						// Assert
						AssertEquals(false, result);
						userContextMock.Verify(context => context.User, Times.AtLeastOnce);
					}
				}
			}

			public void TestTrueIfLoginAuthenticationInfoIsOk()
			{
				// Arrange
				using (var baseEnvironment = new TestBaseEnvironment())
				{
					var userContextMock = new Mock<IUserContext>();
					var userMock = new Mock<IUser>();
					userContextMock.Setup(context => context.User).Returns(userMock.Object);
					var loginAuthenticationInfo = LoginAuthenticationInfo.NewSuccessfulLogin(userMock.Object);
					userContextMock.Setup(context => context.LoginAuthenticationInfo).Returns(loginAuthenticationInfo);

					using (baseEnvironment.SetTemporaryUserContext(userContextMock.Object))
					{
						// Act
						var result = baseEnvironment.IsAuthenticated;

						// Assert
						AssertEquals(true, result);
						userContextMock.Verify(context => context.User, Times.AtLeastOnce);
					}
				}
			}

			public void TestSetTemporaryMasterUserContext()
			{
				IUser oldCurrentUser = Env.Instance.CurrentUser;
				var disposable = new Mock<IDisposable>(MockBehavior.Strict);
				var glowUserDataManagerMock = new Mock<IGlowUserDataManager>();
				disposable.Setup(m => m.Dispose()).Verifiable();
				glowUserDataManagerMock.Setup(m => m.IncreaseTempUserCount()).Returns(disposable.Object).Verifiable();
				ObjectFactory.Substitute("IGlowServiceClientFactory", glowUserDataManagerMock.Object);

				using (Env.Instance.SetTemporaryMasterUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
				{
					AssertEquals("Temporary Environment CurrentUser", User.UnKnownUserName, Env.Instance.CurrentUser.LoginName);

					Task.Factory.StartNew(() =>
					{
						AssertEquals("Temporary Environment CurrentUser", User.UnKnownUserName, Env.Instance.CurrentUser.LoginName);
					}, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();
				}

				AssertEquals("CurrentUser was restored", oldCurrentUser.LoginName, Env.Instance.CurrentUser.LoginName);

				Task.Factory.StartNew(() =>
				{
					AssertEquals("CurrentUser was restored", oldCurrentUser.LoginName, Env.Instance.CurrentUser.LoginName);
				}, TaskCreationOptions.LongRunning).GetAwaiter().GetResult();
				glowUserDataManagerMock.Verify(m => m.IncreaseTempUserCount(), Times.Never);
			}

			public void TestDbCurrentUserDefaultValue()
			{
				using (Env.Instance.SetTemporaryUserContext(null))
				{
					AssertEquals("DEF", Db.GetCurrentUserOrDefault("DEF"));
				}
			}

			public void TestDbCurrentUser()
			{
				AssertEquals(Env.CurrentUser.Initials, Db.GetCurrentUserOrDefault(null));
			}

			public void TestSetTemporaryUserContextRestoredWhenDbErrorFromDispose()
			{
				// arrange
				var factory = new BusinessObjectFactory();
				var oldUserContext = Env.CurrentUserContext;

				// act
				using (factory.DelayedTransaction())
				using (Env.Instance.SuppressSwitchContextCheck(false))
				using (Env.Instance.SetTemporaryUserContext(null))
				{
					_ = ((IDbConnected)factory).Connection.ExecuteNonQuery("select 1; rollback");
				}

				// assert
				AssertEquals(oldUserContext, Env.CurrentUserContext);
			}

			public void TestContextTraceDoesNotLogOnThreadsThatHaveNotStartedTrace()
			{
				using (new TestBaseEnvironment())
				{
					var switchLogger = new UserContextSwitchLogger();

					var thread = new Thread(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (Env.Instance.SuppressSwitchContextCheck())
						{
							var userContext = new UserContext("Jeff", Env.Instance.CurrentBranchPK, Env.Instance.CurrentDepartmentPK);
							Env.Instance.SetTemporaryUserContext(userContext).Dispose();
						}
					});

					using (Env.StartContextSwitchTrace(switchLogger))
					{
						thread.Start();
						thread.Join();
					}

					Assert("No logs should have been created as context switch occured in another thread", switchLogger.Logs.IsEmpty);
				}
			}
		}
	}
}
