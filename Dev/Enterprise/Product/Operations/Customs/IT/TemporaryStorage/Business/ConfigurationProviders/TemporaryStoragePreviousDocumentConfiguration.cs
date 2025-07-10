using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public sealed class TemporaryStoragePreviousDocumentConfiguration : EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentConfiguration
{
	protected override ICollection GetCodeListCore(EU.Business.CusTempStorage.TemporaryStoragePreviousDocument previousDocument) => ZZRefCusCodeListCombinedCollection.GetCachedCollection(
		previousDocument.Factory,
		previousDocument.TemporaryStorageHeader.DataGrouping,
		[Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS],
		ZDateTime.Today,
		null,
		includeParentDataGroupings: false);

	#region CollectionMaxCount

	protected override bool IsCollectionMaxCountValidationEnabledCore() => false;

	#endregion
}
