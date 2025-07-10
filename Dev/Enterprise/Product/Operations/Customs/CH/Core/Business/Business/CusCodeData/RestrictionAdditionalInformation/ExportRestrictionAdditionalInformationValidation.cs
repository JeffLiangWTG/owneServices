using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class ExportRestrictionAdditionalInformationValidation : CusCodeDataValidation
{
	public ExportRestrictionAdditionalInformationValidation(CusCodeData parent) : base(parent)
	{
	}

	public new RestrictionAdditionalInformation Parent => (RestrictionAdditionalInformation)base.Parent;

	Restriction Restriction => Parent.Parent;

	protected PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New((Restriction.Parent as JobComInvoiceLine).JobDeclaration);
	PlausiValidation plausiValidation;

	protected override void CheckCY_Code()
	{
		MandatoryValidation.MessageErrorIfNotEntered(Parent.CY_CodeInfo);
		ListValidation.MessageErrorIfInvalidCode(Parent.CY_CodeInfo);
		PlausiValidation.CheckNP70199(Parent);
		PlausiValidation.CheckNS30116(Restriction);
	}

	protected override void CheckCY_Data()
	{
		base.CheckCY_Data();
		PlausiValidation.CheckNS30110(Parent.CY_DataInfo, Parent);
		PlausiValidation.CheckNP70229(Parent.CY_DataInfo, Parent);
	}
}
