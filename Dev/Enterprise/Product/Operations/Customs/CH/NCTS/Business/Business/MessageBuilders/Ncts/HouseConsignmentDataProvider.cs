using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using FuncsHelper = Enterprise.Customs.CH.Business.FuncsHelper;

namespace Enterprise.Customs.CH.NCTS.Business;

public class HouseConsignmentDataProvider : IHouseConsignment
{
	public static IEnumerable<HouseConsignmentDataProvider> NewCollection(NctsBillCollection bills)
	{
		return bills?.Cast<NctsBill>().OrderBy(bill => bill.SequenceNumber).Select((nctsBill, index) => new HouseConsignmentDataProvider(nctsBill, index + 1));
	}

	protected HouseConsignmentDataProvider(NctsBill nctsBill, int sequenceNumber)
	{
		this.nctsBill = Argument.NotNull(nctsBill, nameof(nctsBill));
		SequenceNumber = sequenceNumber;
	}
	protected readonly NctsBill nctsBill;

	public int? SequenceNumber { get; }

	INctsCommonCargoDescCollection<NctsCommonCargoDesc> GoodsItems
	{
		get
		{
			switch (nctsBill.Header.BH_HeaderType)
			{
				case NctsMovementType.Codes.Departure:
					return nctsBill.GoodsItems;
				case NctsMovementType.Codes.Arrival:
					return nctsBill.ArrivalGoodsItems;
				default:
					return null;
			}
		}
	}

	public string ReferenceNumberUCR => (nctsBill.Header.MovementHeader?.BM_UniqueConsignmentReference.IsEmpty ?? true) ? nctsBill.B0_ReferenceID.ReturnNullIfEmpty() : null;

	public decimal GrossMass => 0.0m;

	public string CountryOfDispatch => (countryOfDispatch ??= FuncsHelper.IsCHNT015V4Active ? GetCountryOfDispatch_V4() : GetCountryOfDispatch_V5()).ReturnNullIfEmpty();
	ZString? countryOfDispatch;

	ZString GetCountryOfDispatch_V4() => nctsBill.B0_RN_NKCountryOfExport;

	ZString GetCountryOfDispatch_V5()
	{
		var country = nctsBill.GoodsItems.SameOrDefault(x => x.EffectiveCountryCodeOfDispatch);
		if ((!nctsBill.Header.IsLinkedOrRelatedExport || (nctsBill.Header.MovementHeader?.BM_RN_NKCountryOfDispatch ?? ZString.Empty) == country) && nctsBill.Header.Bills.SelectMany(x => x.GoodsItems).All(x => x.EffectiveCountryCodeOfDispatch == country))
		{
			country = ZString.Empty;
		}
		return country;
	}

	public string CountryOfDestination => (countryOfDestination ??= GetCountryOfDestination()).ReturnNullIfEmpty();
	ZString? countryOfDestination;

	ZString GetCountryOfDestination()
	{
		var country = nctsBill.GoodsItems.SameOrDefault(x => x.EffectiveCountryCodeOfDestination);
		if (!nctsBill.Header.IsLinkedOrRelatedExport && nctsBill.Header.Bills.SelectMany(x => x.GoodsItems).All(x => x.EffectiveCountryCodeOfDestination == country))
		{
			country = ZString.Empty;
		}
		return country;
	}

	public string SecurityIndicatorFromExportDeclaration => null;

	public IConsignor Consignor => consignor ??= ConsignorDataProvider.New(nctsBill.Consignor, OrgConstants.ContactAllocationType.CUS);
	IConsignor consignor;

	public IConsignee Consignee => consignee ??= ConsigneeDataProvider.New(nctsBill.Consignee);
	IConsignee consignee;

	public IReadOnlyCollection<IConsignmentItem> ConsignmentItems => consignmentItems ??= GetConsignmentItemDataProvidersCore(GoodsItems)?.ToArray();
	IReadOnlyCollection<IConsignmentItem> consignmentItems;

	protected virtual IEnumerable<IConsignmentItem> GetConsignmentItemDataProvidersCore(INctsCommonCargoDescCollection<NctsCommonCargoDesc> goodsItems) => ConsignmentItemDataProvider.NewCollection(goodsItems);

	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??= DocumentDataProvider.NewCollection(nctsBill.PreviousDocuments).ToArray();
	IReadOnlyCollection<IDocument> previousDocuments;

	public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??= DocumentDataProvider.NewCollection(nctsBill.SupportingDocuments).ToArray();
	IReadOnlyCollection<IDocument> supportingDocuments;

	public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ??= TransportDocumentDataProvider.NewCollection(nctsBill.AdditionalDocuments).ToArray();
	IReadOnlyCollection<IDocument> transportDocuments;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= AdditionalInformationDataProvider.NewCollection(nctsBill.AdditionalDocuments, AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??= AdditionalReferenceDataProvider.NewCollection(nctsBill.AdditionalDocuments).ToArray();
	IReadOnlyCollection<IAdditionalReference> additionalReferences;

	public IReadOnlyCollection<IPackaging> Packagings => null;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => null;

	public IReadOnlyCollection<IDepartureTransportMeans> DepartureTransportMeans => null;

	public string TransportChargesMethodOfPayment => null;

	public bool IsPartialRelease => false;

	public bool IsFullRelease => false;

	public bool IsBlocked => false;
}
