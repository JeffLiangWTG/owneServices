using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class TariffSubGroup : FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var asycudaPackedItemQuery = new ZDBOnlySubQuery(typeof(AsycudaPackedItem), AsycudaPackedItemSchema.API_ABL_Bill);
		asycudaPackedItemQuery.AddToFilter(filter);

		return GetAsycudaMainQuery(AsycudaBillSchema.PK, asycudaPackedItemQuery);
	}
}
