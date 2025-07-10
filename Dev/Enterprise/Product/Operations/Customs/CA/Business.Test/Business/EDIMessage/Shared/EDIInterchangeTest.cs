using System;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CAEDIInterchange))]
	sealed class EDIInterchangeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCreateMessageFromInterchange()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Company1";
			company.GC_Code = "HC1";
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company.PK;
			branch1.GB_BranchName = "Branch1";
			branch1.GB_Code = "HB1";
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company.PK;
			branch2.GB_BranchName = "Branch2";
			branch2.GB_Code = "HB2";

			var text = CARMDailyNoticeMessageTestHelper.GetCARMDailyNoticeImporterMessageText();

			var interchange = Factory.NewWithValidTestData<CAEDIInterchange>();
			interchange.EI_GB = branch1.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var message = interchange.CreateMessageFromInterchange(typeof(CARMDailyNoticeMessage), CARMDailyNoticeMessageSubTypeList.Codes.CARMDNoticeImporter);
				AssertEquals(branch1.PK, message.EM_GB);
			}
		}

		public void TestGetMessageTypeToCreate()
		{
			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var interchangeString = "UNB+UNOA:3+INETCECPT+YUSAIRXPN+100721:0718+40++++++1'UNG+CUSRES+QRCLASS/TAR+U10207V1+100721:0718+40+UN+S:99B'RA1022042910'RA11DC942311 CLASS NUMBER NOT NUMERIC'UNE+1+40'UNZ+1+40'";
			var interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("CADEX wrapped text", "RA1022042910'RA11DC942311 CLASS NUMBER NOT NUMERIC'", interchange.ContainedMessages[0].EM_MessageText);
			AssertEquals("Message Type", typeof(QueryMessage), interchange.ContainedMessages[0].GetType());
			Assert(interchange.ShouldSendViaEHub);

			eHubMessagingRegistry.Instance.SendCAViaEHub.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+100720:0717+1++++++1'UNG+CUSRES+QREXCHANGE+U10207V1+100720:0717+39+UN+S:99B'RE10     20100720'RE20COCOLOMBIA                      COPCOLOMBIAN PESO                2010072000000564'UNE+1+39'UNZ+1+39'";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("CADEX wrapped text", "RE10     20100720'RE20COCOLOMBIA                      COPCOLOMBIAN PESO                2010072000000564'", interchange.ContainedMessages[0].EM_MessageText);
			AssertEquals("Message Type", typeof(QueryMessage), interchange.ContainedMessages[0].GetType());
			Assert(!interchange.ShouldSendViaEHub);

			interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+100714:0608+2++++++1'UNG+CUSRES+CCS+U10207V1+100714:0608+19+UN+S:99B'UNH+1+CUSRES:S:99B:UN'BGM++030+9'DTM+137:20100713:102'ERP+:I11'RFF+ABO:216'ERC+943152'ERP+:I99'RFF+ABO:216'ERC+942991'DOC+961'CST++1+0+1'UNT+12+1'UNE+1+19'UNZ+1+19'";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(B3Message), interchange.ContainedMessages[0].GetType());

			interchangeString = @"UNB+UNOA:3+RCCECECPP+YYZPANA+170427:1046+72684'UNG+CUSRES+CCR+U00350V1+170427:1046+23007+UN+S:99B'UNH+1+CUSRES:S:99B:UN'BGM+:::2000+00000000395570'DTM+9:201704271045:203'GIS+1'RFF+ZZZ:102385036'UNT+6+1'UNE+1+23007'UNZ+1+72684";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(TCPMessage), interchange.ContainedMessages[0].GetType());

			interchangeString = @"UNB+UNOA:3+RCCECECPP+YYZPANA+170427:1046+72684'UNG+CUSRES+CCR+U00350V1+170427:1046+23007+UN+S:99B'UNH+1+CUSRES:S:99B:UN'BGM+:::2010+00000000395570'DTM+9:201704271045:203'GIS+1'RFF+ADZ:102385036'UNT+6+1'UNE+1+23007'UNZ+1+72684";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(RSFMessage), interchange.ContainedMessages[0].GetType());

			interchangeString = @"UNB+UNOA:2+INETCECPT+YUSAIRXPN+100707:1027+1++++++1'UNG+CUSRES+CCSSYNTAX++100707:1027+10+UN+S:99B'UNH+1+CUSRES:S:99B:UN+10207'BGM+:::+020+11'DTM+137:201007071025:203'GIS+14'ERP+2:35:29'FTX+AAO+++SEGMENTMOALINE8ELE POS1,2:ELEM TOO LONG'UNT+11+1'UNE+1+10'UNZ+1+10'";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(SyntaxErrorMessage), interchange.ContainedMessages[0].GetType());

			interchangeString = @"UNB+UNOA:1+SENDER+CLIENTID+950115:1130+<NEXTINTERCHANGENUMBER>++CUSRES'UNG+CUSRES+CCR+RECIPIENT DEFINED ID+950115:1130+GREF12345+UN+D:96A'UNH+1+CUSRES:D:96A:UN'BGM+:::257+12345000000216+11'LOC+22+0351:129::3021'DTM+9:200912221030:203'DTM+58:201001021030:203'GIS+1'UNT+26+257'UNE+1+GREF12345'UNZ+1+REF12345'";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(EDIReleaseMessage), interchange.ContainedMessages[0].GetType());

			interchangeString = @"UNB+UNOA:3+INETCECPT+YUSAIRXPN+101231:0605+258++++++1'UNG+CUSDEC+NOTICE+U10207V1+101231:0605+258+UN+S:99B'UNH+1+CUSDEC:S:99B:UN'BGM+++9'DTM+137:20101230:102'RFF+ABP:10207'UNS+D'DMS+K10'DTM+130:20101231:102'DTM+353:20101230:102'NAD+VC+0497'LIN+++000000373'MOA+1:771178'MOA+161:771178'MOA+128:771178'LIN+++000000384'MOA+155:488'MOA+1:13024'MOA+161:13512'MOA+128:13512'LIN+++000000395'MOA+1:86610'MOA+161:86610'MOA+128:86610'DMS+K40'DTM+130:20110131:102'MOA+155:488'MOA+1:870812'MOA+161:871300'MOA+128:871300'UNS+S'UNT+30+1'UNE+1+258'UNZ+1+258'";
			interchange = GetNewInterchange(interchangeString);
			AssertEquals("EDIMessageNumber", 1, interchange.ContainedMessages.Count);
			AssertEquals("Message Type", typeof(K84Message), interchange.ContainedMessages[0].GetType());
		}

		public void TestReplaceBatchNumberPlaceHolder()
		{
			using (CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "CLIENTIDZZ"))
			using (CACustomsDataRegistry.Instance.ShouldBatchNumberBeByInterchange.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var testInterchange = Factory.New<CAEDIInterchange>();
				testInterchange.EI_From = "FROM";
				testInterchange.EI_To = "TO";
				var testMessage = Factory.New<EDIMessage>();
				testMessage.EM_MessageText = "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
				testMessage.EM_IsTestMessage = true;
				testInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
				testInterchange.EI_BodyText = "Hello, " + EDIMessage.UniqueBatchNumberPlaceHolder + " Bye";
				testInterchange.ContainedMessages.Add(testMessage);
				Factory.Save();
				AssertEquals("Hello, 001 Bye", testMessage.EM_MessageText);
				AssertEquals("Hello, 001 Bye", testInterchange.EI_BodyText);
			}
		}

		EDIInterchange GetNewInterchange(string interchangeString)
		{
			var interchange = EDIInterchange.CreateNewInterchangeFromString(Factory, interchangeString, EDIInterchange.ApplicationCodes.CAIMP, false, true);
			interchange.SpawnMessagesFromInterchageTextAndMarkAsReceived();
			return interchange;
		}
	}
}
