using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business.Testing
{
	public class CusGoodsLocationAddressValidationTest : TestCaseWithFactory
	{
		public void TestApplyC0065Rule()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.CGL_LocationUse = "DEP";
			var locationAddress = goodsLocation.Address;
			ValidationTestHelper.AssertFieldIsNotMandatory(locationAddress.E2_GovRegNumInfo, "Location: Authorization No.", "Not mandatory if Rule C0065 is not applied");
		}
	}
}
