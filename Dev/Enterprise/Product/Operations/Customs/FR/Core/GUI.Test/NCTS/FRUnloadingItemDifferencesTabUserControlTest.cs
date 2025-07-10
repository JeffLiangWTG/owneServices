using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	sealed class FRUnloadingItemDifferencesTabUserControlTest : TestCaseWithFactory
	{
		public void TestUnloadedGoodsItemsGridColumns()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			using (var form = new ZForm(header))
			using (var control = new FRUnloadingItemDifferencesTabUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertNull("Column isMissing should no be present for France", control.UnloadedGoodsItemsGrid.GetColumnStyle(nameof(EU.NCTS.Business.NctsArrivalAndUnloadingCargoDesc.IsMissing)));
			}
		}
	}
}
