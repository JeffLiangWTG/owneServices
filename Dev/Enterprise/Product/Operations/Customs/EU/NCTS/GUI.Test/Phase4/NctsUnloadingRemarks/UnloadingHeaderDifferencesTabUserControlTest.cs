using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class UnloadingHeaderDifferencesTabUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNCTSHeaderSealsAreBoundToCusCodeDataCollection()
		{
			var arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus = NctsTransitStatusList.Codes.UnloadingPermissionGranted;
			arrivalHeader.DepartureHeaderContainers.AddNew().Seal1 = "CONT12345";
			arrivalHeader.DepartureHeaderContainers.AddNew().Seal1 = "CONT12346";
			arrivalHeader.Seals.AddNew().CY_Data = "SEAL12345";
			arrivalHeader.ArrivalMovementHeader.Seals.AddNew().CY_Data = "SEAL2000";

			CombineAssertions(() =>
			{
				using (var form = new NctsMovementForm(arrivalHeader))
				{
					form.Show();
					var sealGrid = (ZGrid)form.Controls.Find("SealsGrid", true).Single();
					var gridList = sealGrid.List;
					AssertEquals("Only a single record as the Header Container is not included", 1, gridList.Count);
					sealGrid.Focus();
					sealGrid.ListManager.Position = 0;

					AssertEquals("Value is the Seals value", "SEAL2000", ((Seal)sealGrid.List[0]).CY_Data);
				}
			});
		}

		public void TestControlsCharacterCasing()
		{
			using (var control = new UnloadingHeaderDifferencesTabUserControl())
			{
				CombineAssertions(() =>
				{
					var otherNotesTextBox = control.FindSingle<ZTextBox>("OtherNotesTextBox");
					AssertEquals("OtherNotesTextBox", System.Windows.Forms.CharacterCasing.Normal, otherNotesTextBox.CharacterCasing);

					var vehicleIdChangedValueTextBox = control.FindSingle<ZTextBox>("VehicleIdChangedValueTextBox");
					AssertEquals("VehicleIdChangedValueTextBox", System.Windows.Forms.CharacterCasing.Normal, vehicleIdChangedValueTextBox.CharacterCasing);
				});
			}
		}
	}
}
