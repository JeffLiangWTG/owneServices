using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.Business.Testing;
using Enterprise.Customs.BR.Registry;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class DuimpTaxRegimeUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new DuimpTaxRegimeUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestAddLegalBasisButton_Click()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "87654321";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Angola;

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = ((ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);
					invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.TaxTreatmentTab;
					var duimpTaxRegimeUserControl = invoiceLineUserControl.DuimpTaxRegimeUserControl;

					duimpTaxRegimeUserControl.AddLegalBasisButton.PerformClick();
					AssertEquals("No Legal Basis selected to add to grid, please select a Legal Basis.", UnitTestUserNotification.Instance.LastMessage.Text);

					invoiceLine.DuimpLegalBase = "P03";
					duimpTaxRegimeUserControl.AddLegalBasisButton.PerformClick();
					AssertEquals(1, invoiceLine.DuimpTaxRegimes.Count);

					invoiceLine.DuimpLegalBase = "P03";
					duimpTaxRegimeUserControl.AddLegalBasisButton.PerformClick();
					AssertEquals("Legal Basis already added, please select other Legal Basis to be added.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestLegalBaseGrid_RowsDeletingShouldNotBeDeleted()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "12345678";
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.SouthAfrica;
			invoiceLine.DuimpTaxRegimes.Rebuild();
			AssertEquals(1, invoiceLine.DuimpTaxRegimes.Count);

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = ((ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);
				invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.TaxTreatmentTab;
				var duimpTaxRegimeUserControl = invoiceLineUserControl.DuimpTaxRegimeUserControl;

				var legalGrid = duimpTaxRegimeUserControl.LegalGrid;
				legalGrid.Select(0);
				legalGrid.DeleteMenuItem.PerformClick();
				AssertEquals("The legal base 'P01' cannot be deleted because is mandatory.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLegalBaseGrid_RowsDeletingShouldBeDeleted()
		{
			ReferenceTestDataHelper.CreateTTRefCusProfileAndQuestions(Factory);
			ReferenceTestDataHelper.CreateDuimpLegalBaseCodes(Factory);

			using (BRCustomsDataRegistry.Instance.ShouldLoadTariffAttributesOfTestEnvironment.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
				var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "87654321";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Angola;

				using (var form = new JobDeclarationForm(declaration))
				{
					form.Show();
					form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var invoiceLineUserControl = ((ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl);
					invoiceLineUserControl.CustomsInvoiceLinesBoundGrid.Select(0);
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.TaxTreatmentTab;
					var duimpTaxRegimeUserControl = invoiceLineUserControl.DuimpTaxRegimeUserControl;

					invoiceLine.DuimpLegalBase = "P03";
					duimpTaxRegimeUserControl.AddLegalBasisButton.PerformClick();
					AssertEquals(1, invoiceLine.DuimpTaxRegimes.Count);

					var legalGrid = duimpTaxRegimeUserControl.LegalGrid;
					legalGrid.Select(0);
					legalGrid.DeleteMenuItem.PerformClick();
					AssertEquals(0, invoiceLine.DuimpTaxRegimes.Count);
				}
			}
		}

		public void TestCaptions()
		{
			using (var control = new DuimpTaxRegimeUserControl())
			{
				AssertEquals("LegalBasesGroupBox caption must be Legal Basis", "Legal Basis", control.LegalBasisGroupBox.CaptionResourceString.Caption);
				AssertEquals("AddLegalBasisButton caption must be Add Legal Basis", "Add Legal Basis", control.AddLegalBasisButton.CaptionResourceString.Caption);
			}
		}

		public void TestComponents()
		{
			using (var control = new DuimpTaxRegimeUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<KSplitContainer>("MainSplitContainer must be KSplitContainer", control.MainSplitContainer);
					AssertType<ZGroupBox>("LegalBasesGroupBox must be ZGroupBox", control.LegalBasisGroupBox);
					AssertType<ZGrid>("LegalGrid must be ZGrid", control.LegalGrid);
					AssertType<ZDropEdit>("LegalBaseDropEdit must be ZDropEdit", control.LegalBasisDropEdit);
					AssertType<ZButton>("AddLegalBasisButton must be ZButton", control.AddLegalBasisButton);
					AssertType<AttributesUserControl>("TaxRegimeAttributesUserControl must be AttributesUserControl", control.TaxRegimeAttributesUserControl);
				});
			}
		}
	}
}
