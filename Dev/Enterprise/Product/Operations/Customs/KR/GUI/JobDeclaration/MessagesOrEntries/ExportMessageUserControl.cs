using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ExportMessageUserControl : EntriesAndMessagesUserControl
	{
		public ExportMessageUserControl()
		{
			InitializeComponent();
			AddHeaderColumns();
			AddLineColumns();
			ReOrderColumns();
			ReOrderDetailsTabPage();
		}

		protected override void ChangeControlsVisibility()
		{
			FixedInspectionDateEdit.Visible = !((JobDeclaration)JobDeclaration).IsInspectionDateRelevant;
			if (!FixedInspectionDateEdit.Visible)
			{
				EntryDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(357);
			}
			else
			{
				EntryDetailsGroupBox.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(327);
			}
		}

		void ReOrderDetailsTabPage()
		{
			EntryLinesMessagesTabControl.Controls.Clear();
			EntryLinesMessagesTabControl.Controls.Add(this.MessageTabPage);
			EntryLinesMessagesTabControl.Controls.Add(this.EntryDetailsTabPage);
			EntryLinesMessagesTabControl.Controls.Add(this.EntryLinesTabPage);
		}

		void AddHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.AcceptedDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.DueDateofLoading),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.KR_ActualDateOfLoading),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				}
			});
		}

		void AddLineColumns()
		{
			EntryLineGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.NoOfPacks),
					GroupName = Res.GetData("ExportMessageUserControl|FC150A45-0D97-483C-9DBA-F975C64F4CD1", "Packages Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.PackType),
					GroupName = Res.GetData("ExportMessageUserControl|FC150A45-0D97-483C-9DBA-F975C64F4CD1", "Packages Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				}
			});
		}

		void ReOrderColumns()
		{
			EntriesBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
			EntryLineGrid.ReOrderColumnsAndChangeVisibility(lineColumns);
		}

		readonly string[] headerColumns =
		{
				nameof(CusEntryHeader.FormattedEntryNumber),
				CusEntryHeader.Schema.CH_Status,
				nameof(CusEntryHeader.MessageStatusDescription),
				CusEntryHeader.Schema.CH_EntryStatus,
				nameof(CusEntryHeader.EntryHeaderStatusDescription),
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				nameof(CusEntryHeader.AcceptedDate),
				CusEntryHeader.Schema.CH_EntryReleaseDate,
				nameof(CusEntryHeader.DueDateofLoading),
				nameof(CusEntryHeader.KR_ActualDateOfLoading),
				nameof(CusEntryHeader.CustomsValue),
				nameof(CusEntryHeader.CustomsValueUSD)
		};

		readonly string[] lineColumns =
		{
				CusEntryLine.Schema.CL_LineNumber,
				CusEntryLine.Schema.FormattedTariff,
				nameof(CusEntryLine.TariffDescription),
				nameof(CusEntryLine.CountryOfOriginCode),
				nameof(CusEntryLine.NetWeightInKG),
				nameof(CusEntryLine.CustomsQuantity),
				nameof(CusEntryLine.CustomsUnitQty),
				nameof(CusEntryLine.NoOfPacks),
				nameof(CusEntryLine.PackType),
				nameof(CusEntryLine.CL_CustomsValue),
				nameof(CusEntryLine.CustomsValueUSD)
		};
	}
}
