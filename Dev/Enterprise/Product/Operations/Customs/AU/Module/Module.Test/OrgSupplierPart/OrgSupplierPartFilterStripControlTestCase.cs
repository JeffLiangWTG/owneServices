using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module.Testing
{
	sealed class OrgSupplierPartFilterStripControlTestCase : TestCaseWithFactory
	{
		public void TestShowTariffsOnGrid()
		{
			var products = new OrgSupplierPartCollection(Factory);
			var aProduct = products.AddNew();
			using (var form = new ZForm())
			using (var module = new OrgSupplierPartModule())
			using (var stripControl = new OrgSupplierPartFilterStripControl(products, new OrgSupplierPartFilterStripBusinessObject()))
			{
				form.Controls.Add(stripControl);
				form.Show();
				Assert("AU should be showing Tariff column", stripControl.FilteredGrid.Columns.Any(l => l.ColumnName == "Tariffs"));
			}
		}

		public void TestShowAddInfoOnGrid()
		{
			var products = new OrgSupplierPartCollection(Factory);
			var aProduct = products.AddNew();
			using (var form = new ZForm())
			using (var module = new OrgSupplierPartModule())
			using (var stripControl = new OrgSupplierPartFilterStripControl(products, new OrgSupplierPartFilterStripBusinessObject()))
			{
				form.Controls.Add(stripControl);
				form.Show();
				Assert("AU should be showing AddInfo column", stripControl.FilteredGrid.Columns.Any(l => l.ColumnName == "AddInfoLine"));
			}
		}
	}
}
