using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.Business.Testing
{
	class CusExitItemValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCXI_StatusIsValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Export Customs Status");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "123", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var exitDetail = Factory.New<CusExitDetail>();
			var exitItem = exitDetail.CusExitItems.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(exitItem.CXI_StatusInfo, "XXX", "123");
		}
	}
}
