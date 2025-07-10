using System;
using CargoWise.ApplicationManager.Common;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Testing;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Client.Testing
{
	class AppManagerInvokerTest : TestCase
	{
		MockConfiguration configuration;
		MockAppManagerInvoker invoker;
		Mock<IAppManager> appManagerMock;
		readonly Type invokerType = typeof(MockAppManagerInvoker);

		protected override void SetUp()
		{
			base.SetUp();
			// Strict Mock requires a setup for Dispose, but which also still triggers that functionality, thus callbase is toggled during setup.
			appManagerMock = new Mock<IAppManager>(MockBehavior.Strict);
			appManagerMock.CallBase = true;
			appManagerMock.Setup(m => m.Dispose());
			appManagerMock.CallBase = false;
			configuration = new MockConfiguration();
			configuration.SetAppManagerClient(appManagerMock.Object);
			var installation = new Installation(configuration);
			invoker = new MockAppManagerInvoker(installation);
		}

#if NETFRAMEWORK
		public void TestError()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(AppManagerResult.GetError("moo"));
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", "moo", result.Message);
			appManagerMock.VerifyAll();
		}
#endif
		public void TestExceptionHandled()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Throws(new Exception("Handle Me"));
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsOK", true, result.IsOK);
			appManagerMock.VerifyAll();
		}

		public void TestExceptionRemoting()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Throws(new RemotingException());
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", AppManagerInvoker.RemotingErrorMessage, result.Message);
			appManagerMock.VerifyAll();
		}

		public void TestCustomExceptionRemoting()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Throws(new CustomRemotingException());
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", AppManagerInvoker.RemotingErrorMessage, result.Message);
			appManagerMock.VerifyAll();
		}

		public void TestExceptionUnhandled()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Throws(new Exception("Do Not Handle Me"));
			AssertExceptionThrown(typeof(Exception), "Do Not Handle Me", delegate
			{ invoker.Invoke(); });
			appManagerMock.VerifyAll();
		}

#if NETFRAMEWORK
		public void TestInvocationParameters()
		{
			var request = MutexRequest.Create("x");
			var result = new AppManagerResult(AppManagerResultStatus.Success);
			appManagerMock.Setup(m =>
				m.Invoke(
					typeof(MockAppManagerInvoker).Assembly.Location,
					typeof(MockAppManagerInvoker).FullName,
					"y",
					request))
				.Returns(new AppManagerResult(AppManagerResultStatus.Success));
			AssertEquals("InvokeAppManager().IsOK", true, invoker.InvokeAppManager<MockAppManagerInvoker>("y", request).IsOK);
			appManagerMock.VerifyAll();
		}

		public void TestRetryAlreadyInstalled()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.Retry));
			invoker.SetNeedsToInstall(false);
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsOK", true, result.IsOK);
			appManagerMock.VerifyAll();
		}

		public void TestRetryBusy()
		{
			appManagerMock.SetupSequence(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.Retry))
				.Returns(new AppManagerResult(AppManagerResultStatus.Retry))
				.CallBase();
			invoker.SetNeedsToInstall(true);
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", AppManagerInvoker.ServerBusyErrorMessage, result.Message);
			appManagerMock.Verify(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null), Times.AtLeast(2));
			appManagerMock.VerifyAll();
		}

		public void TestRetryFails()
		{
			appManagerMock.SetupSequence(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.Retry))
				.Returns(AppManagerResult.GetError("boo"))
				.CallBase();
			invoker.SetNeedsToInstall(true);
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", "boo", result.Message);
			appManagerMock.Verify(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null), Times.AtLeast(2));
			appManagerMock.VerifyAll();
		}

		public void TestRetrySucceeds()
		{
			appManagerMock.SetupSequence(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.Retry))
				.Returns(new AppManagerResult(AppManagerResultStatus.Success))
				.CallBase();
			invoker.SetNeedsToInstall(true);
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsOK", true, result.IsOK);
			appManagerMock.Verify(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null), Times.AtLeast(2));
			appManagerMock.VerifyAll();
		}

		public void TestServerBeingUpgraded()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.WaitingForUpgrade));
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", AppManagerInvoker.ServerBeingUpgradedErrorMessage, result.Message);
			appManagerMock.VerifyAll();
		}

		public void TestSuccess()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.Success));
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsOK", true, result.IsOK);
			appManagerMock.VerifyAll();
		}

		public void TestTimedOut()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.TimedOut));
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsError", true, result.IsError);
			AssertEquals("InvokeAppManager().Message", AppManagerInvoker.TimeoutErrorMessage, result.Message);
			appManagerMock.VerifyAll();
		}

		public void TestWarning()
		{
			appManagerMock.Setup(m => m.Invoke(invokerType.Assembly.Location, invokerType.FullName, "test", null))
				.Returns(new AppManagerResult(AppManagerResultStatus.Success, "Warning"));
			InstallationResult result = invoker.Invoke();
			AssertEquals("InvokeAppManager().IsWarning", true, result.IsWarning);
			AssertEquals("InvokeAppManager().Message", "Warning", result.Message);
			appManagerMock.VerifyAll();
		}
#endif

		class MockAppManagerInvoker : AppManagerInvoker, IAppManagerInvocable
		{
			bool needsToInstall;

			public MockAppManagerInvoker(Installation installation)
				: base(installation)
			{
			}

			protected override InstallationResult HandleAppManagerException(Exception ex)
			{
				return (ex.Message == "Handle Me") ? InstallationResult.OK() : null;
			}

			public InstallationResult Invoke()
			{
				return InvokeAppManager<MockAppManagerInvoker>("test", null);
			}

			public new InstallationResult InvokeAppManager<T>(object state, MutexRequest request) where T : IAppManagerInvocable
			{
				return base.InvokeAppManager<T>(state, request);
			}

			protected override bool NeedsToInstallCore()
			{
				return needsToInstall;
			}

			public void SetNeedsToInstall(bool value)
			{
				needsToInstall = value;
			}

			#region IAppManagerInvocable Members

			AppManagerResult IAppManagerInvocable.Invoke(bool waitedForMutex, object state)
			{
				throw new NotImplementedException();
			}

			#endregion
		}
	}

	// Dummy exception for AppMangerInvoker logic
	// https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev/pullrequest/136911
	[Serializable]
	public class RemotingException : Exception
	{
		public RemotingException() { }

#if NETFRAMEWORK
		protected RemotingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
#endif
	}

	[Serializable]
	public class CustomRemotingException : RemotingException
	{
		public CustomRemotingException() { }
#if NETFRAMEWORK
		protected CustomRemotingException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) { }
#endif
	}
}
