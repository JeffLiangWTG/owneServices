using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Customs.Shared.MessageContracts;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ImportSpecificRateProvider : IImportSpecificRate
	{
		public ImportSpecificRateProvider(string chargeType, decimal amount)
		{
			Type = chargeType.SubstringOrNull(2, 1);
			Value = amount;
		}

		public string Type { get; }

		public decimal Value { get; }
	}
}
