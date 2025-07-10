using Enterprise.BatchProcessor;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.Business.AIS
{
	public abstract class AISMessageProcessor<TEDIMessage, TDataProvider> : MessageAttacheeMessageProcessor<TEDIMessage, TDataProvider>
		where TEDIMessage : AISInboundEDIMessage
	{
		protected AISMessageProcessor(LoggingInformation logger, System.Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override IRegistryItem GetEmailGroupRegistryItem() => IECustomsDataRegistry.Instance.SendImportAcknowledgements;

		protected override bool NeedToSendEmailNotification(EDIMessage message) => !(message.EM_LinkedObject is IAISMessageAttacheeWithEmailLogic messageAttachee) ||
																					messageAttachee.ShouldSendEmailNotification(message);
	}
}
