using CargoWise.Customs.Shared.MessageDefinitions.Universal.UniversalEvent;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class XERMessageProcessor : MessageProcessorWithEmailNotification<UniversalEventData>
	{
		public XERMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string StatusOfOutgoingEDIMessage => EDIMessage.Status.ProcessedOK;

		protected override string MessageFriendlyNameCore => Res.GetString("5B543EF7-EA5E-4426-889C-3E4D9FE7B7E0", "xT Error Response Message Processor");

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, UniversalEventData messageObject) => GenerateEmailSubjectText(manifestHeader, messageObject);

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, UniversalEventData messageObject) => Res.GetString("190F5B9A-47EA-440F-BDB4-AD87FE95D00B", $"ICS2: Message sending failed for {manifestHeader.AMA_JobReference} due to xT sending error.");

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2ErrorsTo;

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, UniversalEventData messageObject)
		{
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Error;
			var outgoingMessage = GetOriginalMessage(manifestHeader);
			if (outgoingMessage != null)
			{
				outgoingMessage.EM_Status = EDIMessageStatusList.Codes.Error;
			}
		}
	}
}
