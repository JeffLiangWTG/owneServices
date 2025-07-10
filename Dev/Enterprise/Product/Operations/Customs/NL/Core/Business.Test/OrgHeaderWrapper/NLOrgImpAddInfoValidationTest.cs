using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

public class NLOrgImpAddInfoValidationTest : BusinessObjectValidationTestCase
{
	public void TestVATDeferment()
	{
		var host = Factory.New<OrgCountryData>();
		var obj = new NLOrgImpAddInfo((ZPropertyInfoString)host.OV_ImportCustomsDefaultAddInfoInfo);
		obj.ZO_VATDeferment = "Z";
		AssertHasMessageError(obj.ZO_VATDefermentInfo, ListValidation.InvalidCodeMessageError);
		AssertNoMessageErrorContaining(obj.ZO_VATDefermentInfo, MandatoryValidation.YouHaveNotEntered);

		obj.ZO_VATDeferment = Customs.Business.YesNoList.Codes.Yes;
		AssertNoMessageError(obj.ZO_VATDefermentInfo, ListValidation.InvalidCodeMessageError);
		AssertNoMessageErrorContaining(obj.ZO_VATDefermentInfo, MandatoryValidation.YouHaveNotEntered);

		obj.ZO_VATDeferment = ZString.Empty;
		AssertNoMessageError(obj.ZO_VATDefermentInfo, ListValidation.InvalidCodeMessageError);
		AssertHasMessageErrorContaining(obj.ZO_VATDefermentInfo, MandatoryValidation.YouHaveNotEntered);
	}
}
