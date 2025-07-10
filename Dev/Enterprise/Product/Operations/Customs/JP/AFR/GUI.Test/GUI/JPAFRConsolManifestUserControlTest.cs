using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI.Testing
{
	class JPAFRConsolManifestUserControlTest : TestCaseWithFactory
	{
		public void TestSelectAndShowBill()
		{
			var header = Factory.New<JPAFRHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			using (var form = new ZForm(header))
			{
				var userControl = new JPAFRConsolManifestUserControl(header);
				form.Controls.Add(userControl);
				form.Show();
				Application.DoEvents();

				var billsUserControl = GetControl<JPAFRConsolManifestUserControl, JPAFRBillsUserControl>(userControl, "BillsDetailsUserControl");
				var grid = GetControl<JPAFRBillsUserControl, ZGrid>(billsUserControl, "BillsGrid");

				AssertEquals("hidden to begin with", false, grid.Visible);

				userControl.SelectAndShowBill(bill1.PK);
				AssertEquals("should be shown now", true, grid.Visible);
				AssertEquals("should have selected bill1", bill1, grid.ListManager.GetCurrent());

				userControl.SelectAndShowBill(bill2.PK);
				AssertEquals("should have selected bill2", bill2, grid.ListManager.GetCurrent());
			}
		}

		T GetControl<P, T>(P parent, string name)
			where P : Control
			where T : Control
		{
			return (T)typeof(P).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(parent);
		}
	}
}
