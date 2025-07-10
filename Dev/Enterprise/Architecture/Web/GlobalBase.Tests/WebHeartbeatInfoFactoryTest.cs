using System;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GlobalBase.Tests
{
	class WebHeartbeatInfoFactoryTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestNewHeartbeatInfo()
		{
			var factoryMock = new Mock<WebHeartbeatInfoFactory> { CallBase = true };
			factoryMock.Setup(x => x.New()).CallBase().Verifiable();
			factoryMock.Protected()
				.Setup<Guid>("GetHeartbeatId").CallBase().Verifiable();
			factoryMock.Protected()
				.Setup<string>("GetHostName").CallBase().Verifiable();
			factoryMock.Protected()
				.Setup<Guid>("GetUserPk").CallBase().Verifiable();
			factoryMock.Protected()
				.Setup<int>("GetProcessId").CallBase().Verifiable();
			factoryMock.Protected()
				.Setup<string>("GetHeartBeatType").CallBase().Verifiable();

			var heartBeatInfo = factoryMock.Object.New();

			CombineAssertions(() =>
			{
				AssertEquals(Guid.Empty, heartBeatInfo.HeartbeatId);
				AssertEquals(System.Environment.MachineName, heartBeatInfo.HostName);
				AssertEquals(EnvProxy.Instance.CurrentUser.PK, heartBeatInfo.UserPk);
				AssertEquals(System.Diagnostics.Process.GetCurrentProcess().Id, heartBeatInfo.ProcessId);
				AssertEquals("WEB", heartBeatInfo.HeartbeatType);

				factoryMock.VerifyAll();
				factoryMock.VerifyNoOtherCalls();
			});
		}
	}
}
