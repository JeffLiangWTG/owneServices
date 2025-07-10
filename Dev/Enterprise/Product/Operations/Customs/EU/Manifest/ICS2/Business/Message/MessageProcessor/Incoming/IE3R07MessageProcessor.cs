using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3R07;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3R07MessageProcessor : MessageProcessorWithEmailNotification<Ie3R07Type>
	{
		public IE3R07MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("dd59e73e-b97e-43b2-9593-812e5b10eec2", "Invalidation Acceptance Response");

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3R07Type messageObject)
		{
			return GenerateEmailSubjectText(manifestHeader, messageObject);
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3R07Type messageObject)
		{
			return Res.GetString("c05d352d-669d-4f5a-8e70-634b348b68a9", "ICS2 - Invalidation Acceptance Response for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3R07Type messageObject)
		{
			manifestHeader.AMA_MessageStatus = MessageStatusCodeList.Codes.Sent;
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.CAN;
		}

		protected override Func<Ie3R07Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;
	}
}
