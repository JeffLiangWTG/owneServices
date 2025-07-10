using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class TobaccoValidation : Customs.Business.CusSupportingInfoValidation
{
	public TobaccoValidation(Tobacco parent) : base(parent) { }

	new Tobacco Parent => (Tobacco)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New(Parent.Parent.JobDeclaration);
	PlausiValidation plausiValidation;

	protected override void CheckCSI_Code()
	{
		base.CheckCSI_Code();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
		PlausiValidation.CheckR331abcd(Parent.CSI_CodeInfo, Parent);
		PlausiValidation.CheckR257(Parent);
	}

	protected override void CheckCSI_SubType()
	{
		base.CheckCSI_SubType();

		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_SubTypeInfo);
	}

	protected override void CheckCSI_Description()
	{
		base.CheckCSI_Description();

		if (Parent.CSI_Code != UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
		}
	}

	protected override void CheckCSI_ItemNumber()
	{
		base.CheckCSI_ItemNumber();

		MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ItemNumberInfo);
	}

	protected override void CheckCSI_Value()
	{
		base.CheckCSI_Value();

		if (Parent.CSI_Code != UniversalReferenceConstants.TobaccoMainGroupCodes.ECigarettes)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ValueInfo);
		}
	}

	protected override void CheckCSI_AdditionalDescription()
	{
		base.CheckCSI_AdditionalDescription();

		ListValidation.MessageErrorIfInvalidCode(Parent.CSI_AdditionalDescriptionInfo);
	}
	protected override void CheckCSI_ReferenceNumber()
	{
		base.CheckCSI_ReferenceNumber();

		if (Parent.Parent.JobDeclaration.IsExport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);
		}
	}
	protected override void CheckCSI_UnitOfQuantity()
	{
		base.CheckCSI_UnitOfQuantity();

		if (Parent.Parent.JobDeclaration.IsExport)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_UnitOfQuantityInfo);
		}
	}
}
