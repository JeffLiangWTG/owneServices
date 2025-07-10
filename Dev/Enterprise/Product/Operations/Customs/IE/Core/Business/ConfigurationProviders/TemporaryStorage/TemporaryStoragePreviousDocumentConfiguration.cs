using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocumentConfiguration : EU.Business.CusTempStorage.TemporaryStoragePreviousDocumentConfiguration
	{
		protected override ICollection GetCodeListCore(TemporaryStoragePreviousDocument previousDocument) => ZZRefCusCodeListCombinedCollection.GetCachedCollection(previousDocument.Factory, previousDocument.TemporaryStorageHeader.DataGrouping, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection }, ZDateTime.Today, null, includeParentDataGroupings: true);
	}
}
