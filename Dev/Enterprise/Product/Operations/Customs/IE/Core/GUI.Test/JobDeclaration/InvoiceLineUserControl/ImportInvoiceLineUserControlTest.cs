using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using AutoJobComInvoiceLine = Enterprise.Customs.IE.Business.Declaration.AutoJobComInvoiceLine;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineUserControl))]
	class ImportInvoiceLineUserControlTest : BaseInvoiceLineUserControlForVirtualPropertiesTest<ImportInvoiceLineUserControl>
	{
		public void TestDefaultColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();

				var lineGrid = control.CustomsInvoiceLinesBoundGrid;
				var styles = lineGrid.ColumnStyles;

				var columnStyle = lineGrid.GetColumnStyle(AutoJobComInvoiceLine.Schema.ZG_CountryOfDispatch);
				AssertEquals(23, styles.IndexOf(columnStyle));
				Assert("Country of Dispatch is visible", columnStyle.IsVisible);
				Assert("Country of Dispatch is available", !columnStyle.IsUnavailable);
			}
		}

		public void TestNetWeightInKGColumn_GroupName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.JobDeclaration = declaration;
				control.InitializeGridLayout();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("JI_CustomsQuantity Group Name", "Net Weight in KG", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity).GroupName.Caption);
				AssertEquals("JI_CustomsUnitQty Group Name", "Net Weight in KG", control.CustomsInvoiceLinesBoundGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty).GroupName.Caption);
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestSupplyChainActorTabPageCaptionUCC5()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using ( var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("SupplyChainActorTabPage", "[3/37] Add. Supply Chain Actors", control.FindSingle<ZTabPage>("SupplyChainActorTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestSupplyChainActorTabPageCaptionUCC6()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("SupplyChainActorTabPage", "Add. Supply Chain Actors", control.FindSingle<ZTabPage>("SupplyChainActorTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestPackagesTabPageCaptionUCC5()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("PackagesPivotTabPage", "[6/10] Packages", control.FindSingle<ZTabPage>("PackagesPivotTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestPackagesTabPageCaptionUCC6()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();
				AssertEquals("PackagesPivotTabPage", "[31] Packages", control.FindSingle<ZTabPage>("PackagesPivotTabPage").CaptionResourceString.Caption);
			}
		}

		public void TestPreviousDocumentsTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(
				declaration: declaration,
				tabPageName: "PreviousDocumentsTabPage",
				userControlName: "PreviousDocumentsUserControl",
				expectedCaption: "[2/1] Previous Documents",
				expectedUserControlType: typeof(ImportInvoiceLineLayoutPreviousDocumentsUserControl)
			);
		}

		public void TestPreviousDocumentsTabPageCaptionAndUserControlTypeUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc6 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true);
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(
				declaration: declaration,
				tabPageName: "PreviousDocumentsTabPage",
				userControlName: "PreviousDocumentsUserControl",
				expectedCaption: "Previous Documents",
				expectedUserControlType: typeof(ImportInvoiceLineLayoutPreviousDocumentsUserControl)
			);
		}

		public void TestAdditionalInfosTabPageCaptionAndUserControlType()
		{
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(ImportAdditionalInfosUserControlWithGrid));
		}

		public void TestSupportingDocumentsTabPageCaptionAndUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc5 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true);
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(
				declaration: declaration,
				tabPageName: "SupportingDocumentsTabPage",
				userControlName: "SupportingDocumentsUserControl",
				expectedCaption: "[2/3] Supporting Documents",
				expectedUserControlType: typeof(InvoiceLineLayoutSupportingDocumentsUserControl)
			);
		}

		public void TestSupportingDocumentsTabPageCaptionAndUserControlTypeUCC6()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			using var ucc6 = ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true);
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(
				declaration: declaration,
				tabPageName: "SupportingDocumentsTabPage",
				userControlName: "SupportingDocumentsUserControl",
				expectedCaption: "Supporting Documents",
				expectedUserControlType: typeof(InvoiceLineLayoutSupportingDocumentsUserControl)
			);
		}

		public void TestValuationIndicatorsTabPageCaptionAndUserControlType()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(declaration, "ValueIndicatorsTabPage", "invoiceLineValuationIndicatorsUserControl", "Valuation Indicators", typeof(InvoiceLineValuationIndicatorDropEditsUserControl));
			}
		}

		public void TestOrganizationsUserControlType()
		{
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var frm = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				frm.Controls.Add(control);
				control.JobDeclaration = declaration;
				frm.Show();

				AssertEquals(typeof(ImportInvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlTypeExposed());
			}
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC5Configuration(declaration, true))
			using (var frm = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				frm.Controls.Add(control);
				control.JobDeclaration = declaration;
				frm.Show();

				AssertEquals(typeof(PlugIn.ImportInvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlTypeExposed());
			}
		}

		public void TestFiscalReferencesTabPage()
		{
			DynamicControlTestHelper.AssertTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>("FiscalReferencesTabPage", "FiscalReferencesUserControl", "Fiscal References", typeof(InvoiceLineFiscalReferencesUserControl));

			declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
			EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(declaration, "FiscalReferencesTabPage", "FiscalReferencesUserControl", "[3/40] Fiscal References", typeof(InvoiceLineFiscalReferencesUserControl));
		}

		public void TestPackagesPivotTabPage()
		{
			AssertTabPageCaption("PackagesPivotTabPage", "Packages");
		}

		public void TestContainersTabPage()
		{
			AssertTabPageCaption("ContainersTabPage", "Containers");
		}

		public void TestExciseTaxesTabPageCaption()
		{
			AssertTabPageCaption("ExciseTaxesTabPage", "Excise Taxes");
		}

		public void TestExciseTaxesTabPageVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				CombineAssertions(() =>
				{
					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V2;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
					AssertEquals("ExciseTaxesTabPage should be visible for UCC6", true, invoiceLineControl.ExciseTaxesTabPage.TabVisible);

					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;

					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.Interfaced;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					AssertEquals("ExciseTaxesTabPage should be visible for UCC6 Application Code Interfaced", true, invoiceLineControl.ExciseTaxesTabPage.TabVisible);

					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;

					declaration.JE_ApplicationCode = ImportDeclarationApplicationCodeList.Codes.V1;
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					AssertEquals("ExciseTaxesTabPage should be visible for UCC5", true, invoiceLineControl.ExciseTaxesTabPage.TabVisible);

					AssertNotNull("CusLineTariffDetailsUserControl", invoiceLineControl.ExciseTaxesTabPage.Controls.Find("CusLineTariffDetailsUserControl", true));
				});
			}
		}

		protected override ZBool DefaultDynamicLayoutApplied => ZBool.True;

		protected override ZString DefaultUniversalTariffType => Universal.Constants.TariffTypes.Import;

		static void AssertTabPageCaption(string tabPageName, string expectedCaption)
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(tabPageName);
				tabPage.Show();
				AssertEquals(expectedCaption, tabPage.CaptionResourceString.Caption);
			}
		}

		public void TestTabPagesOrder()
		{
			using (var form = new ZForm())
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				control.JobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				form.Show();
				var tabPages = control.LineDetailTabControl.TabPages.Cast<ZTabPage>().ToArray();

				CombineAssertions(() =>
				{
					AssertEquals("NewLineDetailsTabPage should be the first tab page.", "NewLineDetailsTabPage", tabPages[0].Name);
					AssertEquals("ExciseTaxesTabPage should be the second tab page.", "ExciseTaxesTabPage", tabPages[1].Name);
					AssertEquals("LineChargesTabPage should be the third tab page.", "LineChargesTabPage", tabPages[2].Name);
					AssertEquals("LineDetailsTabPage should be the fourth tab page.", "LineDetailsTabPage", tabPages[3].Name);
					AssertEquals("ContainersTabPage should be the fifth tab page.", "ContainersTabPage", tabPages[4].Name);
					AssertEquals("InvoiceLinePaymentTabPage should be the sixth tab page.", "InvoiceLinePaymentTabPage", tabPages[5].Name);
					AssertEquals("AuthorisationsTabPage should be the seventh tab page.", "AuthorisationsTabPage", tabPages[6].Name);
					AssertEquals("FiscalReferencesTabPage should be the eight tab page.", "FiscalReferencesTabPage", tabPages[7].Name);
					AssertEquals("SupportingDocumentsTabPage should be the nineth tab page.", "SupportingDocumentsTabPage", tabPages[8].Name);
					AssertEquals("AdditionalInfosTabPage should be the tenth tab page.", "AdditionalInfosTabPage", tabPages[9].Name);
					AssertEquals("PreviousDocumentsTabPage should be the eleventh tab page.", "PreviousDocumentsTabPage", tabPages[10].Name);
					AssertEquals("PackagesPivotTabPage should be the twelfth tab page.", "PackagesPivotTabPage", tabPages[11].Name);
					AssertEquals("TaxTabPage should be the thirteenth tab page.", "TaxTabPage", tabPages[12].Name);
					AssertEquals("VehicleTabPage should be the fourteenth tab page.", "VehicleTabPage", tabPages[13].Name);
					AssertEquals("OrganizationsTabPage should be the fifteenth tab page.", "OrganizationsTabPage", tabPages[14].Name);
					AssertEquals("ValueIndicatorsTabPage should be the sixteenth tab page.", "ValueIndicatorsTabPage", tabPages[15].Name);
					AssertEquals("SupplyChainActorTabPage should be the seventeenth tab page.", "SupplyChainActorTabPage", tabPages[16].Name);
					AssertEquals("CustomFieldsTabPage should be the eighteenth tab page.", "CustomFieldsTabPage", tabPages[17].Name);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		}
		JobDeclaration declaration;

		class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
		{
			public Type GetOrganizationsUserControlTypeExposed() => base.GetOrganizationsUserControlType();
		}
	}
}
