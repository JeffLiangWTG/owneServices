using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageSupportingDocumentConfiguration
	{
		public ICollection GetCodeList(TemporaryStorageSupportingDocument supportingDocument) => GetCodeListCore(supportingDocument);

		protected virtual ICollection GetCodeListCore(TemporaryStorageSupportingDocument supportingDocument)
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(supportingDocument.Factory, supportingDocument.TemporaryStorageHeader.DataGrouping, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6 }, ZDateTime.Today, null, includeParentDataGroupings: true);
		}
	}
}
