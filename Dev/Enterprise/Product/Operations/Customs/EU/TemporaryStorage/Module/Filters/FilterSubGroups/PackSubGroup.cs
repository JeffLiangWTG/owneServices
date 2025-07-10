using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.TemporaryStorage.Module;

sealed class PackSubGroup : FilterSubGroup
{
	public override ZQuery GetSubQuery(ZQuery filter)
	{
		var asycudaPackQuery = new ZDBOnlySubQuery(typeof(AsycudaPack), AsycudaPackSchema.APA_ABL_Bill);
		asycudaPackQuery.AddToFilter(filter);

		return GetAsycudaMainQuery(AsycudaBillSchema.PK, asycudaPackQuery);
	}
}
