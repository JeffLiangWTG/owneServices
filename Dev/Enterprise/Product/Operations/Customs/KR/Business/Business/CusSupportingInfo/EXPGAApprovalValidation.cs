using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class EXPGAApprovalValidation : GAApprovalValidation
	{
		public EXPGAApprovalValidation(GAApproval parent)
			: base(parent)
		{
		}

		new GAApproval Parent => (GAApproval)base.Parent;
		bool IsRequirmentTypeThree => RequirementTypeCodeList.IsGADeclarationRequired(Parent.CSI_SubType);

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateNonGAReasonType();
		}

		protected override void CheckCSI_DateOfIssue()
		{
			if (IsRequirmentTypeThree)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DateOfIssueInfo);
			}
		}

		protected override void CheckCSI_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			if (IsRequirmentTypeThree)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_Description()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
		}

		protected override void CheckCSI_AdditionalDescription()
		{
			if (!IsRequirmentTypeThree)
			{
				if (Parent.NonGAReasonType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_AdditionalDescriptionInfo);
				}
				else
				{
					var cusCode = MessageFunctions.GetRefCusCodeList(Parent.Factory, Parent.NonGAReasonType, Constants.ZZ.NKCodeType.ENGAR);
					var isMandatoryDocWhenExemptCode = cusCode?.Attributes?.HasAttribute(Constants.ZZ.CodeListAttributeNames.MandatoryDocWhenExemptCode) ?? false;
					if (isMandatoryDocWhenExemptCode || Parent.CSI_SubType == RequirementTypeCodeList.Codes._9)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_AdditionalDescriptionInfo);
					}
				}
			}
		}

		public void ValidateNonGAReasonType()
		{
			ValidateCalculatedProperty(Parent.NonGAReasonTypeInfo);
		}
		protected void CheckNonGAReasonType()
		{
			if (!Parent.NonGAReasonTypeInfo.ReadOnly && !IsRequirmentTypeThree)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.NonGAReasonTypeInfo);
			}
		}
	}
}
