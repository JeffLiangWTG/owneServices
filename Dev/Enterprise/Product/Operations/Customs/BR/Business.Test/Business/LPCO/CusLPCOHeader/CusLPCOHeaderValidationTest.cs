using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class CusLPCOHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCPH_OH_PermitHolder()
		{
			var lpcoHeader = Factory.New<CusLPCOHeader>();
			ValidationTestHelper.AssertErrorIfNotEntered(lpcoHeader.CPH_OH_PermitHolderInfo);
		}
	}
}

