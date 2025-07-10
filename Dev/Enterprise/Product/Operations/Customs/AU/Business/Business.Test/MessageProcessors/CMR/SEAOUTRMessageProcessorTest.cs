using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEAOUTRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestWithdrawal()
		{
			var underBond = Factory.New<CusUnderbond>();
			underBond.C4_Outurned = ZDateTime.Now;
			var outturnLine1 = underBond.Outturns.AddNew();
			outturnLine1.C5_MasterBill = "OB032482";
			outturnLine1.C5_HouseBill = "HB1";
			outturnLine1.C5_OutturnResultType = CMROutturnResultType.Codes.NilDiscrepancy;

			var outturnLine2 = underBond.Outturns.AddNew();
			outturnLine2.C5_MasterBill = "OB032482";
			outturnLine2.C5_HouseBill = "HB2";
			outturnLine2.C5_OutturnResultType = CMROutturnResultType.Codes.SurplusConsignment;
			outgoingMessage = (CMRSEAOUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			Factory.Save();

			underBond.OutturnStatus.Code = CMRBaseStatuses.Codes.WithdrawalAccepted;
			incomingMessage.EM_MessageText = WithdrawlAccepted;
			incomingMessage.EM_LinkedObject = underBond;
			processor.ProcessMessage(incomingMessage);
			AssertEquals("MessageSubType", CMRMessage.ManifestResponseSubTypes.Clear, incomingMessage.EM_MessageSubType);
		}
		public void TestNoResponseWhenAccepted()
		{
			var bill = Factory.New<CusSCAOceanBill>();
			bill.CB_MessageReference = "S00002158";

			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			var message = bill.Messages.AddNew(typeof(CMRSEAOUTRMessage));
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEAOUTR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEAOUT'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			var processor = new SEAOUTRMessageProcessorTestHelper(new LoggingInformation());
			processor.ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Accecpted.", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals(1, processor.AcknowledgementEmailSendCount);
				AssertEquals(0, processor.ErrorEmailSendCount);
			});
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.SEAOUT;

		protected override ZString GetExpectedMessageName() => "Sea Outturn Report Response - (SEAOUTR)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override Type IncomingMessageType => typeof(CMRSEAOUTRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			underBond = Factory.New<CusUnderbond>();
			var outturn = underBond.Outturns.AddNew();
			outgoingMessage = (CMRSEAOUTRMessage)underBond.Messages.AddNew(IncomingMessageType);
			processor = new SEAOUTRMessageProcessorTestHelper(logger);
		}
		SEAOUTRMessageProcessor processor;
		CusUnderbond underBond;

		const string WithdrawlAccepted = @"UNH+000001+CUSRES:D:99B:UN'BGM+961:::SEAOUTR+3DD5 JE4G AD07:001+11'NAD+MR+AAA374M::95'RFF+ACW:SEAOUT'RFF+AFM:4'RFF+ABO:O00000588/CMT1::003'DTM+310:20170125034125:204'ERP+1'ERC+ADVICE:80:95'ERC+MS5203:6:95'FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'CNT+55:000'UNT+13+000001'";

		sealed class SEAOUTRMessageProcessorTestHelper : SEAOUTRMessageProcessor
		{
			public SEAOUTRMessageProcessorTestHelper(LoggingInformation logger) : base(logger) { }
			protected override void SendReport(EmailDef email)
			{
				SentReport = email;
			}
			public EmailDef SentReport;
		}
	}
}
