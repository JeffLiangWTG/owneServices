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
	public class HouseConsignmentWrapper : IHouseConsignment
	{
		protected HouseConsignmentWrapper(NctsBill bill)
		{
			this.bill = Argument.NotNull(bill, nameof(bill));
		}

		protected NctsBill bill;

		public static HouseConsignmentWrapper New(NctsBill bill) => bill == null ? null : new HouseConsignmentWrapper(bill);

		public string CountryofDispatch => countryofDispatch ?? (countryofDispatch = bill.B0_RN_NKCountryOfExport);
		string countryofDispatch;

		public virtual decimal? GrossMass => grossMass ?? (grossMass = bill.B0_Weight);
		decimal? grossMass;

		public string ReferenceNumberUCR => referenceNumberUCR ?? (referenceNumberUCR = bill.B0_ReferenceID);
		string referenceNumberUCR;

		public IOrganizationWithContact Consignor => consignor ?? (consignor = OrganizationWithContactWrapper.New(bill.Consignor.Organisation));
		IOrganizationWithContact consignor;

		public IOrganization Consignee => consignee ?? (consignee = OrganizationWrapper.New(bill.Consignee.Organisation));
		IOrganization consignee;

		public ICollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActor => additionalSupplyChainActor ?? (additionalSupplyChainActor = GetAdditionalSupplyChainActors());
		ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActor;
		ICollection<IAdditionalSupplyChainActor> GetAdditionalSupplyChainActors()
		{
			var result = new Collection<IAdditionalSupplyChainActor>();
			bill.CusSupplyChainActorReferences.Cast<CusReference>().ForEach(x => result.Add(AdditionalSupplyChainActorWrapper.New(x)));
			return result;
		}

		public ICollection<IDepartureTransportMeans> DepartureTransportMeans => departureTransportMeans ?? (departureTransportMeans = GetDepartureTransportMeans());
		ICollection<IDepartureTransportMeans> departureTransportMeans;

		protected virtual ICollection<IDepartureTransportMeans> GetDepartureTransportMeans()
		{
			if (bill.Header.IsDepartureMovement)
			{
				var result = new Collection<IDepartureTransportMeans>();
				result.Add(DepartureTransportMeansWrapper.New(bill.Header.MovementHeader));
				return result;
			}
			else
			{
				var result = new Collection<IDepartureTransportMeans>();
				bill.Header.ArrivalMovementHeader.ArrivalTransportInfos.Cast<ArrivalCusTransportMeans>().ForEach(transportMeans => result.Add(ArrivalTransportMeansWrapper.New(transportMeans)));
				return result;
			}
		}

		public ICollection<IDocumentWithComplement> PreviousDocument => previousDocument ?? (previousDocument = GetPreviousDocuments());
		ICollection<IDocumentWithComplement> previousDocument;
		ICollection<IDocumentWithComplement> GetPreviousDocuments()
		{
			var result = new Collection<IDocumentWithComplement>();
			bill.PreviousDocuments.Cast<CusSupportingInfo>().ForEach(document => result.Add(DocumentWithComplementWrapper.New(document)));
			return result;
		}

		public ICollection<ISupportingDocument> SupportingDocument => supportingDocument ?? (supportingDocument = GetSupportingDocuments());
		ICollection<ISupportingDocument> supportingDocument;
		protected virtual ICollection<ISupportingDocument> GetSupportingDocuments()
		{
			var result = new Collection<ISupportingDocument>();
			bill.SupportingDocuments.Cast<NctsSupportingDocument>().ForEach(document => result.Add(SupportingDocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IDocument> TransportDocument => transportDocument ?? (transportDocument = GetTransportDocuments());
		ICollection<IDocument> transportDocument;
		protected virtual ICollection<IDocument> GetTransportDocuments()
		{
			var result = new Collection<IDocument>();
			bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == EU.Business.AdditionalInfoSubTypeList.Codes.TransportDocument).ForEach(document => result.Add(DocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IDocument> AdditionalReference => additionalReference ?? (additionalReference = GetAdditionalReferences());
		ICollection<IDocument> additionalReference;
		protected virtual ICollection<IDocument> GetAdditionalReferences()
		{
			var result = new Collection<IDocument>();
			bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference).ForEach(document => result.Add(DocumentWrapper.New(document)));
			return result;
		}

		public ICollection<IAdditionalInformation> AdditionalInformation => additionalInformation ?? (additionalInformation = GetAdditionalInformation());
		ICollection<IAdditionalInformation> additionalInformation;
		ICollection<IAdditionalInformation> GetAdditionalInformation()
		{
			var result = new Collection<IAdditionalInformation>();
			bill.AdditionalDocuments.Cast<CusSupportingInfo>().Where(x => x.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalInformation).ForEach(document => result.Add(AdditionalInformationWrapper.New(document)));
			return result;
		}

		public ITransportCharges TransportCharges => TransportChargesWrapper.New(bill.B0_TransportPaymentMethod);
		public ITransportCharges transportCharges;

		public ICollection<IConsignmentItem> ConsignmentItem => consignmentItem ?? (consignmentItem = GetConsignmentItems());
		ICollection<IConsignmentItem> consignmentItem;
		protected virtual ICollection<IConsignmentItem> GetConsignmentItems()
		{
			if (bill.Header.IsDepartureMovement)
			{
				var result = new Collection<IConsignmentItem>();
				bill.GoodsItems.Cast<NctsDepartureCargoDesc>().OrderBy(x => x.BY_LineNo).ForEach(item => result.Add(ConsignmentItemWrapper.New(item)));
				return result;
			}
			else if (bill.Header.IsArrivalMovement)
			{
				var result = new Collection<IConsignmentItem>();
				bill.ArrivalGoodsItems.Cast<NctsArrivalCargoDesc>().OrderBy(x => x.BY_LineNo).ForEach(item => result.Add(ConsignmentItemWrapper.New(item)));
				return result;
			}
			else
			{
				return null;
			}
		}
	}
}
