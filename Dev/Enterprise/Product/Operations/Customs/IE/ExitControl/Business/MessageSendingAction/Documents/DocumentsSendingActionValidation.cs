using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class DocumentsSendingActionValidation : ZValidation
	{
		public DocumentsSendingActionValidation(DocumentsSendingAction parent) : base(parent)
		{
			Parent = parent;
		}

		DocumentsSendingAction Parent { get; }

		public override Type AutoValidationType => typeof(DocumentsSendingActionValidation);

		public override void ValidateAll()
		{
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateMovementReference();
				ValidateAdditionalInfos();
				ValidateSupportingDocuments();
			}
		}

		void ValidateMovementReference()
		{
			ValidateCalculatedProperty(Parent.MovementReferenceInfo);
		}

		protected void CheckMovementReference()
		{
			if (!Parent.IsValidationSuspended)
			{
				if (Parent.MovementReference.IsEmpty)
				{
					Parent.MovementReferenceInfo.AddError(Res.GetString("78A99478-AA38-4372-810B-2293140780D0", "The Exit Report need to have a Movement Reference Number to send."));
				}
			}
		}

		void ValidateAdditionalInfos()
		{
			if (!Parent.IsValidationSuspended)
			{
				var message = Res.GetString("8913F916-2710-4920-9C24-991D4CC66074", "Cannot send message without any Additional Information input.");
				Parent.RemoveRowError(message);
				if (!Parent.AddInfoCollection.Any())
				{
					Parent.AddRowError(message);
				}
			}
		}

		void ValidateSupportingDocuments()
		{
			if (!Parent.IsValidationSuspended)
			{
				var message = Res.GetString("3F67170D-07CB-4E6B-9AD2-361579AE94ED", "Cannot send message without any eDoc selected.");
				Parent.RemoveRowError(message);
				if (!Parent.SupportingDocuments.Any())
				{
					Parent.AddRowError(message);
				}
			}
		}
	}
}
