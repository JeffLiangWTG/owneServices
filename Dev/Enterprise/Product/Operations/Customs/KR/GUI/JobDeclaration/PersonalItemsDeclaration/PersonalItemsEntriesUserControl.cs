using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	public partial class PersonalItemsEntriesUserControl : EntriesAndMessagesUserControl
	{
		public PersonalItemsEntriesUserControl()
		{
			InitializeComponent();
			AddHeaderColumns();
			RemoveHeaderColumns();
			ReOrderColumns();
			ReOrderDetailsTabPage();
		}

		void ReOrderDetailsTabPage()
		{
			EntryLinesMessagesTabControl.Controls.Clear();
			EntryLinesMessagesTabControl.Controls.Add(this.MessageTabPage);
		}

		void AddHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryHeader.CH_MessageType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryHeader.MessageTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180),
					IsReadOnly = true
				},
			});
		}
		void RemoveHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(Enterprise.Customs.Business.CusEntryHeader.Schema.CH_EntryReleaseDate));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(nameof(CusEntryHeader.CustomsValue)));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(nameof(CusEntryHeader.CustomsValueUSD)));
		}

		void ReOrderColumns()
		{
			EntriesBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
		}

		readonly string[] headerColumns =
		{
				nameof(CusEntryHeader.FormattedEntryNumber),
				CusEntryHeader.Schema.CH_MessageType,
				nameof(CusEntryHeader.MessageTypeDescription),
				CusEntryHeader.Schema.CH_Status,
				nameof(CusEntryHeader.MessageStatusDescription),
				CusEntryHeader.Schema.CH_EntryStatus,
				nameof(CusEntryHeader.EntryHeaderStatusDescription),
				CusEntryHeader.Schema.CH_EntrySubmittedDate
		};
	}
}
