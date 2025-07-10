using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.Messaging;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSGBJobDeclarationMessageManager : GBJobDeclarationMessageManager
	{
		readonly IEnumerable<JobDeclarationMessageSendingObject> objectsToSend;

		public CDSGBJobDeclarationMessageManager(EU.Business.Declaration.JobDeclaration declaration, CDSTransmissionMessageGenerator generator)
			: base(declaration, generator)
		{
			objectsToSend = generator.ObjectsToSend;
		}

		protected override void SaveFactoryAfterSendingMessages(ISendsMessagesToCustoms sender, CancellationToken token)
		{
			PopulateAmendmentReasonIfNeeded();

			base.SaveFactoryAfterSendingMessages(sender, token);
		}

		protected void PopulateAmendmentReasonIfNeeded()
		{
			foreach (var sendingObject in objectsToSend)
			{
				if (sendingObject.IsAmendOrDelete)
				{
					var header = sendingObject.Header;
					var newReasonCode = sendingObject.ChangeAcknowledgementIndicator;
					if (!newReasonCode.IsEmpty && newReasonCode != header.ZG_AmendmentReasonCode)
					{
						header.ZG_AmendmentReasonCode = newReasonCode;
					}

					var newReason = sendingObject.VOCReason;
					if (!newReason.IsEmpty && newReason != header.CH_CustomsMessageRemarks)
					{
						header.CH_CustomsMessageRemarks = newReason;
					}
				}
			}
		}

		protected override string MessageSentNotificationText =>
			ZString.Format("Message(s) have been sent for the following entries:\r\n\r\n{0}",
				GetSentEntriesForMessage());

		ZString GetSentEntriesForMessage()
		{
			return objectsToSend.Select(x =>
			{
				ZString result = x.MessageTypeDescription + " - " + x.LocalReferenceNumber;
				return result;
			}).JoinAsString("\r\n");
		}
	}
}
