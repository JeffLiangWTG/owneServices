using System.Collections.Generic;

namespace Enterprise.Customs.AE.Manifest.Business;

public interface IGoodsInfoProvider
{
	IGoodsDetailsProvider Goods { get; }

	IFreeTextProvider GoodsDescription { get; }

	IReadOnlyCollection<IMeasurementProvider> Measurements { get; }

	IGoodsContainerDetailsProvider GoodsContainer { get; }

	string GoodsMarksDescription { get; }

	string CustomsGoodsIdentifier { get; }

	ILocationProvider OriginCountry { get; }
}
