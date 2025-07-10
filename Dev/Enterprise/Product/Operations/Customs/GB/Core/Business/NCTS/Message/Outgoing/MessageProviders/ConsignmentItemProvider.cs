using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class ConsignmentItemProvider : IConsignmentItemType09
	{
		readonly NctsCommonCargoDesc item;
		public ConsignmentItemProvider(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		public int GoodsItemNumber => item.BY_LineNo;

		public int DeclarationGoodsItemNumber => item.BY_DeclarationGoodsItemNumber;

		public string DeclarationType => item.BY_Type;

		public string CountryOfDispatch => item.CountryOfDispatch?.Code;

		public string CountryOfDestination => item.CountryOfDestination?.Code;

		public string ReferenceNumberUCR => item.BY_CommercialReferenceNumber;

		public IParty Consignee => CachedValueHelper.GetValue(ref cachedConsignee, () => IsInPhase5TransitionPeriod
					? GetConsignee()
					: null);
		IParty GetConsignee()
		{
			var consigneePartyProvider = NctsDataRetrieveMethods.GetJobDocAddress(item, AutoDocAddressTypes.Codes.ConsigneeAddress) is JobDocAddress jobDocAddress ? new PartyProvider(jobDocAddress, IsInPhase5TransitionPeriod) : null;
			if (consigneePartyProvider != null && consigneePartyProvider.IsEmpty)
			{
				consigneePartyProvider = null;
			}
			return consigneePartyProvider;
		}
		CachedValue<IParty> cachedConsignee;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??=
			NctsDataRetrieveMethods.GetCusReferences(item.Factory, item.PK, CusReferenceTypeList.Codes.SupplyChainActor)
			.Select((reference, index) => new AdditionalSupplyChainActorProvider(reference, index + 1)).ToArray<IAdditionalSupplyChainActor>();
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

		public ICommodity Commodity => commodity ??= new CommodityProvider(item);
		ICommodity commodity;

		public IReadOnlyCollection<IPreviousDocumentExtended> PreviousDocuments => previousDocuments ??=
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.PreviousDocument)
			.Select(csi => new PreviousDocumentExtendedProvider(csi, IsInPhase5TransitionPeriod)).ToArray<IPreviousDocumentExtended>();
		IReadOnlyCollection<IPreviousDocumentExtended> previousDocuments;

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??=
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.SupportingDocument)
			.Select(csi => new SupportingDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<ISupportingDocument>();
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??=
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
			.Select(reference => new AdditionalReferenceProvider(reference, IsInPhase5TransitionPeriod)).ToArray<IAdditionalReference>();
		IReadOnlyCollection<IAdditionalReference> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??=
			NctsDataRetrieveMethods.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalInformation)
			.Select(reference => new AdditionalInformationProvider(reference)).ToArray<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		public IReadOnlyCollection<IPackaging> Packagings => packagings ??= item.Packages.Select(package => new PackagingProvider(package)).ToArray<IPackaging>();
		IReadOnlyCollection<IPackaging> packagings;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ??= IsInPhase5TransitionPeriod
			? GetTransportDocuments()
			: Array.Empty<ITransportDocument>();
		IReadOnlyCollection<ITransportDocument> GetTransportDocuments() => NctsDataRetrieveMethods
				.GetCusSupportingInfo(item.Factory, item.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
				.Select(reference => new TransportDocumentProvider(reference, IsInPhase5TransitionPeriod)).ToArray<ITransportDocument>();
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		public string TransportChargesMethodOfPayment => IsInPhase5TransitionPeriod
			? (string)item.BY_TransportChargesMethodOfPayment
		: null;

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => item.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;
	}
}
