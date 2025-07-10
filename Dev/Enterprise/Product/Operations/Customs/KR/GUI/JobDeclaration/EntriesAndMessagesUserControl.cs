using System;
using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	public partial class EntriesAndMessagesUserControl : Customs.GUI.ImportMessageUserControl
	{
		protected override Type GetBaseMessagesTabUserControlType() => typeof(MessagesTabUserControl);

		public EntriesAndMessagesUserControl()
		{
			RemoveHeaderColumns();
			AddHeaderColumns();
			AddLineColumns();
		}

		void RemoveHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(Enterprise.Customs.Business.CusEntryHeader.Schema.EntryNumber));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(Enterprise.Customs.Business.CusEntryHeader.Schema.CH_BGMReference));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(Enterprise.Customs.Business.CusEntryHeader.Schema.CH_MessageType));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(Enterprise.Customs.Business.CusEntryHeader.Schema.CH_MessageTypeDescription));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(Enterprise.Customs.Business.CusEntryHeader.Schema.PackagesCount));
		}

		void AddHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryHeader.FormattedEntryNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = CusEntryHeader.Schema.CH_Status,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryHeader.MessageStatusDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = CusEntryHeader.Schema.CH_EntryStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryHeader.EntryHeaderStatusDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_EntrySubmittedDate,
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = CusEntryHeader.Schema.CH_EntryReleaseDate,
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CustomsValue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CustomsValueUSD),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145),
					IsReadOnly = true
				}
			});
		}

		void AddLineColumns()
		{
			EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_LineNumber).Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			EntryLineGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = CusEntryLine.Schema.FormattedTariff,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.TariffDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(310),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.CountryOfOriginCode),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.NetWeightInKG),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsQuantity),
					GroupName = Res.GetData("ExportMessageUserControl|8D66E5CE-9E5A-4595-9011-3DD7F9603125", "Customs Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.CustomsUnitQty),
					GroupName = Res.GetData("ExportMessageUserControl|8D66E5CE-9E5A-4595-9011-3DD7F9603125", "Customs Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_CustomsValue),
					GroupName = Res.GetData("ExportMessageUserControl|06AA8E9A-684A-411C-8AF4-D5A95B41CB32", "Total Customs Value"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CustomsValueUSD),
					GroupName = Res.GetData("ExportMessageUserControl|06AA8E9A-684A-411C-8AF4-D5A95B41CB32", "Total Customs Value"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				}
			});
		}
	}
}
