using System;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IN.GUI;

public sealed class NonStandardExchangeRatesGridColumnBag
{
	public static NonStandardExchangeRatesGridColumnBag Instance => instance ??= new NonStandardExchangeRatesGridColumnBag();

	[ThreadStatic]
	static NonStandardExchangeRatesGridColumnBag instance;

	NonStandardExchangeRatesGridColumnBag()
	{
		CurrencyTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusSupportingInfo.Schema.CSI_RX_NKCurrency, 40);
		BankNameTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusSupportingInfo.Schema.CSI_Description, 100);
		EffectiveDateDateTimeOffsetEditColumn = new GridColumnReference<ZDateTimeOffsetEditColumnStyleInfo>(CusSupportingInfo.Schema.CSI_EffectiveDate, 100, x => x.DateTimeFormat = ZDateTimePickerFormat.Short);
		CertificateNumberTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusSupportingInfo.Schema.CSI_ReferenceNumber, 100);
		CertificateDateDateEditColumn = new GridColumnReference<ZDateEditColumnStyleInfo>(CusSupportingInfo.Schema.CSI_DateOfIssue, 100, x => x.DateTimeFormat = ZDateTimePickerFormat.Short);
		UnitInINRCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusSupportingInfo.Schema.CSI_Quantity, 100);
		ExchangeRateCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusSupportingInfo.Schema.CSI_Value, 100);
		ExchangeRatePrintInfoTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusSupportingInfo.Schema.CSI_AdditionalDescription, 100);
	}

	public IGridColumnReference CurrencyTextBoxColumn { get; }

	public IGridColumnReference BankNameTextBoxColumn { get; }

	public IGridColumnReference EffectiveDateDateTimeOffsetEditColumn { get; }

	public IGridColumnReference CertificateNumberTextBoxColumn { get; }

	public IGridColumnReference CertificateDateDateEditColumn { get; }

	public IGridColumnReference UnitInINRCalcEditColumn { get; }

	public IGridColumnReference ExchangeRateCalcEditColumn { get; }

	public IGridColumnReference ExchangeRatePrintInfoTextBoxColumn { get; }
}
