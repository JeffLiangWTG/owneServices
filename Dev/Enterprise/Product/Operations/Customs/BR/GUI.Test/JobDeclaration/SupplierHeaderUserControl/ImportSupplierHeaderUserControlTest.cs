using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestImportLabels()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var fobAmountBoundCurrencyControl = control.Controls.Find("JZ_FOBAmountBoundCurrencyControl", true).FirstOrDefault();
					AssertEquals("FOB Value", ((ConvertToLocalCurrencyControl)fobAmountBoundCurrencyControl)?.CaptionResourceString?.Caption);

					var cifAmountBoundCurrencyControl = control.Controls.Find("JZ_CIFAmountBoundCurrencyControl", true).FirstOrDefault();
					AssertEquals("Customs Value", ((ConvertToLocalCurrencyControl)cifAmountBoundCurrencyControl)?.CaptionResourceString?.Caption);
				}
			}
		}

		public void TestColumnsGridVisibilityForImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var actualColumns = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.GetVisibleColumnMappingNames();
					var columns = new string[]
					{
						JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
						JobComInvoiceHeader.Schema.SupplierOrgPK,
						JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress,
						JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion,
						JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier,
						JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
						JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
						JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
						JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						JobComInvoiceHeader.Schema.JZ_PaymentDate,
						JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
						JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
						JobComInvoiceHeader.Schema.JZ_Remarks,
					};
					AssertContainsExactElementsInExactOrder("Columns visible should be", columns, actualColumns);
				}
			}
		}

		public void TestColumnsGridVisibilityForImportSiscomex()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var actualColumns = control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.GetVisibleColumnMappingNames();
					var columns = new string[]
					{
						JobComInvoiceHeader.Schema.JZ_InvoiceNumber,
						JobComInvoiceHeader.Schema.SupplierOrgPK,
						JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress,
						JobComInvoiceHeader.Schema.JZ_IncoTermPlace,
						JobComInvoiceHeader.Schema.JZ_InvoiceAmount,
						JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency,
						JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate,
						JobComInvoiceHeader.Schema.JZ_Calc_BalanceString,
						JobComInvoiceHeader.Schema.JZ_PaymentDate,
						JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill,
						JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice,
						JobComInvoiceHeader.Schema.JZ_Remarks,
					};
					AssertContainsExactElementsInExactOrder("Columns visible should be", columns, actualColumns);
				}
			}
		}

		public void TestImportSupplierHeaderUserControlFields()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl as ImportSupplierHeaderUserControl)
				{
					AssertType<ZCheckBox>(control.ExchangeRateDateOverrideCheckBox);
					AssertType<ZDateEdit>(control.ExchangeRateDateEdit);
					AssertType<ZTextBox>(control.ComplementTextBox);

					AssertEquals("Invoices.Lookups.SupplierList", control.JZ_OA_SupplierAddressControl.BindToOrgList);
				}
			}
		}

		public void TestDefaultColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var visibleColumnCount = control.InvoiceHeadersBoundGrid.Columns.Count(x => x.IsVisible);
					AssertEquals(14, visibleColumnCount);

					var invoiceHeadersBoundGrid = control.InvoiceHeadersBoundGrid;
					invoiceHeadersBoundGrid.ResetColumns();

					Assert("Should have JZ_InvoiceNumber column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_InvoiceNumber));
					Assert("Should have SupplierOrgPK column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.SupplierOrgPK));
					Assert("Should have JZ_OA_SupplierAddress column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_OA_SupplierAddress));
					Assert("Should have JZ_SupplierAuthorityVersion column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion));
					Assert("Should have JZ_SupplierAuthorityIdentifier column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier));
					Assert("Should have JZ_IncoTermPlace column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_IncoTermPlace));
					Assert("Should have JZ_RX_NKInvoice_Currency column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency));
					Assert("Should have JZ_InvoiceCurrExRate column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate));
					Assert("Should have JZ_Calc_BalanceString column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_Calc_BalanceString));
					Assert("Should have JZ_PaymentDate column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_PaymentDate));
					Assert("Should have JZ_CU_RelatedHouseBill column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill));
					Assert("Should have JZ_Calc_GroupInvoice column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_Calc_GroupInvoice));
					Assert("Should have JZ_Remarks column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_Remarks));
					Assert("Should have JZ_InvoiceAmount column in InvoiceHeadersBoundGrid", invoiceHeadersBoundGrid.Columns.Contains(JobComInvoiceHeader.Schema.JZ_InvoiceAmount));

					AssertEquals("JZ_RelatedIndicator should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_RelatedIndicator].IsVisible);
					AssertEquals("JZ_ValuationCode should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.JZ_ValuationCode].IsVisible);
					AssertEquals("ExchangeHedgeType should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.ExchangeHedgeType].IsVisible);
					AssertEquals("ExchangeHedgeFinancialInstitution should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.ExchangeHedgeFinancialInstitution].IsVisible);
					AssertEquals("ExchangeHedgeReason should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.ExchangeHedgeReason].IsVisible);
					AssertEquals("ExchangeHedgeValue should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.ExchangeHedgeValue].IsVisible);
					AssertEquals("ExchangeHedgeROFBACENNumber should be hidden", false, invoiceHeadersBoundGrid.Columns[JobComInvoiceHeader.Schema.ExchangeHedgeROFBACENNumber].IsVisible);
				}
			}
		}

		public void TestSupplierVersionColumnGridVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Import;
			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					AssertEquals("Supplier Identifier should be visible", true, control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier].IsVisible);
					AssertEquals("Supplier Version should be visible", true, control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion].IsVisible);

					declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportSiscomex;
					AssertNull("Supplier Identifier should be null", control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier]);
					AssertNull("Supplier Version should be null", control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion]);

					declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.Export;
					AssertNull("Supplier Identifier should be null", control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier]);
					AssertNull("Supplier Version should be null", control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion]);

					declaration.JE_MessageType = Common.BR.BRJobMessageTypeList.Codes.ImportLicense;
					AssertNull("Supplier Identifier should be null", control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityIdentifier]);
					AssertNull("Supplier Version should be null", control.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[JobComInvoiceHeader.Schema.JZ_SupplierAuthorityVersion]);
				}
			}
		}
	}
}
