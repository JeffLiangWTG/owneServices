using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.AE.Manifest.Business;
using Enterprise.Customs.ASYCUDA.Module;

namespace Enterprise.Customs.AE.Manifest.Module;

public class AEManifestBillModuleCollectionFetchStrategy(AEManifestBillModuleCollection collection) : ASYCUDAManifestBillModuleCollectionFetchStrategy<AsycudaBill>(collection)
{
	protected override bool IsBillCountryGenAddOnColumnRelatedColumn(ZString columnName)
	{
		return billCountryGenAddOnColumnList.Contains(columnName);
	}

	IReadOnlyList<ZString> billCountryGenAddOnColumnList => new[] {
			(ZString)AsycudaBill.Schema.ABL_SplitBillNumber,
		};
}
