using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Xware.Xt.Grpc.Config;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(DirectxTDiagnostics))]
	class DirectxTDiagnosticsTest : NonPersistentBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			TestUtils.InsertRefSysConfig(Factory);
		}

		public void TestWithNullConfiguration()
		{
			var moq = new Mock<DirectxTDiagnostics>();
			moq.Protected()
				.SetupGet<Configuration>("Configuration")
				.Returns((Configuration)null);

			var errMsg = string.Empty;
			var result = moq.Object.CheckConnection(out errMsg);
			AssertEquals("Configuration is null, Diagnostic should have failed and returned false.", expected: false, result);
			AssertContains("Diagnostic message should contain: ", "Failed to get xT configuration. Please check your registry settings, and ensure the Reference Database service tasks (REF/RDU) are running.", errMsg);
		}

		public void TestWhenConnectionThrowsException_Unavailable()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var connector = new DirectxTDiagnostics();
				var errMsg = string.Empty;
				var result = connector.CheckConnection(out errMsg);
				AssertEquals("Diagnostics should have failed due to exception.", expected: false, result);
				AssertContains("Diagnostic message should contain: ", "Unable to form a connection. Please check your connectivity, and ensure Registry settings and Reference Data are up to date.", errMsg);
			}
		}

		public void TestWhenConnectionThrowsException()
		{
			CombineAssertions(() =>
			{
				FuncForTestingWhenConnectionThrowsException("Internal", "xT services are currently in the process of an upgrade. Please try again later or contact WiseTech support for urgent matters.");
				FuncForTestingWhenConnectionThrowsException("InvalidArgument", "There is a problem with the xT server you are trying to connect to. Please raise an incident or contact WiseTech support.");
			});
		}

		public void FuncForTestingWhenConnectionThrowsException(string status, string expectedMsg)
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var moqClientProvider = new Mock<IMsgClientProvider>();
				moqClientProvider.SetupGet(x => x.ErrorMessage).Returns("Connection issue");
				moqClientProvider.SetupGet(x => x.MsgClient).Throws(new MsgServerConnectionException("test", new Exception($"StatusCode=\"{status}\", Detail=\"test\"")));
				var moq = new Mock<DirectxTDiagnostics>();
				moq.CallBase = true;
				moq.Protected()
					.SetupGet<IMsgClientProvider>("MsgClientProvider")
					.Returns(moqClientProvider.Object);

				var errMsg = string.Empty;
				var result = moq.Object.CheckConnection(out errMsg);
				AssertEquals("Diagnostics should have failed due to exception.", expected: false, result);
				AssertContains("Diagnostic message should contain: ", expectedMsg, errMsg);
			}
		}

		public void TestWithValidConfigurationAndConnection()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "HYE";
			registrationKey.ServerCodeForTest = "CMT";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				(var msgClientProvider, _, _) = TestUtils.GetMockedMsgClientProviderWithMessageInspection();
				var moq = new Mock<DirectxTDiagnostics>();
				moq.CallBase = true;
				moq.Protected()
					.SetupGet<IMsgClientProvider>("MsgClientProvider")
					.Returns(msgClientProvider);

				var errMsg = string.Empty;
				var result = moq.Object.CheckConnection(out errMsg);
				AssertEquals("Diagnostics should have successfully connected.", expected: true, result);
				AssertContains("Diagnostic message should contain: ", "Connection successfully established with xT endpoint.", errMsg);
			}
		}

		public void TestSend()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var directxTDiagnostics = new DirectxTDiagnostics();
			directxTDiagnostics.Send();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			var key = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			AssertEquals(key, interchange.EI_From);
			var keyIn6Character = key.Substring(0, 3) + key.Substring(6, 3);
			AssertEquals(keyIn6Character, interchange.EI_To);
			AssertEquals(GlbBranch.CurrentBranch.PK, interchange.EI_GB);
			AssertEquals(interchange.PK, interchange.EI_SessionGUID);
			AssertEquals(ApplicationCodeList.Codes.XMS, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.DirectxT, interchange.EI_InterchangeType);
			AssertEquals("", interchange.EI_HeaderNText);
			AssertEquals($@"<?xml version=""1.0"" ?><DiagnosticMessage><Receiver>{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}</Receiver></DiagnosticMessage>", interchange.EI_BodyText);
			AssertEquals(EDIInterchange.Status.Queued, interchange.EI_Status);
			AssertEquals(EDIInterchange.TransportType.xT, interchange.EI_TransportType);
			Assert(interchange.EI_IsActive);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(interchange.PK, message.EM_EI);
			AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, message.EM_GE);
			Assert(message.EM_IsTestMessage);
			AssertEquals(EDIMessageSubTypeList.Codes.Orders, message.EM_MessageSubType);
			AssertEquals(EDIMessage.ApplicationCodes.XMS, message.EM_ApplicationCode);
			AssertEquals(EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIInterchangeTypeList.Codes.DirectxT, message.EM_MessageType);
			AssertEquals(EDIInterchange.TransportType.xT, message.EM_TransportType);
		}

		public void TestCheck()
		{
			var directxTDiagnostics = new DirectxTDiagnostics();
			Assert(!directxTDiagnostics.Check());

			directxTDiagnostics.Send();
			Assert(!directxTDiagnostics.Check());

			var newInterchange = Factory.New<XmlEDIInterchange>();
			newInterchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			newInterchange.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			newInterchange.EI_SessionGUID = directxTDiagnostics.Interchange.PK;
			newInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.DirectxT;
			newInterchange.EI_Status = EDIInterchange.Status.Queued;
			Factory.Save();

			Assert(directxTDiagnostics.Check());
		}
	}
}
