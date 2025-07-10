using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(ImportSupplierHeaderUserControl))]
	sealed class ImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<ImportSupplierHeaderUserControl, JobDeclaration>
	{
		public void TestColumnLayoutContext()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("Column layout context should have been set", nameof(Customs.GUI.DeclarationType.Import), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestGridId()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutT+oYGAheR1633dT3oeP+lw==", control.InvoiceHeadersBoundGrid.GridId);
			}
		}

		public void TestInvHeaders()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var grid = control.InvoiceHeadersBoundGrid;
					var index = 0;

					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_OH_Supplier));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.ShipperOrgPK));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_OA_ShipperAddress));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_IncoTerm));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceAmount));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_RX_NKInvoice_Currency));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_InvoiceCurrExRate));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_Calc_BalanceString));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.InvoiceLineTotal));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_CU_RelatedHouseBill));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_ImportCargoManagementNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_ValuationCode));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_BlanketValuationDeclarationNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_Weight));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_WeightUQ));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_NetWeight));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_NetWeightUQ));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_PaymentTerms));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_COOStatus));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_ValuationDecAttachCode));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_RN_NKDefaultOrigin));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.CriteriaForDeterminingCountryOfOrigin));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_COOLabelLocation));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_COOLabelType));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_COOExemptionReason));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceHeader.JZ_Remarks));

					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_IncoTermPlace)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_PaymentDate)).IsVisible);
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
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OnlineTradeType)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OA_DistributorAddress)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OA_SellerAddress)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OH_SellingAgent)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginNo)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginIssuingCountry)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginIssueDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginCriteriaCode)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginStatus)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginAgencyName)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginAreaName)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.CertificateOfOriginPersonName)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.PurchaseOrderNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.PurchaseOrderDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.ContractNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.ContractDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ProvPricingYN)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ProvAdditionalRate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ImpContractExpiryDate)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ProvAdditionalAmount)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_EstimatedDateOfFinalPrice)).IsVisible);

					AssertEquals("Supplier Name", grid.GetColumnStyle(nameof(JobComInvoiceHeader.SupplierName)).CaptionResourceString.Caption);
					AssertEquals("Customs Value", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)).CaptionResourceString.Caption);
					AssertEquals("Customs Value", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBAmount)).GroupName.Caption);
					AssertEquals("Currency", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)).CaptionResourceString.Caption);
					AssertEquals("Customs Value", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_FOBCurrency)).GroupName.Caption);
				}
			}
		}

		public void TestAddOrRemoveColumns()
		{
			using (var userControl = new ImportSupplierHeaderUserControl())
			{
				userControl.InitializeGridLayout();
				var grid = userControl.InvoiceHeadersBoundGrid;
				var valuationCodeColumn = grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_ValuationCode));
				AssertNotNull("ValuationCode Column exists", valuationCodeColumn);
				AssertEquals("ValuationCode Column is visible", true, valuationCodeColumn.IsVisible);

				var manufacturerOrgPKColumn = grid.GetColumnStyle(nameof(JobComInvoiceHeader.ManufacturerOrgPK));
				AssertNotNull("ManufacturerOrgPK Column exists", manufacturerOrgPKColumn);
				AssertEquals("ManufacturerOrgPK Column is addible", false, manufacturerOrgPKColumn.IsVisible);

				var manufacturerAddressColumn = grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OA_ManufacturerAddress));
				AssertNotNull("ManufacturerAddress Column exists", manufacturerAddressColumn);
				AssertEquals("ManufacturerAddress Column is addible", false, manufacturerAddressColumn.IsVisible);

				AssertNull("Importer Column not exists", grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_OH_Buyer)));

				AssertNull(grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFAmount)));
				AssertNull(grid.GetColumnStyle(nameof(JobComInvoiceHeader.JZ_Calc_CIFCurrency)));
			}
		}

		public void TestInvoiceHeader_InvoiceChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

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

					AssertNull(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
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
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

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

					AssertNull(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
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
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

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

					AssertNull(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
				}
			}
		}

		public void TestInvoiceHeader_MailItemsGrid()
		{
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._26;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = (ImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl)
				{
					control.InvoiceTabControl.SelectedTab = control.MailItemsTabPage;
					var grid = control.MailItemsGrid;
					var index = 0;
					AssertEquals(grid.Columns[index].ColumnStyle.GetType(), typeof(ZTextBoxColumnStyle));
					AssertEquals(grid.Columns[index++].ColumnName, Parcel.Schema.CSI_ReferenceNumber);
					AssertEquals(grid.Columns[index].ColumnStyle.GetType(), typeof(ZTextBoxColumnStyle));
					AssertEquals(grid.Columns[index++].ColumnName, Parcel.Schema.CSI_ReferenceNumber2);
					AssertEquals(grid.Columns[index].ColumnStyle.GetType(), typeof(ZDropEditColumnStyle));
					AssertEquals(grid.Columns[index++].ColumnName, Parcel.Schema.CSI_Code);
				}
			}
		}

		public void TestReorderTabPages()
		{
			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._26;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var tabControl = control.InvoiceTabControl;
					var index = 0;
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["ComInvoiceDetailsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["CustomsDetailsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["COAndFTATabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["MailItemsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["ValuationDeclarationTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["CustomFieldsTabPage"]);

					AssertNotNull(tabControl.TabPages["MailItemsTabPage"]);
				}
			}

			declaration.JE_ProcedureType = DeclarationProcedureTypeCodeList.Codes._12;
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var control = brokerageControl.SupplierHeaderUserControl)
				{
					var tabControl = control.InvoiceTabControl;
					var index = 0;
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["ComInvoiceDetailsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["CustomsDetailsTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["COAndFTATabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["ValuationDeclarationTabPage"]);
					AssertEquals(tabControl.TabPages[index++], tabControl.TabPages["CustomFieldsTabPage"]);

					AssertNull(tabControl.TabPages["MailItemsTabPage"]);
				}
			}
		}

		public void TestCOAndFTATabPage()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;
				AssertEquals(typeof(ImportSupplierHeaderUserControl), brokerageControl.SupplierHeaderUserControl.GetType());

				using (var supplierHeaderUserControl = (ImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl)
				{
					supplierHeaderUserControl.InvoiceTabControl.SelectedTab = supplierHeaderUserControl.COAndFTATabPage;
					var control = supplierHeaderUserControl.FindSingle<COAndFTADetailsUserControl>("COAndFTADetailsUserControl");
					AssertNotNull(control);
				}
			}
		}
		public void TestInvoiceHeader_BottomArea()
		{
			using (var control = new ImportSupplierHeaderUserControl())
			{
				control.Show();
				
				var panel = control.FindSingle<KPanel>("BottomPanel");
				AssertEquals(true, panel.Visible);

				var rightBottomPanel = panel.FindSingle<ZPanel>("RightBottomPanel");
				AssertEquals(true, rightBottomPanel.Visible);
				AssertEquals(false, rightBottomPanel.FindSingle<ConvertToLocalCurrencyControl>("JZ_CIFAmountBoundCurrencyControl").Visible);
				AssertEquals(false, rightBottomPanel.FindSingle<ConvertToLocalCurrencyControl>("JZ_Calc_TNIBoundInvoiceCurrencyControl").Visible);

				AssertEquals(true, rightBottomPanel.FindSingle<ZCalcEdit>("CustomsValueKRW").Visible);
				AssertEquals(true, rightBottomPanel.FindSingle<ZCalcEdit>("CustomsValueUSD").Visible);
			}
		}

		public void TestValuationDeclarationTabPage_MethodTypeIsNotOne()
		{
			var invoice = declaration.Invoices.AddNew();
			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;

				using (var userControl = (ImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl)
				{
					AssertValuationMethodBTabPage(ValuationCodeList.Codes.MethodTwo, userControl, "MethodTwoToThreeTabPage");
					AssertValuationMethodBTabPage(ValuationCodeList.Codes.MethodThree, userControl, "MethodTwoToThreeTabPage");
					AssertValuationMethodBTabPage(ValuationCodeList.Codes.MethodFourA, userControl, "MethodFourTabPage");
					AssertValuationMethodBTabPage(ValuationCodeList.Codes.MethodFourB, userControl, "MethodFourTabPage");
					AssertValuationMethodBTabPage(ValuationCodeList.Codes.MethodFive, userControl, "MethodFiveToSixTabPage");
					AssertValuationMethodBTabPage(ValuationCodeList.Codes.MethodSix, userControl, "MethodFiveToSixTabPage");

					invoice.JZ_ValuationCode = "XX";
					userControl.InvoiceTabControl.SelectedTab = userControl.ValuationDeclarationTabPage;
					var valuationDeclarationTabControl = userControl.FindSingle<TabControl>("ValuationDeclarationTabControl");
					AssertEquals(2, valuationDeclarationTabControl.TabPages.Count);
					AssertEquals("DetailsTabPage", valuationDeclarationTabControl.TabPages[0].Name);
					AssertEquals("MethodTwoToSixTabPage", valuationDeclarationTabControl.TabPages[1].Name);
				}
			}

			void AssertValuationMethodBTabPage(ZString valuationCode, ImportSupplierHeaderUserControl userControl, ZString valuationMethodBTabPageName)
			{
				invoice.JZ_ValuationCode = valuationCode;
				userControl.InvoiceTabControl.SelectedTab = userControl.ValuationDeclarationTabPage;
				var valuationDeclarationTabControl = userControl.FindSingle<TabControl>("ValuationDeclarationTabControl");
				AssertEquals(3, valuationDeclarationTabControl.TabPages.Count);
				AssertEquals("DetailsTabPage", valuationDeclarationTabControl.TabPages[0].Name);
				AssertEquals("MethodTwoToSixTabPage", valuationDeclarationTabControl.TabPages[1].Name);
				AssertEquals(typeof(ValuationDeclarationMethodTwoToSixUserControl), valuationDeclarationTabControl.TabPages[1].Controls[0].GetType());
				AssertEquals(valuationMethodBTabPageName, valuationDeclarationTabControl.TabPages[2].Name);
			}
		}

		public void TestValuationDeclarationTabPage_MethodTypeIsOne()
		{
			var invoice = declaration.Invoices.AddNew();
			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;

				using (var userControl = (ImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl)
				{
					AssertValuationMethodATabPage(ZString.Empty, userControl);
					AssertValuationMethodATabPage(ValuationCodeList.Codes.MethodOne, userControl);
				}
			}

			void AssertValuationMethodATabPage(ZString valuationCode, ImportSupplierHeaderUserControl userControl)
			{
				invoice.JZ_ValuationCode = valuationCode;
				userControl.InvoiceTabControl.SelectedTab = userControl.ValuationDeclarationTabPage;
				var valuationDeclarationTabControl = userControl.FindSingle<TabControl>("ValuationDeclarationTabControl");
				AssertEquals(3, valuationDeclarationTabControl.TabPages.Count);
				AssertEquals("DetailsTabPage", valuationDeclarationTabControl.TabPages[0].Name);
				AssertEquals("QuestionTabPage", valuationDeclarationTabControl.TabPages[1].Name);
				AssertEquals("PriceTabPage", valuationDeclarationTabControl.TabPages[2].Name);
			}
		}

		public void TestValuationDeclarationTabPage_SelectedInvoiceChange()
		{
			var invoice_MethodA = declaration.Invoices.AddNew();
			invoice_MethodA.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;

			var invoice_MethodB = declaration.Invoices.AddNew();
			invoice_MethodB.JZ_ValuationCode = ValuationCodeList.Codes.MethodFourA;

			using (var form = new JobDeclarationFormForTest(declaration))
			{
				form.Show();
				var brokerageControl = form.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoicesTabPage;

				using (var userControl = (ImportSupplierHeaderUserControl)brokerageControl.SupplierHeaderUserControl)
				{
					userControl.InvoiceHeadersBoundGrid.CurrentRowIndex = 0;
					AssertValuationMethodTabPage(userControl, "QuestionTabPage", "PriceTabPage");
					invoice_MethodA.JZ_ValuationCode = ValuationCodeList.Codes.MethodTwo;
					AssertValuationMethodTabPage(userControl, "MethodTwoToSixTabPage", "MethodTwoToThreeTabPage");

					userControl.InvoiceHeadersBoundGrid.CurrentRowIndex = 1;
					AssertValuationMethodTabPage(userControl, "MethodTwoToSixTabPage", "MethodFourTabPage");
					invoice_MethodB.JZ_ValuationCode = ValuationCodeList.Codes.MethodOne;
					AssertValuationMethodTabPage(userControl, "QuestionTabPage", "PriceTabPage");
				}
			}

			void AssertValuationMethodTabPage(ImportSupplierHeaderUserControl userControl, ZString pageName1, ZString pageName2)
			{
				userControl.InvoiceTabControl.SelectedTab = userControl.ValuationDeclarationTabPage;
				var valuationDeclarationTabControl = userControl.FindSingle<TabControl>("ValuationDeclarationTabControl");
				AssertEquals(3, valuationDeclarationTabControl.TabPages.Count);
				AssertEquals("DetailsTabPage", valuationDeclarationTabControl.TabPages[0].Name);
				AssertEquals(pageName1, valuationDeclarationTabControl.TabPages[1].Name);
				AssertEquals(pageName2, valuationDeclarationTabControl.TabPages[2].Name);
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		}
		JobDeclaration declaration;

		protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit", "IncoTermExplainButton" });
	}
}
