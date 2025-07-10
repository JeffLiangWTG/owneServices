using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public class InvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
	{
		public void TestOrganisationControls()
		{
			using (var control = new InvoiceLineOrganizationsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("ConsignorAddressControl", true, control.FindSingleOrDefault<ZAddressControl>("ConsignorAddressControl")?.Visible);
					AssertEquals("ConsigneeAddressControl", true, control.FindSingleOrDefault<ZAddressControl>("ConsigneeAddressControl")?.Visible);
					AssertEquals("SupplyChainActorReferencesUserControl", true, control.FindSingleOrDefault<SupplyChainActorReferencesUserControl>("SupplyChainActorReferencesUserControl")?.Visible);
				});
			}
		}

		public void TestControls_AdditionalSupplyChainActorGroupBox()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();

			using (var form = new ZForm(declaration))
			using (var control = new InvoiceLineOrganizationsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				System.Windows.Forms.Application.DoEvents();

				AssertNotNull("SupplyChainActorReferencesUserControl", control.FindSingleOrDefault<SupplyChainActorReferencesUserControl>(c => c.Name == "SupplyChainActorReferencesUserControl"));
			}
		}
	}
}
