using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckAGC_Number_Mandatory_NotPhase5Departure()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var cusAuthorizationUsage = nctsHeader.CusAuthorizationUsages.AddNew();

			ValidationTestHelper.AssertErrorIfNotEntered(cusAuthorizationUsage.AGC_NumberInfo);
		}
	}
}
