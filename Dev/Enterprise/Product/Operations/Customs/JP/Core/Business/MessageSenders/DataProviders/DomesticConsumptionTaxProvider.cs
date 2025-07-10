using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions.Outbound;

namespace Enterprise.Customs.JP.Business
{
	sealed class DomesticConsumptionTaxProvider : IDomesticConsumptionTax
	{
		public DomesticConsumptionTaxProvider(CusLineTariffDetail tariff)
		{
			Argument.NotNull(tariff, nameof(tariff));
			this.tariff = tariff;
		}

		readonly CusLineTariffDetail tariff;

		public string Type => tariff.BZ_Tariff;

		public string ReductionCode => tariff.BZ_ExemptionReductionCode;

		public decimal? ReductionAmount => tariff.BZ_Value;
	}
}
