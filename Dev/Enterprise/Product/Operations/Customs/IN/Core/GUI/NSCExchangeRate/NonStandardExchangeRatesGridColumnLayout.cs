using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public class NonStandardExchangeRatesGridColumnLayout : IGridColumnLayoutProvider
{
	public IGridColumnLayout Layout => layout ??= CreateLayout();
	IGridColumnLayout layout;

	#region Implementation

	IGridColumnLayout CreateLayout()
	{
		var columnBag = NonStandardExchangeRatesGridColumnBag.Instance;

		var builder = GridColumnLayoutBuilder.Create();
		builder.AddColumn(columnBag.CurrencyTextBoxColumn);
		builder.AddColumn(columnBag.BankNameTextBoxColumn);
		builder.AddColumn(columnBag.EffectiveDateDateTimeOffsetEditColumn);
		builder.AddColumn(columnBag.CertificateNumberTextBoxColumn);
		builder.AddColumn(columnBag.CertificateDateDateEditColumn);
		builder.AddColumn(columnBag.UnitInINRCalcEditColumn);
		builder.AddColumn(columnBag.ExchangeRateCalcEditColumn);
		builder.AddColumn(columnBag.ExchangeRatePrintInfoTextBoxColumn);

		return builder.Build();
	}

	#endregion
}
