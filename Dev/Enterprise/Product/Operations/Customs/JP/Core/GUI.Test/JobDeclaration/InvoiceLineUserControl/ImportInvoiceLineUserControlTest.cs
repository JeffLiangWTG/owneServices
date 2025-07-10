using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineUserControl))]
	sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestDutyReductionCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var additionalDetailsTabPage = control.FindSingle<ZTabPage>("AdditionalDetailsTabPage");
				control.LineDetailTabControl.SelectedTab = additionalDetailsTabPage;

				var dutyReductionCodeFindBox = control.FindSingle<ZCodeFindBox>("DutyReductionCodeFindBox");
				AssertEquals("Must be in Additional Details tab page", true, additionalDetailsTabPage.Contains(dutyReductionCodeFindBox));
				Assert(dutyReductionCodeFindBox.Visible);

				var dutyReductionAmountCalcEdit = control.FindSingle<ZCalcEdit>("DutyReductionAmountCalcEdit");
				AssertEquals("Must be in Additional Details tab page", true, additionalDetailsTabPage.Contains(dutyReductionAmountCalcEdit));
				Assert(dutyReductionAmountCalcEdit.Visible);
			}
		}

		public void TestDynamicLayout()
		{
			using (var control = new ImportInvoiceLineUserControlForTest())
			{
				AssertType<ImportInvoiceLineLayouts>(control.GetNewInvoiceLineDetailsPanelLayoutExposed());
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestTariffColumn()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var grid = control.CustomsInvoiceLinesBoundGrid;
				var tariffColumn = grid.GetColumnStyle("JI_FormattedTariff") as TariffColumnStyleInfo;

				AssertContainsExactElementsInAnyOrder(new List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Subheading }, tariffColumn.SelectNomenclatureModes);
			}
		}

		public void TestDutyRateAndConcessionOrderColumns()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var grid = control.CustomsInvoiceLinesBoundGrid;
				var dutyRateColumn = grid.GetColumnStyle("JI_DutyRateFormula");
				var concessionOrderColumn = grid.GetColumnStyle("JI_ConcessionOrder");

				AssertEquals("Is Visible", true, dutyRateColumn.IsVisible);
				AssertEquals("Is Visible", true, concessionOrderColumn.IsVisible);
			}
		}

		public void TestPrimaryPreferenceColumns()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var grid = control.CustomsInvoiceLinesBoundGrid;
				var preferenceColumn = grid.GetColumnStyle("JI_Calc_Preference");
				var originCertifierColumn = grid.GetColumnStyle("JI_Calc_OriginCertifier");
				var certificateOfOriginCertifierColumn = grid.GetColumnStyle("JI_Calc_CertificateOfOriginCertifier");

				AssertEquals("Is Visible", true, preferenceColumn.IsVisible);
				AssertEquals("Is Visible", true, originCertifierColumn.IsVisible);
				AssertEquals("Is Visible", true, certificateOfOriginCertifierColumn.IsVisible);
			}
		}

		public void TestBondedDateColumn()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var grid = control.CustomsInvoiceLinesBoundGrid;
				var bondedDateColumn = grid.GetColumnStyle("JI_BondedDate");
				AssertNotNull(bondedDateColumn);
				Assert(!bondedDateColumn.IsMandatory);
				Assert(bondedDateColumn.IsVisible);
			}
		}

		public void TestAdvanceRulingOnClassificationAndOrigin()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var control = new ImportInvoiceLineUserControl())
			{
				control.SetDataBinding(declaration, "");
				var preAdviceClass = control.FindSingle<ZTextBox>("AdvanceRulingOnClassificationTextBox");
				var preAdviceOrigin = control.FindSingle<ZTextBox>("AdvanceRulingOnOriginTextBox");
				AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage")?.Contains(preAdviceClass));
				AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage")?.Contains(preAdviceOrigin));
				AssertEquals("Pre-Advice Class MaxLength", 9, preAdviceClass?.MaxLength);
				AssertEquals("Pre-Advice Origin MaxLength", 7, preAdviceOrigin?.MaxLength);
			}
		}

		public void TestDomesticConsumptionTaxesGrid()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var grid = control.FindSingle<ZGrid>("DomesticConsumptionTaxesGrid");
				AssertEquals("Must be in Domestic Consumption Taxes tab page", true, control.FindSingle<ZTabPage>("DomesticConsumptionTaxesTabPage")?.Contains(grid));

				var tariffTypeColumn = grid.GetColumnStyle("BZ_Type");
				var tariffTypeDescriptionColumn = grid.GetColumnStyle("BZ_TypeDescription");
				var tariffCodeColumn = grid.GetColumnStyle("BZ_Tariff");
				var rateFormulaColumn = grid.GetColumnStyle("RateFormula");
				var exemptionReductionCodeColumn = grid.GetColumnStyle("BZ_ExemptionReductionCode");
				var exemptionReductionCodeDescriptionColumn = grid.GetColumnStyle("BZ_ExemptionReductionCodeDescription");
				var reductionValueColumn = grid.GetColumnStyle("BZ_Value");

				CombineAssertions(() =>
				{
					ColumnTestHelper(tariffTypeColumn);
					ColumnTestHelper(tariffTypeDescriptionColumn);
					ColumnTestHelper(tariffCodeColumn);
					ColumnTestHelper(rateFormulaColumn);
					ColumnTestHelper(exemptionReductionCodeColumn);
					ColumnTestHelper(exemptionReductionCodeDescriptionColumn);
					ColumnTestHelper(reductionValueColumn);
				});
			}
		}

		public void TestDomesticConsumptionTaxesGridGroupName()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				var grid = control.FindSingle<ZGrid>("DomesticConsumptionTaxesGrid");
				var tariffTypeColumn = grid.GetColumnStyle("BZ_Type");
				var tariffTypeDescriptionColumn = grid.GetColumnStyle("BZ_TypeDescription");
				var exemptionReductionCodeColumn = grid.GetColumnStyle("BZ_ExemptionReductionCode");
				var exemptionReductionCodeDescriptionColumn = grid.GetColumnStyle("BZ_ExemptionReductionCodeDescription");

				AssertEquals(tariffTypeColumn.GroupName, tariffTypeDescriptionColumn.GroupName);
				AssertEquals(exemptionReductionCodeColumn.GroupName, exemptionReductionCodeDescriptionColumn.GroupName);
			}
		}

		public void TestImportInvoiceLineBottomPanelMinimumSize()
		{
			using var importInvoiceLineUserControl = new ImportInvoiceLineUserControlForTest();
			var bottomPanel = importInvoiceLineUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "BottomPanel");
			var expectedSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 250, isInStandardDpi: true);
			AssertEquals(expectedSize, bottomPanel.MinimumSize);
		}

		public void TestTradeControlOrderAppendix()
		{
			using (var control = new ImportInvoiceLineUserControl())
			{
				CombineAssertions(() =>
				{
					var dropEdit = control.FindSingle<ZDropEdit>("TradeControlOrderAppendixDropEdit");
					AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage").Contains(dropEdit));
					AssertEndsWith("DropEdit is to bind to JI_TradeControlOrderAppendix", ".JI_TradeControlOrderAppendix", control.BindingSource.GetBindingMember(dropEdit));
				});
			}
		}

		void ColumnTestHelper(ZGridColumnInfo column)
		{
			AssertNotNull(column);
			Assert(!column.IsMandatory);
			Assert(column.IsVisible);
		}

		sealed class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
		{
			public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayoutExposed() => GetNewInvoiceLineDetailsPanelLayout();
		}
	}
}
