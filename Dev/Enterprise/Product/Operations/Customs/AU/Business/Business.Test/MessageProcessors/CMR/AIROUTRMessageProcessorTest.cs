using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIROUTRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestProcessClearMessage()
		{
			incomingMessage.EM_MessageText = ClearAIROUTMessage;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
		}

		public void TestProcessPartiallyClearedMessage()
		{
			incomingMessage.EM_MessageText = PartialClearAIROUTMessage;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
		}

		public void TestErrorAdviceMessageDoesNotSetInconsistentStateError()
		{
			incomingMessage.EM_MessageText = ErrorAdviceAIROUTMessage;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			AssertEquals("This error advice message should not set rejected log", false, underBond.CusUnderbondOutturnLogManager.HasNonExistantLineAtCustoms);
		}

		public void TestAmendDeleteNonExistantLinesSetsInconsistentStateError()
		{
			incomingMessage.EM_MessageText = RejectedNonExistantLineAIROUTMessage;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Rejected, incomingMessage.EM_MessageSubType);
			AssertEquals("This reject message should set NonExistantLine log", true, underBond.CusUnderbondOutturnLogManager.HasNonExistantLineAtCustoms);
		}

		public void TestRejectedLinesInAcceptedMessageSetMessageStatus()
		{
			var underBond = Factory.New<CusUnderbond>();
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "09100239238";
			outturnLine1.C5_HouseBill = "HB1";
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "09100239238";
			outturnLine2.C5_HouseBill = "HB2";
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			Factory.Save();

			incomingMessage.EM_MessageText = OriginalAcceptedWithErrors;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			Factory.Save();
			AssertEquals("Line 1 of message accepted, should not set MessageStatus.", ZString.Empty, underBond.Outturns[0].C5_MessageStatus);
			AssertEquals("The error message relates to line 2 & should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[1].C5_MessageStatus);
		}

		public void TestRejectedLinesInAcceptedMsgWhereHBHasImbeddedSpaces()
		{
			var underBond = Factory.New<CusUnderbond>();
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "08100293823";
			outturnLine1.C5_HouseBill = "S00293992";
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "08100293823";
			outturnLine2.C5_HouseBill = "S90223020/1";
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;

			var outturnLine3 = underBond.Outturns.AddNew();
			outturnLine3.C5_MasterBill = "08100293823";
			outturnLine3.C5_HouseBill = "HB 2-203299";
			outturnLine3.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine4 = underBond.Outturns.AddNew();
			outturnLine4.C5_MasterBill = "08100293823";
			outturnLine4.C5_HouseBill = "B02092092";
			outturnLine4.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;

			var outturnLine5 = underBond.Outturns.AddNew();
			outturnLine5.C5_MasterBill = "08100293823";
			outturnLine5.C5_HouseBill = "HB 2-203299/A";
			outturnLine5.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusPackages;

			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);

			incomingMessage.EM_MessageText = OriginalAcceptedWithErrors2;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			AssertEquals("Line 1 of message accepted, should not set MessageStatus.", ZString.Empty, underBond.Outturns[0].C5_MessageStatus);
			AssertEquals("Line 2 should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[1].C5_MessageStatus);
			AssertEquals("Line 3 of message accepted, should not set MessageStatus.", ZString.Empty, underBond.Outturns[2].C5_MessageStatus);
			AssertEquals("Line 4 should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[3].C5_MessageStatus);
			AssertEquals("Line 5 should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[4].C5_MessageStatus);
		}

		public void TestFindsRejectedLinesInAcceptedMsgWithoutMBInDBSetsMsgStatus()
		{
			var underBond = Factory.New<CusUnderbond>();
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "";
			outturnLine1.C5_HouseBill = "HB1";
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "";
			outturnLine2.C5_HouseBill = "HB2";
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			Factory.Save();

			incomingMessage.EM_MessageText = OriginalAcceptedWithErrors;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			Factory.Save();
			AssertEquals("Line 1 of message accepted, should not set MessageStatus.", ZString.Empty, underBond.Outturns[0].C5_MessageStatus);
			AssertEquals("The error message relates to line 2 & should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[1].C5_MessageStatus);
		}

		public void TestFindsRejectedLinesInAcceptedMsgFromHBParentIDSetsMsgStatus()
		{
			var cusHawb1 = Factory.NewWithValidTestData<CusHAWB>();
			cusHawb1.CS_HAWB = "HB1";

			var cusHawb2 = Factory.NewWithValidTestData<CusHAWB>();
			cusHawb2.CS_HAWB = "HB2";

			var underBond = Factory.New<CusUnderbond>();
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "";
			outturnLine1.C5_HouseBill = "";
			outturnLine1.C5_ParentID = cusHawb1.PK;
			outturnLine1.C5_ParentTableCode = cusHawb1.TablePrefix;
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "";
			outturnLine2.C5_HouseBill = "";
			outturnLine2.C5_ParentID = cusHawb2.PK;
			outturnLine2.C5_ParentTableCode = cusHawb2.TablePrefix;
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			Factory.Save();

			incomingMessage.EM_MessageText = OriginalAcceptedWithErrors;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			Factory.Save();
			AssertEquals("Line 1 of message accepted, should not set MessageStatus.", ZString.Empty, underBond.Outturns[0].C5_MessageStatus);
			AssertEquals("The error message relates to line 2 & should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[1].C5_MessageStatus);
		}

		public void TestFindsRejectedLinesInAcceptedMsgFromMBParentIDSetsMsgStatus()
		{
			var cusMawb1 = Factory.NewWithValidTestData<CusMAWB>();
			cusMawb1.CM_MAWB = "08100293992";

			var cusMawb2 = Factory.NewWithValidTestData<CusMAWB>();
			cusMawb2.CM_MAWB = "09100239238";

			var underBond = Factory.New<CusUnderbond>();
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "";
			outturnLine1.C5_HouseBill = "";
			outturnLine1.C5_ParentID = cusMawb1.PK;
			outturnLine1.C5_ParentTableCode = cusMawb1.TablePrefix;
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "";
			outturnLine2.C5_HouseBill = "";
			outturnLine2.C5_ParentID = cusMawb2.PK;
			outturnLine2.C5_ParentTableCode = cusMawb2.TablePrefix;
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			Factory.Save();

			incomingMessage.EM_MessageText = OriginalAcceptedWithErrors;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			Factory.Save();
			AssertEquals("Line 1 of message accepted, should not set MessageStatus.", ZString.Empty, underBond.Outturns[0].C5_MessageStatus);
			AssertEquals("The error message relates to line 2 & should set MessageStatus on that rejected line.", CMRUnderbondStatuses.Codes.OutturnAcceptedThisOutturnLineRejected, underBond.Outturns[1].C5_MessageStatus);
		}

		public void TestWithdrawalAcceptedResetsOutturnedDate()
		{
			var underBond = Factory.New<CusUnderbond>();
			underBond.C4_Outurned = ZDateTime.Now;
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "09100239238";
			outturnLine1.C5_HouseBill = "HB1";
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "09100239238";
			outturnLine2.C5_HouseBill = "HB2";
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			Factory.Save();

			underBond.OutturnStatus.Code = CMRBaseStatuses.Codes.WithdrawalAccepted;
			incomingMessage.EM_MessageText = WithdrawlAccepted;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
			Factory.Save();
			AssertEquals("Outturned date should be reset", ZDateTime.Empty, underBond.C4_Outurned);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.AIROUT;

		protected override ZString GetExpectedMessageName() => "Air Waybill Outturn Report Response - (AIROUTR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRAIROUTRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			underBond = Factory.New<CusUnderbond>();
			underBond.Outturns.AddNew();
			outgoingMessage = (CMRAIROUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			processor = new AIROUTRMessageProcessorTestHelper(logger);
		}
		AIROUTRMessageProcessor processor;
		CusUnderbond underBond;

		const string ClearAIROUTMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+31B3 C77H 40FC:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:9'RFF+ABO:U00001802/CMT1::001'DTM+310:20120511051326:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";
		const string PartialClearAIROUTMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+6G84 326G 8FC:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:4'RFF+ABO:U00000348/DAT8::034'DTM+310:20120530094906:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ERROR:80:95'ERC+CG1518:6:95'FTX+AAO+++MWB=08155550084,HWB=HS0813 GOODS DESCRIPTION IS MANDATORY FOR OUTTURN RESULT TYPE OF SC OR SU MWB=08155550084,HWB=HS0813'CNT+55:001'UNT+17+000001'";
		const string ErrorAdviceAIROUTMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+3GH2 FG5J 7DFC:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:4'RFF+ABO:U00000348/DAT10::037'DTM+310:20120530101338:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ADVICE:80:95'ERC+CG2012:6:95'FTX+AAO+++AIR OUTTURN ADDED AND FOUND MATCHING AIR CARGO REPORT'CNT+55:000'UNT+17+000001'";
		const string RejectedNonExistantLineAIROUTMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+47C5 54HI CG5C:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:4'RFF+ABO:U00001790/CMT3::010'DTM+310:20120531083310:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5201:6:95'FTX+AAO+++THIS TRANSACTION WAS REJECTED'ERP+1'ERC+ERROR:80:95'ERC+CG1535:6:95'FTX+AAO+++REPORT REJECTED, ATTEMPTED TO AMEND/DELETE A LINE THAT DOESN?'T EXIST MWB=08123456789,HWB=HB6'ERP+1'ERC+ERROR:80:95'ERC+CG1530:6:95'FTX+AAO+++CHANGE NOT PERMITTED WOULD RESULT IN ALL LINES BEING DELETED AT LEAST 1 LINE MUST BE LEFT ON THE AWO'CNT+55:002'UNT+21+000001'";
		const string OriginalAcceptedWithErrors = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+248A E4B6 BGFC:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:9'RFF+ABO:U00001830/CMT1::001'DTM+310:20120622052201:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ERROR:80:95'ERC+CG1518:6:95'FTX+AAO+++MWB=09100239238,HWB=HB2 GOODS DESCRIPTION IS MANDATORY FOR OUTTURN RESULT TYPE OF SC OR SU MWB=09100239238,HWB=HB2'CNT+55:001'UNT+17+000001'";
		const string OriginalAcceptedWithErrors2 = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+1A9F G599 E5C:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:9'RFF+ABO:U00001831/CMT1::001'DTM+310:20120625015134:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ERROR:80:95'ERC+CG1518:6:95'FTX+AAO+++MWB=08100293823,HWB=B02092092 GOODS DESCRIPTION IS MANDATORY FOR OUTTURN RESULT TYPE OF SC OR SU MWB=08100293823,HWB=B02092092'ERP+1'ERC+ERROR:80:95'ERC+CG1518:6:95'FTX+AAO+++MWB=08100293823,HWB=HB 2-203299/A GOODS DESCRIPTION IS MANDATORY FOR OUTTURN RESULT TYPE OF SC OR SU MWB=08100293823,HWB=HB 2-203299/A'ERP+1'ERC+ERROR:80:95'ERC+CG1518:6:95'FTX+AAO+++MWB=08100293823,HWB=S90223020/1 GOODS DESCRIPTION IS MANDATORY FOR OUTTURN RESULT TYPE OF SC OR SU MWB=08100293823,HWB=S90223020/1'CNT+55:003'UNT+25+000001'";
		const string WithdrawlAccepted = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIROUTR+1152 FG43 4515:001+11'NAD+MR+AAA374M::95'RFF+ACW:AIROUT'RFF+AFM:50'RFF+ABO:U00003352/CMT2::005'DTM+310:20150723013706:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5202:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'ERP+1'ERC+ADVICE:80:95'ERC+CG2252:6:95'FTX+AAO+++AWO HAS BEEN SUCCESSFULLY WITHDRAWN'CNT+55:000'UNT+17+000001'";

		sealed class AIROUTRMessageProcessorTestHelper : AIROUTRMessageProcessor
		{
			public AIROUTRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
