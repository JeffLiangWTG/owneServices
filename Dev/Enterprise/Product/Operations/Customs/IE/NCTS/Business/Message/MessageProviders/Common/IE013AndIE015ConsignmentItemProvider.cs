using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AES;
using AddInfoSchema = Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo.Schema;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE013AndIE015ConsignmentItemProvider : IIE013AndIE015ConsignmentItem
	{
		public IE013AndIE015ConsignmentItemProvider(NctsDepartureCargoDesc cargoDesc)
		{
			this.cargoDesc = Argument.NotNull(cargoDesc, nameof(cargoDesc));
			bill = Argument.NotNull((NctsBill)cargoDesc.Bill, nameof(cargoDesc.Bill));
			nctsHeader = Argument.NotNull((NctsHeader)bill.Header, nameof(bill.Header));
			movementHeader = Argument.NotNull(nctsHeader.MovementHeader, nameof(nctsHeader.MovementHeader));
		}
		readonly NctsDepartureCargoDesc cargoDesc;
		readonly NctsBill bill;
		readonly NctsHeader nctsHeader;
		readonly NctsDepartureMovementHeader movementHeader;

		bool IsInTransitionPeriod => nctsHeader.IsInPhase5TransitionPeriod;

		public short GoodsItemNumber => cargoDesc.BY_LineNo;

		public int DeclarationGoodsItemNumber => cargoDesc.BY_DeclarationGoodsItemNumber;

		public string DeclarationType => cargoDesc.BY_Type;

		public string CountryOfDispatch => cargoDesc.BY_RN_NKCountryOfDispatch;

		public string CountryOfDestination
		{
			get
			{
				var result = cargoDesc.BY_RN_NKCountryOfDestination;
				return result.IsEmpty ? movementHeader.BM_RL_NKDestinationPort : result;
			}
		}

		public string ReferenceNumberUCR => cargoDesc.BY_CommercialReferenceNumber;

		public IParty Consignee => CachedValueHelper.GetValue(ref consigneeCached, () => PartyProvider.New(cargoDesc.Consignee));
		CachedValue<IParty> consigneeCached;

		public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActor ?? (additionalSupplyChainActor = cargoDesc.CusSupplyChainActorReferences.Select(x => new AdditionalSupplyChainActorProvider(x)).ToArray());
		IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;

		public IIE013AndIE015CommodityType Commodity => CachedValueHelper.GetValue(ref commodityCached, () => new IE013AndIE015CommodityTypeProvider(cargoDesc));
		CachedValue<IIE013AndIE015CommodityType> commodityCached;

		public IReadOnlyCollection<IPackaging> Packages => packages ?? (packages = cargoDesc.Packages.Select(p => new PackagingProvider(p.B5_UnitType, p.B5_UnitCount.ToZInt(), p.B5_MarksAndNumbers)).ToArray());
		IReadOnlyCollection<PackagingProvider> packages;

		public IReadOnlyCollection<IIE013AndIE015ConsignmentItemPreviousDocument> PreviousDocuments
		{
			get
			{
				if (previousDocuments == null)
				{
					IEnumerable<CusSupportingInfo> bizObjs;
					if (IsInTransitionPeriod)
					{
						bizObjs = bill.PreviousDocuments.Cast<CusSupportingInfo>()
							.Union(cargoDesc.PreviousDocuments.Cast<CusSupportingInfo>())
							.Union(cargoDesc.Header.PreviousDocuments.Cast<CusSupportingInfo>());
					}
					else
					{
						bizObjs = cargoDesc.PreviousDocuments.Cast<CusSupportingInfo>();
					}
					var merged = Extensions.GetAggregatedData(GetPreviousDocumentKeys(), bizObjs);

					previousDocuments = merged.Select(p => new IE013AndIE015ConsignmentItemPreviousDocumentProvider(p)).ToArray();
				}
				return previousDocuments;
			}
		}
		IReadOnlyCollection<IE013AndIE015ConsignmentItemPreviousDocumentProvider> previousDocuments;
		string[] GetPreviousDocumentKeys() => new[]
		{
			AddInfoSchema.CSI_Code,
			AddInfoSchema.CSI_ItemNumber,
			AddInfoSchema.CSI_UnitOfQuantity2,
			AddInfoSchema.CSI_Quantity2,
			AddInfoSchema.CSI_UnitOfQuantity,
			AddInfoSchema.CSI_Quantity,
			AddInfoSchema.CSI_ReferenceNumber,
			AddInfoSchema.CSI_ReferenceNumber2,
		};

		public IReadOnlyCollection<ISupportingDocument> SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					IEnumerable<CusSupportingInfo> bizObjs;
					if (IsInTransitionPeriod)
					{
						bizObjs = bill.SupportingDocuments.Cast<CusSupportingInfo>()
							.Union(cargoDesc.SupportingDocuments.Cast<CusSupportingInfo>())
							.Union(cargoDesc.Header.MovementHeader.SupportingDocuments.Cast<CusSupportingInfo>());
					}
					else
					{
						bizObjs = cargoDesc.SupportingDocuments.Cast<CusSupportingInfo>();
					}
					var merged = Extensions.GetAggregatedData(GetSupportingDocumentKeys(), bizObjs);

					supportingDocuments = merged.Select(p => new SupportingDocumentProvider(p)).ToArray();
				}
				return supportingDocuments;
			}
		}
		IReadOnlyCollection<ISupportingDocument> supportingDocuments;
		string[] GetSupportingDocumentKeys() => new[]
		{
			AddInfoSchema.CSI_Code,
			AddInfoSchema.CSI_ItemNumber,
			AddInfoSchema.CSI_ReferenceNumber,
			AddInfoSchema.CSI_ReferenceNumber2,
		};

		public IReadOnlyCollection<IDocument> TransportDocuments
		{
			get
			{
				if (transportDocuments == null)
				{
					var bizObjs = GetMergedAdditionalInfos(AdditionalInfoSubTypeList.Codes.TransportDocument, GetAdditionalInfoKeys());
					transportDocuments = bizObjs.Select(p => new DocumentProvider(p)).ToArray();
				}
				return transportDocuments;
			}
		}
		IReadOnlyCollection<DocumentProvider> transportDocuments;

		public IReadOnlyCollection<IDocument> AdditionalReferences
		{
			get
			{
				if (additionalReferences == null)
				{
					var bizObjs = GetMergedAdditionalInfos(AdditionalInfoSubTypeList.Codes.AdditionalReference, GetAdditionalInfoKeys());
					additionalReferences = bizObjs.Select(p => new DocumentProvider(p)).ToArray();
				}
				return additionalReferences;
			}
		}
		IReadOnlyCollection<DocumentProvider> additionalReferences;

		public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations
		{
			get
			{
				if (additionalInformations == null)
				{
					var bizObjs = GetMergedAdditionalInfos(AdditionalInfoSubTypeList.Codes.AdditionalInformation, new[] { AddInfoSchema.CSI_Code, AddInfoSchema.CSI_Description });
					additionalInformations = bizObjs.Select(p => AdditionalInformationProvider.New(p)).ToArray();
				}
				return additionalInformations;
			}
		}
		IReadOnlyCollection<AdditionalInformationProvider> additionalInformations;

		IEnumerable<AdditionalInfo> GetMergedAdditionalInfos(string subType, string[] keys)
		{
			IEnumerable<AdditionalInfo> result;
			if (IsInTransitionPeriod)
			{
				result = MessageProviderHelper.FindAdditionalDocuments(nctsHeader, subType)
					.Union(MessageProviderHelper.FindAdditionalDocuments(bill, subType))
					.Union(MessageProviderHelper.FindAdditionalDocuments(cargoDesc, subType));
			}
			else
			{
				result = MessageProviderHelper.FindAdditionalDocuments(cargoDesc, subType);
			}
			result = Extensions.GetAggregatedData(keys, result);

			return result;
		}
		string[] GetAdditionalInfoKeys() => new[]
		{
			AddInfoSchema.CSI_Code,
			AddInfoSchema.CSI_ReferenceNumber
		};

		public string MethodOfPayment => cargoDesc.BY_TransportChargesMethodOfPayment;
	}
}
