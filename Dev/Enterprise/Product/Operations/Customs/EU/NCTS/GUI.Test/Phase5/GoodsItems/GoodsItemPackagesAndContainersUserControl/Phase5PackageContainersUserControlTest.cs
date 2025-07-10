using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class Phase5PackageContainersUserControlTest : TestCaseWithFactory
	{
		public void TestPackageContainersTabUserControl()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var control = new Phase5PackageContainersUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("When the data member is Bills.GoodsItems no Exception occurs.", () => control.SetDataBinding(nctsHeader, "Bills.GoodsItems"));
					AssertExceptionThrown<ArgumentException>("When the data member is Bills.GoodsItems.Packages an Exception occurs.", () => control.SetDataBinding(nctsHeader, "Bills.GoodsItems.Packages"));
				});
			}
		}
	}
}
