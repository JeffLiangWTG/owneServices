using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N09;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N09MessageProcessor : MessageProcessorWithEmailNotification<Ie3N09Type>
	{
		public IE3N09MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("B900FBA8-D464-4674-81BB-8E93973F36B5", "AEO Control Notification");

		protected override Func<Ie3N09Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N09Type messageObject)
		{
			return Res.GetString("2DE04B3A-C872-41CA-B42E-020D48A49065", "ICS2 - AEO Control Notification for {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N09Type messageObject)
		{
			return GenerateEmailSubjectText(manifestHeader, messageObject);
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N09Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.AEO;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}
