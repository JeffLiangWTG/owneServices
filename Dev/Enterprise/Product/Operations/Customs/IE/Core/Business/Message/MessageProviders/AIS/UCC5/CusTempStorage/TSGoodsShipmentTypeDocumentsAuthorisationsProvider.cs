using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	class TSGoodsShipmentTypeDocumentsAuthorisationsProvider : ITSGoodsShipmentTypeDocumentsAuthorisations
	{
		internal TSGoodsShipmentTypeDocumentsAuthorisationsProvider(TemporaryStorageHeader header)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.bill = Argument.NotNull(header.MasterBill, nameof(header.MasterBill));
			this.authorizationUsage = header.AuthorizationUsage as CusAuthorizationUsage;
		}

		public string UCR => bill.ABL_UCRNumber;

		public IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> SimplifiedDeclarationDocuments => simplifiedDeclarationDocuments ??= bill.PreviousDocuments.Count > 0
			? bill.PreviousDocuments.Select(x => new SimplifiedDeclarationDocumentWritingOffProvider(x)).ToArray()
			: header.PreviousDocuments.Select(x => new SimplifiedDeclarationDocumentWritingOffProvider(x)).ToArray();
		IReadOnlyCollection<ISimplifiedDeclarationDocumentWritingOff> simplifiedDeclarationDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations
			=> additionalInformations ??= bill.AdditionalInfos.Where(x => x.IsAnAdditionalInformation).Select(AdditionalInformationProvider.New).ToArray();
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;

		public IReadOnlyCollection<IIdType> ProducedDocuments => producedDocuments ??= bill.SupportingDocuments.Select(ProducedDocumentsWritingOffProvider.New).ToArray();
		IReadOnlyCollection<IIdType> producedDocuments;

		public IWarehouseIdentification Warehouse
			=> CachedValueHelper.GetValue(ref warehouse, () => authorizationUsage != null ? WarehouseIdentificationProvider.New(Constants.IDType.V, authorizationUsage.AGC_Number) : null);
		CachedValue<IWarehouseIdentification> warehouse;

		readonly TemporaryStorageHeader header;
		readonly TemporaryStorageBill bill;
		readonly CusAuthorizationUsage authorizationUsage;
	}
}
