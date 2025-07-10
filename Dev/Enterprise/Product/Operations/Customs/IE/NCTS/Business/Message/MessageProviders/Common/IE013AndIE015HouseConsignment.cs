using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AES;
using AddInfoSchema = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015HouseConsignment : IIE013AndIE015HouseConsignment
	{
		public IE013AndIE015HouseConsignment(NctsBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}
		readonly NctsBill bill;

		bool IsInTransitionPeriod => bill.IsInPhase5TransitionPeriod;

		public string CountryOfDispatch => bill.B0_RN_NKCountryOfExport;

		public string CountryOfDestination => bill.B0_RN_NKCountryOfDestination;

		public decimal GrossMass => bill.B0_Weight;

		public string ReferenceNumberUCR => bill.B0_ReferenceID;

		public IParty Consignor => CachedValueHelper.GetValue(ref consignorCached, () => PartyProvider.New(bill.Consignor));
		CachedValue<IParty> consignorCached;

		public IParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => PartyProvider.New(bill.Consignee));
		CachedValue<IParty> consigneeCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActor ?? (additionalSupplyChainActor = bill.CusSupplyChainActorReferences.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		public IReadOnlyCollection<ITransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = DepartureTransportMeansProvider.GetTransportMeans(bill));
		IReadOnlyCollection<ITransportMeans> departureTransportMeans;

		public IReadOnlyCollection<IDocumentWithComplementOfInformation> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					if (IsInTransitionPeriod)
					{
						previousDocuments = Array.Empty<PreviousDocumentProvider>();
					}
					else
					{
						previousDocuments = Extensions.GetAggregatedData(GetPreviousDocumentsKeys(), bill.PreviousDocuments).Select(p => new PreviousDocumentProvider(p)).ToArray();
					}
				}
				return previousDocuments;
			}
		}
		IReadOnlyCollection<PreviousDocumentProvider> previousDocuments;
		string[] GetPreviousDocumentsKeys() => new[] { AddInfoSchema.CSI_Code, AddInfoSchema.CSI_ReferenceNumber, AddInfoSchema.CSI_ReferenceNumber2 };

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					if (IsInTransitionPeriod)
					{
						supportingDocuments = Array.Empty<SupportingDocumentProvider>();
					}
					else
					{
						supportingDocuments = Extensions.GetAggregatedData(GetSupportingDocumentKeys(), bill.SupportingDocuments).Select(p => new SupportingDocumentProvider(p)).ToArray();
					}
				}
				return supportingDocuments;
			}
		}
		IReadOnlyCollection<SupportingDocumentProvider> supportingDocuments;
		string[] GetSupportingDocumentKeys() => new[]
		{
			AddInfoSchema.CSI_Code,
			AddInfoSchema.CSI_ItemNumber,
			AddInfoSchema.CSI_ReferenceNumber,
			AddInfoSchema.CSI_ReferenceNumber2
		};

		public IReadOnlyCollection<IDocument> AdditionalReferences
		{
			get
			{
				if (additionalReferences == null)
				{
					if (IsInTransitionPeriod)
					{
						additionalReferences = Array.Empty<DocumentProvider>();
					}
					else
					{
						var addInfos = MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.AdditionalReference);
						addInfos = Extensions.GetAggregatedData(GetAdditionalInfoKeys(), addInfos);
						additionalReferences = addInfos.Select(p => new DocumentProvider(p)).ToArray();
					}
				}
				return additionalReferences;
			}
		}
		IReadOnlyCollection<DocumentProvider> additionalReferences;

		public IReadOnlyCollection<IDocument> TransportDocuments
		{
			get
			{
				if (transportDocuments == null)
				{
					if (IsInTransitionPeriod)
					{
						transportDocuments = Array.Empty<DocumentProvider>();
					}
					else
					{
						var addInfos = MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.TransportDocument);
						addInfos = Extensions.GetAggregatedData(GetAdditionalInfoKeys(), addInfos);
						transportDocuments = addInfos.Select(p => new DocumentProvider(p)).ToArray();
					}
				}
				return transportDocuments;
			}
		}
		IReadOnlyCollection<DocumentProvider> transportDocuments;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations
		{
			get
			{
				if (additionalInformations == null)
				{
					if (IsInTransitionPeriod)
					{
						additionalInformations = Array.Empty<AdditionalInformationProvider>();
					}
					else
					{
						var addInfos = MessageProviderHelper.FindAdditionalDocuments(bill, AdditionalInfoSubTypeList.Codes.AdditionalInformation);
						addInfos = Extensions.GetAggregatedData(GetAdditionalInfoKeys(), addInfos);
						additionalInformations = addInfos.Select(p => AdditionalInformationProvider.New(p)).ToArray();
					}
				}
				return additionalInformations;
			}
		}
		IReadOnlyCollection<AdditionalInformationProvider> additionalInformations;

		public string MethodOfPayment => bill.B0_TransportPaymentMethod;

		public IReadOnlyCollection<IIE013AndIE015ConsignmentItem> ConsignmentItems => consignmentItems ?? (consignmentItems = bill.GoodsItems.Select(p => new IE013AndIE015ConsignmentItemProvider(p)).ToArray());
		IReadOnlyCollection<IE013AndIE015ConsignmentItemProvider> consignmentItems;

		string[] GetAdditionalInfoKeys() => new[]
		{
			AddInfoSchema.CSI_Code,
			AddInfoSchema.CSI_ReferenceNumber
		};
	}
}
