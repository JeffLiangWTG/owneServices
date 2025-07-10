using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ExportSupplierHeaderUserControl))]
	sealed class ExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Export), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvHeaders()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var grid = control.InvoiceHeadersBoundGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.ManufacturerOrgPK));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_OA_ManufacturerAddress));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_OH_Buyer));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_DRWApplicantType));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_IncoTerm));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceAmount));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_RX_NKInvoice_Currency));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceCurrExRate));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_Calc_BalanceString));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.InvoiceLineTotal));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_Weight));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_WeightUQ));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_NetWeight));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_NetWeightUQ));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_NoOfPacks));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.NoOfPacksPackType));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_PaymentTerms));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_LetterOfCreditNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_RN_NKDefaultOrigin));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.CertificateOfOriginIssueStatus));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_COOLabelLocation));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_Remarks));

					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceCurrLandedCostExRate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_LetterOfCreditDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentNo)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentAmount)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentExRate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceDisplaySequence)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Volume)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_VolumeUQ)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ImportCargoManagementNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_IncoTermPlace)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentDate)).IsVisible);

					AssertEquals("Packages", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_NoOfPacks)).GroupName, grid.GetColumnStyle(nameof(JobComInvoiceHeader.NoOfPacksPackType)).GroupName);
					AssertNull(grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OH_Supplier)));
					AssertNull(grid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)));
				}
			}
		}

		public void TestInvoiceHeader_InvoiceChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var grid = control.InvoiceChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsIncludedInITOT);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_DistributeBy);

					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_Percentage).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_PrepaidCollect).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
				}
			}
		}

		public void TestInvoiceHeader_ApportionedChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					control.ChargesTabControl.SelectedTab = control.ApportionedTabPage;
					var grid = control.ApportionedChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsIncludedInITOT);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount);

					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_FullOrPartialApportionment).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
				}
			}
		}

		public void TestGroupCharges_AllInvoices()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var grid = control.BaseGroupChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Percentage);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);
					AssertEquals(grid.Columns[index++].ColumnName, "J7_Calc_IsIncludedInITOT");
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_DistributeBy);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_FullOrPartialApportionment);

					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.ChargeCodeDescription).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_PrepaidCollect).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
				}
			}
		}

		public void TestInvHeaders_BottomArea()
		{
			using (var control = new ExportSupplierHeaderUserControl())
			{
				AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JZ_CIFAmountBoundCurrencyControl").Visible);
				AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JZ_Calc_TNIBoundInvoiceCurrencyControl").Visible);
			}
		}

		public void TestInvHeaders_SEDDetails()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (ExportSupplierHeaderUserControl control = (ExportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl)
				{
					control.InvoiceTabControl.SelectedTab = control.FindSingle<ZTabPage>("SEDDetailsTabPage");

					var certificateOfOriginGroupBox = control.FindSingle<ZGroupBox>("CertificateOfOriginGroupBox");
					AssertEquals(certificateOfOriginGroupBox.Visible, true);
					var certificateOfOriginDynamicLayoutPanel = control.FindSingle<DynamicLayoutPanel>("CertificateOfOriginDynamicLayoutPanel");
					AssertEquals(certificateOfOriginDynamicLayoutPanel.Visible, true);

					AssertEquals(true, certificateOfOriginDynamicLayoutPanel.Controls.Contains(control.FindSingle<ZCodeFindBox>("GoodsOriginCodeFindBox")));
					AssertEquals(true, certificateOfOriginDynamicLayoutPanel.Controls.Contains(control.FindSingle<ZDropEdit>("COOIssueStatusDropEdit")));
					AssertEquals(true, certificateOfOriginDynamicLayoutPanel.Controls.Contains(control.FindSingle<ZDropEdit>("COODeterminationRuleDropEdit")));
					AssertEquals(true, certificateOfOriginDynamicLayoutPanel.Controls.Contains(control.FindSingle<ZDropEdit>("COOLabelLocationDropEdit")));

					var manufacturerGroupBox = control.FindSingle<ZGroupBox>("ManufacturerGroupBox");
					AssertEquals(manufacturerGroupBox.Visible, true);
					var manufacturerDynamicLayoutPanel = control.FindSingle<DynamicLayoutPanel>("ManufacturerDynamicLayoutPanel");
					AssertEquals(manufacturerDynamicLayoutPanel.Visible, true);

					AssertEquals(true, manufacturerDynamicLayoutPanel.Controls.Contains(control.FindSingle<ZAddressControl>("ManufacturerAddressControl")));
					var manufacturerUnipassIDTextBox = control.FindSingle<ZTextBox>("ManufacturerUnipassIDTextBox");
					AssertEquals(true, manufacturerDynamicLayoutPanel.Controls.Contains(manufacturerUnipassIDTextBox));
					AssertEquals(true, manufacturerUnipassIDTextBox.ReadOnly);
					var manufacturerIPCCodeFindBox = control.FindSingle<ZCodeFindBox>("ManufacturerIPCCodeFindBox");
					AssertEquals(true, manufacturerDynamicLayoutPanel.Controls.Contains(manufacturerIPCCodeFindBox));
					AssertEquals(true, manufacturerIPCCodeFindBox.ReadOnly);

					var importerGroupBox = control.FindSingle<ZGroupBox>("ImporterGroupBox");
					AssertEquals(importerGroupBox.Visible, true);
					var importerDynamicLayoutPanel = control.FindSingle<DynamicLayoutPanel>("ImporterDynamicLayoutPanel");
					AssertEquals(importerDynamicLayoutPanel.Visible, true);

					AssertEquals(true, importerDynamicLayoutPanel.Controls.Contains(control.FindSingle<ZOrganisationFindBox>("BuyerGuidFindBox")));
					var buyerIDTextBox = control.FindSingle<ZTextBox>("BuyerIDTextBox");
					AssertEquals(true, importerDynamicLayoutPanel.Controls.Contains(buyerIDTextBox));
					AssertEquals(true, buyerIDTextBox.ReadOnly);
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		}
		JobDeclaration declaration;

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit", "IncoTermExplainButton" });
	}
}
