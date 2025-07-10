using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI.Testing
{
	class NumbersUserControlTest : TestCaseWithFactory
	{
		public void TestExpiryDateControl()
		{
			using (var userControl = new NumbersUserControl())
			{
				var expiryDateEdit = userControl.FindSingle<ZDateEdit>("ExpiryDateEdit");
				AssertEquals("CE_ExpiryDate", true, expiryDateEdit.Visible);
			}
		}

		public void TestExpiryDateColumn()
		{
			using (var form = new ZForm())
			using (var userControl = new NumbersUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.Show();

				var numbersGrid = userControl.NumbersGrid;
				var expiryDateColumn = numbersGrid.GetColumnStyle(CusEntryNumber.Schema.CE_ExpiryDate);
				AssertEquals("CE_ExpiryDate column is available", false, expiryDateColumn.IsUnavailable);
				AssertEquals("CE_ExpiryDate column is not visible as default", false, expiryDateColumn.IsVisible);
			}
		}
	}
}
