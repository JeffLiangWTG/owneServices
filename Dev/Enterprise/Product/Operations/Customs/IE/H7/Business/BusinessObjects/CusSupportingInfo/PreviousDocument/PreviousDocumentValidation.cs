using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.H7.Business
{
	public class PreviousDocumentValidation : EU.H7.Business.PreviousDocumentValidation
	{
		public PreviousDocumentValidation(AutoCusSupportingInfo parent)
			: base(parent)
		{
		}

		protected new PreviousDocument Parent => (PreviousDocument)base.Parent;

		protected override void CheckCSI_Code()
		{
			if (Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				if (Parent.CSI_Code.IsEmpty && !Parent.CSI_ReferenceNumber.IsEmpty)
				{
					Parent.CSI_CodeInfo.AddMessageError(Parent.ValidationMessage.GetBR2010RuleMessage(Parent.CSI_CodeInfo));
				}
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo, Parent.ValidationMessage.InvalidValueRuleMessage);
			}
			else if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo, Parent.ValidationMessage.InvalidValueRuleMessage);
				ValidateEmptyValue(Parent.CSI_CodeInfo, Res.GetString("95944b2a-37f4-490e-be76-42233152d76a", "You have not entered a Previous Document type."));
			}
			else
			{
				base.CheckCSI_Code();
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			if (Parent.CSI_ParentTableCode == AsycudaPackedItemSchema.Constants.Prefix)
			{
				if (Parent.CSI_ReferenceNumber.IsEmpty && !Parent.CSI_Code.IsEmpty)
				{
					Parent.CSI_ReferenceNumberInfo.AddMessageError(Parent.ValidationMessage.GetBR2010RuleMessage(Parent.CSI_ReferenceNumberInfo));
				}
			}
			else if (Parent is CusSupportingInfo { Parent: AsycudaBill bill })
			{
				ValidateEmptyValue(Parent.CSI_ReferenceNumberInfo, Res.GetString("c434c182-f021-4579-9a6f-cb4bb7f5d3da", "You have not entered a Previous Document reference number."));
			}
			else
			{
				base.CheckCSI_ReferenceNumber();
			}
		}

		void ValidateEmptyValue(ZPropertyInfo info, string errorMessage)
		{
			if (info.Value.IsEmpty)
			{
				info.AddMessageError(Parent.ValidationMessage.GetBR2010RuleMessage(errorMessage));
			}
		}
	}
}
