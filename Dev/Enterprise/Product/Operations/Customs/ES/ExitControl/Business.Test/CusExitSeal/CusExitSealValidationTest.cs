using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class CusExitSealValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBK_UnloadingState()
		{
			var header = Factory.New<CusExitHeader>();
			var container = header.CusExitContainers.AddNew();
			CombineAssertions(() =>
			{
				var seal1 = (CusExitSeal)container.AllSealNumbers.AddNew();
				seal1.BK_UnloadingState = ZString.Empty;
				AssertListValidationInvalidCodeError(seal1.BK_UnloadingStateInfo, false);

				seal1.BK_UnloadingState = "asd";
				AssertListValidationInvalidCodeError(seal1.BK_UnloadingStateInfo, true);

				seal1.BK_UnloadingState = DiscrepanciesStatusCodeList.Codes.Missing;
				AssertListValidationInvalidCodeError(seal1.BK_UnloadingStateInfo, false);
			});
		}
	}
}
