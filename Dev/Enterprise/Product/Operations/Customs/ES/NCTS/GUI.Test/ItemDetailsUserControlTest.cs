using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class ItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestIsVehiclesCheckBoxVisibility()
		{
			using (var form = new ZForm())
			using (var control = new ItemDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var isVehiclesCheckBox = (ZCheckBox)control.Controls.Find("IsVehiclesCheckBox", true).FirstOrDefault();
				AssertEquals("IsVehicles visible", true, isVehiclesCheckBox.Visible);
			}
		}

		[RequiresSTA]
		public void TestIsVehiclesCheckBoxIsAlignedWithItemNumberTextBox()
		{
			using (var form = new ZForm())
			using (var control = new ItemDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var isVehiclesCheckBox = (ZCheckBox)control.Controls.Find("IsVehiclesCheckBox", true).FirstOrDefault();
				var itemNumberTextBox = (ZTextBox)control.Controls.Find("ItemNumberTextBox", true).FirstOrDefault();
				AssertEquals("IsVehiclesCheckBox is aligned with ItemNumberTextBox on the Y axis", itemNumberTextBox.Location.Y, isVehiclesCheckBox.Location.Y);
			}
		}

		public void TestCustomsThirdQtyDropEdit_Visibility()
		{
			using (var control = new ItemDetailsUserControl())
			{
				var customsThirdQtyDropEdit = control.FindSingle<ZCalcDropEdit>("CustomsThirdQtyDropEdit");
				AssertEquals("Should be visible for ES", true, customsThirdQtyDropEdit.Visible);
			}
		}
	}
}
