using System;
using CargoWise.Customs.EU.MessageDefinitions.ICS2.IE3N11;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public sealed class IE3N11MessageProcessor : MessageProcessorWithEmailNotification<Ie3N11Type>
	{
		public IE3N11MessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("ff24d7fd-5636-497f-938b-16cf265b3115", "Pending Notification");

		protected override Func<Ie3N11Type, string> GetLocalReferenceNumber => messageObject => messageObject.TransportDocument.DocumentNumber;

		protected override void NotifyPreProcessFailure(EDIMessage message, Ie3N11Type messageObject)
		{
			SendEmailNotification(message.Factory, null, messageObject);
		}

		protected override string GenerateEmailSubjectText(AsycudaManifestHeader manifestHeader, Ie3N11Type messageObject)
		{
			return Res.GetString("cb791ac1-5093-4a6b-8e2f-e63b54be9d0d", "ICS2 Unmatched or Not Sent Master Bill {0}", GetLocalReferenceNumber(messageObject));
		}

		protected override string GenerateEmailBodyText(AsycudaManifestHeader manifestHeader, Ie3N11Type messageObject)
		{
			var htmlBuilder = new ZStringBuilder();
			htmlBuilder.AppendLine(Res.GetString("47f96308-ed2d-46d8-8fcf-28de818493e4", "Records show that Master Bill Number {0} has not had an ENS filed.", GetLocalReferenceNumber(messageObject)));

			if (manifestHeader is null)
			{
				htmlBuilder.AppendLine(Res.GetString("4b1cec05-cc35-4664-9a18-b2f523ab97f8", "There is no ICS2 Manifest found for this Master Bill."));
			}

			return htmlBuilder.ToStringWithDelimiterBetweenAppends("<br />");
		}

		protected override void UpdateManifestHeader(AsycudaManifestHeader manifestHeader, Ie3N11Type messageObject)
		{
			manifestHeader.RegistrationStatus = EUICS2CustomsStatusList.Codes.PND;
		}

		protected override IRegistryItem GetEmailGroupRegistryItemCore() => ICS2CustomsDataRegistry.Instance.EnableICS2UnmatchedOrNotSentTo;
	}
}
