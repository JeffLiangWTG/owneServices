using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class OrgSupplierPartTaxUserControlTest : TestCaseWithFactory
{
	public void TestPortTaxRatesDropEdit()
	{
		using (var control = new OrgSupplierPartTaxUserControl())
		{
			control.Show();

			var portTaxRatesDropEdit = (ZDropEdit)control.Controls.Find("ZG_PortTaxRatesDropEdit", true).First();
			AssertEquals(true, portTaxRatesDropEdit.Visible);
		}
	}
}
