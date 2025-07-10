using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.GB.Ccsuk.Connection.Testing
{
	public class PasswordChangeTests : TestCaseWithFactory
	{
		public void TestChangePassword_Irrelevant()
		{
			var log = new TestLogger();
			var settings = new CcsukIpaddressesSetting() { LocalIpAddress = "127.3.4.5", CcsukParticipantIpAddress = "172.22.6.7" };
			var mocker = new Mock<TcpIpSenderReceiver>(log, settings) { CallBase = true };
			var changePasswordResponse = new PasswordResponse("SM07DANIELROXX0600");  // change not needed
			var logonResponse = new LogonResponse("SM02DANIELROXX0000");  // logon OK
			mocker.Protected().SetupSequence<Body>("UploadAndGetResponse", ItExpr.IsAny<Body>(), ItExpr.IsAny<Messaging.Business.EDIInterchange>())
				.Returns(changePasswordResponse)
				.Returns(logonResponse);

			var sender = mocker.Object;
			sender.LogonOrChangePassword();
			mocker.Verify();
			var logs = log.ToString();
			AssertContains("password change is unnecessary", logs);
			AssertContains("Logged on", logs);
			AssertEquals("", GBCustomsDataRegistry.Instance.CcsukPassword_New.Value);
			AssertEquals("Password not updated in rego", "oldPassword", GBCustomsDataRegistry.Instance.CcsukPassword.Value);
		}

		public void TestChangePassword_Fail()
		{
			var log = new TestLogger();
			var settings = new CcsukIpaddressesSetting() { LocalIpAddress = "127.3.4.5", CcsukParticipantIpAddress = "172.22.6.7" };
			var mocker = new Mock<TcpIpSenderReceiver>(log, settings) { CallBase = true };
			var changePasswordResponse = new PasswordResponse("SM07DANIELROXX0400");  // change failed
			var logonResponse = new LogonResponse("SM02DANIELROXX0000");  // logon OK
			mocker.Protected().SetupSequence<Body>("UploadAndGetResponse", ItExpr.IsAny<Body>(), ItExpr.IsAny<Messaging.Business.EDIInterchange>())
				.Returns(changePasswordResponse)
				.Returns(logonResponse);
			var sender = mocker.Object;
			sender.LogonOrChangePassword();
			mocker.Verify();
			var logs = log.ToString();
			AssertContains("password change unsuccessful", logs);
			AssertContains("New password same as existing password", logs);// response code 04
			AssertContains("Logged on", logs);
			AssertEquals("", GBCustomsDataRegistry.Instance.CcsukPassword_New.Value);
			AssertEquals("Password not updated in rego", "oldPassword", GBCustomsDataRegistry.Instance.CcsukPassword.Value);
		}

		public void TestChangePassword_OK()
		{
			var log = new TestLogger();
			var settings = new CcsukIpaddressesSetting() { LocalIpAddress = "127.3.4.5", CcsukParticipantIpAddress = "172.22.6.7" };
			var mocker = new Mock<TcpIpSenderReceiver>(log, settings) { CallBase = true };
			var changePasswordResponse = new PasswordResponse("SM07DANIELROXX0000");  // change OK
			var logonResponse = new LogonResponse("SM02DANIELROXX0000");  // logon OK
			mocker.Protected().SetupSequence<Body>("UploadAndGetResponse", ItExpr.IsAny<Body>(), ItExpr.IsAny<Messaging.Business.EDIInterchange>())
				.Returns(changePasswordResponse)
				.Returns(logonResponse);
			var sender = mocker.Object;
			sender.LogonOrChangePassword();
			mocker.Verify();
			var logs = log.ToString();
			AssertContains("password changed OK", logs);
			AssertContains("Logging on", logs);
			AssertContains("Logged on", logs);
			AssertEquals("Request to change is wiped", "", GBCustomsDataRegistry.Instance.CcsukPassword_New.Value);
			AssertEquals("Password changed in rego", "newPassword", GBCustomsDataRegistry.Instance.CcsukPassword.Value);
		}

		public void TestChangePassword_DoNotWantToChange()
		{
			GBCustomsDataRegistry.Instance.CcsukPassword_New.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			var log = new TestLogger();
			var settings = new CcsukIpaddressesSetting() { LocalIpAddress = "127.3.4.5", CcsukParticipantIpAddress = "172.22.6.7" };
			var mocker = new Mock<TcpIpSenderReceiver>(log, settings) { CallBase = true };
			var logonResponse = new LogonResponse("SM02DANIELROXX0000");  // logon OK
			mocker.Protected().Setup<Body>("UploadAndGetResponse", ItExpr.IsAny<Body>(), ItExpr.IsAny<Messaging.Business.EDIInterchange>())
				.Returns(logonResponse);
			var sender = mocker.Object;
			sender.LogonOrChangePassword();
			mocker.Verify();
			var logs = log.ToString();
			AssertNotContains("password", logs);
			AssertContains("Logged on", logs);
			AssertEquals("", GBCustomsDataRegistry.Instance.CcsukPassword_New.Value);
			AssertEquals("oldPassword", GBCustomsDataRegistry.Instance.CcsukPassword.Value);
		}

		protected override void SetUp()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.IsInTestMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "DANIELROXX");
			GBCustomsDataRegistry.Instance.CcsukLocalIpForBindingListener_Legacy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "127.44.1.1");
			GBCustomsDataRegistry.Instance.CcsukNetworkIpToDeclareForCallBack_Legacy.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "127.44.1.1");
			GBCustomsDataRegistry.Instance.CcsukPassword.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "oldPassword");
			GBCustomsDataRegistry.Instance.CcsukPassword_New.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "newPassword");
		}

		internal class TestLogger : Integration.ILogger
		{
			public void Log(Integration.LogType type, string message, Exception ex)
			{
				Log(type, message);
			}

			public void Log(Integration.LogType type, string message)
			{
				sb.Append(message);
			}

			public override string ToString()
			{
				return sb.ToStringWithNewLineBetweenAppends();
			}

			readonly ZStringBuilder sb = new ZStringBuilder();
		}
	}
}
