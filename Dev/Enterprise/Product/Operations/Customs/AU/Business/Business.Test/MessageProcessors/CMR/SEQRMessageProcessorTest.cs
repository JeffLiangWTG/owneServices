using System;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEQRMessageProcessorTest : CMRMessageResponseProcessorTest
	{
		public void TestNoResponseWhenAccepted()
		{
			var bill = Factory.New<CusSCAOceanBill>();
			bill.CB_MessageReference = "S00002158";

			var group = Factory.New<GlbGroup>();
			group.Staff.AddNew().GS_EmailAddress = "blah@blah.com";
			Factory.Save();

			var message = bill.Messages.AddNew(typeof(CMRSEQRMessage));
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::SEQR+1AI4 918G D215:001+11'
NAD+MR+FGE973N::95'
RFF+ACW:SEQ'
RFF+AFM:9'
RFF+ABO:42243-60087/SYD3::009'
DTM+310:20051011062214:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5203:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITHOUT ERRORS AND WARNINGS'
CNT+55:000'
UNT+13+000001'".Replace("\r\n", "");
			var processor = new TestHelperSEQRMessageProcessor(new LoggingInformation());
			processor.ProcessMessage(message);
			CombineAssertions(() =>
			{
				AssertEquals("Accecpted.", EDIMessage.Status.Received, message.EM_Status);
				AssertEquals(1, processor.AcknowledgementEmailSendCount);
				AssertEquals(0, processor.ErrorEmailSendCount);
			});
		}
		protected override CMRMessageResponseProcessor GetMessageProcessor() => processor;

		protected override ZString GetExpectedMessageCode() => "SEQ";

		protected override ZString GetExpectedMessageName() => "SEA Cargo Establishment Query Response - (SEQR)";

		protected override Type IncomingMessageType => typeof(CMRSEQRMessage);

		protected override void SetUp()
		{
			base.SetUp();
			var logger = new LoggingInformation();
			processor = new SEQRMessageProcessor(logger);
		}
		SEQRMessageProcessor processor;

		sealed class TestHelperSEQRMessageProcessor : SEQRMessageProcessor
		{
			public TestHelperSEQRMessageProcessor(LoggingInformation logger) : base(logger) { }
		}
	}
}
