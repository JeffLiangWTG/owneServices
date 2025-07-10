using System.Windows.Forms;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI
{
	public partial class InvoiceLineChargesUserControl : EU.GUI.InvoiceLineChargesUserControl
	{
		public InvoiceLineChargesUserControl()
		{
			InitializeChargesGridLayout();
			InitializeApportionedChargesGridLayout();
		}

		void InitializeApportionedChargesGridLayout()
		{
			using (ApportionedChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ApportionedChargesGrid.ColumnStyles.Add(CreateFixedRateColumn());
				ApportionedChargesGrid.ColumnStyles.Add(CreateExchangeRateDateColumn());
				ApportionedChargesGrid.ReOrderColumns(
					[
						InvoiceLineCharge.Schema.J7_ChargeType,
						InvoiceLineCharge.Schema.ChargeCodeDescription,
						InvoiceLineCharge.Schema.J7_Amount,
						InvoiceLineCharge.Schema.J7_RX_NKCurrency,
						InvoiceLineCharge.Schema.J7_IsDutiable,
						InvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable,
						InvoiceLineCharge.Schema.J7_IsGSTApplicable,
						InvoiceLineCharge.Schema.J7_IsIncludedInITOT,
						InvoiceLineCharge.Schema.J7_FullOrPartialApportionment,
						InvoiceLineCharge.Schema.IsJ7_ExchangeRateIATA,
						InvoiceLineCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						InvoiceLineCharge.Schema.J7_ExchangeRate,
						InvoiceLineCharge.Schema.J7_ExchangeRateDate
					]);
			}
		}

		void InitializeChargesGridLayout()
		{
			using (ChargesGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				ChargesGrid.RemoveFromAvailableColumns(InvoiceLineCharge.Schema.ChargeCodeDescription);
				ChargesGrid.ColumnStyles.Add(CreateFixedRateColumn());
				ChargesGrid.ColumnStyles.Add(CreateExchangeRateDateColumn());
				ChargesGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					CaptionResourceString = Res.GetData("F8588EBD-9EEE-486E-A86B-77F9C7E94227", "Description"),
					CharacterCasing = CharacterCasing.Normal,
					ColumnName = InvoiceLineCharge.Schema.J7_ChargeDescription,
					IsMandatory = true,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(184)
				});
				ChargesGrid.ReOrderColumns(
					[
						InvoiceLineCharge.Schema.J7_ChargeType,
						InvoiceLineCharge.Schema.J7_ChargeDescription,
						InvoiceLineCharge.Schema.J7_Amount,
						InvoiceLineCharge.Schema.J7_RX_NKCurrency,
						InvoiceLineCharge.Schema.J7_IsDutiable,
						InvoiceLineCharge.Schema.J7_IsStatisticalValueApplicable,
						InvoiceLineCharge.Schema.J7_IsGSTApplicable,
						InvoiceLineCharge.Schema.J7_Percentage,
						InvoiceLineCharge.Schema.J7_IsIncludedInITOT,
						InvoiceLineCharge.Schema.J7_Calc_IsIncludedInInvoiceAmount,
						InvoiceLineCharge.Schema.IsJ7_ExchangeRateIATA,
						InvoiceLineCharge.Schema.IsJ7_ExchangeRateUserEnterable,
						InvoiceLineCharge.Schema.J7_ExchangeRate,
						InvoiceLineCharge.Schema.J7_ExchangeRateDate
					]);
			}
		}

		static ZCheckBoxColumnStyleInfo CreateFixedRateColumn()
		{
			return new ZCheckBoxColumnStyleInfo()
			{
				ColumnName = InvoiceLineCharge.Schema.IsJ7_ExchangeRateIATA,
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47)
			};
		}

		static ZDateEditColumnStyleInfo CreateExchangeRateDateColumn()
		{
			return new ZDateEditColumnStyleInfo()
			{
				CaptionResourceString = Res.GetData("1D1C91C5-5535-4538-8750-CA07052500F7", "Exchange Rate Date"),
				ColumnName = InvoiceLineCharge.Schema.J7_ExchangeRateDate,
				IsMandatory = true,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(121),
				DateTimeFormat = ZArchitecture.Core.ZDateTimePickerFormat.Short
			};
		}
	}
}
