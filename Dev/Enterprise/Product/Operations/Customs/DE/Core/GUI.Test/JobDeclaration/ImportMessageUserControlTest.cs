using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ImportMessageUserControlTest : TestCaseWithFactory
	{
		public void TestEntriesBoundGrid_ColumnsWidth()
		{
			var grid = userControl.EntriesBoundGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Style", 100, grid.GetColumnStyle(CusEntryHeader.Schema.Style).Width);
				AssertEquals("Sub Style", 80, grid.GetColumnStyle(CusEntryHeader.Schema.SubStyle).Width);
				AssertEquals("Description", 80, grid.GetColumnStyle(CusEntryHeader.Schema.Description).Width);
				AssertEquals("LocalReferenceNumber", 100, grid.GetColumnStyle(CusEntryHeader.Schema.LocalReferenceNumber).Width);
			});
		}

		public void TestEntriesBoundGrid_ColumnsCaption()
		{
			var grid = userControl.EntriesBoundGrid;
			CombineAssertions(() =>
			{
				AssertEquals("Style", "Declaration Type", grid.GetColumnStyle(CusEntryHeader.Schema.Style).CaptionResourceString.Caption);
				AssertEquals("Sub Style", "Sub style", grid.GetColumnStyle(CusEntryHeader.Schema.SubStyle).CaptionResourceString.Caption);
				AssertEquals("Description", "Description", grid.GetColumnStyle(CusEntryHeader.Schema.Description).CaptionResourceString.Caption);
				AssertEquals("LocalReferenceNumber", "LRN", grid.GetColumnStyle(CusEntryHeader.Schema.LocalReferenceNumber).CaptionResourceString.Caption);
			});
		}

		public void TestEntriesBoundGrid_ColumnsOrder()
		{
			using (var userControl = new ImportMessageUserControl())
			{
				var columnNames = userControl.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
				AssertSequencesEqual("Columns", new[] { Customs.Business.CusEntryHeader.Schema.EntryNumber, Customs.Business.AutoCusEntryHeader.Schema.CH_BGMReference, Customs.Business.CusEntryHeader.Schema.PackagesCount,
					Customs.Business.CusEntryHeader.Schema.Duty, Customs.Business.CusEntryHeader.Schema.VAT, Customs.Business.AutoCusEntryHeader.Schema.CH_EntryStatus,
					$"{nameof(CusEntryHeader.CusEntryNumber)}+{Common.AutoCusEntryNum.Schema.CE_IssueDate}", Customs.Business.AutoCusEntryHeader.Schema.CH_EntrySubmittedDate, Customs.Business.AutoCusEntryHeader.Schema.CH_Status,
					Customs.Business.CusEntryHeader.Schema.MessageStatusDescription, Customs.Business.CusEntryHeader.Schema.MovementReferenceNumber, Customs.Business.AutoCusEntryHeader.Schema.CH_MessageType,
					Customs.Business.CusEntryHeader.Schema.CH_MessageTypeDescription, Customs.Business.CusEntryHeader.Schema.EntryHeaderStatusDescription, Customs.Business.CusEntryHeader.Schema.DeclarationUCR,
					Customs.Business.AutoCusEntryHeader.Schema.CH_EntryReleaseDate, EU.Business.Declaration.CusEntryHeader.Schema.EntryTypeFriendlyName, Customs.Business.AutoCusEntryHeader.Schema.CH_TotalPaid,
					Customs.Business.CusEntryHeader.Schema.CH_WarehouseTransactionStatus, Customs.Business.CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription, Customs.Business.AutoCusEntryHeader.Schema.CH_ExitedStatus,
					CusEntryHeader.Schema.Style, CusEntryHeader.Schema.SubStyle, CusEntryHeader.Schema.Description, CusEntryHeader.Schema.LocalReferenceNumber, },
					columnNames);
			}
		}

		public void TestEntryLineGrid_Columns()
		{
			CombineAssertions(() =>
			{
				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				var columns = entryLineGrid.ColumnStyles.Cast<ZGridColumnInfo>();

				var customsStatusColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.ZG_CustomsStatus));
				AssertEquals("ZG_CustomsStatus Column Caption", "Customs Status", customsStatusColumn.CaptionResourceString.Caption);
				AssertEquals("ZG_CustomsStatus Column Width", 85, customsStatusColumn.Width);
				AssertEquals("ZG_CustomsStatus Column IsReadOnly", true, customsStatusColumn.IsReadOnly);

				var customsStatusDescriptionColumn = columns.Single(x => x.ColumnName == nameof(CusEntryLine.CustomsStatusDescription));
				AssertEquals("CustomsStatusDescription Column Caption", "Customs Status Description", customsStatusDescriptionColumn.CaptionResourceString.Caption);
				AssertEquals("CustomsStatusDescription Column Width", 145, customsStatusDescriptionColumn.Width);
				AssertEquals("CustomsStatusDescription Column IsReadOnly", true, customsStatusDescriptionColumn.IsReadOnly);
			});
		}

		public void TestEntriesBoundGrid_WarehouseTransactionStatusColumnsVisible()
		{
			var entryHeaderLineGrid = userControl.FindSingle<ZGrid>("EntriesBoundGrid");

			CombineAssertions(() =>
			{
				var warehouseTransactionStatusColumn = entryHeaderLineGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus);
				AssertEquals("WarehouseTransactionStatus Column visible", true, warehouseTransactionStatusColumn.IsVisible);

				var warehouseTransactionStatusDescriptionColumn = entryHeaderLineGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatusDescription);
				AssertEquals("warehouseTransactionStatusDescriptionColumn Column visible", true, warehouseTransactionStatusDescriptionColumn.IsVisible);
			});
		}

		public void TestEntryLineAdditionalDataUserControl()
		{
			AssertType(typeof(EU.GUI.UCC6EntryLineAdditionalDataUserControl), userControl.FindSingle<EU.GUI.EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ImportMessageUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		ImportMessageUserControl userControl;
	}
}
