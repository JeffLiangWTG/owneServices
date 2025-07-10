using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.H7.Business
{
	public class TransportCostsProvider : ITransportCosts, IMoney
	{
		public TransportCostsProvider(decimal amount, string currency)
		{
			Amount = amount;
			Currency = currency;
		}

		public static TransportCostsProvider NewOrNull(AsycudaBill bill)
		{
			var amount = bill.ABL_InsuranceValue + bill.ABL_TransportValue;
			var roundedAmount = Utilities.Round(amount, AmountDecimalPlaces);
			var currency = GetValidCurrency(amount, bill.ABL_RX_NKTransportValueCurrency);
			return new TransportCostsProvider(roundedAmount, currency);
		}

		public static TransportCostsProvider NewOrNullPerPackedItem(AsycudaBill bill)
		{
			var packedItemQuantity = bill.PackedItems.Count;
			var amount = bill.ABL_InsuranceValue + bill.ABL_TransportValue;
			if (packedItemQuantity == 0)
			{
				return null;
			}

			var currency = GetValidCurrency(amount, bill.ABL_RX_NKTransportValueCurrency);
			var costsToTheDestinationAmount = Utilities.Round(amount / packedItemQuantity, AmountDecimalPlaces);
			return new TransportCostsProvider(costsToTheDestinationAmount, currency);
		}

		static string GetValidCurrency(decimal amount, string currency)
		{
			return amount == 0 ? null : currency;
		}

		public decimal Amount { get; }

		public string Currency { get; }

		static int AmountDecimalPlaces => 2;
	}
}
