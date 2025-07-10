using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TSGoodsShipmentItemTypeDocumentsAuthorisationsProvider : ITSGoodsShipmentItemTypeDocumentsAuthorisations
	{
		internal TSGoodsShipmentItemTypeDocumentsAuthorisationsProvider(TemporaryStoragePackedItem packedItem)
		{
			this.packedItem = packedItem;
		}
		readonly TemporaryStoragePackedItem packedItem;

		public IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> SimplifiedDeclarationDocuments => simplifiedDeclarationDocuments ??= packedItem.PreviousDocuments.Select(x => new SimplifiedDeclarationDocumentWritingOffProvider(x)).ToArray();
		IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> simplifiedDeclarationDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= MessageProviderHelper.FindAdditionalInfos(packedItem, EU.Business.AdditionalInfoSubTypeList.Codes.AdditionalInformation).Select(AdditionalInformationProvider.New).ToArray();
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IIdType> ProducedDocuments => producedDocuments ??= packedItem.SupportingDocuments.Select(ProducedDocumentsWritingOffProvider.New).ToArray();
		IReadOnlyCollection<IIdType> producedDocuments;
	}
}
