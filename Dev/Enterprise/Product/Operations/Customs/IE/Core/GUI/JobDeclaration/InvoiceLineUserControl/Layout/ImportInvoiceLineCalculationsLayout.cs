using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI
{
	public sealed class ImportInvoiceLineCalculationsLayout : IPanelLayoutProvider
	{
		PanelLayout Layout { get; } = CreateLayout();

		PanelLayout IPanelLayoutProvider.Layout => Layout;

		static PanelLayout CreateLayout()
		{
			var euBag = InvoiceLineSummaryControlBag.Instance;
			var builder = new ImportInvoiceLineCalculationsLayoutBuilder();
			var commonBag = builder.CommonBag;
			builder.AddControlBag(euBag);

			builder.AddColumn();
			builder.Add(commonBag.CurrentInvoiceLabel, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.BalanceConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(commonBag.LinesEnteredConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(commonBag.LinesTotalConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(commonBag.SummaryLabel, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(commonBag.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(euBag.GSTVATDeferredConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(euBag.CustomsValueConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(euBag.ValueForVatConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(euBag.StatisticalValueConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);
			builder.Add(commonBag.CIFConvertToLocalCurrencyControl, ControlWidthClass.CustomWidth);

			return builder.Build();
		}
	}
}
