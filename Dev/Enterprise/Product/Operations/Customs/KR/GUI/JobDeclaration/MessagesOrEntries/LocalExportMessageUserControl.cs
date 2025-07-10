using System.Windows.Forms;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	public partial class LocalExportMessageUserControl : EntriesAndMessagesUserControl
	{
		public LocalExportMessageUserControl()
		{
			InitializeComponent();
			RemoveHeaderColumns();
			AddHeaderColumns();
			AddLineColumns();
			RemoveLineColumns();
			ReOrderColumns();
			ReOrderDetailsTabPage();
		}

		void RemoveHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate));
			EntriesBoundGrid.ColumnStyles.Remove(EntriesBoundGrid.GetColumnStyle(nameof(CusEntryHeader.CustomsValueUSD)));
		}

		void AddHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.FormattedRefNumber),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_MessageType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.MessageTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.AcceptedDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.MostRecentCustomsReviewDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalGrossWeightInKG),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalPackages),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.KR_ActualDateOfLoading),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				}
			});
		}

		void AddLineColumns()
		{
			EntryLineGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = CusEntryLine.Schema.CL_Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.SupportingDocumentNo),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.SupportingDocumentType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true,
					IsVisible = false
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.SupportingDocumentTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.InvoiceQuantity),
					GroupName = Res.GetData("LocalExportMessageUserControl|055265A9-E0B1-413C-A412-C9539772D942", "Invoice Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					CharacterCasing = CharacterCasing.Upper,
					ColumnName = nameof(CusEntryLine.InvoiceUQ),
					GroupName = Res.GetData("LocalExportMessageUserControl|055265A9-E0B1-413C-A412-C9539772D942", "Invoice Qty"),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50),
					IsReadOnly = true
				},
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

		void RemoveLineColumns()
		{
			EntryLineGrid.ColumnStyles.Remove(EntryLineGrid.GetColumnStyle(nameof(CusEntryLine.TariffDescription)));
			EntryLineGrid.ColumnStyles.Remove(EntryLineGrid.GetColumnStyle(nameof(CusEntryLine.CountryOfOriginCode)));
			EntryLineGrid.ColumnStyles.Remove(EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CustomsQuantity));
			EntryLineGrid.ColumnStyles.Remove(EntryLineGrid.GetColumnStyle(CusEntryLine.Schema.CustomsUnitQty));
			EntryLineGrid.ColumnStyles.Remove(EntryLineGrid.GetColumnStyle(nameof(CusEntryLine.CustomsValueUSD)));
		}

		void ReOrderColumns()
		{
			EntriesBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
			EntryLineGrid.ReOrderColumnsAndChangeVisibility(lineColumns);
		}

		void ReOrderDetailsTabPage()
		{
			EntryLinesMessagesTabControl.Controls.Clear();
			EntryLinesMessagesTabControl.Controls.Add(this.MessageTabPage);
			EntryLinesMessagesTabControl.Controls.Add(this.EntryDetailsTabPage);
			EntryLinesMessagesTabControl.Controls.Add(this.EntryLinesTabPage);
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
				CusEntryHeader.Schema.CH_EntrySubmittedDate,
				nameof(CusEntryHeader.FormattedRefNumber),
				nameof(CusEntryHeader.AcceptedDate),
				nameof(CusEntryHeader.MostRecentCustomsReviewDate),
				nameof(CusEntryHeader.TotalGrossWeightInKG),
				nameof(CusEntryHeader.TotalPackages),
				nameof(CusEntryHeader.CustomsValue)
		};

		readonly string[] lineColumns =
		{
				CusEntryLine.Schema.CL_LineNumber,
				CusEntryLine.Schema.FormattedTariff,
				CusEntryLine.Schema.CL_Description,
				nameof(CusEntryLine.SupportingDocumentNo),
				nameof(CusEntryLine.SupportingDocumentType),
				nameof(CusEntryLine.SupportingDocumentTypeDescription),
				nameof(CusEntryLine.NetWeightInKG),
				nameof(CusEntryLine.InvoiceQuantity),
				nameof(CusEntryLine.InvoiceUQ),
				nameof(CusEntryLine.NoOfPacks),
				nameof(CusEntryLine.PackType),
				nameof(CusEntryLine.CL_CustomsValue),
		};
	}
}
