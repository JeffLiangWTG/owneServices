using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class ConsignmentItemWrapper : IConsignmentItem
	{
		protected ConsignmentItemWrapper(NctsCommonCargoDesc item)
		{
			this.item = Argument.NotNull(item, nameof(item));
		}

		protected readonly NctsCommonCargoDesc item;

		public static ConsignmentItemWrapper New(NctsCommonCargoDesc item) => item == null ? null : new ConsignmentItemWrapper(item);

		public string GoodsItemNumber => goodsItemNumber ?? (goodsItemNumber = GetGoodsItemNumber());

		protected virtual string GetGoodsItemNumber() => item.BY_LineNo.ToString();

		string goodsItemNumber;

		public string DeclarationGoodsItemNumber => declarationGoodsItemNumber ?? (declarationGoodsItemNumber = item.BY_DeclarationGoodsItemNumber.ToString());
		string declarationGoodsItemNumber;

		public string DeclarationType => declarationType ?? (declarationType = item.BY_Type);
		string declarationType;

		public string CountryOfDispatch => countryOfDispatch ?? (countryOfDispatch = item.BY_RN_NKCountryOfDispatch);
		string countryOfDispatch;

		public string CountryOfDestination => countryOfDestination ?? (countryOfDestination = item.BY_RN_NKCountryOfDestination);
		string countryOfDestination;

		public string ReferenceNumberUCR => referenceNumberUCR ?? (referenceNumberUCR = item.BY_CommercialReferenceNumber);
		string referenceNumberUCR;

		public IOrganization Consignee => consignee ?? (consignee = item is NctsDepartureCargoDesc departureItem ? OrganizationWrapper.New(departureItem.Consignee.Organisation) : null);
		IOrganization consignee;

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActors());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;
		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();
			if (item is NctsDepartureCargoDesc departureItem)
			{
				departureItem.CusSupplyChainActorReferences.Cast<CusReference>().ForEach(x => result.Add(AdditionalSupplyChainActorWrapper.New(x)));
				return result;
			}
			else
			{
				return null;
			}
		}

		public virtual ICommodity Commodity => commodity ?? (commodity = CommodityWrapper.New(item));
		ICommodity commodity;

		public ICollection<IPackaging> Packaging => packaging ?? (packaging = GetPackagings());
		ICollection<IPackaging> packaging;
		protected virtual ICollection<IPackaging> GetPackagings()
		{
			var result = new Collection<IPackaging>();
			item.Packages.Cast<NctsPackage>().ForEach(package => result.Add(PackagingWrapper.New(package)));
			return result;
		}

		public ICollection<IPreviousDocument> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocuments());
		ICollection<IPreviousDocument> previousDocument;
		ICollection<IPreviousDocument> GetPreviousDocuments()
		{
			if (item is NctsDepartureCargoDesc departureItem)
			{
				var result = new Collection<IPreviousDocument>();
				departureItem.PreviousDocuments.Cast<NctsPreviousDocument>().ForEach(document => result.Add(PreviousDocumentWrapper.New(document)));
				return result;
			}
			else
			{
				return null;
			}
		}

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocuments());
		ICollection<ISupportingDocument> supportingDocument;
		protected virtual ICollection<ISupportingDocument> GetSupportingDocuments()
		{
			var result = new Collection<ISupportingDocument>();
			item.SupportingDocuments.Cast<NctsSupportingDocument>().ForEach(document => result.Add(SupportingDocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocuments());
		ICollection<IDocument> transportDocument;
		protected virtual ICollection<IDocument> GetTransportDocuments()
		{
			var result = new Collection<IDocument>();
			item.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument).ForEach(document => result.Add(DocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IDocument> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReferences());
		ICollection<IDocument> additionalReference;
		protected virtual ICollection<IDocument> GetAdditionalReferences()
		{
			var result = new Collection<IDocument>();
			item.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ForEach(document => result.Add(DocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;
		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var result = new Collection<IAdditionalInformation>();
			item.AdditionalInfos.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ForEach(document => result.Add(AdditionalInformationWrapper.New(document)));
			return result;
		}

		public ITransportCharges TransportCharges => transportCharges ?? (transportCharges = TransportChargesWrapper.New(item.Header.MovementHeader.BM_MethodOfPayment));
		ITransportCharges transportCharges;
	}
}
