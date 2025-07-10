using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(Phase5GoodsItemPackagesAndContainersUserControl))]
	sealed class Phase5GoodsItemPackagesAndContainersUserControlTest : TestCaseWithFactory
	{
		public void TestLayoutProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			using (var control = new UserControlForTest())
			{
				AssertType<NctsPhase5LayoutProvider>("LayoutProvider", control.LayoutProvider_Exposed);
			}
		}

		class UserControlForTest : Phase5GoodsItemPackagesAndContainersUserControl
		{
			public INctsPhase5LayoutProvider LayoutProvider_Exposed => LayoutProvider;
		}
	}
}
