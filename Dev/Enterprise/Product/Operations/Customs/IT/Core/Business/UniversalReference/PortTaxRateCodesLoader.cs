using System.Collections.Immutable;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.Business;

public interface IPortTaxRateCodesLoader
{
	ImmutableHashSet<ZString> GetAllRateCodes();
}

sealed class PortTaxRateCodesLoader : IPortTaxRateCodesLoader
{
	public PortTaxRateCodesLoader(BusinessObjectFactory factory, ZDateTime valuationDate)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.valuationDate = valuationDate;
	}

	ImmutableHashSet<ZString> IPortTaxRateCodesLoader.GetAllRateCodes()
	{
		var harbourRateLoader = new RefHarbourRate.Loader(factory);
		var harbourRates = harbourRateLoader.Load(
			UniversalReferenceConstants.RefHarbourRateTypeCode.Taxes,
			Core.Constants.CountryCodes.Italy,
			Core.Constants.ContainerModes.All,
			valuationDate,
			port: ZString.Empty);

		return harbourRates
			.Where(x => !x.ZXF_PortTaxType.IsEmpty)
			.Select(x => x.ZXF_PortTaxType)
			.ToImmutableHashSet();
	}

	readonly BusinessObjectFactory factory;
	readonly ZDateTime valuationDate;
}
