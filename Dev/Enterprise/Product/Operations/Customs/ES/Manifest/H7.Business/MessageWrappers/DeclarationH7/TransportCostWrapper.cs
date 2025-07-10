using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.ES.Manifest.H7.Business;

public class TransportCostWrapper(AsycudaBill bill) : IH7Value
{
	public ZDecimal Amount
	{
		get
		{
			var insuranceValue = bill.ABL_InsuranceValue;

			if (!bill.ABL_RX_NKInsuranceValueCurrency.EqualsIgnoringCase(bill.ABL_RX_NKTransportValueCurrency))
			{
				var currencyConverter = bill.Header.CurrencyConverter;
				var insuranceCurrency = new Currency(bill.ABL_RX_NKInsuranceValueCurrency);
				var transportCurrency = new Currency(bill.ABL_RX_NKTransportValueCurrency);
				var insuranceMoney = new Money(insuranceValue, insuranceCurrency);
				var convertedInsuranceMoney = currencyConverter.ConvertRounded(insuranceMoney, transportCurrency);
				insuranceValue = convertedInsuranceMoney.Amount;
			}

			var totalValue = (ZDecimal)(insuranceValue + bill.ABL_TransportValue);
			return totalValue.Round(2);
		}
	}

	public ZString CurrencyCode => bill.ABL_RX_NKTransportValueCurrency;
}
