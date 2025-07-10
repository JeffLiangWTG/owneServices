using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStoragePreviousDocumentConfiguration
	{
		public ICollection GetCodeList(TemporaryStoragePreviousDocument previousDocument) => GetCodeListCore(previousDocument);

		protected virtual ICollection GetCodeListCore(TemporaryStoragePreviousDocument previousDocument)
		{
			var temporaryStorageHeader = previousDocument.TemporaryStorageHeader;
			var messageType = temporaryStorageHeader.AMA_MessageType;
			var purpose = messageType == PNTSMessageTypeList.Codes.PresentationNotification ? Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Presentation : Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Declaration;
			var attributeFilterList = new RefCusCodeListAttributeFilter[]
			{
				new RefCusCodeListAttributeFilter(RefCusCodeListAttributeTypes.Codes.Purpose, JoinCondition.And, purpose)
			};
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(previousDocument.Factory, temporaryStorageHeader.DataGrouping, new ZString[] { Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS }, ZDateTime.Today, attributeFilterList, includeParentDataGroupings: true);
		}

		#region CollectionMaxCount

		public bool IsCollectionMaxCountValidationEnabled() => IsCollectionMaxCountValidationEnabledCore();

		public int CollectionMaxCount => IsCollectionMaxCountValidationEnabled() ? CollectionMaxCountCore : -1;

		protected virtual bool IsCollectionMaxCountValidationEnabledCore() => true;

		protected virtual int CollectionMaxCountCore => 1;

		#endregion
	}
}
