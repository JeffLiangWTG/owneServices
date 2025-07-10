using System;
using System.Linq;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAB3InterchangeProviderTest : InterchangeProviderTestCase
	{
		[TestTimeZoneUNLOCO("CAYYZ")]
		[TestDate(2024, 05, 05, 05, 05, 05)]
		public override void TestMessagesPopulateNewInterchange()
		{
			TestDateAttribute.UseUNLOCO = true;

			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABCDEFGH");
			CACustomsDataRegistry.Instance.DisplaySequentialOfTransactionNumberSeparately.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

			var importer = Factory.New<OrgHeader>();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "HGFEDCBA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_UseImporterAccountSecurityNumber = true;
			declaration.JE_OH_Importer = importer.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageText = "Message 1 text";

			var message2 = collection.AddNew();
			message2.EM_IsTestMessage = false;
			message2.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "Message 2 text";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			var message3 = collection.AddNew();
			message3.EM_IsTestMessage = false;
			message3.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageText = "Message 3 text";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_LinkedObject = entryHeader;

			var interchanges = new CAB3CInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message3.EM_Status);

			AssertEquals(3, interchanges.Length);

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, interchanges[0].EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.SendPending, interchanges[0].EI_Status);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchanges[0].EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchanges[0].EI_From);
			AssertEquals("EI_To", "CBSAPID", interchanges[0].EI_To);
			AssertEquals("Correct Status, not eHub", EDIInterchange.Status.SendPending, interchanges[0].EI_Status);

			AssertEquals("Contained message", 1, interchanges[0].ContainedMessages.Count);
			AssertCollectionContains(message1, interchanges[0].ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText1", "Message 1 text", interchanges[0].EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText1", ExpectedHeaderText, interchanges[0].EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText1", ExpectedFooterText, interchanges[0].EI_FooterText);

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, interchanges[1].EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.SendPending, interchanges[1].EI_Status);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchanges[1].EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchanges[1].EI_From);
			AssertEquals("EI_To", "CBSAPID", interchanges[1].EI_To);
			AssertEquals("Correct Status, not eHub", EDIInterchange.Status.SendPending, interchanges[1].EI_Status);

			AssertEquals("Contained message", 1, interchanges[1].ContainedMessages.Count);
			AssertCollectionContains(message2, interchanges[1].ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText2", "Message 2 text", interchanges[1].EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText2", ExpectedHeaderText2, interchanges[1].EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText2", ExpectedFooterText, interchanges[1].EI_FooterText);

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, interchanges[2].EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.SendPending, interchanges[2].EI_Status);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchanges[1].EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchanges[2].EI_From);
			AssertEquals("EI_To", "CBSAPID", interchanges[2].EI_To);
			AssertEquals("Correct Status, not eHub", EDIInterchange.Status.SendPending, interchanges[2].EI_Status);

			AssertEquals("Contained message", 1, interchanges[2].ContainedMessages.Count);
			AssertCollectionContains(message3, interchanges[2].ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText3", "Message 3 text", interchanges[2].EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText3", ExpectedHeaderText3, interchanges[2].EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText3", ExpectedFooterText, interchanges[2].EI_FooterText);

			collection.RemoveAndDeleteAll();

			var message4 = collection.AddNew();
			message4.EM_IsTestMessage = false;
			message4.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_MessageText = "Trade Chain Partner Message Text";
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			var message5 = collection.AddNew();
			message5.EM_IsTestMessage = false;
			message5.EM_MessageType = MessageTypeList.Codes.CSARevenueSummaryForm;
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5.EM_MessageText = "CSA Revenue Summary Form Message Text";
			message5.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			interchanges = new IMPInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message4.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message5.EM_Status);

			AssertEquals(2, interchanges.Length);

			var tcpInt = interchanges.Where(x => x.EI_InterchangeType == message4.EM_MessageType).FirstOrDefault();
			var rsfInt = interchanges.Where(x => x.EI_InterchangeType == message5.EM_MessageType).FirstOrDefault();
			AssertNotNull(tcpInt);
			AssertNotNull(rsfInt);
			AssertEquals("tcpInt.EI_HeaderText", "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+240505:0105+<<INTERCHANGENUMBERPLACEHOLDER>>++CUSPED'UNG+CUSPED+TSITE+CSAUPDATE+240505:0105+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B+98765ABCDEFGH'", tcpInt.EI_HeaderText);
			AssertEquals("rsfInt.EI_HeaderText", "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+240505:0105+<<INTERCHANGENUMBERPLACEHOLDER>>++CUSDEC'UNG+CUSDEC+TSITE+CP+240505:0105+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B'", rsfInt.EI_HeaderText);
		}
		const string ExpectedHeaderText3 = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+240505:0105+<<INTERCHANGENUMBERPLACEHOLDER>>'UNG+CUSDEC+TSITE+KI+240505:0105+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B+12345HGFEDCBA'";
		const string ExpectedHeaderText2 = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+240505:0105+<<INTERCHANGENUMBERPLACEHOLDER>>'UNG+CUSDEC+TSITE+KI+240505:0105+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B+98765ABCDEFGH'";
		const string ExpectedHeaderText = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+240505:0105+<<INTERCHANGENUMBERPLACEHOLDER>>'UNG+CUSDEC+TSITE+KI+240505:0105+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B+98765ABCDEFGH'";
		const string ExpectedFooterText = "UNE+1+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'";

		[TestDate(2024, 05, 05, 05, 05, 05)]
		public void TestSendingGroupingAndIsTestSetter()
		{
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABCDEFGH");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

			var importer = Factory.New<OrgHeader>();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "HGFEDCBA";

			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_UseImporterAccountSecurityNumber = true;
			declaration.JE_OH_Importer = importer.PK;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageText = "Message 1 text";

			var message2 = collection.AddNew();
			message2.EM_IsTestMessage = false;
			message2.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message2.EM_MessageText = "Message 0 text";

			var message3 = collection.AddNew();
			message3.EM_IsTestMessage = true;
			message3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageText = "Message 2 text";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			var message4 = collection.AddNew();
			message4.EM_IsTestMessage = false;
			message4.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message4.EM_MessageText = "Message 3 text";
			message4.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message4.EM_LinkedObject = entryHeader;

			var message5 = collection.AddNew();
			message5.EM_IsTestMessage = true;
			message5.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message5.EM_MessageText = "Message 4 text";
			message5.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message5.EM_LinkedObject = entryHeader;

			var interchanges = new CAB3CInterchangeProvider(collection).Interchanges;

			AssertEquals(5, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("Correct EDIIntechange Statous, for eHub", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("Correct EDIInterchange TransportType, for eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
				AssertEquals("B3C", interchange.EI_InterchangeType);
			}

			AssertEquals("CBSATID", interchanges[2].EI_To);
			AssertEquals("CBSATID", interchanges[4].EI_To);
			AssertEquals(message3.EM_EI, interchanges[2].PK);
			AssertEquals(message5.EM_EI, interchanges[4].PK);

			AssertEquals("CBSAPID", interchanges[0].EI_To);
			AssertEquals("CBSAPID", interchanges[1].EI_To);
			AssertEquals("CBSAPID", interchanges[3].EI_To);
			AssertEquals(message1.EM_EI, interchanges[0].PK);
			AssertEquals(message2.EM_EI, interchanges[1].PK);
			AssertEquals(message4.EM_EI, interchanges[3].PK);
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new CAB3CInterchangeProvider(collection);
		}
	}
}
