using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AIRCRRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestErrorMessage()
		{
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("AIRCRRErrorResponse.txt")).Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Rejected, incomingMessage.EM_MessageSubType);
		}

		public void TestProcessClearMessage()
		{
			incomingMessage.EM_MessageText = clearAIRCRRMessage;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
		}

		public void TestUpdateCS_IsResponsePending()
		{
			incomingMessage.EM_MessageText = clearAIRCRRMessage;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("CS_IsResponsePending", false, hAWB.CS_IsResponsePending);
		}

		public void TestUpdateCS_IsPreAlerted()
		{
			incomingMessage.EM_MessageText = clearAIRCRRMessage;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("CS_IsPrealerted", true, hAWB.CS_IsPrealerted);
		}

		[ExpectNoExceptions]
		public void TestAIRCRRandCARSTProcessedInSameFactoryDoesNotCauseException()
		{
			CTOCusMAWB cTOMAWB = Factory.New<CTOCusMAWB>();
			CTOCusHAWB cTOHAWB = cTOMAWB.ChildBills.AddNew();
			cTOHAWB.CS_MessageReference = "M00000005";
			cTOHAWB.CS_IsResponsePending = true;
			cTOHAWB.CS_IsPrealerted = false;
			cTOHAWB.CS_HAWB = "08134635134";
			outgoingMessage = (CMRAIRCRRMessage)cTOHAWB.Messages.AddNew(IncomingMessageType);
			incomingMessage.EM_MessageText = @"UNH+000004+CUSRES:D:99B:UN'
BGM+961:::AIRCRR+15BA DF11 7JF8:001+11'
NAD+MR+AAA374M::95'
RFF+ACW:AIRCR'
RFF+AFM:9'
RFF+ABO:M00000005/DAT1::001'
DTM+310:20080402114057:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5202:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'
ERP+1'
ERC+ADVICE:80:95'
ERC+CG2021:6:95'
FTX+AAO+++AIR IMPENDING ARRIVAL NOT YET REPORTED'
CNT+55:000'
UNT+17+000004'".Replace("\r\n", "");
			processor.ProcessMessage(incomingMessage);

			CMRCARSTMessage cARSTMessage = Factory.New<CMRCARSTMessage>();
			cARSTMessage.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+3CBD BHF7 751F:1+8'
DTM+9:20051104171710062728:ZZZ'
DTM+132:20051104:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+016++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+5961P::95'
NAD+MR+FGC344L::95'
NAD+UD+75066013018::95'
RFF+ABO:M00000005/DAT1::2'
RFF+MWB:08134635134'
DOC+1'
PAC+0000029'
UNT+16+000001'".Replace("\r\n", "");
			cARSTMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			cARSTMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			CARSTMessageProcessor cARSTProcessor = new CARSTMessageProcessor(logger);
			cARSTProcessor.ProcessMessage(cARSTMessage);
		}

		public void TestHVLVRegistrySettings()
		{
			incomingMessage.EM_LinkedObject = hAWB;
			processor.ProcessMessage(incomingMessage);

			hAWB.CS_IsHVLV = false;
			Env.Registry.AUCustoms.AirCargoSendErrors = Core.Constants.EmailTo.NoEmails;
			AssertEquals("It should return normal settings if the BizO is not HVLV.", Core.Constants.EmailTo.NoEmails, processor.ErrorEmailMode);
			Env.Registry.AUCustoms.HVLVAirCargoSendErrors = Core.Constants.EmailTo.StaffMember;
			AssertEquals("It should return normal settings if the BizO is not HVLV even if the HVLV registry item is assgined.", Core.Constants.EmailTo.NoEmails, processor.ErrorEmailMode);
			hAWB.CS_IsHVLV = true;
			AssertEquals("It should return HVLV settings if the BizO is HVLV.", Core.Constants.EmailTo.StaffMember, processor.ErrorEmailMode);

			hAWB.CS_IsHVLV = false;
			Env.Registry.AUCustoms.AirCargoSendAcknowledgements = Core.Constants.EmailTo.NoEmails;
			AssertEquals("It should return normal settings if the BizO is not HVLV.", Core.Constants.EmailTo.NoEmails, processor.AcknowledgementEmailMode);
			Env.Registry.AUCustoms.HVLVAirCargoSendAcknowledgements = Core.Constants.EmailTo.StaffMember;
			AssertEquals("It should return normal settings if the BizO is not HVLV even if the HVLV registry item is assgined.", Core.Constants.EmailTo.NoEmails, processor.AcknowledgementEmailMode);
			hAWB.CS_IsHVLV = true;
			AssertEquals("It should return HVLV settings if the BizO is HVLV.", Core.Constants.EmailTo.StaffMember, processor.AcknowledgementEmailMode);

			hAWB.CS_IsHVLV = false;
			Env.Registry.AUCustoms.AirCargoSendImpediments = Core.Constants.EmailTo.NoEmails;
			AssertEquals("It should return normal settings if the BizO is not HVLV.", Core.Constants.EmailTo.NoEmails, processor.ImpedimentEmailMode);
			Env.Registry.AUCustoms.HVLVAirCargoSendImpediments = Core.Constants.EmailTo.StaffMember;
			AssertEquals("It should return normal settings if the BizO is not HVLV even if the HVLV registry item is assgined.", Core.Constants.EmailTo.NoEmails, processor.ImpedimentEmailMode);
			hAWB.CS_IsHVLV = true;
			AssertEquals("It should return HVLV settings if the BizO is HVLV.", Core.Constants.EmailTo.StaffMember, processor.ImpedimentEmailMode);
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.AIRCR;

		protected override ZString GetExpectedMessageName() => "Air Cargo Report Response(AIRCRR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		readonly string clearAIRCRRMessage = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::AIRCRR+490F DAGH CDGE:001+11'NAD+MR+AAA374M:110:95'RFF+ACW:AIRCR'RFF+AFM:9'RFF+ABO:S00002158/1::001'DTM+310:20041216010409:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

		protected override Type IncomingMessageType => typeof(CMRAIRCRRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			CusMAWB mAWB = Factory.New<CusMAWB>();
			hAWB = mAWB.ChildBills.AddNew();
			hAWB.CS_MessageReference = "S00002158";
			hAWB.CS_IsResponsePending = true;
			hAWB.CS_IsPrealerted = false;
			outgoingMessage = (CMRAIRCRRMessage)hAWB.Messages.AddNew(IncomingMessageType);
			processor = new AIRCRRMessageProcessorTestHelper(logger);
		}

		AIRCRRMessageProcessorTestHelper processor;
		CusHAWB hAWB;

		sealed class AIRCRRMessageProcessorTestHelper : AIRCRRMessageProcessor
		{
			public AIRCRRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;

			public new ZString AcknowledgementEmailMode => base.AcknowledgementEmailMode;
			public new ZString ImpedimentEmailMode => base.ImpedimentEmailMode;
			public new ZString ErrorEmailMode => base.ErrorEmailMode;
		}
	}
}
