using System.Linq;

namespace Enterprise.Customs.IE.Business
{
	public class DocumentsSendingActionValidation : CusEntryHeaderMessageSendingActionValidation
	{
		public DocumentsSendingActionValidation(DocumentsSendingAction parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckMovementReference();
			CheckAdditionalInfos();
			CheckSupportingDocuments();
		}

		void CheckMovementReference()
		{
			if (!Parent.IsValidationSuspended)
			{
				if (Parent.MovementReference.IsEmpty)
				{
					Parent.MovementReferenceInfo.AddError(Res.GetString("DE912A30-70C5-4434-B81C-47837EB8BAC6", "The entry need to have a Movement Reference Number to send."));
				}
			}
		}

		void CheckAdditionalInfos()
		{
			if (!Parent.IsValidationSuspended)
			{
				var message = Res.GetString("1B3FFD89-9ADC-4F5B-9203-588A63C13F62", "Cannot send message without any Additional Information(populated from Entry Instruction -> Documents Requested) input.");
				Parent.RemoveRowError(message);
				if (!Parent.AddInfoCollection.Any())
				{
					Parent.AddRowError(message);
				}
			}
		}

		void CheckSupportingDocuments()
		{
			if (!Parent.IsValidationSuspended)
			{
				var message = Res.GetString("84E9CB50-BB15-4277-A5BF-A9B94A625214", "Cannot send message without any eDoc selected.");
				Parent.RemoveRowError(message);
				if (!Parent.SupportingDocuments.Any())
				{
					Parent.AddRowError(message);
				}
			}
		}

		new DocumentsSendingAction Parent => (DocumentsSendingAction)base.Parent;
	}
}
