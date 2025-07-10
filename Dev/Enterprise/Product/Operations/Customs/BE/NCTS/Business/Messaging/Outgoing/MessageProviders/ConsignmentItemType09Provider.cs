using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class ConsignmentItemType09Provider : IConsignmentItemType09
	{
		readonly NctsCommonCargoDesc item;
		public ConsignmentItemType09Provider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		public int GoodsItemNumber => item.BY_LineNo;

		public int DeclarationGoodsItemNumber => item.BY_DeclarationGoodsItemNumber;

		public string DeclarationType => item.BY_Type;

		public string CountryOfDispatch => item.CountryOfDispatch?.Code;

		public string CountryOfDestination => item.CountryOfDestination?.Code;

		public string ReferenceNumberUCR => item.BY_CommercialReferenceNumber;

		public IParty Consignee => CachedValueHelper.GetValue(ref consignee, () =>
		{
			return NctsDataRetrieveMethods.GetJobDocAddress(item, AutoDocAddressTypes.Codes.ConsigneeAddress) is JobDocAddress jobDocAddress ? new PartyProvider(jobDocAddress) : null;
		});
		CachedValue<IParty> consignee;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ?? (additionalSupplyChainActors = NctsDataRetrieveMethods.GetCusReferences(item.Factory, item.PK, CusReferenceTypeList.Codes.SupplyChainActor).Select((reference, index) => new AdditionalSupplyChainActorProvider(reference, index + 1)).ToArray<IAdditionalSupplyChainActor>());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public ICommodity Commodity => commodity ?? (commodity = new CommodityProvider(item));
		ICommodity commodity;

		public IReadOnlyCollection<IPreviousDocumentExtended> PreviousDocuments => previousDocuments ?? (previousDocuments =
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.PreviousDocument)
			.Select(csi => new PreviousDocumentExtendedProvider(csi)).ToArray<IPreviousDocumentExtended>());
		IReadOnlyCollection<IPreviousDocumentExtended> previousDocuments;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ?? (supportingDocuments =
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.SupportingDocument)
			.Select(csi => new SupportingDocumentProvider(csi)).ToArray<ISupportingDocument>());
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences ?? (additionalReferences =
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
			.Select(reference => new AdditionalReferenceProvider(reference)).ToArray<IDocument>());
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation =
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalInformation)
			.Select(reference => new AdditionalInformationProvider(reference)).ToArray<IAdditionalInformation>());
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		public IReadOnlyCollection<IPackaging> Packagings => packagings ?? (packagings = item.Packages.Select(package => new PackagingProvider(package)).ToArray<IPackaging>());
		IReadOnlyCollection<IPackaging> packagings;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ?? (transportDocuments =
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
			.Select(reference => new TransportDocumentProvider(reference)).ToArray<ITransportDocument>());
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		public string TransportChargesMethodOfPayment => item.BY_TransportChargesMethodOfPayment;

		IReadOnlyCollection<IDocument> IConsignmentItemType09.AdditionalReferences => null;
	}
}
