using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business.BatchProcessor.Testing
{
	sealed class BatchProcessorUtilitiesTest : TestCaseWithFactory
	{
		#region TestRegistry

		public void TestBasicRegistryChecksForMessageSending()
		{
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "");
			CACustomsDataRegistry.Instance.SecurityKeyAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "");
			AssertEquals(@"The CBSA Client ID is not configured,
The Network Client ID is not configured,
The Transmission Site is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", BatchProcessorUtilities.BasicRegistryChecksForMessageSending(true, false));

			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "111");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "2222");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "333");
			AssertEquals(ZString.Empty, BatchProcessorUtilities.BasicRegistryChecksForMessageSending(true, false));

			AssertEquals(@"The CBSA Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", BatchProcessorUtilities.BasicRegistryChecksForMessageSending(false, false));

			CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "555");
			AssertEquals(ZString.Empty, BatchProcessorUtilities.BasicRegistryChecksForMessageSending(false, false));

			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, ZString.Empty);
			AssertEquals(@"The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.
Sending messages for this job is not allowed as it has been marked as inactive. Please check why this job has been deactivated and, if required, mark as active so you can send messages (see 'Make Active' on the actions menu).", BatchProcessorUtilities.BasicRegistryChecksForMessageSending(false, true));
			AssertEquals(@"The Network Client ID is not configured,
in the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", BatchProcessorUtilities.BasicRegistryChecksForMessageSending(true, false));
		}

		#endregion

		#region TestCompanyInCanada

		public void TestCompanyInCanada()
		{
			BatchProcessorUtilities.ResetCompanyInCanadaForTesting();
			var company = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "EDI");
			AssertEquals("Pre-condition, company EDI is set for Canada", Constants.CountryCodes.Canada, company.GC_RN_NKCountryCode);
			Assert("CompanyInCanada", BatchProcessorUtilities.CompanyInCanada);
			try
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				{
					BatchProcessorUtilities.ResetCompanyInCanadaForTesting();
					Assert("No CompanyInCanada", !BatchProcessorUtilities.CompanyInCanada);
				}
			}
			finally
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
				{
					BatchProcessorUtilities.ResetCompanyInCanadaForTesting();
					Assert("CompanyInCanada", BatchProcessorUtilities.CompanyInCanada);
				}
			}
		}

		#endregion

		#region TestClientIDs

		public void TestCBSAClientID()
		{
			AssertEquals("Test CBSA Client ID", "RCCECECPW", BatchProcessorUtilities.CBSAClientID(true));
			AssertEquals("Prod CBSA Client ID", "RCCECECPW", BatchProcessorUtilities.CBSAClientID(false));
		}

		public void TestMailBoxID()
		{
			AssertEquals("Mail Box ID", ZString.Empty, BatchProcessorUtilities.MailBoxID);
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "12345");
			AssertEquals("Mail Box ID", "12345", BatchProcessorUtilities.MailBoxID);
		}

		public void TestCBSAeHubID()
		{
			AssertEquals("Test CBSA eHub ID", "CACustomsTest", BatchProcessorUtilities.CBSAeHubID(true));
			AssertEquals("Prod CBSA eHub ID", "CACustoms", BatchProcessorUtilities.CBSAeHubID(false));
		}

		public void TestIsCBSAeHubID()
		{
			Assert("Test CBSA eHub ID", BatchProcessorUtilities.IsCBSAeHubID("CACustomsTest"));
			Assert("Prod CBSA eHub ID", BatchProcessorUtilities.IsCBSAeHubID("CACustoms"));
			Assert("None CBSA eHub ID", !BatchProcessorUtilities.IsCBSAeHubID("TestID"));
		}

		#endregion

		#region TestGetApplicationCode

		public void TestGetApplicationCode()
		{
			//G7 Export
			var g7Messages =
				new[]
					{
						"UNB+UNOA:3+CBSAID+SENDERID+020925:1015+12345678901234'\r\nUNG+CUSRES+G7CCR+CLIENTID+020925:1015+123456PASSWORD+CC+D:00A'\r\nUNH+12345600REFNBR+CUSRES:D:00A:UN'",
						"UNB+UNOA:3+CLIENTNETWORKID+CBSANETWORKID+021113:1110+AB123'\r\nUNG+GSIMEX+CC123123:1004+ET+021113:1110+WC123+CC+D:00A:EX1STP'\r\nUNH+ABCA1231+GSIMEX:D:00A:CC:EX1STP'\r\n"
					};

			AssertApplicationCodeForMessages(EDIInterchange.ApplicationCodes.CAEXP, g7Messages);

			//ACI
			var aciMessages =
				new[]
					{
						"UNB+UNOA:3+CLIENTSNETWORKID+CBSANETWORKID+040220:0855+12345678901234'\r\nUNG+CUSRES+CCR+RECIPIENTIND+040220:0855+43210987654321+UN+D:00A'",
						"UNB+UNOA:3+ CLIENTSNETWORKID+CBSANETWORKID +040121:0930+123456789'UNG+GSMCAR+24681012+SRP+040121:0930+246810+UN+D:00A:SUPRPT'UNH+123456+GSMCAR:D:00A:UN:SUPRPT'\r\nBGM+85+ABCD1234+9'\r\nCST++687::96'\r\nTDT+20++1++8080'\r\nCNI+1'\r\nDOC+704+9990CCN20040204'\r\nRFF+ABE:8080SRN20040204'\r\nLOC+8+CA:::MISSISSAUGA'",
						"UNB+UNOA:3+CLIENTID+CBSAPID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHGT+130627:1206+2+UN+D:11B'",
						"UNB+UNOA:3+CLIENTID+CBSAPID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHGP+130627:1206+2+UN+D:11B'",
						"UNB+UNOA:3+CLIENTTID+CBSATID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHCMGT+130627:1206+3+UN+D:11B'",
						"UNB+UNOA:3+CLIENTTID+CBSATID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHCMGP+130627:1206+3+UN+D:11B'",
						"UNB+UNOA:2+CBSATID+CLIENTTID+130731:1209+1++++++1'UNG+GOVCBR+CCR+U10207V1+20130731:1209+1+UN+D:11B'"
					};

			AssertApplicationCodeForMessages(EDIInterchange.ApplicationCodes.CAACI, aciMessages);

			//Import
			var impMessages =
				new[]
					{
						@"UNB+UNOA:1+C&E NETWORK ID+CLIENT ID+950115:1130+REF12345++CUSRES'UNG+CUSRES+CCR+RECIPIENT DEFINED ID+950115:1130+GREF12345+UN+D:96A'UNH+MREF12345+CUSRES:D:96A:UN'",
						"'UNG+CUSDEC+U41091N1+KI+100527:0349+4+UN+S:99B+12345PASSWORD'UNH+12+CUSDEC:S:99B:UN'",
						"UNG+CUSRES+CCS+TRANSMISSION SITE+100212:1447+1622+UN+S:99B",
						"'UNG+CUSDEC+U10207V1+QA+100608:1857+9+UN+S:99B+10207YUSEN'UNH+18+CUSDEC:S:99B:UN'",
						"'UNG+CUSDEC+U10207V1+QE+100608:1857+9+UN+S:99B+10207YUSEN'UNH+18+CUSDEC:S:99B:UN'",
						"UNG+CUSDEC+U41091N1+RT+100526:0605+1+UN+D:96A+12345PASSWORD",
						"UNG+CUSRES+CCR+RECIPIENT+950115:1130+GREF12345+UN+D:96A'",
						"UNG+CUSRES+CCSSYNTAX++100707:1027+10+UN+S:99B'",
						"UNG+CUSRES+QRCLASS/TAR+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+BROADCAST+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+CLASSFILE+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+GSTFILE+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+EXCISETAX+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+TARIFFCODE+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+EXCHANGERATE+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSRES+QREXCHANGE+U10207V1+100721:0718+40+UN+S:99B'",
						"UNG+CUSREP+U10207V1+PARSTST+100907:0222+44+UN+D:96A'",
						"UNG+CUSRES+CCR+U00350V1+170427:1046+23007+UN+S:99B'"
					};

			AssertApplicationCodeForMessages(EDIInterchange.ApplicationCodes.CAIMP, impMessages);

			//Other
			const string unknownMessage = @"XXXX";
			AssertEquals("Other message Application Code", EDIInterchange.ApplicationCodes.CACustoms, BatchProcessorUtilities.GetApplicationCode(unknownMessage));
		}

		static void AssertApplicationCodeForMessages(string appCode, IList<string> messages)
		{
			for (var i = 0; i < messages.Count; i++)
			{
				AssertEquals(string.Format("Application code for {0} message", i), appCode, BatchProcessorUtilities.GetApplicationCode(messages[i]));
			}
		}

		#endregion

		#region TestGetMessageType

		public void TestGetMessageType()
		{
			var messageText = "UNB+UNOA:3+CLIENTNETWORKID+CBSANETWORKID+021113:1110+AB123'\r\nUNG+GSIMEX+CC123123:1004+ET+021113:1110+WC123+CC+D:00A:EX1STP'\r\nUNH+ABCA1231+GSIMEX:D:00A:CC:EX1STP'\r\nBGM+914+UNIQUEIDENTIFYINGNUMBER+9'\r\nLOC+42+0009::96'\r\nLOC+172+0351::96'\r\nDTM+129:200512281120:203'\r\nMEA+WT+AAD+KGM:1000'";
			AssertEquals("MessageType", MessageTypeList.Codes.G7Export, BatchProcessorUtilities.GetMessageType(messageText));

			messageText = "UNB+UNOA:3+ CLIENTSNETWORKID+CBSANETWORKID +040121:0930+123456789'\r\nUNG+GSMCAR+24681012+SRP+040121:0930+246810+UN+D:00A:SUPRPT'\r\nUNH+123456+GSMCAR:D:00A:UN:SUPRPT'\r\nBGM+85+ABCD1234+9'\r\nCST++687::96'\r\nTDT+20++1++8080'\r\nCNI+1'\r\nDOC+704+9990CCN20040204'\r\nRFF+ABE:8080SRN20040204'\r\nLOC+8+CA:::MISSISSAUGA'";
			AssertEquals("MessageType", MessageTypeList.Codes.SupplementaryCargoReport, BatchProcessorUtilities.GetMessageType(messageText));

			messageText = "UNB+UNOA:2+INETCECPT+YUSEMFTEST+130731:1209+1++++++1'\r\nUNG+GOVCBR+CCR+U10207V1+20130731:1209+1+UN+D:11B'\r\nUNH+1+GOVCBR:D:11B:UN'\r\nBGM+313+803636474747+11'\r\nDTM+9:201307311207:203'\r\nRFF+AGO:HBL-803636474747'\r\nRCS+11'";
			AssertEquals("MessageType", EDIInterchange.ApplicationCodes.CACustoms, BatchProcessorUtilities.GetMessageType(messageText));

			messageText = "UNB+UNOA:2+INETCECPT+YUSEMFTEST+130731:1209+1++++++1'\r\nUNG+GOVCBR+CCR+U10207V1+20130731:1209+1+UN+D:11B'\r\nUNH+1+GOVCBR:D:11B:UN'\r\nBGM+313+803636474747+11'\r\nDTM+9:201307311207:203'\r\nRFF+AGO:CLS-803636474747'\r\nRCS+11'";
			AssertEquals("MessageType", EDIInterchange.ApplicationCodes.CACustoms, BatchProcessorUtilities.GetMessageType(messageText));

			var messages = new[] { "UNB+UNOA:3+CLIENTID+CBSAPID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHGT+130627:1206+2+UN+D:11B'", "UNB+UNOA:3+CLIENTID+CBSAPID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHGP+130627:1206+2+UN+D:11B'" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.ACIHouseBill, messages);

			messages = new[] { "UNB+UNOA:3+CLIENTID+CBSAPID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHCMGT+130627:1206+2+UN+D:11B'", "UNB+UNOA:3+CLIENTID+CBSAPID+130627:1206+1'UNG+GOVCBR+TSITE+ACIHCMGP+130627:1206+2+UN+D:11B'" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.ACIForwarderClose, messages);

			messages = new[] { "UNG+CUSDEC+U41091N1+RT+100526:0605+1+UN+D:96A+12345PASSWORD", "UNG+CUSRES+CCR+RECIPIENT+950115:1130+GREF12345+UN+D:96A'" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.EDIRelease, messages);

			messages = new[] { "UNG+CUSRES+CCR+RT+100526:0605+1+UN+S:99B+12345PASSWORD'", "UNG+CUSRES+CCR+RECIPIENT+950115:1130+GREF12345+UN+S:99B'" };
			var bodies = new[] { "BGM+:::1000+BATCHNUMBER", "BGM+:::2000+BATCHNUMBER" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.TradeChainPartner, messages, bodies);

			messages = new[] { "UNG+CUSRES+CCR+RT+100526:0605+1+UN+S:99B+12345PASSWORD'BGM+:::1010+BATCHNUMBER", "UNG+CUSRES+CCR+RECIPIENT+950115:1130+GREF12345+UN+S:99B'BGM+:::2010+BATCHNUMBER" };
			bodies = new[] { "BGM+:::1010+BATCHNUMBER", "BGM+:::2010+BATCHNUMBER" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.CSARevenueSummaryForm, messages, bodies);

			messages = new[] { "'UNG+CUSDEC+U41091N1+KI+100527:0349+4+UN+S:99B+12345PASSWORD'UNH+12+CUSDEC:S:99B:UN'", "UNG+CUSRES+CCS+TRANSMISSION SITE+100212:1447+1622+UN+S:99B" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.B3CUSDEC, messages);

			messageText = "UNG+CUSRES+CCSSYNTAX++100707:1027+10+UN+S:99B'";
			AssertEquals("MessageType", MessageTypeList.Codes.SyntaxError, BatchProcessorUtilities.GetMessageType(messageText));

			messages = new[]
						{
							"'UNG+CUSDEC+U10207V1+QA+100608:1857+9+UN+S:99B+10207YUSEN'UNH+18+CUSDEC:S:99B:UN'",
							"UNG+CUSRES+QRCLASS/TAR+U10207V1+100721:0718+40+UN+S:99B'",
							"UNG+CUSRES+BROADCAST+U10207V1+100721:0718+40+UN+S:99B'",
							"UNG+CUSRES+CLASSFILE+U10207V1+100721:0718+40+UN+S:99B'",
							"UNG+CUSRES+GSTFILE+U10207V1+100721:0718+40+UN+S:99B'",
							"UNG+CUSRES+EXCISETAX+U10207V1+100721:0718+40+UN+S:99B'",
							"UNG+CUSRES+TARIFFCODE+U10207V1+100721:0718+40+UN+S:99B'",
							"'UNG+CUSDEC+U10207V1+QE+100608:1857+9+UN+S:99B+10207YUSEN'UNH+18+CUSDEC:S:99B:UN'",
							"UNG+CUSRES+EXCHANGERATE+U10207V1+100721:0718+40+UN+S:99B'",
							"UNG+CUSRES+QREXCHANGE+U10207V1+100721:0718+40+UN+S:99B'"
						};
			AssertMessageTypeForMessages(MessageTypeList.Codes.Query, messages);
			AssertEquals("QRCLASSTAR QueryMessageSubType", QueryMessageSubTypes.Codes.QRCLASSTAR, BatchProcessorUtilities.GetQueryMessageSubType(messages[1]));
			AssertEquals("GSTFILE QueryMessageSubType", QueryMessageSubTypes.Codes.GSTFILE, BatchProcessorUtilities.GetQueryMessageSubType(messages[4]));

			messages = new[] { "'UNG+CUSREP+U10207V1+PARSTST+100907:0222+44+UN+D:96A'" };
			AssertMessageTypeForMessages(MessageTypeList.Codes.RNSRequest, messages);

			messages = new[]
						{
							"UNG+CUSDEC+NOTICE+U10207V1+101110:0602+155+UN+S:99B'",
							"UNG+CUSDEC+K84++110330:0607+567+UN+S:99B'",
							"UNG+CUSRES+OVERDUE REPORT+U10207V1+110407:0608+594+UN+S:99B'",
							"UNG+CUSDEC+OVERDUE REPORT+U10207V1+110407:0608+594+UN+S:99B'",
									 };
			AssertMessageTypeForMessages(MessageTypeList.Codes.K84Report, messages);
			AssertEquals("Daily K84 Message Sub Type", K84ReportTypes.Codes.Daily, BatchProcessorUtilities.GetK84MessageSubType(messages[0]));
			AssertEquals("Monthly K84 Message Sub Type", K84ReportTypes.Codes.Monthly, BatchProcessorUtilities.GetK84MessageSubType(messages[1]));
			AssertEquals("Overdue K84 Message Sub Type", K84ReportTypes.Codes.Overdue, BatchProcessorUtilities.GetK84MessageSubType(messages[2]));
			AssertEquals("Overdue K84 Message Sub Type", K84ReportTypes.Codes.Overdue, BatchProcessorUtilities.GetK84MessageSubType(messages[3]));

			messageText = @"xxxx";
			AssertEquals("Other Message Type", EDIInterchange.ApplicationCodes.CACustoms, BatchProcessorUtilities.GetMessageType(messageText));
		}

		static void AssertMessageTypeForMessages(string messageType, IList<string> messages)
		{
			for (var i = 0; i < messages.Count; i++)
			{
				AssertEquals(string.Format("Message type for {0} message", i), messageType, BatchProcessorUtilities.GetMessageType(messages[i]));
			}
		}

		static void AssertMessageTypeForMessages(string messageType, IList<string> messages, IList<string> bodies)
		{
			for (var i = 0; i < messages.Count; i++)
			{
				AssertEquals(string.Format("Message type for {0} message", i), messageType, BatchProcessorUtilities.GetMessageType(messages[i], bodies[i]));
			}
		}

		#endregion

		#region TestGetReturnEMailAddresses

		public void TestGetReturnEMailAddresses()
		{
			var testOrg = Factory.New<OrgHeader>();
			testOrg.OH_Code = "TESTORG";
			testOrg.MainAddress.OA_Email = "blah1@blah.com";
			var returnEMailAddresses = BatchProcessorUtilities.GetReturnEMailAddresses("RCCECECPW", testOrg);
			AssertEquals("Only 1 address", 1, returnEMailAddresses.Length);
			AssertEquals("Is the default address", "blah1@blah.com", returnEMailAddresses[0]);

			using (CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPT"))
			{
				returnEMailAddresses = BatchProcessorUtilities.GetReturnEMailAddresses("RCCECECPW", testOrg);
				AssertEquals("Only 1 address", 1, returnEMailAddresses.Length);
				AssertEquals("Is the default address", "blah1@blah.com", returnEMailAddresses[0]);
			}

			var newContact = testOrg.Contacts.AddNew();
			newContact.OC_ContactName = "#CAP";
			newContact.OC_Email = "blah2@blah.com";
			newContact = testOrg.Contacts.AddNew();
			newContact.OC_ContactName = "#CAT1";
			newContact.OC_Email = "blah3@blah.com";
			newContact = testOrg.Contacts.AddNew();
			newContact.OC_ContactName = "#CAT2";
			newContact.OC_Email = "blah4@blah.com";

			using (CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "RCCECECPW"))
			{
				returnEMailAddresses = BatchProcessorUtilities.GetReturnEMailAddresses("RCCECECPW", testOrg);
				AssertEquals("Only 1 address", 1, returnEMailAddresses.Length);
				AssertEquals("1st test address", "blah2@blah.com", returnEMailAddresses[0]);
			}

			using (CACustomsDataRegistry.Instance.CBSAProdNetworkIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INETCECPT"))
			{
				returnEMailAddresses = BatchProcessorUtilities.GetReturnEMailAddresses("RCCECECPW", testOrg);
				AssertEquals("2 addresses", 2, returnEMailAddresses.Length);
				AssertEquals("1st test address", "blah3@blah.com", returnEMailAddresses[0]);
				AssertEquals("2nd test address", "blah4@blah.com", returnEMailAddresses[1]);
			}
		}

		#endregion

		#region TestGetAccountSecurityCodeFromEDIMessage

		public void TestGetAccountSecurityCodeFromEDIMessage()
		{
			CACustomsDataRegistry.Instance.AccountSecurityNo.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "98765");
			CACustomsDataRegistry.Instance.AccountSecurityNoPassword.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "ppsid");

			var importer = Factory.New<OrgHeader>();
			var addInfo = OrgImpAddInfo.Get(importer);
			addInfo.ZO_AccountSecurityNumber = "12345";
			addInfo.ZO_AccountSecirityPassword = "HGFEDCBA";

			var message1 = Factory.New<EDIMessage>();
			message1.EM_IsTestMessage = false;
			message1.EM_MessageType = MessageTypeList.Codes.TradeChainPartner;
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.CAIMP;
			message1.EM_LinkedObject = importer;
			message1.EM_MessageText = "Message 1 text";

			var accountSecurityCode = BatchProcessorUtilities.GetAccountSecurityCodeFromEDIMessage(message1);
			var accountSecurityPassword = BatchProcessorUtilities.GetAccountSecurityPasswordFromEDIMessage(message1);
			AssertEquals("should comes from OrgImpAddInfo", "12345", accountSecurityCode);
			AssertEquals("should comes from OrgImpAddInfo", "HGFEDCBA", accountSecurityPassword);

			addInfo.ZO_AccountSecurityNumber = ZString.Empty;
			addInfo.ZO_AccountSecirityPassword = ZString.Empty;

			accountSecurityCode = BatchProcessorUtilities.GetAccountSecurityCodeFromEDIMessage(message1);
			accountSecurityPassword = BatchProcessorUtilities.GetAccountSecurityPasswordFromEDIMessage(message1);
			AssertEquals("should comes from Registry", "98765", accountSecurityCode);
			AssertEquals("should comes from Registry", "ppsid", accountSecurityPassword);
		}

		#endregion
	}
}
