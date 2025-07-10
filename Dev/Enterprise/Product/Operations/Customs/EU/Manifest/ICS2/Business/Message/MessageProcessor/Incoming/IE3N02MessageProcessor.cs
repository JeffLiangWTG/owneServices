using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N02;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N02MessageProcessor : MessageProcessorWithEmailNotification<Ie3N02Type>
	{
		public IE3N02MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("19096C14-8C07-46A9-931A-E8AA8BEE859A", "ENS Not Complete");

		protected override Func<Ie3N02Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N02Type messageObject)
		{
			return Res.GetString("23256FBC-C3D1-4ADB-9463-C17C8B723FDD", "ICS2 - ENS Not Complete for {0}/{1}", manifestHeader.AMA_JobReference, manifestHeader.AMA_MasterBill);
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N02Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder();
			htmlBuilder.AppendLine();

			var messageInfoTableBuilder = new HtmlTableCreator();

			messageInfoTableBuilder.WriteRow(manifestHeader.AMA_MasterBill, MessageFriendlyName);

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N02Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.NCN;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2UnmatchedOrNotSentTo;
	}
}
