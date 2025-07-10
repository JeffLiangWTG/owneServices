using System.Collections.Generic;
using Enterprise.Customs.AE.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

public interface IConsignmentDetailsProvider
{
	IReadOnlyCollection<IMonetaryAmountProvider> MonetaryAmounts { get; }

	IReadOnlyCollection<ILocationProvider> Locations { get; }

	IReadOnlyCollection<IPartyFromOrgAddressProvider> Parties { get; }

	IReadOnlyCollection<IGoodsInfoProvider> Packs { get; }

	string ManifestNature { get; }
}
