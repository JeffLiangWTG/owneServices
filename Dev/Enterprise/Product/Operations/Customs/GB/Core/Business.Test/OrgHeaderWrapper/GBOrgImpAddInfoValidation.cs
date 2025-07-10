using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GBOrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVATDeferrmentType()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var obj = GBOrgImpAddInfo.Get(orgHeader);
			var euObj = EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.UnitedKingdom);

			obj.ZO_VATDeferType = "Z";
			AssertHasMessageErrors(obj.ZO_VATDeferTypeInfo);

			euObj.ZO_UseFr3FiscalRepresentation = true;
			obj.ZO_VATDeferType = "A";
			AssertHasMessageError(obj.ZO_VATDeferTypeInfo, "This field is mutually exclusive with EU 'Use postponed VAT accounting?' field.");

			euObj.ZO_UseFr3FiscalRepresentation = false;
			obj.Validation.ValidateZO_VATDeferType();
			AssertNoMessageError(obj.ZO_VATDeferTypeInfo, "This field is mutually exclusive with EU 'Use postponed VAT accounting?' field.");

			euObj.ZO_UseFr3FiscalRepresentation = true;
			obj.ZO_VATDeferType = "";
			AssertNoMessageError(obj.ZO_VATDeferTypeInfo, "This field is mutually exclusive with EU 'Use postponed VAT accounting?' field.");
		}
	}
}

