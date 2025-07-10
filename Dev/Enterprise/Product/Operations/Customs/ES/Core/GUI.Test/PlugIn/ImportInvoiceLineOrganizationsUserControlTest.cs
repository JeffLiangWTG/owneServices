using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class ImportInvoiceLineOrganizationsUserControlTest : TestCaseWithFactory
{
	public void TestConsigneeAddressControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var consigneeAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsigneeAddressControl");
			AssertEquals("ConsigneeAddressControl for IMP and UCC6", expected: false, consigneeAddressControl.Visible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var consigneeAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsigneeAddressControl");
			AssertEquals("ConsigneeAddressControl for IMP and not UCC6", expected: false, consigneeAddressControl.Visible);
		}
	}

	public void TestConsignorControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var consignorAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsignorAddressControl");
			AssertEquals("ConsignorAddressControl for IMP and UCC6", expected: true, consignorAddressControl.Visible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var consignorAddressControl = control.FindSingleOrDefault<ZAddressControl>("ConsignorAddressControl");
			AssertEquals("ConsignorAddressControl for IMP and not UCC6", expected: false, consignorAddressControl.Visible);
		}
	}

	public void TestBuyerControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var buyerDocAddressControl = control.FindSingleOrDefault<ZDocAddressControl>("BuyerDocAddressControl");
			AssertEquals("BuyerDocAddressControl for IMP and UCC6", expected: true, buyerDocAddressControl.Visible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var buyerDocAddressControl = control.FindSingleOrDefault<ZDocAddressControl>("BuyerDocAddressControl");
			AssertEquals("BuyerDocAddressControl for IMP and not UCC6", expected: false, buyerDocAddressControl.Visible);
		}
	}

	public void TestSellerControlVisibility()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var sellerDocAddressControl = control.FindSingleOrDefault<ZDocAddressControl>("SellerDocAddressControl");
			AssertEquals("SellerDocAddressControl for IMP and UCC6", expected: true, sellerDocAddressControl.Visible);
		}

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineOrganizationsUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			form.Show();
			var sellerDocAddressControl = control.FindSingleOrDefault<ZDocAddressControl>("SellerDocAddressControl");
			AssertEquals("SellerDocAddressControl for IMP and not UCC6", expected: false, sellerDocAddressControl.Visible);
		}
	}
}
