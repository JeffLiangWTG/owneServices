using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class NCTSHouseConsignmentProvider : IHouseConsignmentType10
	{
		protected readonly NctsBill bill;
		readonly EU.NCTS.Business.NctsHeader nctsHeader;

		public NCTSHouseConsignmentProvider(NctsBill bill, ZInt sequenceNumber)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
			nctsHeader = Argument.NotNull(bill.Header, nameof(bill.Header));
			SequenceNumber = sequenceNumber;
		}

		public int SequenceNumber { get; }

		public string CountryOfDispatch => !IsInPhase5TransitionPeriod ? (string)bill.B0_RN_NKCountryOfExport : null;

		public string CountryOfDestination => !IsInPhase5TransitionPeriod ? (string)bill.B0_RN_NKCountryOfDestination : null;

		public decimal GrossMass => WeightRounding.Round(IsInPhase5TransitionPeriod, bill.B0_Weight);

		public string ReferenceNumberUCR => !IsInPhase5TransitionPeriod ? (string)bill.B0_ReferenceID : null;

		public IParty Consignor => !IsInPhase5TransitionPeriod ? GetConsignor() : null;
		IParty GetConsignor()
		{
			var consignor = ConsignorAddress != null ? cachedConsignor ??= new KnownEoriPartyProvider(ConsignorAddress, true, IsInPhase5TransitionPeriod) : null;
			if (consignor != null)
			{
				var consignorPartyProvider = (PartyProvider)consignor;
				if (consignorPartyProvider != null && consignorPartyProvider.IsEmpty)
				{
					consignor = null;
				}
			}
			return consignor;
		}
		IParty cachedConsignor;

		protected JobDocAddress ConsignorAddress => consignorAddress ??= JobDocAddress.Load(bill, Enterprise.MasterFiles.Integration.DocAddressType.ConsignorDocumentaryAddress);
		JobDocAddress consignorAddress;

		public IParty Consignee => !IsInPhase5TransitionPeriod ? GetConsignee() : null;
		IParty GetConsignee()
		{
			var consignee = ConsigneeAddress != null ? cachedConsignee ??= new KnownEoriPartyProvider(ConsigneeAddress, true, IsInPhase5TransitionPeriod) : null;
			if (consignee != null)
			{
				var consigneeProvider = (PartyProvider)consignee;
				if (consigneeProvider != null && consigneeProvider.IsEmpty)
				{
					consignee = null;
				}
			}
			return consignee;
		}
		IParty cachedConsignee;

		protected JobDocAddress ConsigneeAddress => consigneeAddress ??= JobDocAddress.Load(bill, Enterprise.MasterFiles.Integration.DocAddressType.ConsigneeAddress);
		JobDocAddress consigneeAddress;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= NctsDataRetrieveMethods
			.GetCusReferences(bill.Factory, bill.PK).Select((cr, index) => new AdditionalSupplyChainActorProvider(cr, index + 1)).ToArray<IAdditionalSupplyChainActor>();
		IAdditionalSupplyChainActor[] additionalSupplyChainActors;

		public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ??=
			!IsInPhase5TransitionPeriod ? GetDepartureTransportMeans() : Array.Empty<IDepartureTransportMeans>();
		IReadOnlyCollection<IDepartureTransportMeans> GetDepartureTransportMeans() => bill.DepartureTransportInfos
			.Select(ccd => new NCTSBillDepartureTransportMeansProvider(ccd, bill.InlandTransportModeAtDeparture)).ToArray<IDepartureTransportMeans>();
		IReadOnlyCollection<IDepartureTransportMeans> departureTransportMeans;

		public IReadOnlyCollection<IPreviousDocument> PreviousDocuments => previousDocuments ??=
			!IsInPhase5TransitionPeriod ? GetPreviousDocuments() : Array.Empty<IPreviousDocument>();
		IReadOnlyCollection<IPreviousDocument> GetPreviousDocuments() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.PreviousDocument).Select(csi => new PreviousDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<IPreviousDocument>();
		IReadOnlyCollection<IPreviousDocument> previousDocuments;

		public IReadOnlyCollection<ITransportDocument> TransportDocuments => transportDocuments ??=
			!IsInPhase5TransitionPeriod ? GetTransportDocuments() : Array.Empty<ITransportDocument>();
		IReadOnlyCollection<ITransportDocument> GetTransportDocuments() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.TransportDocument)
			.Select(csi => new TransportDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<ITransportDocument>();
		IReadOnlyCollection<ITransportDocument> transportDocuments;

		public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??=
			!IsInPhase5TransitionPeriod ? GetAdditionalReferences() : Array.Empty<IAdditionalReference>();
		IReadOnlyCollection<IAdditionalReference> GetAdditionalReferences() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalReference)
			.Select(csi => new AdditionalReferenceProvider(csi, IsInPhase5TransitionPeriod)).ToArray<IAdditionalReference>();
		IReadOnlyCollection<IAdditionalReference> additionalReferences;

		public string TransportChargesMethodOfPayment => !IsInPhase5TransitionPeriod ? (string)bill.B0_TransportPaymentMethod : null;

		public IReadOnlyCollection<IConsignmentItemType09> ConsignmentItems => consignmentItems ??= GetConsignmentItems();
		IReadOnlyCollection<IConsignmentItemType09> consignmentItems;

		IReadOnlyCollection<IConsignmentItemType09> GetConsignmentItems() => bill.GoodsItems.Select(item => new ConsignmentItemProvider(item)).OrderBy(x => x.GoodsItemNumber).ToArray<IConsignmentItemType09>();

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments => supportingDocuments ??=
			!IsInPhase5TransitionPeriod ? GetSupportingDocuments() : Array.Empty<ISupportingDocument>();
		IReadOnlyCollection<ISupportingDocument> GetSupportingDocuments() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.SupportingDocument)
			.Select(csi => new SupportingDocumentProvider(csi, IsInPhase5TransitionPeriod)).ToArray<ISupportingDocument>();
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation => additionalInformation ??=
			!IsInPhase5TransitionPeriod ? GetAdditionalInformation() : Array.Empty<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformation() => NctsDataRetrieveMethods
			.GetCusSupportingInfo(bill.Factory, bill.PK, Constants.CusSupportingInfoTypes.Other, Constants.CusSupportingInfoSubTypes.AdditionalInformation)
			.Select(csi => new AdditionalInformationProvider(csi)).ToArray<IAdditionalInformation>();
		IReadOnlyCollection<IAdditionalInformation> additionalInformation;

		bool IsInPhase5TransitionPeriod => CachedValueHelper.GetValue(ref isInPhase5TransitionPeriod, () => nctsHeader.IsInPhase5TransitionPeriod);
		CachedValue<bool> isInPhase5TransitionPeriod;
	}
}
