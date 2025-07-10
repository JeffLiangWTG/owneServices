using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public abstract class AISUCC5MessageProcessor<TEDIMessage, TDataProvider> : MessageAttacheeMessageProcessor<TEDIMessage, TDataProvider>
		where TEDIMessage : AISUCC5InboundEDIMessage
	{
		protected AISUCC5MessageProcessor(LoggingInformation logger, System.Type xmlObjectType) : base(logger, xmlObjectType) { }

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.IECustomsUCC5Import;

		protected override IRegistryItem GetEmailGroupRegistryItem() => IECustomsDataRegistry.Instance.SendImportAcknowledgements;

		protected override bool NeedToSendEmailNotification(EDIMessage message) => true;
	}
}
