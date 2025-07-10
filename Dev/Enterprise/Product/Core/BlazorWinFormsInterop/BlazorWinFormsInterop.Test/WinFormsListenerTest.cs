using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Microsoft.AspNet.SignalR.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.BlazorWinFormsInterop.Test
{
	[GuiTest]
	public class WinFormsListenerTest : TestCaseWithFactory
	{
		GlbGroup userGroup;
		GlbStaff staff;

		protected override void SetUp()
		{
			using (var hub = new BlazorWinFormsInteropHub())
			{
				hub.OnDisconnected(false);
			}

			userGroup = Factory.NewWithValidTestData<GlbGroup>();
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.Groups.Add(userGroup);
			Factory.Save();

			base.SetUp();
		}

		public void TestListen()
		{
			using (var listener = new WinFormsListener())
			{
				Assert(listener.ListenUrl == null);
				listener.StartListener();
				Assert("Listen URL should be initialised, should have a hostname, should have a GUID, but was " + listener.ListenUrl,
					Regex.IsMatch(listener.ListenUrl, @"^http://[a-z0-9.\-]{2,}:7070/cargowise/blazorwinformintegration/[0-9a-f\-]{32,}/[^+/]{44,}$", RegexOptions.IgnoreCase));
			}
		}

		[TestRequiresAdministrativePrivileges("Setting URL ACL requires admin")]
		public void TestUrlReservation()
		{
			using (var httpApi = new HttpApi())
			{
				if (httpApi.GetHttpServiceConfigUrlAclInfo(WinFormsListener.UrlAclUrlPrefix) != null)
				{
					httpApi.DeleteHttpServiceConfigUrlAclInfo(WinFormsListener.UrlAclUrlPrefix);
				}

				HttpServiceConfig.EnsureHttpServiceConfig();

				AssertNotNull(httpApi.GetHttpServiceConfigUrlAclInfo(WinFormsListener.UrlAclUrlPrefix));
			}
		}

		public void TestOpenModuleIfEnabledReturnsFalseIfOnlyStartListenerCalled()
		{
			using (var listener = new WinFormsListener())
			{
				listener.StartListener();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				Assert(!success);
			}
		}

		public void TestOpenModuleIfEnabledForConvertedModule()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			CreateWinzorFeature(userGroup, true);
			CreateFeature(userGroup, "RefAirline", true);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var listener = new WinFormsListener())
			{
				Task<string> openModuleTask = null;
				mockProgramLauncher.Setup(o => o.Launch(It.IsAny<Uri>(), It.IsAny<string>())).Callback(() => openModuleTask = ListenForOpenModule(listener));
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				Assert(success);
				AssertEquals("dummyTestQueryString", openModuleTask?.Result);
				mockProgramLauncher.Verify(p => p.Launch(It.Is<Uri>(s => s == new Uri(Env.Registry.BlazorUrl)), It.Is<string>(s => s == listener.ListenUrl)));
			}
		}

		public void TestOpenModuleIfAllEnabledForWinzor()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			CreateWinzorFeature(userGroup, true);
			CreateFeature(userGroup, StmFeatureTest.WinzorAllFeaturesCode, true);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var listener = new WinFormsListener())
			{
				Task<string> openModuleTask = null;
				mockProgramLauncher.Setup(o => o.Launch(It.IsAny<Uri>(), It.IsAny<string>())).Callback(() => openModuleTask = ListenForOpenModule(listener));
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				Assert(success);
				AssertEquals("dummyTestQueryString", openModuleTask.GetAwaiter().GetResult());
				mockProgramLauncher.Verify(p => p.Launch(It.Is<Uri>(s => s == new Uri(Env.Registry.BlazorUrl)), It.Is<string>(s => s == listener.ListenUrl)));
			}
		}

		public void TestExitHybridModeIfAllEnabledForWinzor()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			CreateWinzorFeature(userGroup, true);
			CreateFeature(userGroup, StmFeatureTest.WinzorAllFeaturesCode, true);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var listener = new WinFormsListener())
			{
				Task<string> exitWinzorModeTask = null;
				mockProgramLauncher.Setup(o => o.Launch(It.IsAny<Uri>(), It.IsAny<string>())).Callback(() => exitWinzorModeTask = ListenForExitWinzorMode(listener));
				listener.Initialise();
				var success = listener.ExitHybridMode();
				AssertEquals(true, success);
				AssertEquals("The Winzor is closed", exitWinzorModeTask.GetAwaiter().GetResult());
				mockProgramLauncher.Verify(p => p.Launch(It.Is<Uri>(s => s == new Uri(Env.Registry.BlazorUrl)), It.Is<string>(s => s == listener.ListenUrl)));
			}
		}

		public void TestExitHybridModeWhenWinFormListenerDisabled()
		{
			using (var listener = new WinFormsListener())
			{
				listener.Disable();
				var success = listener.ExitHybridMode();
				AssertEquals(false, success);
				AssertEquals(false, BlazorWinFormsInteropHub.Connected);
				ErrorReporter.Clear();
			}
		}

		public void TestExitHybridModeWhenWinFormListenerUnInitialise()
		{
			using (var listener = new WinFormsListener())
			{
				var success = listener.ExitHybridMode();
				AssertEquals(false, success);
				AssertEquals(false, BlazorWinFormsInteropHub.Connected);
			}
		}

		public void TestOpenModuleIfEnabledReturnsFalseWhenInitialisationFails()
		{
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			mockProgramLauncher.Setup(p => p.Launch(It.IsAny<Uri>(), It.IsAny<string>())).Throws(new Exception());
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			using (var listener = new WinFormsListener())
			{
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				AssertEquals(false, success);
				mockProgramLauncher.VerifyAll();
				ErrorReporter.Clear();
			}
		}

		public void TestOpenModuleIfEnabledReturnsFalseWhenListenerDisabled()
		{
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			using (var listener = new WinFormsListener())
			{
				listener.Initialise();
				listener.Disable();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				AssertEquals(false, success);
				mockProgramLauncher.Verify(o => o.Launch(It.IsAny<Uri>(), It.IsAny<string>()), Times.Once);
			}
		}

		public void TestOpenModuleIfEnabledReturnsFalseWhenUninitialised()
		{
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			using (var listener = new WinFormsListener())
			{
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				AssertEquals(false, success);
				mockProgramLauncher.Verify(o => o.Launch(It.IsAny<Uri>(), It.IsAny<string>()), Times.Never);
			}
		}

		public void TestOpenModuleIfEnabledTimeout()
		{
			DataRegistry.Instance.FeatureTestModeEnabled = true;
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			CreateWinzorFeature(userGroup, true);
			CreateFeature(userGroup, "RefAirline", true);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var listener = new WinFormsListener())
			{
				DateTime startTime = DateTime.UtcNow;
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				DateTime endTime = DateTime.UtcNow;
				Assert(!success);
				AssertGreaterThanOrEqualTo((endTime - startTime).TotalSeconds, Env.Registry.BlazorBackchannelConnectionTimeout.TotalSeconds);
			}
		}

		public void TestOpenModuleIfEnabledSuccessFalseIfNoFeature()
		{
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var listener = new WinFormsListener())
			{
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				Assert(!success);
			}
		}

		public void TestOpenModuleIfEnabledSuccessFalseIfNoFeatureForUser()
		{
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			CreateWinzorFeature(userGroup, true);
			CreateFeature(userGroup, "RefAirline", true);

			using (var listener = new WinFormsListener())
			{
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				Assert(!success);
			}
		}

		public void TestOpenModuleIfEnabledSuccessFalseIfNoActiveFeatureForUser()
		{
			var mockProgramLauncher = new Mock<IBlazorClientAppLauncher>();
			ObjectFactory.Substitute(mockProgramLauncher.Object);
			CreateWinzorFeature(userGroup, true);
			CreateFeature(userGroup, "RefAirline", false);

			using (new TemporaryUserContext { StaffLoginName = staff.GS_LoginName }.Set())
			using (var listener = new WinFormsListener())
			{
				listener.Initialise();
				listener.OpenModule("RefAirline", "dummyTestQueryString", out var success);
				Assert(!success);
			}
		}

		Task<string> ListenForOpenModule(WinFormsListener listener)
		{
			return Task.Run(() =>
			{
				using (var hubConnection = new HubConnection(listener.ListenUrl))
				{
					var openModuleCompletionSource = new TaskCompletionSource<string>();
					var hubProxy = hubConnection.CreateHubProxy(nameof(BlazorWinFormsInteropHub));
					hubProxy.On(nameof(IBlazorClient.OpenUrlInWinzorMode), s =>
					{
						openModuleCompletionSource.SetResult(s);
					});
					hubConnection.Start().Wait();
					hubProxy.Invoke(nameof(IBlazorHub.BackchannelConnected));
					return openModuleCompletionSource.Task.Result;
				}
			});
		}

		Task<string> ListenForExitWinzorMode(WinFormsListener listener)
		{
			return Task.Run(() =>
			{
				using (var hubConnection = new HubConnection(listener.ListenUrl))
				{
					var exitWinzorModeCompletionSource = new TaskCompletionSource<string>();
					var hubProxy = hubConnection.CreateHubProxy(nameof(BlazorWinFormsInteropHub));
					hubProxy.On(nameof(IBlazorClient.ExitWinzorMode), () =>
					{
						exitWinzorModeCompletionSource.SetResult("The Winzor is closed");
					});
					hubConnection.Start().Wait();
					hubProxy.Invoke(nameof(IBlazorHub.BackchannelConnected));
					return exitWinzorModeCompletionSource.Task.Result;
				}
			});
		}

		void CreateFeature(GlbGroup group, string featureName, bool isActive)
		{
			var featureTest = Factory.NewWithValidTestData<StmFeatureTest>();
			featureTest.SFT_FeatureName = featureName;
			featureTest.SFT_IsActive = isActive;
			featureTest.SFT_GG_Group = group.PK;
			Factory.Save();
		}

		void CreateWinzorFeature(GlbGroup group, bool isActive)
		{
			CreateFeature(group, StmFeatureTest.WinzorFeatureCode, isActive);
		}

		protected override void TearDown()
		{
			// Tests fail the TaskTestListener which checks that all tests started during the test have finished when the test exits.
			// The reason is that Microsoft.Owin.Host.HttpListener.OwinHttpListener creates some tasks from WebApp.Start.
			// Those tasks finish when Dispose() is called but they do so asynchronously. Dispose() does not block.

			// In production this HTTP listener lasts for the lifetime of the process so we don't really care about async cleanup;
			// this just keeps tests clean. The exceptions we ignore here are ones that have been seen intermittently on DAT.

			AsyncHelper.WaitAllActiveTasksForTest(ex => ex is ObjectDisposedException
														|| ex is HttpListenerException
														|| (ex is AggregateException && (
															ex.InnerException is ObjectDisposedException
															|| ex.InnerException is HttpListenerException
														)));

			base.TearDown();
		}
	}
}
