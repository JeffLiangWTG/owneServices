using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class ExportMessageUserControlTest : TestCaseWithFactory
	{
		public void TestEntriesBoundGrid_ColumnsWidth()
		{
			using (var userControl = new ExportMessageUserControl())
			{
				var grid = userControl.EntriesBoundGrid;
				CombineAssertions(() =>
				{
					AssertEquals("SubStyle", 80, grid.GetColumnStyle(CusEntryHeader.Schema.SubStyle).Width);
					AssertEquals("Style", 100, grid.GetColumnStyle(CusEntryHeader.Schema.Style).Width);
					AssertEquals("Description", 80, grid.GetColumnStyle(CusEntryHeader.Schema.Description).Width);
					AssertEquals("LocalReferenceNumber", 100, grid.GetColumnStyle(CusEntryHeader.Schema.LocalReferenceNumber).Width);
				});
			}
		}

		public void TestEntriesBoundGrid_ColumnsCaption()
		{
			using (var userControl = new ExportMessageUserControl())
			{
				var grid = userControl.EntriesBoundGrid;
				CombineAssertions(() =>
				{
					AssertEquals("SubStyle", "Type (Time)", grid.GetColumnStyle(CusEntryHeader.Schema.SubStyle).CaptionResourceString.Caption);
					AssertEquals("Style", "Type (Procedure)", grid.GetColumnStyle(CusEntryHeader.Schema.Style).CaptionResourceString.Caption);
					AssertEquals("Description", "Description", grid.GetColumnStyle(CusEntryHeader.Schema.Description).CaptionResourceString.Caption);
				});
			}
		}

		public void TestEntriesBoundGrid_ColumnsOrder()
		{
			using (var userControl = new ExportMessageUserControl())
			{
				var columnNames = userControl.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertSequencesEqual("Columns", new[] { Customs.Business.CusEntryHeader.Schema.EntryNumber, Customs.Business.AutoCusEntryHeader.Schema.CH_BGMReference, CusEntryHeader.Schema.LocalReferenceNumber,
					Customs.Business.CusEntryHeader.Schema.PackagesCount, Customs.Business.CusEntryHeader.Schema.Duty, Customs.Business.CusEntryHeader.Schema.VAT,
					Customs.Business.AutoCusEntryHeader.Schema.CH_EntryStatus, $"{nameof(CusEntryHeader.CusEntryNumber)}+{Common.AutoCusEntryNum.Schema.CE_IssueDate}",
					Customs.Business.AutoCusEntryHeader.Schema.CH_EntrySubmittedDate, Customs.Business.AutoCusEntryHeader.Schema.CH_Status, Customs.Business.CusEntryHeader.Schema.MessageStatusDescription,
					Customs.Business.CusEntryHeader.Schema.MovementReferenceNumber, Customs.Business.AutoCusEntryHeader.Schema.CH_MessageType, Customs.Business.CusEntryHeader.Schema.CH_MessageTypeDescription,
					Customs.Business.CusEntryHeader.Schema.EntryHeaderStatusDescription, Customs.Business.CusEntryHeader.Schema.DeclarationUCR, Customs.Business.AutoCusEntryHeader.Schema.CH_EntryReleaseDate,
					EU.Business.Declaration.CusEntryHeader.Schema.EntryTypeFriendlyName, CusEntryHeader.Schema.SubStyle, CusEntryHeader.Schema.Style, CusEntryHeader.Schema.Description, Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid,
					Customs.Business.CusEntryHeader.Schema.CH_WarehouseTransactionStatus, Customs.Business.CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription, 
					Customs.Business.AutoCusEntryHeader.Schema.CH_ExitedStatus },
					columnNames);
			}
		}

		public void TestEntriesBoundGrid_WarehouseTransactionStatusColumnsVisible()
		{
			using (var userControl = new ExportMessageUserControl())
			{
				var entryHeaderLineGrid = userControl.EntriesBoundGrid;

				CombineAssertions(() =>
				{
					var warehouseTransactionStatusColumn = entryHeaderLineGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus);
					AssertEquals("WarehouseTransactionStatus Column visible", true, warehouseTransactionStatusColumn.IsVisible);

					var warehouseTransactionStatusDescriptionColumn = entryHeaderLineGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription);
					AssertEquals("warehouseTransactionStatusDescriptionColumn Column visible", true, warehouseTransactionStatusDescriptionColumn.IsVisible);
				});
			}
		}
	}
}
