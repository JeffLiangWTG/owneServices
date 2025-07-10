using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.CusTempStorage
{
	public class TemporaryStorageSupportingDocumentConfiguration : EU.Business.CusTempStorage.TemporaryStorageSupportingDocumentConfiguration
	{
		protected override ICollection GetCodeListCore(TemporaryStorageSupportingDocument supportingDocument)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(supportingDocument.Factory, supportingDocument.TemporaryStorageHeader.DataGrouping, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection }, ZDateTime.Today, null, includeParentDataGroupings: true);
		}
	}
}
