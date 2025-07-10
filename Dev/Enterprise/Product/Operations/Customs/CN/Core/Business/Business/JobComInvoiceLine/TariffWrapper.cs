using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CN.Business
{
	public class TariffWrapper : ITariff
	{
		public TariffWrapper(JobComInvoiceLine invoiceLine)
		{
			tariff = invoiceLine.UniversalTariff;
		}

		public TariffView tariff;
		ZString ITariff.Code => tariff.ZZ1_TariffCode;
		ZString ITariff.Description => tariff.ZZ1_Description;
		ZString ITariff.UQ1 => tariff.ZZ1_ZZ8_UQ1;
		ZString ITariff.UQ2 => tariff.ZZ1_ZZ8_UQ2;
		ZString ITariff.UQ3 => ZString.Empty;
		ZString ITariff.UQ4 => ZString.Empty;
		ZString ITariff.UQ5 => ZString.Empty;
	}
}
