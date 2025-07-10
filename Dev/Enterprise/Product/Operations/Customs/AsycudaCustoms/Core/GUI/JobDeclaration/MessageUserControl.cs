using System.Windows.Forms;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	public partial class MessageUserControl : Customs.GUI.ImportMessageUserControl
	{
		public MessageUserControl()
		{
			InitializeComponent();
			SetupEntryHeaderColumns();
		}
		protected override bool SupportWarehouseTransactionStatusColumns
		{
			get { return true; }
		}

		void SetupEntryHeaderColumns()
		{
			EntriesBoundGrid.ReadOnly = false;

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.GUI.ZDropEditColumnStyleInfo
			{
				CaptionResourceString = Res.GetData("43062377-3840-4C87-B70C-17DDFDAA316A", "Entry Status"),
				ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(82)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CharacterCasing = CharacterCasing.Upper,
				IsReadOnly = true,
				ColumnName = CusEntryHeader.Schema.EntryHeaderStatusDescription,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CH_BondAcquittedDate,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CH_BondValidToDate,
				DateTimeFormat = ZDateTimePickerFormat.Short,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.CustomsValue,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			EntriesBoundGrid.ColumnStyles.Add(new ZArchitecture.ZCalcEditColumnStyleInfo
			{
				ColumnName = CusEntryHeader.Schema.ValueForVAT,
				IsReadOnly = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			});

			var entryNumberColumn = EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber);
			entryNumberColumn.IsReadOnly = false;
			entryNumberColumn.CharacterCasing = CharacterCasing.Upper;
		}
	}
}
