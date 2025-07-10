using System;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportMessageUserControl : EntriesAndMessagesUserControl
	{
		public ImportMessageUserControl()
		{
			InitializeComponent();
			AddHeaderColumns();
			AddLineColumns();
			ReOrderColumns();
			ReOrderDetailsTabPage();
		}

		void AddHeaderColumns()
		{
			EntriesBoundGrid.ColumnStyles.AddRange(new ZTextBoxColumnStyleInfo[]
			{
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.CH_MessageType),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.MessageTypeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					IsReadOnly = true
				},
				new ZDateEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.AcceptedDate),
					DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.Freight),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.Insurance),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.AdditionalAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.DeductedAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalAmountPayable),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.FormattedTotalDutyAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalVAT),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalValueForVAT),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalVATExemptionValue),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalSpecialConsumptionTax),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalTransportationTax),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalLiquorTax),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalEducationTax),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalAgricultureTax),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.PenaltyForLateDeclaration),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.PenaltyForMissedDeclaration),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.TotalInvoiceAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.InvoiceAmountCurrency),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
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
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryHeader.PackagesUQ),
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
					ColumnName = nameof(CusEntryLine.DutyRateCodeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZTextBoxColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.PreferenceCodeDescription),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.DutyRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.DutyFeeAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.DutyReductionRate),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_DutyReductionAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.VATAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.DomesticTaxAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.EducationTaxAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.AgricultureTaxAmount),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = CusEntryLine.Schema.CL_ValueForVAT,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					IsReadOnly = true
				},
				new ZCalcEditColumnStyleInfo
				{
					ColumnName = nameof(CusEntryLine.CL_ValueExemptForVAT),
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140),
					IsReadOnly = true
				}
			});
		}

		void ReOrderColumns()
		{
			EntriesBoundGrid.ReOrderColumnsAndChangeVisibility(headerColumns);
			EntryLineGrid.ReOrderColumnsAndChangeVisibility(lineColumns);
		}

		void ReOrderDetailsTabPage()
		{
			EntryLinesMessagesTabControl.Controls.Clear();
			EntryLinesMessagesTabControl.Controls.Add(MessageTabPage);
			EntryLinesMessagesTabControl.Controls.Add(EntryDetailsTabPage);
			EntryLinesMessagesTabControl.Controls.Add(EntryLinesTabPage);
			EntryLinesMessagesTabControl.Controls.Add(SubsequentMessagesTabPage);
		}
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EntryDetailsInnerPanel.UpdateLayout(new EntryDetailsLayout());
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
			nameof(CusEntryHeader.AcceptedDate),
			CusEntryHeader.Schema.CH_EntryReleaseDate,
			nameof(CusEntryHeader.CustomsValue),
			nameof(CusEntryHeader.CustomsValueUSD),
			nameof(CusEntryHeader.Freight),
			nameof(CusEntryHeader.Insurance),
			nameof(CusEntryHeader.AdditionalAmount),
			nameof(CusEntryHeader.DeductedAmount),
			nameof(CusEntryHeader.TotalAmountPayable),
			nameof(CusEntryHeader.FormattedTotalDutyAmount),
			nameof(CusEntryHeader.TotalVAT),
			nameof(CusEntryHeader.TotalValueForVAT),
			nameof(CusEntryHeader.TotalVATExemptionValue),
			nameof(CusEntryHeader.TotalSpecialConsumptionTax),
			nameof(CusEntryHeader.TotalTransportationTax),
			nameof(CusEntryHeader.TotalLiquorTax),
			nameof(CusEntryHeader.TotalEducationTax),
			nameof(CusEntryHeader.TotalAgricultureTax),
			nameof(CusEntryHeader.PenaltyForLateDeclaration),
			nameof(CusEntryHeader.PenaltyForMissedDeclaration),
			nameof(CusEntryHeader.TotalInvoiceAmount),
			nameof(CusEntryHeader.InvoiceAmountCurrency),
			nameof(CusEntryHeader.TotalGrossWeightInKG),
			nameof(CusEntryHeader.TotalPackages),
			nameof(CusEntryHeader.PackagesUQ),
		};

		readonly string[] lineColumns =
		{
				CusEntryLine.Schema.CL_LineNumber,
				CusEntryLine.Schema.FormattedTariff,
				nameof(CusEntryLine.DutyRateCodeDescription),
				nameof(CusEntryLine.PreferenceCodeDescription),
				nameof(CusEntryLine.DutyRate),
				nameof(CusEntryLine.TariffDescription),
				nameof(CusEntryLine.CountryOfOriginCode),
				nameof(CusEntryLine.NetWeightInKG),
				CusEntryLine.Schema.CustomsQuantity,
				nameof(CusEntryLine.CustomsUnitQty),
				CusEntryLine.Schema.CL_CustomsValue,
				nameof(CusEntryLine.CustomsValueUSD),
				nameof(CusEntryLine.DutyFeeAmount),
				nameof(CusEntryLine.DutyReductionRate),
				nameof(CusEntryLine.CL_DutyReductionAmount),
				nameof(CusEntryLine.VATAmount),
				nameof(CusEntryLine.DomesticTaxAmount),
				nameof(CusEntryLine.EducationTaxAmount),
				nameof(CusEntryLine.AgricultureTaxAmount),
				CusEntryLine.Schema.CL_ValueForVAT,
				nameof(CusEntryLine.CL_ValueExemptForVAT)
		};
	}
}
