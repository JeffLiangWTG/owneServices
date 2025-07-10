using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.IT.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class Ucc6ExportEntryInstructionAdditionalInfoValidation : AdditionalInfoValidation
{
	public Ucc6ExportEntryInstructionAdditionalInfoValidation(AdditionalInfo parent) : base(parent)
	{
	}

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateRuleE1301();
		ValidateMoreThan99DocumentsAreNotAllowedPerType();
	}

	internal void ValidateRuleE1301()
	{
		if (Declaration?.IsTransitionPeriodAES30 ?? false)
		{
			Parent.AddRowMessageError(ValidationCaptions.AdditionalInfo.AddInfoNotAllowedInTransitionPeriodRuleE1301);
		}
	}

	internal void ValidateMoreThan99DocumentsAreNotAllowedPerType()
	{
		var parent = Parent;
		if (!parent.Lookups.SubTypeList.ContainsCode(parent.CSI_SubType))
		{
			return;
		}

		if (CusEntryInstruction?.AdditionalInfos
				.Where(i => i.CSI_SubType == parent.CSI_SubType)
				.IsCountMoreThan(99) ?? false)
		{
			parent.AddRowMessageError(ValidationCaptions.AdditionalInfo.GetUcc6ExportCusEntryInstructionMoreThan99DocumentsAreNotAllowedMessage(parent.CSI_SubType));
		}
	}

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
	}

	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();
		CheckMandatoryFieldBasedOnAttributes(RefCusCodeListAttributeName.ReferenceNumber);
	}

	void CheckMandatoryFieldBasedOnAttributes(ZString attributeName)
	{
		var parent = Parent;
		const string attributeValue = RefCusCodeListAttributeValues.Yes;

		if (parent.IsATransportDocument
			|| (parent.IsAnAdditionalReference && (parent.RefCusCode?.HasAttribute(attributeName, attributeValue) ?? false)))
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_ReferenceNumberInfo);
		}
	}

	JobDeclaration Declaration => CusEntryInstruction?.JobDeclaration;
	CusEntryInstruction CusEntryInstruction => Parent?.Parent as CusEntryInstruction;
}
