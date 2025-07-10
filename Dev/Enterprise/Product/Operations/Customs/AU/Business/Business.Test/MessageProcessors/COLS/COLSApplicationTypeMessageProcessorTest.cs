using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class COLSApplicationTypeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestApplicationCode()
		{
			AssertEquals(EDIMessage.ApplicationCodes.COLS, processor.ApplicationCode);
		}

		public void TestGetProcessor()
		{
			AssertType<AddNewLodgementResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.AddNewLodgement));
			AssertType<AddAdditionalDocumentResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.AddAdditionalDocument));
			AssertType<LodgementStatusResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.LodgementStatus));
			AssertType<MakeAnEnquiryResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.MakeAnEnquiry));
			AssertType<RequestAReassessmentResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.RequestAReassessment));
			AssertType<SwitchAepLodgementResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.SwitchAepLodgement));
			AssertType<PaymentStatusResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.PaymentStatus));
			AssertType<AddAttachmentResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.AddAttachment));
			AssertType<XtMessageErrorResponseProcessor>(processor.GetProcessor(AUCOLSMessageTypeList.Codes.XtMessageError));

			logger.ClearLogs();
			CombineAssertions("Unknow message type", () =>
			{
				AssertNull(processor.GetProcessor("UNK"));
				AssertContains("Unknown Message Type: UNK", string.Join("\r\n", logger.Logs));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
			processor = new COLSApplicationTypeMessageProcessor(logger);
		}

		LoggingInformation logger;
		COLSApplicationTypeMessageProcessor processor;
	}
}
