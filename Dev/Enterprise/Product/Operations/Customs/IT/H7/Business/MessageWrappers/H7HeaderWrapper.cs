using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts;
using CargoWise.Customs.IT.MessageContracts.Declaration;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class H7HeaderWrapper : IH7Header
{
	public H7HeaderWrapper(AsycudaBill bill)
	{
		this.bill = Argument.NotNull(bill, nameof(bill));
		this.header = bill.Header;
	}

	readonly AsycudaBill bill;
	readonly AsycudaManifestHeader header;

	public string AdditionalDeclarationType => bill.ABL_ShipmentType;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformation
	{
		get
		{
			if (additionalInfos == null)
			{
				var addInfosList = new List<AdditionalInformationWrapper>();
				bill.AdditionalInfos
					.Where(info => info.IsAnAdditionalInformation)
					.ForEach(info => addInfosList.Add(new AdditionalInformationWrapper(info)));

				additionalInfos = addInfosList.AsReadOnly();
			}

			return additionalInfos;
		}
	}

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences {
		get
		{
			if (additionalReferences == null)
			{
				var addRefList = new List<AdditionalReferenceWrapper>();
				bill.AdditionalInfos
					.Where(info => info.IsAnAdditionalReference)
					.ForEach(info => addRefList.Add(new AdditionalReferenceWrapper(info)));

				additionalReferences = addRefList.AsReadOnly();
			}

			return additionalReferences;
		}
	}

	public IDeclarationAmendment Amendment => null;

	public IH7EoriTrader Declarant => CachedValueHelper.GetValue(ref declarant, () => new H7EoriTraderWrapper(header.Declarant));

	CachedValue<IH7EoriTrader> declarant;

	public string DeclarationCustomsOffice => header.PresentationOffice;

	public string DeferredPayment => header.AMA_PaymentAccountNumber;

	public IEoriTrader Exporter => CachedValueHelper.GetValue(ref exporter, () => new EoriTraderWrapper(identificationNumber: null, address: bill.GetShipperAddress()));
	CachedValue<IEoriTrader> exporter;

	public IReadOnlyCollection<IFiscalReference> FiscalReferences 
	{
		get
		{
			if (fiscalReferences == null)
			{
				var fr5Condition = !bill.ABL_SellerRegNo.IsEmpty && bill.ABL_SellerRegNoType == OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
				if (fr5Condition)
				{
					var fiscalReferenceList = new List<FiscalReferenceWrapper>
					{
						new FiscalReferenceWrapper(bill.ABL_SellerRegNo, FiscalReferenceCodeList.Codes.FR5_Vendor)
					};

					fiscalReferences = fiscalReferenceList.AsReadOnly();
				}
				else
				{
					fiscalReferences = new List<FiscalReferenceWrapper>().AsReadOnly();
				}
			}

			return fiscalReferences;
		}
	}

	public decimal GrossMass => Constants.Weight.Convert(bill.ABL_GrossWeight, bill.ABL_GrossWeightUQ, Constants.Weight.Kilograms);

	public IEoriTrader Importer
	{
		get
		{
			if (bill.ABL_ConsigneeRegNoType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori && !bill.ABL_ConsigneeRegNo.IsEmpty)
			{
				return CachedValueHelper.GetValue(ref importer, () =>
					new EoriTraderWrapper(
						identificationNumber: bill.ABL_ConsigneeRegNo,
						address: bill.GetConsigneeAddress()
					)
				);
			}

			return null;
		}
	}

	CachedValue<IEoriTrader> importer;

	public string Lrn => IT.Business.ITEDIMessage.ITMessageNumberPlaceholder;

	public IH7LocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoods, () => new H7LocationOfGoodsWrapper(bill.CusGoodsLocation));
	CachedValue<IH7LocationOfGoods> locationOfGoods;

	public IH7Representative Representative => CachedValueHelper.GetValue(ref representative, () => new H7RepresentativeWrapper(header));
	CachedValue<IH7Representative> representative;

	public IReadOnlyCollection<IPreviousDocument> PreviousDocuments
	{
		get
		{
			if (previousDocuments == null)
			{
				var prevDocList = new List<PreviousDocumentWrapper>();
				bill.PreviousDocuments.ForEach(doc => prevDocList.Add(new PreviousDocumentWrapper(doc)));

				previousDocuments = prevDocList.AsReadOnly();
			}

			return previousDocuments;
		}
	}

	public IReadOnlyCollection<ISupportingDocument> SupportingDocuments
	{
		get
		{
			if (supportingDocuments == null)
			{
				var supportingDocsList = new List<SupportingDocumentWrapper>();
				bill.SupportingDocuments
					.ForEach(doc => supportingDocsList.Add(new SupportingDocumentWrapper(doc)));

				supportingDocuments = supportingDocsList.AsReadOnly();
			}

			return supportingDocuments;
		}
	}

	public IReadOnlyCollection<ITransportDocument> TransportDocuments
	{
		get
		{
			if (transportDocuments == null)
			{
				var transportDocsList = new List<TransportDocumentWrapper>();
				bill.AdditionalInfos
					.Where(doc => doc.IsATransportDocument)
					.ForEach(doc => transportDocsList.Add(new TransportDocumentWrapper(doc)));

				transportDocuments = transportDocsList.AsReadOnly();
			}
			
			return transportDocuments;
		}
	}

	public ICost TransportCosts => CachedValueHelper.GetValue(ref transportCosts, () => new CostWrapper(bill.ABL_InsuranceValue + bill.ABL_TransportValue, bill.ABL_RX_NKTransportValueCurrency));
	CachedValue<ICost> transportCosts;

	public string Ucr => bill.ABL_UCRNumber;

	ReadOnlyCollection<AdditionalReferenceWrapper> additionalReferences;
	ReadOnlyCollection<AdditionalInformationWrapper> additionalInfos;
	ReadOnlyCollection<SupportingDocumentWrapper> supportingDocuments;
	ReadOnlyCollection<PreviousDocumentWrapper> previousDocuments;
	ReadOnlyCollection<TransportDocumentWrapper> transportDocuments;
	ReadOnlyCollection<FiscalReferenceWrapper> fiscalReferences;
}
