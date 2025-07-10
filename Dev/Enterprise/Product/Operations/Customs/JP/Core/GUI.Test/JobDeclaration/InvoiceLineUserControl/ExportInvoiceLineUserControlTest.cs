using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.JP.Business;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI.Testing
{
	sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
	{
		public void TestDutyRefundCodeFindBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var additionalDetailsTabPage = control.FindSingle<ZTabPage>("AdditionalDetailsTabPage");
				control.LineDetailTabControl.SelectedTab = additionalDetailsTabPage;

				var dutyRefundCodeFindBox = control.FindSingle<ZCodeFindBox>("DutyRefundCodeFindBox");
				AssertEquals("Must be in Additional Details tab page", true, additionalDetailsTabPage.Contains(dutyRefundCodeFindBox));
				Assert(dutyRefundCodeFindBox.Visible);
			}
		}

		public void TestDynamicLayout()
		{
			using (var control = new ExportInvoiceLineUserControlForTest())
			{
				AssertType<ExportInvoiceLineLayouts>(control.GetNewInvoiceLineDetailsPanelLayoutExposed());
			}
		}

		public void TestTariffColumn()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				var grid = control.CustomsInvoiceLinesBoundGrid;
				var tariffColumn = grid.GetColumnStyle("JI_FormattedTariff") as TariffColumnStyleInfo;

				AssertContainsExactElementsInAnyOrder(new List<SelectionStyle> { SelectionStyle.Tariff, SelectionStyle.Heading }, tariffColumn.SelectNomenclatureModes);
			}
		}

		public void TestFEFTADropEdit()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var additionalDetailsTabPage = control.FindSingle<ZTabPage>("AdditionalDetailsTabPage");
				control.LineDetailTabControl.SelectedTab = additionalDetailsTabPage;
				var fEFTADropEdit = control.FindSingle<ZDropEdit>("FEFTADropEdit");
				AssertEquals("Must be in Additional Details tab page", true, additionalDetailsTabPage.Contains(fEFTADropEdit));
				Assert(fEFTADropEdit.Visible);
			}
		}

		public void TestFEFTAArticle48ColumnInCustomsInvoiceLinesBoundGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.Invoices.AddNew().InvoiceLines.AddNew();

			using (var form = new ZForm(declaration))
			using (var control = new ExportInvoiceLineUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var grid = control.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");
				var fEFTAArticle48Column = grid.GetColumnStyle(JobComInvoiceLine.Schema.JI_FEFTAArticle48);
				var fEFTAArticle48DescriptionColumn = grid.GetColumnStyle("FEFTAArticle48Description");

				AssertNotNull(fEFTAArticle48Column);
				AssertNotNull(fEFTAArticle48DescriptionColumn);
				Assert("Is not Visible", !fEFTAArticle48Column.IsVisible);
				Assert("Is not Visible", !fEFTAArticle48DescriptionColumn.IsVisible);
				AssertEquals(fEFTAArticle48DescriptionColumn.GroupName, fEFTAArticle48Column.GroupName);
			}
		}

		public void TestOtherLawsGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				CombineAssertions(() =>
				{
					var otherLawsAndRegulationsGroupBox = control.FindSingle<ZGroupBox>("OtherLawsAndRegulationsGroupBox");
					AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage")?.Contains(otherLawsAndRegulationsGroupBox));
					AssertEquals("English Caption", "Other Laws and Regulations", otherLawsAndRegulationsGroupBox?.CaptionResourceString.Caption);

					var otherLawsGrid = control.FindSingle<ZGrid>("OtherLawsGrid");
					AssertEquals("Must be in Additional Details tab page", true, otherLawsAndRegulationsGroupBox.Contains(otherLawsGrid));
					AssertEndsWith("Binds to correct field", "OtherLaws", control.BindingSource.GetBindingMember(otherLawsGrid));

					AssertEquals("MaximumRows of OtherLawsGrid must be equal to CusOtherLawReferenceCollection.MaxRowCount", CusOtherLawReferenceCollection<CusOtherLawReferenceForInvoiceLines>.MaxRowCount, otherLawsGrid.MaximumRows);
				});
			}
		}

		public void TestColumnLayoutContextForInvoiceLinesGrid()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestConsumptionTaxFields()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				CombineAssertions(() =>
				{
					var consumptionTaxField = control.FindSingle<ZDropEdit>("DomesticConsumptionTaxExemptionCodeDropEdit");
					AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage")?.Contains(consumptionTaxField));
					AssertEndsWith("Binds to correct field", ".JI_DomesticConsumptionTaxExemptionCode", control.BindingSource.GetBindingMember(consumptionTaxField));

					var partiallyAppliedField = control.FindSingle<ZCheckBox>("DomesticConsumptionTaxExemptionIsPartialCheckBox");
					AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage")?.Contains(partiallyAppliedField));
					AssertEquals("English Caption", "Partially Applied", partiallyAppliedField?.CaptionResourceString.Caption);
					AssertEndsWith("Binds to correct field", ".JI_DomesticConsumptionTaxExemptionIsPartial", control.BindingSource.GetBindingMember(partiallyAppliedField));
				});
			}
		}

		public void TestTradeControlOrderAppendix()
		{
			using (var control = new ExportInvoiceLineUserControl())
			{
				CombineAssertions(() =>
				{
					var foreignExchangeActField = control.FindSingle<ZDropEdit>("TradeControlOrderAppendixDropEdit");
					AssertEquals("Must be in Additional Details tab page", true, control.FindSingle<ZTabPage>("AdditionalDetailsTabPage")?.Contains(foreignExchangeActField));
					AssertEndsWith("Foreign Exchange Act is to bind to JI_TradeControlOrderAppendix", ".JI_TradeControlOrderAppendix", control.BindingSource.GetBindingMember(foreignExchangeActField));
				});
			}
		}

		public void TestExportInvoiceLineBottomPanelMinimumSize()
		{
			using var exportInvoiceLineUserControl = new ExportInvoiceLineUserControlForTest();
			var bottomPanel = exportInvoiceLineUserControl.FindSingleOrDefault<ZPanel>(c => c.Name == "BottomPanel");
			var expectedSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 250, isInStandardDpi: true);
			AssertEquals(expectedSize, bottomPanel.MinimumSize);
		}

		public void TestExportControlNumberVisible()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclartion.JE_MessageType = JobMessageTypeList.Codes.Export;
			jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Air;

			jobDeclartion.CustomsEntryInstructions.AddNew();

			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				var invoiceLinesTabPage = form.CustomsBrokerageUserControl.FindSingle<ZTabPage>("InvoiceLinesTabPage");
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = invoiceLinesTabPage;
				form.Show();
				var userControl = invoiceLinesTabPage.FindSingle<ExportInvoiceLineUserControl>("ExportInvoiceLineUserControl");
				var grid = userControl.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");

				var exportControlNumberColumn = grid.GetColumnStyle("ExportControlNumber");

				Assert(exportControlNumberColumn.IsUnavailable);

				jobDeclartion.JE_TransportMode = Business.TransportTypeList.Codes.Sea;
				Assert(!exportControlNumberColumn.IsUnavailable);

				AssertEquals(137, exportControlNumberColumn.Width);
			}
		}

		sealed class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
		{
			public IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayoutExposed() => GetNewInvoiceLineDetailsPanelLayout();
		}
	}
}
