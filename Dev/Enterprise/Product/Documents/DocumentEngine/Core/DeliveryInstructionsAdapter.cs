using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineIntegration;

namespace Enterprise.DocumentEngine
{
	public class DeliveryInstructionsAdapter : DeliveryInstructions
	{
		public DeliveryInstructionsAdapter(DeliveryInstructionsBase deliveryInstructions)
		{
			Destination = DeliveryInstructionDestination.TakenFromContact;
			IsDraft = deliveryInstructions.IsDraft;
			Language = deliveryInstructions.Language;
			BackgroundDelivery = deliveryInstructions.UseBackgroundDelivery;

			PrinterDelivery.PrintQueuePK = deliveryInstructions.PrinterId;
			PrinterDelivery.NumberOfCopies = deliveryInstructions.Copies;

			var coverNote = deliveryInstructions.CoverNote;
			if (!string.IsNullOrEmpty(coverNote))
			{
				IncludeCoverNote = true;
				CoverNote = coverNote;
			}
			else
			{
				IncludeCoverNote = false;
			}

			Recipients.RemoveAndDeleteAll();
			foreach (var recipient in deliveryInstructions.Recipients)
			{
				var adapterRecipient = Recipients.AddNew();
				adapterRecipient.OrgHeaderPK = recipient.OrganizationId;
				adapterRecipient.Name = recipient.Name;
				adapterRecipient.DeliveryMethod = recipient.DeliveryMethod;
				adapterRecipient.AttachmentType = recipient.EmailAttachmentType;
				adapterRecipient.Email = recipient.Email;
				adapterRecipient.EmailCarbonCopyRecipientsAsString = recipient.CC;
				adapterRecipient.EmailBlindCarbonCopyRecipientsAsString = recipient.BCC;
				adapterRecipient.Fax = recipient.FaxNumber;
			}
		}
	}
}
