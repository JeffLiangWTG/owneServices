using System;
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
	sealed class EXPInterchangeProviderTest : InterchangeProviderTestCase
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
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			Enterprise.Messaging.Business.EDIMessage message1 = collection.AddNew();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.G7Export;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAEXP;
			message1.EM_MessageText = "Message 1 text";

			Enterprise.Messaging.Business.EDIMessage message2 = collection.AddNew();
			message2.EM_IsTestMessage = false;
			message2.EM_MessageType = MessageTypeList.Codes.G7Export;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "Message 2 text";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAEXP;

			EDIInterchange[] interchanges = new EXPInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);

			AssertEquals(1, interchanges.Length);
			EDIInterchange interchange = interchanges[0];

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAEXP, interchange.EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.SendPending, interchange.EI_Status);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchange.EI_From);
			AssertEquals("EI_To", "CBSAPID", interchange.EI_To);
			AssertEquals("messages", 2, interchange.ContainedMessages.Count);
			AssertCollectionContains(message1, interchange.ContainedMessages);
			AssertCollectionContains(message2, interchange.ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText", ExpectedBodyText, interchange.EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText", ExpectedHeaderText, interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText", ExpectedFooterText, interchange.EI_FooterText);
			AssertEquals("Correct Status, not eHub", EDIInterchange.Status.SendPending, interchange.EI_Status);
		}

		const string ExpectedBodyText = "Message 1 textMessage 2 text";
		const string ExpectedHeaderText = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080827:0806+<<INTERCHANGENUMBERPLACEHOLDER>>'UNG+GSIMEX+TSITE+EP+080827:0806+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+CC+D:00A:EX1STP'";
		const string ExpectedFooterText = "UNE+2+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'";

		[TestDate(2008, 8, 27, 12, 6, 25)]
		public void TestSendingInterchangeThroughEHub()
		{
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");

			NonDependentEDIMessageCollection collection = new NonDependentEDIMessageCollection(Factory);
			Enterprise.Messaging.Business.EDIMessage message1 = collection.AddNew();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.G7Export;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAEXP;
			message1.EM_MessageText = "Message 1 text";

			EDIInterchange[] interchanges = new EXPInterchangeProvider(collection).Interchanges;
			EDIInterchange interchange = interchanges[0];
			AssertEquals("Correct EDIInterchange Status, for eHub", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("Correct EDIInterchange TransportType, for eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("Correct EDIInterchange To", "CBSATID", interchange.EI_To);
			AssertEquals("Correct ediMessage Status, for eHub", EDIMessage.Status.Sent, message1.EM_Status);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new EXPInterchangeProvider(collection);
		}
	}
}
