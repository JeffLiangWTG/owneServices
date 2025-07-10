using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobComInvoiceLineHarbourRateProvider : IHarbourRateProvider
{
	public JobComInvoiceLineHarbourRateProvider(JobComInvoiceLine invoiceLine)
	{
		this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
	}

	RefHarbourRate IHarbourRateProvider.HarbourRate => Factory.GetValue(ref harbourRateCached, () =>
	{
		var portCode = invoiceLine.Declaration?.BarrierPort ?? ZString.Empty;
		var portTaxRate = invoiceLine.ZG_PortTaxRate;

		if (portCode.IsEmpty || portTaxRate.IsEmpty)
		{
			return null;
		}

		var rateLoader = new RefHarbourRate.Loader(Factory);
		var harbourRates = rateLoader.Load(
			UniversalReferenceConstants.RefHarbourRateTypeCode.Taxes,
			Core.Constants.CountryCodes.Italy,
			Core.Constants.ContainerModes.All,
			invoiceLine.EffectiveAssessmentDate,
			portCode,
			commodity: portTaxRate);
		return harbourRates.FirstOrDefault();
	});

	BusinessObjectFactory Factory => invoiceLine.Factory;

	CachedProperty<RefHarbourRate> harbourRateCached;

	readonly JobComInvoiceLine invoiceLine;
}
