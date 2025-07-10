using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class JobComInvoiceLineValueCalculator : IJobComInvoiceLineValueCalculator
	{
		public JobComInvoiceLineValueCalculator(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public ZString GetTariffDescription(ZString tariff) => GetTariffDescriptionCore(tariff);
		protected virtual ZString GetTariffDescriptionCore(ZString tariff) => invoiceLine.UniversalTariff?.FullTariffDescription(invoiceLine.EffectiveAssessmentDate, includeSectionHeadings: false, includeChapterHeading: false, useTariffPreferredLanguage: true, countryPreferedLanguage: invoiceLine.LanguageForTariffDescription) ?? ZString.Empty;
	}
}
