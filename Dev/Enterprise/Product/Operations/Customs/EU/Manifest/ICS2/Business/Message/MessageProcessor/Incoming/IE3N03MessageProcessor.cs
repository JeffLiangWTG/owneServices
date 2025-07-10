using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N03;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N03MessageProcessor : MessageProcessorWithEmailNotification<Ie3N03Type>
	{
		public IE3N03MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("929926D3-E88F-4255-BB70-F0095C3F67F0", "Assessment Complete Notification");

		protected override Func<Ie3N03Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N03Type messageObject)
		{
			return Res.GetString("990CB12F-2569-47E5-83CF-B7E0D62F5664", "ICS2 - Risk Assessment Complete {0}/{1}", manifestHeader.AMA_JobReference, messageObject.Mrn);
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N03Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder(Res.GetString("435A206E-389E-4C13-9FD9-C3D785106DD6", "Risk Assessment Completed - {0}", GetISO8601DateTimeWithSecondsPrecision(messageObject.CompletionDate.DateTime)));

			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N03Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.ASC;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}
