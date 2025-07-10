using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(eHubDiagnostics))]
	class eHubDiagnosticsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSend()
		{
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals(0, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var eHubDiagnostics = new eHubDiagnostics();
			eHubDiagnostics.Send();

			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIInterchange)));
			AssertEquals(1, Factory.GetDatabaseCount(typeof(EDIMessage)));

			var interchange = Factory.LoadTop1<EDIInterchange>(new ZQuery());
			AssertEquals(EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_From);
			AssertEquals(GlbCompany.CurrentCompany.LicenceKeyIdentifier, interchange.EI_To);
			AssertEquals(GlbBranch.CurrentBranch.PK, interchange.EI_GB);
			AssertEquals(interchange.PK, interchange.EI_SessionGUID);
			AssertEquals(ApplicationCodeList.Codes.XMS, interchange.EI_ApplicationCode);
			AssertEquals(EDIInterchangeTypeList.Codes.TST, interchange.EI_InterchangeType);
			AssertEquals("<EDIDelivery><FileName></FileName><EmailSubject></EmailSubject></EDIDelivery>", interchange.EI_HeaderNText);
			AssertEquals(@"<ns0:Test xmlns:ns0=""http://www.edi.com.au/EnterpriseService/""><A><a1>a1</a1><a2>a2</a2></A><B><b1>b1</b1><b2>b2</b2></B></ns0:Test>", interchange.EI_BodyText);
			AssertEquals(EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			Assert(interchange.EI_IsActive);

			var message = Factory.LoadTop1<EDIMessage>(new ZQuery());
			AssertEquals(interchange.PK, message.EM_EI);
			AssertEquals(GlbBranch.CurrentBranch.PK, message.EM_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, message.EM_GE);
			Assert(message.EM_IsTestMessage);
			AssertEquals(EDIMessageSubTypeList.Codes.Orders, message.EM_MessageSubType);
			AssertEquals(EDIMessage.ApplicationCodes.XMS, message.EM_ApplicationCode);
			AssertEquals(EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIInterchangeTypeList.Codes.TST, message.EM_MessageType);
			AssertEquals(EDIInterchange.TransportType.eHub, message.EM_TransportType);
		}

		public void TestCheck()
		{
			var eHubDiagnostics = new eHubDiagnostics();
			Assert(!eHubDiagnostics.Check());

			eHubDiagnostics.Send();
			Assert(!eHubDiagnostics.Check());

			var newInterchange = Factory.New<XmlEDIInterchange>();
			newInterchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			newInterchange.EI_To = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
			newInterchange.EI_SessionGUID = eHubDiagnostics.Interchange.PK;
			newInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.TST;
			Factory.Save();

			Assert(eHubDiagnostics.Check());
		}
	}
}
