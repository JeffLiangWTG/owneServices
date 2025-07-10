using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	public class UnloadingHeaderDifferencesTabUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestIsBrokenGenAddOnColumn()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			arrivalHeader.DepartureHeaderContainers.AddNew().Seal1 = "CONT12345";
			arrivalHeader.DepartureHeaderContainers.AddNew().Seal1 = "CONT12346";
			var seal = arrivalHeader.Seals.AddNew();
			seal.CY_Data = "SEAL12345";
			using (var form = new NctsMovementForm(arrivalHeader))
			{
				form.Show();
				var sealGrid = (ZGrid)form.Controls.Find("SealsGrid", true).Single();
				var gridList = sealGrid.List;

				CombineAssertions(() =>
				{
					AssertEquals("Only a single record as the Header Container is not included", 1, gridList.Count);
					sealGrid.Focus();
					sealGrid.ListManager.Position = 0;
					AssertEquals("Value is the Seals value", "SEAL12345", sealGrid[sealGrid.CurrentCell]);
					AssertEquals("IsBroken default value is false", false, sealGrid[sealGrid.CurrentCell.RowNumber, sealGrid.CurrentCell.ColumnNumber + 1]);
					seal.IsBroken = true;
					AssertEquals("IsBroken value is true", true, sealGrid[sealGrid.CurrentCell.RowNumber, sealGrid.CurrentCell.ColumnNumber + 1]);
				});
			}
		}
	}
}
