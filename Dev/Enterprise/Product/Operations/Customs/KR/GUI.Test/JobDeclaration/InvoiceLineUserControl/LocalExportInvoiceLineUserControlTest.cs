using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.GUI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class LocalExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new LocalExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be Localexport", Constants.ColumnLayoutContextLocalExport, control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvLines_LineDetailsGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(LocalExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					var grid = control.CustomsInvoiceLinesBoundGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_LineNo);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Calc_Invoice);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PartNo);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_FormattedTariff);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_InvoiceQuantity);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_InvoiceUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.UnitPrice);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_LinePrice);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_RX_NKLinePriceCurr);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Description);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CustomsQuantity);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Weight);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_WeightUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NetWeight);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NetWeightUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NoOfPacks);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PackType);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.SupportingDocumentCode));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.SupportingDocumentReferenceNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_InboundDate));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_OriginalStateDocType));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_PreviousEntryNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_Ingredient));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.JI_SerialNumber));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.CusEntryLine) + "+" + nameof(Business.CusEntryLine.CL_LineNumber));
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_Volume)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_VolumeUQ)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_OrderNumber)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_Calc_OrderLineNumberAndSubLine)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_RH_NKCommodity_Code)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomAttrib1)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomAttrib2)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomAttrib3)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomAttrib4)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomAttrib5)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomAttrib6)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomTextBlob1)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_PartAttrib1)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_PartAttrib1)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.JI_PartAttrib2)).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(nameof(JobComInvoiceLine.CustomsUnitPrice)).IsVisible);
				}
			}
		}

		public void TestInvLines_LineDetailsTab()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(LocalExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (LocalExportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.FindSingle<ZTabPage>("LineDetailsTabPage");

					AssertEquals(true, control.Contains(control.FindSingle<ZCodeFindBox>("PartNoCodeFindBox")));
					AssertEquals(true, control.Contains(control.FindSingle<ZCodeFindBox>("TariffFindBox")));
					AssertEquals(true, control.Contains(control.FindSingle<LongTextControl>("DescriptionLongTextControl")));
					AssertEquals(true, control.Contains(control.FindSingle<ZDropEdit>("SupportingDocumentCodeDropEdit")));
					AssertEquals(true, control.Contains(control.FindSingle<ZTextBox>("SupportingDocumentReferenceNumberTextBox")));
					AssertEquals(true, control.Contains(control.FindSingle<ZDropEdit>("OriginalStateDocTypeDropEdit")));
					AssertEquals(true, control.Contains(control.FindSingle<ZTextBox>("PreviousEntryNumberTextBox")));
					AssertEquals(true, control.Contains(control.FindSingle<ZDateEdit>("InboundDateEdit")));
					AssertEquals(true, control.Contains(control.FindSingle<ZTextBox>("IngredientTextBox")));
					AssertEquals(true, control.Contains(control.FindSingle<ZTextBox>("SerialNumberTextBox")));

					var localExportGroupBoxUserControl = control.FindSingle<LocalExportInvoiceLineDetailsGroupBoxUserControl>("LocalExportGroupBoxUserControl");
					var quantityAndWeightGroupBox = localExportGroupBoxUserControl.FindSingle<ZGroupBox>("QuantityAndWeightGroupBox");
					var userControl = localExportGroupBoxUserControl.FindSingle<LocalExportQuantityAndWeightUserControl>("LocalExportQuantityAndWeightUserControl");
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcDropEdit>("CustomsQtyCalcDropEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcEdit>("CustomsUnitPriceCalcEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcDropEdit>("NetWeightCalcDropEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcDropEdit>("WeightCalcDropEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcDropEdit>("PackagesCalcDropEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcDropEdit>("InvoiceQtyCalcDropEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcEdit>("UnitPriceCalcEdit")));
					AssertEquals(true, userControl.Contains(userControl.FindSingle<ZCalcFindBox>("PriceCalcDropEdit")));
				}
			}
		}

		public void TestInvoiceLines_InvoiceChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(LocalExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.LineChargesTabPage;
					var grid = control.InvoiceLineCharges.ChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.ChargeCodeDescription);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Percentage);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount);

					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsIncludedInITOT).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
				}
			}
		}

		public void TestInvoiceLines_ApportionedChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(LocalExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.LineChargesTabPage;
					var grid = control.InvoiceLineCharges.ApportionedChargesGrid;
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ChargeType);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.ChargeCodeDescription);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_Amount);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_RX_NKCurrency);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_ExchangeRate);
					AssertEquals(grid.Columns[index++].ColumnName, InvoiceCharge.Schema.J7_IsDutiable);

					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsIncludedInITOT).IsVisible);
					AssertEquals(false, grid.GetColumnStyle(InvoiceCharge.Schema.J7_FullOrPartialApportionment).IsVisible);
					AssertNull(grid.GetColumnStyle("J7_IsGSTApplicable"));
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsDutiable).Caption, "Add to FOB?");
					AssertEquals(grid.GetColumnStyle(InvoiceCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount).Caption, "Included In Invoice");
				}
			}
		}

		public void TestInvLines_LineDetailsLineCalculations()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(LocalExportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (LocalExportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					AssertEquals(false, control.JI_Calc_DutyConvertToLocalCurrencyControl.Visible);
					AssertEquals(false, control.JI_Calc_GSTConvertToLocalCurrencyControl.Visible);
					AssertEquals(true, control.CL_LineNumberCalcEdit.Visible);

					AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_FreightConvertToLocalCurrencyControl").Visible);
					AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_InsuranceConvertToLocalCurrencyControl").Visible);
					AssertEquals(false, control.FindSingle<ConvertToLocalCurrencyControl>("JI_Calc_CIFConvertToLocalCurrencyControl").Visible);
				}
			}
		}

		[TestDate(2023, 10, 19)]
		public void TestTariffFindBox()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				var entry = declaration.CustomsEntryHeaders.AddNew();
				entry.EntryNumber = "4500122000001X";
				entry.CusEntryNumber.CE_IssueDate = new ZDateTime(2022, 06, 06);
				var entryLine = entry.MergedLines.AddNew();
				var invoiceLine = declaration.InvoiceLines.AddNew();
				invoiceLine.JI_CL = entryLine.PK;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					var lineDetailsTab = (ZTabPage)control.LineDetailTabControl.TabPages["LineDetailsTabPage"];
					control.LineDetailTabControl.SelectedTab = lineDetailsTab;
					var tariffFindBox = lineDetailsTab.FindSingle<Universal.GUI.TariffFindBox>("TariffFindBox");

					AssertNotNull(tariffFindBox.GetTariffType);
					AssertNotNull(tariffFindBox.GetDataGrouping);
					AssertNotNull(tariffFindBox.GetEffectiveDate);

					AssertEquals("HSN", tariffFindBox.GetTariffType());
					AssertEquals("KR", tariffFindBox.GetDataGrouping());
					AssertEquals("2022-06-06", tariffFindBox.GetEffectiveDate().ToString("yyyy-MM-dd"));
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.LocalExport;
			declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
	}
}
