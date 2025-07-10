using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.H7.Business;

namespace Enterprise.Customs.GB.H7.Business
{
	public class SupportingDocumentValidation : EU.H7.Business.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
			zValidationInternals = this;
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		protected override void CheckCSI_Code()
		{
			var info = Parent.CSI_CodeInfo;

			if (IsCSICodeAndReferenceDuplicate())
			{
				info.AddMessageError(Res.GetString("34da0578-0544-41d0-afd9-2ae82838a730", "A row with this document type and reference number already exists on this Bill."));
			}

			MandatoryValidation.MessageErrorIfNotEntered(info);
			ListValidation.MessageErrorIfInvalidCode(info);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			if (Parent.CSI_ReferenceNumber.IsEmpty && Parent.ReferenceNumberRequired)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCSI_Actions();
			ValidateCSI_Availability();
		}

		public void ValidateCSI_Actions()
		{
			zValidationInternals.Validate(Parent.CSI_ActionsInfo, new RunValidationInvoker(() =>
			{
				CheckCSI_Actions();
			}));
		}

		protected void CheckCSI_Actions()
		{
			var info = Parent.CSI_ActionsInfo;
			if (info.Value.IsEmpty)
			{
				if (Parent.Lookups.ActionList.Count > 0 && Parent.CSI_Code != "C600" && Parent.CSI_Code != "C601") // these two types should allow the Action to be blank
				{
					info.AddMessageError(Res.GetString("92fbb7dd-c229-406d-8507-2f563ff96b40", "Please enter an Action."));
				}
			}
			else if (!Parent.IsAvailabilityAndActionCombinationValid)
			{
				Parent.CSI_ActionsInfo.AddMessageError(Res.GetString("dc4448de-b624-43fb-b795-bb588405a68c", "This Action cannot be used with the selected Availability. Please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAV'."));
			}

			ListValidation.MessageErrorIfInvalidCode(info);
		}

		public void ValidateCSI_Availability()
		{
			zValidationInternals.Validate(Parent.CSI_AvailabilityInfo, new RunValidationInvoker(() =>
			{
				CheckCSI_Availability();
			}));
		}

		protected void CheckCSI_Availability()
		{
			var info = Parent.CSI_AvailabilityInfo;
			if (info.Value.IsEmpty)
			{
				if (Parent.Lookups.AvailabilityList.Count > 0 && Parent.CSI_Code != "C600" && Parent.CSI_Code != "C601")
				{
					info.AddMessageError(Res.GetString("0b2e41e1-2a50-4d2d-8ba8-63e5a6105f18", "Please enter an Availability."));
				}
			}
			else if (!Parent.IsAvailabilityAndActionCombinationValid)
			{
				Parent.CSI_AvailabilityInfo.AddMessageError(Res.GetString("d5e1e208-c81c-4656-9c4f-f6947ac9fbd9", "This Availability cannot be used with the selected Action. Please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAV'."));
			}
			ListValidation.MessageErrorIfInvalidCode(info);
		}

		bool IsCSICodeAndReferenceDuplicate()
		{
			var parentSupportingDocuments = default(ISupportingDocumentCollection<SupportingDocument>);
			if (Parent.Parent is AsycudaBill bill)
			{
				parentSupportingDocuments = bill.SupportingDocuments;
			}
			else if (Parent.Parent is AsycudaPackedItem packedItem)
			{
				parentSupportingDocuments = packedItem.SupportingDocuments;
			}

			if (parentSupportingDocuments != null)
			{
				return parentSupportingDocuments.Where(document => document != Parent).Any(document => document.CSI_Code == Parent.CSI_Code && document.CSI_ReferenceNumber == Parent.CSI_ReferenceNumber);
			}

			return false;
		}

		readonly IValidationInternals zValidationInternals;
	}
}
