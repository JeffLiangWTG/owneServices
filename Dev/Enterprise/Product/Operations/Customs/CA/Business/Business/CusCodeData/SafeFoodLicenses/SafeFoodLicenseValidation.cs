using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business;

public class SafeFoodLicenseValidation : CusCodeDataValidation
{
	public SafeFoodLicenseValidation(SafeFoodLicense parent) : base(parent)
	{
	}

	protected override void CheckCY_Code()
	{
		CheckCY_CodeIsNotEmpty();
		PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CY_CodeInfo);
	}
}
