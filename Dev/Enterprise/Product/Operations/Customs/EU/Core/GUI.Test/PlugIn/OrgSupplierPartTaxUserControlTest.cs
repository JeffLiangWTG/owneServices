using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class OrgSupplierPartTaxUserControlTest : TestCaseWithFactory
	{
		public void TestOrgSupplierPartTaxUserControlImplementsISupportingInfoUserControls()
		{
			using (var orgSupplierPartTaxUserControl = new OrgSupplierPartTaxUserControl())
			{
				var supplierPartSupportingInfoTaxUserControl = orgSupplierPartTaxUserControl as ISupportingInfoUserControls;
				AssertNotNull("OrgSupplierPartTaxUserControl must implements ISupportingInfoUserControls", supplierPartSupportingInfoTaxUserControl);

				AssertEquals("GridBindingMember", "FilteredInvoiceLines", supplierPartSupportingInfoTaxUserControl.GridBindingMember);

				var taxGrid = orgSupplierPartTaxUserControl.Controls.Find("TaxGrid", true).First() as ZGrid;
				AssertEquals("Grid", taxGrid, supplierPartSupportingInfoTaxUserControl.Grid);
			}
		}
	}
}
