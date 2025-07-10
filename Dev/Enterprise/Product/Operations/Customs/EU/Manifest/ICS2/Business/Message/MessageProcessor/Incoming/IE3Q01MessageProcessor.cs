using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3Q01;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3Q01MessageProcessor : MessageProcessorWithEmailNotification<Ie3Q01Type>
	{
		public IE3Q01MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("1172b101-7730-48c1-b30e-85ebf54b5ada", "Do Not Load Request");

		protected override Func<Ie3Q01Type, string> GetMasterReferenceNumber => messageObject => messageObject.Mrn;

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3Q01Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.DNL;
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3Q01Type messageObject)
		{
			return Res.GetString("0c4f8a40-a619-4114-84a5-86682151fd38", "ICS2 Do Not Load Notification for {0}/{1}", manifestHeader.AMA_JobReference, GetMasterReferenceNumber(messageObject));
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3Q01Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder();
			htmlBuilder.AppendLine();

			var messageInfoTableBuilder = new HtmlTableCreator(new string[] { Res.GetString("8e694d01-52a9-4e41-8b72-472547bb400c", "Bill Number"), Res.GetString("b7386cba-29a9-4ea2-b3fe-ee59458fcfe2", "Description") });
			messageInfoTableBuilder.WriteRow(messageObject.TransportDocumentHouse.DocumentNumber, Res.GetString("77399e08-4aa5-4a10-9c4b-604beef718ad", "Do Not Load"));

			htmlBuilder.Append(messageInfoTableBuilder.ToHtml());
			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2AcknowledgementsTo;
	}
}
