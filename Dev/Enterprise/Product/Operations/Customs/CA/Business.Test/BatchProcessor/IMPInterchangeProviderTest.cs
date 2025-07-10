using System;
using System.Linq;
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
	sealed class IMPInterchangeProviderTest : InterchangeProviderTestCase
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
			message2.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_MessageText = "Message 2 text";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			var message3 = collection.AddNew();
			message3.EM_IsTestMessage = false;
			message3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageText = "Message 3 text";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_LinkedObject = entryHeader;

			var interchanges = new IMPInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals(EDIMessage.Status.Sent, message3.EM_Status);

			AssertEquals(2, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, interchange.EI_ApplicationCode);
				AssertEquals("EI_Status", EDIInterchange.Status.SendPending, interchange.EI_Status);
				AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_From", "CLIENTID", interchange.EI_From);
				AssertEquals("EI_To", "CBSAPID", interchange.EI_To);
				AssertEquals("Correct Status, not eHub", EDIInterchange.Status.SendPending, interchange.EI_Status);

				if (interchange.ContainedMessages.Count == 1)
				{
					Assert("Contain importer account security code and password", interchange.EI_HeaderText.Contains("12345HGFEDCBA"));
				}
				else
				{
					AssertEquals("messages", 2, interchange.ContainedMessages.Count);
					AssertCollectionContains(message1, interchange.ContainedMessages);
					AssertCollectionContains(message2, interchange.ContainedMessages);
					AssertMultilineASCIIEquals("EI_BodyText", ExpectedBodyText, interchange.EI_BodyText);
					AssertMultilineASCIIEquals("EI_HeaderText", ExpectedHeaderText, interchange.EI_HeaderText);
					AssertMultilineASCIIEquals("EI_FooterText", ExpectedFooterText, interchange.EI_FooterText);
				}
			}

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
			AssertEquals("UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080827:0806+<<INTERCHANGENUMBERPLACEHOLDER>>++CUSPED'UNG+CUSPED+TSITE+CSAUPDATE+080827:0806+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B+98765ABCDEFGH'", tcpInt.EI_HeaderText);
			AssertEquals("UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080827:0806+<<INTERCHANGENUMBERPLACEHOLDER>>++CUSDEC'UNG+CUSDEC+TSITE+CP+080827:0806+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+S:99B'", rsfInt.EI_HeaderText);
		}
		const string ExpectedBodyText = "Message 1 textMessage 2 text";
		const string ExpectedHeaderText = "UNA:+.? 'UNB+UNOA:1+CLIENTID+CBSAPID+080827:0806+<<INTERCHANGENUMBERPLACEHOLDER>>'UNG+CUSDEC+TSITE+RP+080827:0806+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+D:96A+98765ABCDEFGH'";
		const string ExpectedFooterText = "UNE+2+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'";

		[TestTimeZoneUNLOCO("CAYYZ")]
		[TestDate(2008, 8, 27, 12, 6, 25)]
		public void TestCUSREPStatusQueryMessagesPopulateNewInterchange()
		{
			TestDateAttribute.UseUNLOCO = true;

			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABCDEFGH");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.RNSRequest;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageText = "Message 1 text";

			var interchanges = new IMPInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);

			AssertEquals(1, interchanges.Length);
			var interchange = interchanges[0];

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, interchange.EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchange.EI_From);
			AssertEquals("EI_To", "CBSAPID", interchange.EI_To);
			AssertEquals("messages", 1, interchange.ContainedMessages.Count);
			AssertCollectionContains(message1, interchange.ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText", ExpectedCUSREPBodyText, interchange.EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText", ExpectedCUSREPHeaderText, interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText", ExpectedCUSREPFooterText, interchange.EI_FooterText);
		}
		const string ExpectedCUSREPBodyText = "Message 1 text";
		const string ExpectedCUSREPHeaderText = "UNA:+.? 'UNB+UNOA:3+CLIENTID+CBSAPID+080827:0806+<<INTERCHANGENUMBERPLACEHOLDER>>++CUSREP'UNG+CUSREP+TSITE+PARSPDN+080827:0806+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>+UN+D:96A'";
		const string ExpectedCUSREPFooterText = "UNE+1+<<FUNCTIONALGROUPNUMBERPLACEHOLDER>>'UNZ+1+<<INTERCHANGENUMBERPLACEHOLDER>>'";

		[TestTimeZoneUNLOCO("CAYYZ")]
		[TestDate(2008, 8, 27, 12, 6, 25)]
		public void TestCUSREPArrivalCertificationMessagesPopulateNewInterchange()
		{
			TestDateAttribute.UseUNLOCO = true;

			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSATID");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CBSAPID");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTID");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "TSITE");
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ABCDEFGH");
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);

			var collection = new NonDependentEDIMessageCollection(Factory);
			var message1 = collection.AddNew();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.RNSRequest;
			message1.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_MessageText = "Message 1 text";

			var interchanges = new IMPInterchangeProvider(collection).Interchanges;

			AssertEquals(EDIMessage.Status.Sent, message1.EM_Status);

			AssertEquals(1, interchanges.Length);
			var interchange = interchanges[0];

			AssertEquals("EI_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, interchange.EI_ApplicationCode);
			AssertEquals("EI_Status", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
			AssertEquals("EI_TransportType", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			AssertEquals("EI_ReceiveTransmit", EDIInterchange.Direction.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals("EI_From", "CLIENTID", interchange.EI_From);
			AssertEquals("EI_To", "CBSAPID", interchange.EI_To);
			AssertEquals("messages", 1, interchange.ContainedMessages.Count);
			AssertCollectionContains(message1, interchange.ContainedMessages);
			AssertMultilineASCIIEquals("EI_BodyText", ExpectedCUSREPBodyText, interchange.EI_BodyText);
			AssertMultilineASCIIEquals("EI_HeaderText", ExpectedCUSREPHeaderText, interchange.EI_HeaderText);
			AssertMultilineASCIIEquals("EI_FooterText", ExpectedCUSREPFooterText, interchange.EI_FooterText);
		}

		[TestDate(2008, 8, 27, 12, 6, 25)]
		public void TestSendingInterchangeThroughEHub()
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
			message2.EM_MessageText = "Message 2 text";
			message2.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;

			var message3 = collection.AddNew();
			message3.EM_IsTestMessage = false;
			message3.EM_MessageType = MessageTypeList.Codes.EDIRelease;
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message3.EM_MessageText = "Message 3 text";
			message3.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message3.EM_LinkedObject = entryHeader;

			var interchanges = new IMPInterchangeProvider(collection).Interchanges;

			AssertEquals(2, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("Correct EDIInterchange Status, for eHub", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("Correct EDIInterchange TransportType, for eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
			}
			AssertEquals("Correct EDIMessage Status _1, for eHub", EDIMessage.Status.Sent, message1.EM_Status);
			AssertEquals("Correct EDIMessage Status _2, for eHub", EDIMessage.Status.Sent, message2.EM_Status);
			AssertEquals("Correct EDIMessage Status _3, for eHub", EDIMessage.Status.Sent, message3.EM_Status);
		}

		[TestDate(2008, 8, 27, 12, 6, 25)]
		public void TestSendingInterchangeGroupingAndIsTestSetter()
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

			var message7 = collection.AddNew();
			message7.EM_IsTestMessage = true;
			message7.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			message7.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message7.EM_MessageText = "Message 7 text";
			message7.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message7.EM_LinkedObject = entryHeader;

			var message8 = collection.AddNew();
			message8.EM_IsTestMessage = false;
			message8.EM_MessageType = MessageTypeList.Codes.XTypeEntry;
			message8.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message8.EM_MessageText = "Message 8 text";
			message8.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message8.EM_LinkedObject = entryHeader;

			var interchanges = new IMPInterchangeProvider(collection).Interchanges;

			AssertEquals(4, interchanges.Length);
			foreach (var interchange in interchanges)
			{
				AssertEquals("Correct EDIIntechange Status, for eHub", EDIInterchange.Status.eHubQueued, interchange.EI_Status);
				AssertEquals("Correct EDIInterchange TransportType, for eHub", EDIInterchange.TransportType.eHub, interchange.EI_TransportType);
				switch (interchange.EI_InterchangeType + "/" + interchange.EI_To)
				{
					case "B3X/CBSAPID":
						AssertEquals(message8.EM_EI, interchange.PK);
						break;
					case "B3X/CBSATID":
						AssertEquals(message7.EM_EI, interchange.PK);
						break;
					case "REL/CBSATID":
						AssertEquals(message3.EM_EI, interchange.PK);
						break;
					case "REL/CBSAPID":
						AssertEquals(message2.EM_EI, interchange.PK);
						AssertEquals(message1.EM_EI, interchange.PK);
						break;
					default:
						Fail("Something Wrong");
						break;
				}
			}
		}

		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new IMPInterchangeProvider(collection);
		}
	}
}
