using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public class ImportCostsProvider : IImportCosts
	{
		public ImportCostsProvider(decimal value, string currency, ZDecimal rate, bool isFixed)
		{
			CurrencyRateAgreedFlag = isFixed;
			Value = value;
			CurrencyCode = currency;
			CurrencyRate = isFixed ? rate.FormatDecimal(9) : ZDecimal.Zero;
		}

		public decimal Value { get; }

		public string CurrencyCode { get; }

		public bool CurrencyRateAgreedFlag { get; }

		public decimal CurrencyRate { get; }
	}
}
