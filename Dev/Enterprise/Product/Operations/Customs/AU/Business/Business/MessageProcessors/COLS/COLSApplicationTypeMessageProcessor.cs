using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class COLSApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public COLSApplicationTypeMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => "Cargo Online Lodgement System Message";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.COLS;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			GetProcessor(message.EM_MessageType)?.ProcessMessage(message);
		}

		internal COLSMessageProcessor GetProcessor(ZString messageType)
		{
			switch (messageType)
			{
				case AUCOLSMessageTypeList.Codes.AddNewLodgement:
					return new AddNewLodgementResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.AddAdditionalDocument:
					return new AddAdditionalDocumentResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.AddAttachment:
					return new AddAttachmentResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.LodgementStatus:
					return new LodgementStatusResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.MakeAnEnquiry:
					return new MakeAnEnquiryResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.RequestAReassessment:
					return new RequestAReassessmentResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.SwitchAepLodgement:
					return new SwitchAepLodgementResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.PaymentStatus:
					return new PaymentStatusResponseProcessor(Logger);
				case AUCOLSMessageTypeList.Codes.XtMessageError:
					return new XtMessageErrorResponseProcessor(Logger);
				default:
					Logger.LogError($"Unknown Message Type: {messageType}");
					return null;
			}
		}
	}
}
