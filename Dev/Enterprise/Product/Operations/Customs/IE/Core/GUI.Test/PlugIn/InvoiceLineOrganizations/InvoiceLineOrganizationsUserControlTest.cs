using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class InvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
	{
		public void TestOrganisationControls()
		{
			using (var control = new InvoiceLineOrganizationsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("SupplyChainActorReferencesUserControl should be shown.", true, control.FindSingle<SupplyChainActorReferencesUserControl>("SupplyChainActorReferencesUserControl").Visible);
				});
			}
		}
	}
}
