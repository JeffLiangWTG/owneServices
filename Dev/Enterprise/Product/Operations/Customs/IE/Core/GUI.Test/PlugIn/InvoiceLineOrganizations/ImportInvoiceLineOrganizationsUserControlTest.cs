using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.PlugIn.Testing
{
	public class ImportInvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
	{
		public void TestUCC5AndImportOrganisationControls()
		{
			var declaration = Factory.New<Business.Declaration.JobDeclaration>();
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineOrganizationsUserControl())
			{
				form.Controls.Add(control);
				form.SetDataBinding(declaration, null);
				form.Show();

				CombineAssertions(() =>
				{
					var consignorAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsignorAddressControl");
					AssertEquals("ConsignorAddressControl", true, consignorAddressControl?.Visible);
					AssertEquals("Caption", "Exporter", consignorAddressControl.CaptionResourceString.Caption);

					var consigneeAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsigneeAddressControl");
					AssertEquals("ConsigneeAddressControl", false, consigneeAddressControl?.Visible);

					var sellerDocAddressControl = control.FindSingleOrDefault<ZDocAddressControl>("SellerDocAddressControl");
					AssertEquals("SellerDocAddressControl", true, sellerDocAddressControl?.Visible);

					var buyerDocAddressControl = control.FindSingleOrDefault<ZDocAddressControl>("BuyerDocAddressControl");
					AssertEquals("BuyerDocAddressControl", true, buyerDocAddressControl?.Visible);

					var supplyChainActorReferencesUserControl = control.FindSingleOrDefault<SupplyChainActorReferencesUserControl>("SupplyChainActorReferencesUserControl");
					AssertEquals("SupplyChainActorReferencesUserControl", false, supplyChainActorReferencesUserControl?.Visible);
				});
			}
		}
	}
}
