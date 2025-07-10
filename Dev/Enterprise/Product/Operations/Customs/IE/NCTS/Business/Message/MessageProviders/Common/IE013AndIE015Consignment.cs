using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AES;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.IE.NCTS.Business.MessageProviderHelper;
using AddInfoSchema = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema;
using Extensions = Enterprise.Customs.EU.Business.Extensions;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015Consignment : IIE013AndIE015Consignment
	{
		public IE013AndIE015Consignment(NctsHeader header)
		{
			this.header = header;
			movementHeader = header.MovementHeader;
		}
		readonly NctsHeader header;
		readonly NctsDepartureMovementHeader movementHeader;

		bool IsInTransitionPeriod => header.IsInPhase5TransitionPeriod;

		public string ReferenceNumberUCR => movementHeader.BM_UniqueConsignmentReference;

		public string CountryOfDestination => movementHeader.BM_RL_NKDestinationPort;

		public string CountryOfDispatch => movementHeader.BM_RN_NKCountryOfDispatch;

		public string InlandTransportMode => movementHeader.BM_InlandTransportMode;

		public string BorderTransportMode => movementHeader.BM_ExportTransportMode;

		public bool HasContainer => header.Factory.GetValue(ref hasContainerCached, () =>
		{
			return header.DepartureHeaderContainers.Cast<NctsDepartureHeaderContainer>().Any(x => !x.BC_ContainerNum.IsEmpty);
		});
		CachedProperty<bool> hasContainerCached;

		public decimal GrossMass => movementHeader.BM_GrossWeight;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActor ?? (additionalSupplyChainActor = header.MovementHeader.CusSupplyChainActors.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipments
		{
			get
			{
				if (transportEquipments is null)
				{
					transportEquipments = TransportEquipmentWithSealsProvider.GetEquipments(header);
				}
				return transportEquipments;
			}
		}
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipments;

		public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => new LocationOfGoodsProvider(header.MovementHeader));
		CachedValue<ILocationOfGoods> locationOfGoods;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = DepartureTransportMeansProvider.GetTransportMeans(movementHeader));
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<string> CountriesOfRouting => countriesOfRouting ?? (countriesOfRouting = header.CountriesOfRouting.Select(p => p.CY_Data.ToString()).Where(p => !string.IsNullOrWhiteSpace(p)).ToArray());
		IReadOnlyCollection<string> countriesOfRouting;

		public IReadOnlyCollection<IActiveTransportMeans> ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = new ActiveTransportMeansProvider(movementHeader).AsReadOnlyCollection());
		IReadOnlyCollection<IActiveTransportMeans> activeBorderTransportMeans;

		public ICarrier Carrier => CachedValueHelper.GetValue(ref carrierCached, () =>
		{
			return PartyMatchesPrincipal(movementHeader.Carrier) ? null : CarrierProvider.New(movementHeader.Carrier, false);
		});
		CachedValue<ICarrier> carrierCached;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () =>
		{
			return PartyMatchesPrincipal(header.Consignor) ? null : PartyProvider.New(header.Consignor, PartyProvider.FallBackStyle.EORIOnly);
		});
		CachedValue<IParty> consignorCached;

		public IParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => PartyProvider.New(header.Consignee, PartyProvider.FallBackStyle.EORIOnly));
		CachedValue<IParty> consigneeCached;

		public IPort PlaceOfLoading => CachedValueHelper.GetValue(ref placeOfLoading, () =>
		{
			PortProvider portProvider;
			if (movementHeader.BM_PortOfPresentationCode.Length > 2)
			{
				portProvider = new PortProvider { UNLocode = movementHeader.BM_PortOfPresentationCode };
			}
			else
			{
				portProvider = new PortProvider { Country = movementHeader.BM_PortOfPresentationCode, Location = movementHeader.BM_PlaceOfLoading };
			}
			return portProvider;
		});
		CachedValue<IPort> placeOfLoading;

		public IPort PlaceOfUnloading => CachedValueHelper.GetValue(ref placeOfUnloading, () =>
		{
			PortProvider portProvider;
			if (movementHeader.BM_ForeignDestPortKCode.Length > 2)
			{
				portProvider = new PortProvider { UNLocode = movementHeader.BM_ForeignDestPortKCode };
			}
			else
			{
				portProvider = new PortProvider { Country = movementHeader.BM_ForeignDestPortKCode, Location = movementHeader.BM_PlaceOfUnloading };
			}
			return portProvider;
		});
		CachedValue<IPort> placeOfUnloading;

		IReadOnlyCollection<TResult> MergeOrEmptyIfIsInTransitionPeriod<TSource, TResult>(string[] mergeKey, IEnumerable<TSource> source, Func<TSource, TResult> create) where TSource : BusinessObject
			=> IsInTransitionPeriod ? Array.Empty<TResult>() : Extensions.GetAggregatedData(mergeKey, source).Select(create).ToArray();

		public IReadOnlyCollection<IDocumentWithComplementOfInformation> PreviousDocuments => previousDocuments
			?? (previousDocuments = MergeOrEmptyIfIsInTransitionPeriod(GetPreviousDocumentKeys(), header.PreviousDocuments, p => new PreviousDocumentProvider(p)));
		IReadOnlyCollection<PreviousDocumentProvider> previousDocuments;
		string[] GetPreviousDocumentKeys() => new[] { AddInfoSchema.CSI_Code, AddInfoSchema.CSI_ReferenceNumber, AddInfoSchema.CSI_ReferenceNumber2 };

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments
			?? (supportingDocuments = MergeOrEmptyIfIsInTransitionPeriod(GetSupportingDocumentKeys(), header.MovementHeader.SupportingDocuments, p => new SupportingDocumentProvider(p)));
		IReadOnlyCollection<SupportingDocumentProvider> supportingDocuments;
		string[] GetSupportingDocumentKeys() => new[] { AddInfoSchema.CSI_Code, AddInfoSchema.CSI_ItemNumber, AddInfoSchema.CSI_ReferenceNumber, AddInfoSchema.CSI_ReferenceNumber2 };

		public IReadOnlyCollection<IDocument> AdditionalReferences => additionalReferences
			?? (additionalReferences = MergeOrEmptyIfIsInTransitionPeriod(GetAdditionalInfoKeys(), FindAdditionalDocuments(header, AdditionalInfoSubTypeList.Codes.AdditionalReference), p => new DocumentProvider(p)));
		IReadOnlyCollection<IDocument> additionalReferences;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments
			?? (transportDocuments = MergeOrEmptyIfIsInTransitionPeriod(GetAdditionalInfoKeys(), FindAdditionalDocuments(header, AdditionalInfoSubTypeList.Codes.TransportDocument), p => new DocumentProvider(p)));
		IReadOnlyCollection<IDocument> transportDocuments;

		string[] GetAdditionalInfoKeys() => new[] { AddInfoSchema.CSI_Code, AddInfoSchema.CSI_ReferenceNumber };

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations
			?? (additionalInformations = MergeOrEmptyIfIsInTransitionPeriod(GetAdditionalInformationKeys(), FindAdditionalDocuments(header, AdditionalInfoSubTypeList.Codes.AdditionalInformation), p => AdditionalInformationProvider.New(p)));
		IReadOnlyCollection<IAdditionalInformation> additionalInformations;
		string[] GetAdditionalInformationKeys() => new[] { AddInfoSchema.CSI_Code, AddInfoSchema.CSI_Description };

		public string MethodOfPayment => movementHeader.BM_MethodOfPayment;

		public IReadOnlyCollection<IIE013AndIE015HouseConsignment> HouseConsignments => houseConsignment ?? (houseConsignment = header.Bills.OrderBy(b => b.SequenceNumber).Select(p => new IE013AndIE015HouseConsignment(p)).ToArray());
		IReadOnlyCollection<IIE013AndIE015HouseConsignment> houseConsignment;

		ZBool PartyMatchesPrincipal(JobDocAddress jobDocAddress)
		{
			return header.Principal.OrganisationPK == jobDocAddress.OrganisationPK;
		}
	}
}
