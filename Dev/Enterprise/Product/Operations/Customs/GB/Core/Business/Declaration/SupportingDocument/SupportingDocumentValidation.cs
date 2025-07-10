using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class SupportingDocumentValidation : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentValidation
	{
		public SupportingDocumentValidation(SupportingDocument parent)
			: base(parent)
		{
		}

		protected new SupportingDocument Parent => (SupportingDocument)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			using (((ISingleElementListInternal)Parent).SuspendListChanged())
			{
				ValidateCSI_Actions();
				ValidateCSI_Availability();
			}
		}

		public void ValidateCSI_Actions()
		{
			ValidateCalculatedProperty(Parent.CSI_ActionsInfo);
		}

		public void ValidateCSI_Availability()
		{
			ValidateCalculatedProperty(Parent.CSI_AvailabilityInfo);
		}

		protected virtual void CheckCSI_Actions()
		{
			if (Parent.CSI_Actions.IsEmpty)
			{
				if (Parent.Lookups.ActionList.Count > 0 && Parent.CSI_Code != "C600" && Parent.CSI_Code != "C601") // these two types should allow the Action to be blank
				{
					Parent.CSI_ActionsInfo.AddMessageError(G1_ActionInfoEmpty);
				}
			}
			else if (!IsValidAvailabilityAndActions())
			{
				Parent.CSI_ActionsInfo.AddMessageError(Res.GetString("85BD778B-8838-4DB8-9D60-CB84EC48E1AC", "This action cannot be used with the selected availability, please choose a different combination.The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAC'"));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ActionsInfo, Parent.Lookups.ActionList);
		}
		public const string G1_ActionInfoEmpty = "Please enter an Action.";

		protected virtual void CheckCSI_Availability()
		{
			if (Parent.CSI_Availability.IsEmpty)
			{
				if (Parent.Lookups.AvailabilityList.Count > 0 && Parent.CSI_Code != "C600" && Parent.CSI_Code != "C601")
				{
					Parent.CSI_AvailabilityInfo.AddMessageError(CSI_AvailabilityInfoEmpty);
				}
			}
			else if (!IsValidAvailabilityAndActions())
			{
				Parent.CSI_AvailabilityInfo.AddMessageError(Res.GetString("CD1F18F4-0337-44AB-956A-8C21EC3B603D", "This availability cannot be used with the selected action, please choose a different combination. The list of valid combinations for this Code can be seen by putting the cursor on the Code field and pressing F3; the combinations are visible as attributes of type 'ACTAC'"));
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_AvailabilityInfo, Parent.Lookups.AvailabilityList);
		}
		public const string CSI_AvailabilityInfoEmpty = "Please enter an Availability.";

		bool IsValidAvailabilityAndActions()
		{
			var cusCode = Parent.RefCusCode;
			if (cusCode == null
				|| !Parent.Lookups.AvailabilityList.ContainsCode(Parent.CSI_Availability)
				|| !Parent.Lookups.ActionList.ContainsCode(Parent.CSI_Actions))
			{
				return true;
			}

			return IsAvailabilityAndActionCombinationValidAttribute(Parent.CSI_Availability, Parent.CSI_Actions);
		}

		protected override void CheckCSI_UnitOfQuantity()
		{
			if (!Parent.CSI_UnitOfQuantity.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_UnitOfQuantityInfo, Parent.Lookups.UnitOfQuantityList);
			}
		}
		internal bool IsAvailabilityAndActionCombinationValidAttribute(string availability, string action)
		{
			var validAvailabilities = Parent.RefCusCode?.GetAttributesValues(RefCusCodeListAttributeTypes.Codes.ACTAV);
			return validAvailabilities != null && validAvailabilities.Contains(availability + action);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (Parent.CSI_ReferenceNumber.IsEmpty && !Parent.CSI_Code.IsEmpty && Parent.RefCusCode != null && Parent.RefCusCode.HasAttribute(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReferenceNumber, "Y"))
			{
				Parent.CSI_ReferenceNumberInfo.AddMessageError("This supporting document type requires a reference/ID");
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();
			if (Parent.CSI_Description.IsEmpty && !Parent.CSI_Code.IsEmpty && Parent.RefCusCode != null && Parent.RefCusCode.HasAttribute(GBCommonConstants.RefCusCodeListAttributeCodes.SupportingDocumentReason, "Y"))
			{
				Parent.CSI_DescriptionInfo.AddMessageError("This supporting document type requires a reason/description");
			}
		}
	}
}
