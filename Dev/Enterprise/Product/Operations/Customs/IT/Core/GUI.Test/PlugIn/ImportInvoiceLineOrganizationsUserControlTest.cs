using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ImportInvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
{
	public void TestConsigneeAddressControlVisibility()
	{
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			var consigneeAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsigneeAddressControl");
			AssertEquals("ConsigneeAddressControl for IMP", false, consigneeAddressControl.Visible);
		}
	}
}
