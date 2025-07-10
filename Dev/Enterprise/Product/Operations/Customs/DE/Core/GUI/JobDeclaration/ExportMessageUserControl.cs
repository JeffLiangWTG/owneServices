using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportMessageUserControl : EU.GUI.MessageUserControl
	{
		public ExportMessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
		}

		protected override bool SupportWarehouseTransactionStatusColumns => true;

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("B4FE6B41-3506-4E1C-8D19-E6AFA1839A15", "Type (Time)"),
					ColumnName = CusEntryHeader.Schema.SubStyle,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("055D4367-C472-4487-8A7C-3EDF54DEAA64", "Type (Procedure)"),
					ColumnName = CusEntryHeader.Schema.Style,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Res.GetData("182BC207-3440-41C2-B126-C0CD18B8BBEB", "Description"),
					ColumnName = CusEntryHeader.Schema.Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.LocalReferenceNumber,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true,
				}
			});
			EntriesBoundGrid.ReOrderColumns(ColumnsInOrder);
			EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_WarehouseTransactionStatus).IsVisible = true;
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_HasManualWhsUpdate));
		}

		string[] ColumnsInOrder
		{
			get
			{
				if (columnNamesInSortOrder == null)
				{
					columnNamesInSortOrder = new[]
					{
						Customs.Business.CusEntryHeader.Schema.EntryNumber,
						Customs.Business.AutoCusEntryHeader.Schema.CH_BGMReference,
						CusEntryHeader.Schema.LocalReferenceNumber,
						Customs.Business.CusEntryHeader.Schema.PackagesCount,
						Customs.Business.CusEntryHeader.Schema.Duty,
						Customs.Business.CusEntryHeader.Schema.VAT,
						Customs.Business.AutoCusEntryHeader.Schema.CH_EntryStatus,
						$"{nameof(CusEntryHeader.CusEntryNumber)}+{Common.AutoCusEntryNum.Schema.CE_IssueDate}", // constant string
						Customs.Business.AutoCusEntryHeader.Schema.CH_EntrySubmittedDate,
						Customs.Business.AutoCusEntryHeader.Schema.CH_Status,
						Customs.Business.CusEntryHeader.Schema.MessageStatusDescription,
						Customs.Business.CusEntryHeader.Schema.MovementReferenceNumber,
						Customs.Business.AutoCusEntryHeader.Schema.CH_MessageType,
						Customs.Business.CusEntryHeader.Schema.CH_MessageTypeDescription,
						Customs.Business.CusEntryHeader.Schema.EntryHeaderStatusDescription,
						Customs.Business.CusEntryHeader.Schema.DeclarationUCR,
						Customs.Business.AutoCusEntryHeader.Schema.CH_EntryReleaseDate,
						EU.Business.Declaration.CusEntryHeader.Schema.EntryTypeFriendlyName,
						CusEntryHeader.Schema.SubStyle,
						CusEntryHeader.Schema.Style,
						CusEntryHeader.Schema.Description
					};
				}
				return columnNamesInSortOrder;
			}
		}
		string[] columnNamesInSortOrder;
	}
}
