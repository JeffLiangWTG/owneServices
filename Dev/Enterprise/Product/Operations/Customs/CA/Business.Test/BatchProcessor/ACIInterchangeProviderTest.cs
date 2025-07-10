using System;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ACIInterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestTimeZoneUNLOCO("CAYYZ")]
		[TestDate(2008, 8, 27, 12, 6, 25)]
		public override void TestMessagesPopulateNewInterchange()
		{
			TestDateAttribute.UseUNLOCO = true;

			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			Enterprise.Messaging.Business.EDIMessage message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message1.EM_MessageText = "Message 1 text";

			Enterprise.Messaging.Business.EDIMessage message2 = collection.AddNew();
			message2.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "Message 2 text";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;

			EDIInterchange[] interchanges = new ACIInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);

			AssertEquals(1, interchanges.Length);
			EDIInterchange interchange = interchanges[0];

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAACI, interchange.EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.SendPending, interchange.EI_Status);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchange.EI_From);
			AssertEquals("EI_To", "CBSAPID", interchange.EI_To);
			AssertEquals("messages", 2, interchange.ContainedMessages.Count);
			AssertCollectionContains(message1, interchange.ContainedMessages);
			AssertCollectionContains(message2, interchange.ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText", ExpectedSuppReportBodyText, interchange.EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText", ExpectedSuppReportHeaderText, interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText", ExpectedSuppReportFooterText, interchange.EI_FooterText);
			AssertEquals("Correct Status, not eHub", EDIInterchange.Status.SendPending, interchange.EI_Status);
		}
		const string ExpectedSuppReportBodyText = "Message 1 textMessage 2 text";
		const string ExpectedSuppReportHeaderText = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080827:0806+<<INTERCHANGENUMBERPLACEHOLDER>>'UNG+GSMCAR+TSITE+SRP+080827:0806+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+D:00A:SUPRPT'";
		const string ExpectedSuppReportFooterText = "UNE+2+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'";

		[TestTimeZoneUNLOCO("CAYYZ")]
		[TestDate(2013, 6, 27, 12, 6, 25)]
		public void TestEManifestForwarderMessagesPopulateNewInterchange()
		{
			TestDateAttribute.UseUNLOCO = true;

			using (CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				AssertEManifestForwarderMessagesPopulateNewInterchange(ExpectedHouseBodyText);
			}
			using (CACustomsDataRegistry.Instance.IncludeAssociationAssignedCode.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				AssertEManifestForwarderMessagesPopulateNewInterchange(ExpectedHouseBodyText.Replace(":ACIHG", "").Replace(":ACIHCM", ""));
			}
		}

		void AssertEManifestForwarderMessagesPopulateNewInterchange(ZString expectedHouseBodyText)
		{
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

			var collection = new NonDependentEDIMessageCollection(Factory);

			var closeMessage1 = collection.AddNew();
			closeMessage1.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			closeMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			closeMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			closeMessage1.EM_MessageText = "Close Message 1 text'";

			var message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message1.EM_MessageText = "Message 1 text'";

			var closeMessage2 = collection.AddNew();
			closeMessage2.EM_MessageType = MessageTypeList.Codes.ACIForwarderClose;
			closeMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			closeMessage2.EM_MessageText = "Close Message 2 text'";
			closeMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;

			var message2 = collection.AddNew();
			message2.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "Message 2 text'";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;

			var message3 = collection.AddNew();
			message3.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageText = "Message 3 text'";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;

			var interchanges = new ACIInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, closeMessage1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, closeMessage2.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message3.EM_Status);

			AssertEquals(1, interchanges.Length);
			EDIInterchange interchange = interchanges[0];

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAACI, interchange.EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchange.EI_From);
			AssertEquals("EI_To", "CBSAPID", interchange.EI_To);
			AssertEquals("messages", 5, interchange.ContainedMessages.Count);
			AssertCollectionContains(closeMessage1, interchange.ContainedMessages);
			AssertCollectionContains(closeMessage2, interchange.ContainedMessages);
			AssertCollectionContains(message1, interchange.ContainedMessages);
			AssertCollectionContains(message2, interchange.ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText", expectedHouseBodyText, interchange.EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText", ExpectedHouseHeaderText, interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText", ExpectedHouseFooterText, interchange.EI_FooterText);
		}
		const string ExpectedHouseBodyText = "UNG+GOVCBR+TSITE+ACIHGP+130627:0806+1+UN+D:11B:ACIHG'Message 1 text'Message 2 text'Message 3 text'UNE+3+1'UNG+GOVCBR+TSITE+ACIHCMGP+130627:0806+2+UN+D:11B:ACIHCM'Close Message 1 text'Close Message 2 text'UNE+2+2'";
		const string ExpectedHouseHeaderText = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+130627:0806+<<INTERCHANGENUMBERPLACEHOLDER>>'";
		const string ExpectedHouseFooterText = "UNZ+2+<<INTERCHANGENUMBERPLACEHOLDER>>'";

		[TestDate(2008, 8, 27, 12, 6, 25)]
		public void TestSendingInterchangeThroughEHub()
		{
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Test);

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			Enterprise.Messaging.Business.EDIMessage message1 = collection.AddNew();
			message1.EM_MessageType = MessageTypeList.Codes.SupplementaryCargoReport;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAACI;
			message1.EM_MessageText = "Message 1 text";

			EDIInterchange[] interchanges = new ACIInterchangeProvider(collection).Interchanges;

			AssertEquals(1, interchanges.Length);
			EDIInterchange interchange = interchanges[0];
			AssertEquals("Correct EDIInterchange Status, for eHub", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("Correct EDIInterchange TransportType, for eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("Correct EDIInterchange To", "CBSATID", interchange.EI_To);
			AssertEquals("Correct EDIMessage Status, for eHub", EDIMessage.Status.Sent, message1.EM_Status);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new ACIInterchangeProvider(collection);
		}
	}
}
