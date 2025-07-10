using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing
{
	class ExportInvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
	{
		public void TestOrganisationControls()
		{
			using (var control = new ExportInvoiceLineOrganizationsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("ConsignorAddressControl", true, control.FindSingleOrDefault<ZAddressControl>("ConsignorAddressControl")?.Visible);
					AssertEquals("ConsigneeAddressControl", false, control.FindSingleOrDefault<ZAddressControl>("ConsigneeAddressControl")?.Visible);
					AssertEquals("SupplyChainActorReferencesUserControl", true, control.FindSingleOrDefault<SupplyChainActorReferencesUserControl>("SupplyChainActorReferencesUserControl")?.Visible);
				});
			}
		}
	}
}
