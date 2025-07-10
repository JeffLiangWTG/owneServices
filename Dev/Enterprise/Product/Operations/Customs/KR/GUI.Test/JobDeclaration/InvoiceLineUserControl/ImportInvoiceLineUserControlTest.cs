using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Universal.GUI;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestInvLines_LineDetailsGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

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
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CustomsQuantity);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Description);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Weight);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_WeightUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NetWeight);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_NetWeightUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_DrawbackQuantity);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_DrawbackUQ);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CountryOfOrigin);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.CriteriaForDeterminingCountryOfOrigin));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_COOLabelLocation);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_COOLabelType);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_COOExemptionReason);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Model);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_BrandCode);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_BrandName);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_Ingredient);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DutyRateCode));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PrimaryPreference);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DutyRate));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_AdditionalDutyType);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_AdditionalDutyRate);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_SecondaryPreference);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DutyReductionRate));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_InstallmentCode);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_SpecificUseCodeDutyRatePermitNo);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_IsSpecificUseCode);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_DomesticTaxCode);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DomesticTaxType));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DomesticTaxRate));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_DomesticTaxExemptionCode);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DomesticTaxBaseQtyOrPrice));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.DomesticTaxBaseUnit));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_ZZF_NKTaxType);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_VATReductionCode);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.EducationTaxExemptIndicator));
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.AgricultureTaxClassification));
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CEI);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_LotNumber);
					AssertEquals(grid.Columns[index++].ColumnName, nameof(JobComInvoiceLine.CusEntryLine) + "+" + KR.Business.CusEntryLine.Schema.CL_LineNumber);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_SequenceNumber);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_ProductTypeCode);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_ParentLine);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA1);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA2);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_PostClearanceProcedureGA3);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_MightRequireInspection);
					AssertEquals(grid.Columns[index++].ColumnName, JobComInvoiceLine.Schema.JI_CourierCargoSelectivityIndicator);
				}
			}
		}

		public void TestUseUniversalTariff()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();

			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;

				var formattedTariffColumn = form.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid.GetColumnStyle(nameof(JobComInvoiceLine.JI_FormattedTariff));
				AssertNotNull("The Tariff column exists", formattedTariffColumn);
				AssertEquals(typeof(TariffColumnStyleInfo), formattedTariffColumn.GetType());

				var columnAsUniversalTariff = (TariffColumnStyleInfo)formattedTariffColumn;
				AssertEquals(Core.Constants.CountryCodes.KoreaSouth, columnAsUniversalTariff.GetDataGrouping());
				AssertEquals(Universal.Constants.TariffTypes.HarmonizedSystem, columnAsUniversalTariff.GetTariffType());
			}
		}

		public void TestChangeColumnsInGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var invoiceLinesGrid = (ZGrid)control.Controls.Find("CustomsInvoiceLinesBoundGrid", true)[0];

				CombineAssertions(() =>
				{
					AssertNotNull("JI_PrimaryPreference column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_PrimaryPreference));
					AssertNotNull("JI_SecondaryPreference column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_SecondaryPreference));
					AssertNotNull("JI_CustomsQuantity column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsQuantity));
					AssertNotNull("JI_CustomsSecondQuantity column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsSecondQuantity));
					AssertNotNull("JI_CustomsThirdQuantity column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsThirdQuantity));

					AssertNotNull("JI_InstallmentCode column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_InstallmentCode));
					AssertNotNull("JI_IsSpecificUseCode column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_IsSpecificUseCode));
					AssertNotNull("JI_AdditionalDutyType column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_AdditionalDutyType));
					AssertNotNull("JI_AdditionalDutyRate column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_AdditionalDutyRate));
					AssertNotNull("JI_ZZF_NKTaxType column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ZZF_NKTaxType));
					AssertNotNull("JI_VATReductionCode column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_VATReductionCode));
					AssertNotNull("JI_DomesticTaxCode column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_DomesticTaxCode));
					AssertNotNull("JI_DomesticTaxExemptionCode column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_DomesticTaxExemptionCode));
					AssertNotNull("EducationTaxExemptIndicator column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.EducationTaxExemptIndicator)));

					AssertNotNull("CertificateOfOriginProductType column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.CertificateOfOriginProductType)));
					AssertNotNull("IssuedInThirdCountry column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.IssuedInThirdCountry)));
					AssertNotNull("JI_RN_NKSecondCommercialInvoiceCountry column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RN_NKSecondCommercialInvoiceCountry));
					AssertNotNull("CountryOfOriginExporterNumber column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.CountryOfOriginExporterNumber)));
					AssertNotNull("COOSplitOrder) column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.COOSplitOrder)));
					AssertNotNull("JI_COOSupportingDocType column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_COOSupportingDocType));
					AssertNotNull("COOIssuerType column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.COOIssuerType)));
					AssertNotNull("COOTotalNetWeight column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.COOTotalNetWeight)));
					AssertNotNull("COOTotalNetWeightUQ column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.COOTotalNetWeightUQ)));
					AssertNotNull("COOSequenceNumber column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.COOLineNumber)));
					AssertNotNull("JI_CustomsFifthQuantity column exists", invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsFifthQuantity));
					AssertNotNull("COOUsedQuantityUQ column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.COOUsedQuantityUQ)));
					AssertNotNull("JI_CustomsFourthQuantity column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.JI_CustomsFourthQuantity)));
					AssertNotNull("JI_InstallationCost column exists", invoiceLinesGrid.GetColumnStyle(nameof(JobComInvoiceLine.JI_InstallationCost)));

					var customsUnitQtyColumn = invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsUnitQty);
					AssertNotNull("JI_CustomsUnitQty column exists", customsUnitQtyColumn);
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), customsUnitQtyColumn.GetType());

					var customsThirdUnitQty = invoiceLinesGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_CustomsThirdUnitQty);
					AssertNotNull("JI_CustomsThirdUnitQty column exists", customsThirdUnitQty);
					AssertEquals(typeof(ZTextBoxColumnStyleInfo), customsThirdUnitQty.GetType());

					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.CustomsUnitPrice));
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomsSecondQuantity);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomsSecondUnitQty);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_Volume);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_VolumeUQ);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_OrderNumber);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.InvoiceHeader) + "+" + nameof(Business.JobComInvoiceHeader.JZ_InvoiceDisplaySequence));
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_SerialNumber);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomAttrib1);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomAttrib2);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomAttrib3);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomAttrib4);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomAttrib5);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomAttrib6);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomTextBlob1);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_PartAttrib1);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_PartAttrib2);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_PartAttrib3);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_MatchingKey);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_ClassUsageComment);
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_GS_NKClassUsageCommentReviewer);
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.CertificateOfOriginProductType));
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.IssuedInThirdCountry));
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_RN_NKSecondCommercialInvoiceCountry);
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.CountryOfOriginExporterNumber));
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.COOSplitOrder));
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_COOSupportingDocType);
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.COOIssuerType));
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.COOTotalNetWeight));
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.COOTotalNetWeightUQ));
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.COOLineNumber));
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomsFifthQuantity);
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.COOUsedQuantityUQ));
					AddibleColumnAssert(invoiceLinesGrid, JobComInvoiceLine.Schema.JI_CustomsFourthQuantity);
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.JI_InstallationCost));
					AddibleColumnAssert(invoiceLinesGrid, nameof(JobComInvoiceLine.JI_CoveredByCOOExporter));
				});
			}
		}

		void AddibleColumnAssert(ZGrid grid, ZString columnName)
		{
			var column = grid.GetColumnStyle(columnName);
			AssertNotNull("column exists", column);
			AssertEquals(false, column.IsVisible);
		}

		public void TestInvoiceLines_InvoiceChargesGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

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

					AssertNull(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
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
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

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

					AssertNull(grid.GetColumnStyle(InvoiceCharge.Schema.J7_IsGSTApplicable));
				}
			}
		}
		public void TestInvoiceLines_LineCalculationGroupBoxTest()
		{
			var invoiceLine = declaration.Invoices[0].InvoiceLines.AddNew();
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.LineChargesTabPage;
					var panel = control.LineSummaryPanel;

					var sequenceNoCalEdit = panel.FindSingle<ZCalcEdit>("KR_SequenceNoCalcEdit");
					AssertEquals(true, sequenceNoCalEdit.Visible);
					AssertEquals(true, sequenceNoCalEdit.ReadOnly);

					var lineNumberCalcEdit = panel.FindSingle<ZCalcEdit>("CL_LineNumberCalcEdit");
					AssertEquals(true, lineNumberCalcEdit.ReadOnly);
					AssertEquals(true, lineNumberCalcEdit.Visible);

					var calcCIFEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("JI_Calc_CIFConvertToLocalCurrencyControl");
					AssertEquals(false, calcCIFEdit.Visible);

					var calcFOBEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("JI_Calc_FOBConvertToLocalCurrencyControl");
					AssertEquals("Customs Value", calcFOBEdit.CaptionResourceString.Caption);

					AssertEquals("KRW", invoiceLine.LocalCurrency.Code);

					var calcSCTEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("SpecialConsumptionTaxAmountConvertToLocalCurrencyControl");
					AssertEquals("FilteredInvoiceLines.JI_Calc_SpecialConsumptionTaxIncludingWHEstimate", calcSCTEdit.BindToAmount);
					AssertEquals("FilteredInvoiceLines.Lookups+CurrencyList", calcSCTEdit.BindToList);
					AssertEquals("FilteredInvoiceLines.JI_RX_LocalCurrency", calcSCTEdit.BindToUnit);
					AssertEquals(true, calcSCTEdit.ReadOnly);

					var calcTRTEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("TransportationTaxAmountConvertToLocalCurrencyControl");
					AssertEquals("FilteredInvoiceLines.JI_Calc_TransportationTaxIncludingWHEstimate", calcTRTEdit.BindToAmount);
					AssertEquals("FilteredInvoiceLines.Lookups+CurrencyList", calcTRTEdit.BindToList);
					AssertEquals("FilteredInvoiceLines.JI_RX_LocalCurrency", calcTRTEdit.BindToUnit);
					AssertEquals(true, calcTRTEdit.ReadOnly);

					var calcLQTEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("LiquorTaxAmountConvertToLocalCurrencyControl");
					AssertEquals("FilteredInvoiceLines.JI_Calc_LiquorTaxIncludingWHEstimate", calcLQTEdit.BindToAmount);
					AssertEquals("FilteredInvoiceLines.Lookups+CurrencyList", calcLQTEdit.BindToList);
					AssertEquals("FilteredInvoiceLines.JI_RX_LocalCurrency", calcLQTEdit.BindToUnit);
					AssertEquals(true, calcLQTEdit.ReadOnly);

					var calcEDTEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("EducationTaxAmountConvertToLocalCurrencyControl");
					AssertEquals("FilteredInvoiceLines.JI_Calc_EducationTaxIncludingWHEstimate", calcEDTEdit.BindToAmount);
					AssertEquals("FilteredInvoiceLines.Lookups+CurrencyList", calcEDTEdit.BindToList);
					AssertEquals("FilteredInvoiceLines.JI_RX_LocalCurrency", calcEDTEdit.BindToUnit);
					AssertEquals(true, calcEDTEdit.ReadOnly);

					var calcAGTEdit = panel.FindSingle<Customs.GUI.ConvertToLocalCurrencyControl>("AgricultureTaxAmountConvertToLocalCurrencyControl");
					AssertEquals("FilteredInvoiceLines.JI_Calc_AgricultureTaxIncludingWHEstimate", calcAGTEdit.BindToAmount);
					AssertEquals("FilteredInvoiceLines.Lookups+CurrencyList", calcAGTEdit.BindToList);
					AssertEquals("FilteredInvoiceLines.JI_RX_LocalCurrency", calcAGTEdit.BindToUnit);
					AssertEquals(true, calcAGTEdit.ReadOnly);
				}
			}
		}

		public void TestExemptionOrSpecificUseRateTabPageVisibility()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.KoreaSouth, Constants.ZZ.TariffTypes.DutyReductionExemption);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.KoreaSouth, tariffType.PK, "A093000004", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(1), "관세법 제93조제4호 해당물품");
			helper.CreateTariffAttribute(Customs.KR.Messaging.Constants.ZZ.TariffAttributes.IsDutyExempt, Constants.YesNo.Yes, tariff1);
			Factory.Save();

			var invoiceLine = declaration.Invoices[0].InvoiceLines.AddNew();
			invoiceLine.JI_SecondaryPreference = "A093000004";
			invoiceLine.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.Invalid, invoiceLine.DutyReductionClassificationCode);

			using var testForm = new JobDeclarationFormForTest(declaration);
			testForm.Show();
			var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;

			brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
			var control = (ImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl;

			var grid = testForm.CustomsBrokerageUserControl.InvoiceLinesUserControl.CustomsInvoiceLinesBoundGrid;
			AssertEquals(false, control.ExemptionOrSpecificUseRateTabPage.TabVisible);

			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			invoiceLine.JI_InstallmentCode = ZString.Empty;
			AssertEquals(ZString.Empty, invoiceLine.DutyReductionClassificationCode);
			AssertEquals(false, control.ExemptionOrSpecificUseRateTabPage.TabVisible);

			invoiceLine.JI_IsSpecificUseCode = true;
			AssertEquals(ImportDutyReductionClassificationList.Codes.SpecificUseCodeOnly, invoiceLine.DutyReductionClassificationCode);
			AssertEquals(true, control.ExemptionOrSpecificUseRateTabPage.TabVisible);

			invoiceLine.JI_IsSpecificUseCode = false;
			invoiceLine.JI_SecondaryPreference = "A093000004";
			AssertEquals(ImportDutyReductionClassificationList.Codes.DutyExemption, invoiceLine.DutyReductionClassificationCode);
			AssertEquals(true, control.ExemptionOrSpecificUseRateTabPage.TabVisible);

			invoiceLine.JI_SecondaryPreference = ZString.Empty;
			invoiceLine.JI_InstallmentCode = "ABC";
			AssertEquals(ImportDutyReductionClassificationList.Codes.InstallmentPayment, invoiceLine.DutyReductionClassificationCode);
			AssertEquals(true, control.ExemptionOrSpecificUseRateTabPage.TabVisible);
		}

		public void TestInvoiceLines_OtherDetails_ImmediateDeliveryGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (ImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.OtherDetailsTabPage;
					var grid = (ZGrid)control.Controls.Find("ImmediateDeliveryGrid", true)[0];
					var index = 0;
					AssertEquals(grid.Columns[index++].ColumnName, ImmediateDelivery.Schema.CY_Order);
					AssertEquals(grid.Columns[index++].ColumnName, ImmediateDelivery.Schema.CY_Data);
				}
			}
		}

		public void TestInvoiceLines_OtherDetails_HSExtensionGrid()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (ImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.OtherDetailsTabPage;
					var hsExtensionGroupBox = control.FindSingle<ZGroupBox>("HSExtensionGroupBox");
					var grid = hsExtensionGroupBox.FindSingle<ZGrid>("HSExtensionGrid");
					AssertEquals(grid.Columns[0].ColumnName, nameof(HSExtensionCode.ClassificationType));
					AssertEquals(grid.Columns[1].ColumnName, HSExtensionCode.Schema.CY_Code);
					AssertEquals(grid.Columns[2].ColumnName, nameof(HSExtensionCode.CategoryDescription));
				}
			}
		}

		public void TestLineDetailsTab()
		{
			using (var testForm = new JobDeclarationFormForTest(declaration))
			{
				testForm.Show();

				var brokerageControl = testForm.CustomsBrokerageUserControl as CustomsBrokerageUserControlForTest;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				AssertEquals(typeof(ImportInvoiceLineUserControl), brokerageControl.InvoiceLinesUserControl.GetType());

				using (var control = (ImportInvoiceLineUserControl)brokerageControl.InvoiceLinesUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.FindSingle<ZTabPage>("LineDetailsTabPage");

					AssertEquals(true, control.Contains(control.FindSingle<ZGroupBox>("DetailsGroupBox")));
					AssertEquals(true, control.Contains(control.FindSingle<DynamicLayoutPanel>("DetailsPanel")));

					AssertEquals(true, control.Contains(control.FindSingle<ZGroupBox>("QuantityAndWeightGroupBox")));
					AssertEquals(true, control.Contains(control.FindSingle<DynamicLayoutPanel>("QuantityAndWeightPanel")));

					AssertEquals(true, control.Contains(control.FindSingle<ZGroupBox>("DutyAndTaxInfoGroupBox")));
					AssertEquals(true, control.Contains(control.FindSingle<DynamicLayoutPanel>("DutyAndTaxInfoPanel")));
				}
			}
		}

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew();
			declaration.Invoices.AddNew();
		}
		JobDeclaration declaration;
	}
}
