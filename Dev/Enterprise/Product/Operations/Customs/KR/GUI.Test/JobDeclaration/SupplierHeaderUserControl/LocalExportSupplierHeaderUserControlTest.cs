using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(LocalExportSupplierHeaderUserControl))]
	sealed class LocalExportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<LocalExportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new LocalExportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", Constants.ColumnLayoutContextLocalExport, control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvHeaders()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(LocalExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var grid = control.InvoiceHeadersBoundGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_OH_Manufacturer));
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
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.SupportingDocumentCode));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.SupportingDocumentReferenceNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InboundDate));

					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_IncoTermPlace)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_CU_RelatedHouseBill)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_ChargesExcludedFromITOT)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ExporterBankName)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ExporterBankAccountNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ExporterBankSWIFTCode)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceCurrLandedCostExRate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_LetterOfCreditNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_LetterOfCreditDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentNo)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentAmount)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentExRate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_InvoiceDisplaySequence)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Volume)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_VolumeUQ)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Remarks)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_GroupInvoice)).IsVisible);

					AssertEquals(null, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFAmount)));
					AssertEquals(null, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFCurrency)));

					AssertEquals("Packages", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_NoOfPacks)).GroupName, grid.GetColumnStyle(nameof(JobComInvoiceHeader.NoOfPacksPackType)).GroupName);
				}
			}
		}

		public void TestInvHeaders_BottomArea()
		{
			using (var control = new LocalExportSupplierHeaderUserControl())
			{
				AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JZ_CIFAmountBoundCurrencyControl").Visible);
				AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JZ_Calc_TNIBoundInvoiceCurrencyControl").Visible);
			}
		}

		public void TestInvoiceHeader_InvoiceChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(LocalExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

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
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
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
				AssertEquals(typeof(LocalExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

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
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
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
				AssertEquals(typeof(LocalExportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

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
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
		}
		JobDeclaration declaration;

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit", "IncoTermExplainButton" });
	}
}
