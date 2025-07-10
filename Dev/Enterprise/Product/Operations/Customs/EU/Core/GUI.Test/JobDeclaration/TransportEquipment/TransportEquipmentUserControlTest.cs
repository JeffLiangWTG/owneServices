using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI.Testing
{
	class TransportEquipmentUserControlTest : TestCaseWithFactory
	{
		public void TestUserControls()
		{
			using (var control = new TransportEquipmentUserControl())
			{
				CombineAssertions(() =>
				{
					var equipmentsGroupBox = control.EquipmentsGroupBox;
					AssertEquals("EquipmentsGroupBox Caption", "Equipments", equipmentsGroupBox.CaptionResourceString.Caption);

					var equipmentsGrid = control.EquipmentsGrid;
					AssertNotNull("Column Transport Equipment ID", equipmentsGrid.GetColumnStyle(CusEquipment.Schema.CEQ_IdentificationNumber));

					var sealsGroupBox = control.SealsGroupBox;
					AssertEquals("SealsGroupBox Caption", "Seals", sealsGroupBox.CaptionResourceString.Caption);

					var sealsGrid = control.SealsGrid;
					var sealNumberColumn = sealsGrid.GetColumnStyle(CusSeal.Schema.BK_SealNumber);
					AssertEquals("CharacterCasing", CharacterCasing.Upper, sealNumberColumn.CharacterCasing);
					AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120), sealNumberColumn.Width);
				});
			}
		}
	}
}
