using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ExportPreviousProceduresUserControlTest : TestCaseWithFactory
	{
		public void TestDocumentGridColumnSize()
		{
			using (var control = new ExportPreviousProceduresUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Procedure", 72, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Procedure).Width);
					AssertEquals("CSI_ReferenceNumber", 130, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).Width);
					AssertEquals("CSI_SubType", 167, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).Width);
					AssertEquals("CSI_DateOfIssue", 100, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_DateOfIssue).Width);
					AssertEquals("CSI_LineNo", 66, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_LineNo).Width);
					AssertEquals("Status", 105, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.Status).Width);
					AssertEquals("CSI_Description", 160, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Description).Width);
					AssertEquals("FormattedTariff", 106, previousDocumentGrid.GetColumnStyle(nameof(PreviousDocument.FormattedTariff)).Width);
					AssertEquals("CSI_Quantity", 107, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity).Width);
					AssertEquals("CSI_UnitOfQuantity", 38, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).Width);
					AssertEquals("CSI_Quantity2", 93, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Quantity2).Width);
					AssertEquals("CSI_UnitOfQuantity2", 38, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).Width);
					AssertEquals("CSI_CustomsOffice", 151, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_CustomsOffice).Width);
					AssertEquals("UsualProcessingFlag", 128, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.UsualProcessingFlag).Width);
				});
			}
		}

		public void TestDocumentGridColumnCharacterCasing()
		{
			using (var control = new ExportPreviousProceduresUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Procedure", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Procedure).CharacterCasing);
					AssertEquals("CSI_ReferenceNumber", CharacterCasing.Normal, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_ReferenceNumber).CharacterCasing);
					AssertEquals("CSI_SubType", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_SubType).CharacterCasing);
					AssertEquals("Status", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.Status).CharacterCasing);
					AssertEquals("CSI_Description", CharacterCasing.Normal, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_Description).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity).CharacterCasing);
					AssertEquals("CSI_UnitOfQuantity2", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_UnitOfQuantity2).CharacterCasing);
					AssertEquals("CSI_CustomsOffice", CharacterCasing.Upper, previousDocumentGrid.GetColumnStyle(PreviousDocument.Schema.CSI_CustomsOffice).CharacterCasing);
				});
			}
		}

		public void TestDocumentsGridColumns()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var collection = new PreviousDocumentCollection(previousDocument, true);

			using (var control = new ExportPreviousProceduresUserControl())
			{
				var previousDocumentGrid = (ZGrid)control.Controls.Find("PreviousDocumentsGrid", true).Single();
				previousDocumentGrid.SetDataBinding(collection, string.Empty);
				control.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Column Count", 14, previousDocumentGrid.Columns.Count);
					AssertEquals("CSI_Procedure", CusSupportingInfo.Schema.CSI_Procedure, previousDocumentGrid.Columns[0].ColumnName);
					AssertEquals("CSI_ReferenceNumber", CusSupportingInfo.Schema.CSI_ReferenceNumber, previousDocumentGrid.Columns[1].ColumnName);
					AssertEquals("CSI_SubType", CusSupportingInfo.Schema.CSI_SubType, previousDocumentGrid.Columns[2].ColumnName);
					AssertEquals("CSI_DateOfIssue", CusSupportingInfo.Schema.CSI_DateOfIssue, previousDocumentGrid.Columns[3].ColumnName);
					AssertEquals("CSI_LineNo", CusSupportingInfo.Schema.CSI_LineNo, previousDocumentGrid.Columns[4].ColumnName);
					AssertEquals("Status", PreviousDocument.Schema.Status, previousDocumentGrid.Columns[5].ColumnName);
					AssertEquals("CSI_Description", CusSupportingInfo.Schema.CSI_Description, previousDocumentGrid.Columns[6].ColumnName);
					AssertEquals("FormattedTariff", nameof(PreviousDocument.FormattedTariff), previousDocumentGrid.Columns[7].ColumnName);
					AssertEquals("CSI_Quantity", CusSupportingInfo.Schema.CSI_Quantity, previousDocumentGrid.Columns[8].ColumnName);
					AssertEquals("CSI_UnitOfQuantity", CusSupportingInfo.Schema.CSI_UnitOfQuantity, previousDocumentGrid.Columns[9].ColumnName);
					AssertEquals("CSI_Quantity2", CusSupportingInfo.Schema.CSI_Quantity2, previousDocumentGrid.Columns[10].ColumnName);
					AssertEquals("CSI_UnitOfQuantity2", CusSupportingInfo.Schema.CSI_UnitOfQuantity2, previousDocumentGrid.Columns[11].ColumnName);
					AssertEquals("CSI_CustomsOffice", CusSupportingInfo.Schema.CSI_CustomsOffice, previousDocumentGrid.Columns[12].ColumnName);
					AssertEquals("UsualProcessingFlag", PreviousDocument.Schema.UsualProcessingFlag, previousDocumentGrid.Columns[13].ColumnName);
				});
			}
		}

		public void TestPreviousDocumentsExportATZLPanel()
		{
			using (var control = new ExportPreviousProceduresUserControl())
			{
				control.SetPreviousDocumentsGridColumnsVisible(PreviousProcedureList.Codes._ATZL);
				var panel = control.FindSingle<PreviousProcedureExportATZLPanel>();
				AssertNotNull(panel);
				AssertEquals("FilteredInvoiceLines.PreviousProcedureMaster.CSI_ReferenceNumber2", panel.LocalReferenceTextBox.BindTo);
				AssertEquals("FilteredInvoiceLines.PreviousProcedureMaster.AuthorizationNumber", panel.AuthorizationNumberDropDown.BindTo);
			}
		}

		public void TestPreviousProcedureExportATAVPanel()
		{
			using (var control = new ExportPreviousProceduresUserControl())
			{
				control.SetPreviousDocumentsGridColumnsVisible(PreviousProcedureList.Codes._ATAV);
				var panel = control.FindSingle<PreviousProcedureExportATAVPanel>();
				AssertNotNull(panel);
				AssertEquals("FilteredInvoiceLines.PreviousProcedureMaster.AuthorizationNumber", panel.FindSingle<ZDropEdit>("AuthorizationNumberDropEdit").BindTo);
			}
		}

		[TestDate(2020, 09, 08)]
		public void TestFormattedTariffColumn()
		{
			using (var control = new ExportPreviousProceduresUserControl())
			{
				var previousDocumentGrid = control.FindSingle<ZGrid>("PreviousDocumentsGrid");
				var columnStyle = previousDocumentGrid.GetColumnStyle(nameof(PreviousDocument.FormattedTariff));
				CombineAssertions(() =>
				{
					AssertType<Universal.GUI.TariffColumnStyleInfo>("ColumnStyleInfo: Type", columnStyle);
					var tariffColumnStyleInfo = columnStyle as Universal.GUI.TariffColumnStyleInfo;
					AssertEquals("ColumnStyleInfo: TariffType", Universal.Constants.TariffTypes.Import, tariffColumnStyleInfo.TariffType);
					AssertEquals("ColumnStyleInfo: GetCountryCode", Core.Constants.CountryCodes.Germany, tariffColumnStyleInfo.GetCountryCode());
					AssertEquals("ColumnStyleInfo: GetDataGrouping", Core.Constants.CountryCodes.Germany, tariffColumnStyleInfo.GetDataGrouping());
					AssertEquals("ColumnStyleInfo: EffectiveDate", new ZDateTime(2020, 09, 08), tariffColumnStyleInfo.GetEffectiveDate.Invoke());
				});
			}
		}
	}
}
