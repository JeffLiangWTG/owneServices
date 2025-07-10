using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class TariffWrapper : ITariff
	{
		public TariffWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		ZString ITariff.Code => invoiceLine.JI_Tariff;

		ZString ITariff.Description => invoiceLine.TariffDescription;

		ZString ITariff.UQ1 => invoiceLine.CustomsUQ;

		ZString ITariff.UQ2 => invoiceLine.SecondUQ;

		ZString ITariff.UQ3 => ZString.Empty;

		ZString ITariff.UQ4 => ZString.Empty;

		ZString ITariff.UQ5 => ZString.Empty;
	}
}
