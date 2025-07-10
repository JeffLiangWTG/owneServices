using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class DEOrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVATClaimBack()
		{
			var host = Factory.New<OrgCountryData>();
			var obj = new DEOrgImpAddInfo((ZPropertyInfoString)host.OV_ImportCustomsDefaultAddInfoInfo);
			obj.ZO_VATClaimBack = "Z";
			AssertHasMessageError(obj.ZO_VATClaimBackInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(obj.ZO_VATClaimBackInfo, MandatoryValidation.YouHaveNotEntered);

			obj.ZO_VATClaimBack = Customs.Business.YesNoList.Codes.Yes;
			AssertNoMessageError(obj.ZO_VATClaimBackInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(obj.ZO_VATClaimBackInfo, MandatoryValidation.YouHaveNotEntered);

			obj.ZO_VATClaimBack = ZString.Empty;
			AssertNoMessageError(obj.ZO_VATClaimBackInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(obj.ZO_VATClaimBackInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
