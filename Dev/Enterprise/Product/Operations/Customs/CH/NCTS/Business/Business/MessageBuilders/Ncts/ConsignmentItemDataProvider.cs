using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using FuncsHelper = Enterprise.Customs.CH.Business.FuncsHelper;

namespace Enterprise.Customs.CH.NCTS.Business;

public class ConsignmentItemDataProvider : IConsignmentItem
{
	public static IEnumerable<ConsignmentItemDataProvider> NewCollection(INctsCommonCargoDescCollection<NctsCommonCargoDesc> goodsItems)
	{
		return goodsItems?.OrderBy(goodsItem => goodsItem.BY_LineNo).Select(goodsItem => new ConsignmentItemDataProvider(goodsItem));
	}

	protected ConsignmentItemDataProvider(NctsCommonCargoDesc goodsItem)
	{
		this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		departureGoodsItem = goodsItem as NctsDepartureCargoDesc;
	}
	readonly NctsCommonCargoDesc goodsItem;
	readonly NctsDepartureCargoDesc departureGoodsItem;

	NctsDepartureMovementHeader departureMovementHeader => departureGoodsItem?.MoveHeader;

	bool IsNationalTransit => departureMovementHeader?.IsNationalTransitSwitzerland ?? false;

	public int GoodsItemNumber => goodsItem.BY_LineNo;

	public int? DeclarationGoodsItemNumber => goodsItem.BY_DeclarationGoodsItemNumber;

	public string CountryOfDispatch => (countryOfDispatch ??= FuncsHelper.IsCHNT015V4Active ? GetCountryOfDispatchV4() : GetCountryOfDispatchV5()).ReturnNullIfEmpty();
	ZString? countryOfDispatch;

	ZString GetCountryOfDispatchV4() =>  goodsItem.BY_RN_NKCountryOfDispatch;

	ZString GetCountryOfDispatchV5()
	{
		var country = departureGoodsItem.EffectiveCountryCodeOfDispatch;
		if (departureGoodsItem.Bill.GoodsItems.Cast<NctsDepartureCargoDesc>().All(x => x.EffectiveCountryCodeOfDispatch == country))
		{
			country = ZString.Empty;
		}
		return country;
	}

	public string CountryOfDestination => (countryOfDestination ??= FuncsHelper.IsCHNT015V4Active ? GetCountryOfDestinationV4() : GetCountryOfDestinationV5()).ReturnNullIfEmpty();
	ZString? countryOfDestination;

	ZString GetCountryOfDestinationV4() => NctsDataProviderHelper.IsSendDestinationCountryAtConsignmentLevelV4((NctsHeader)goodsItem.Header) ? ZString.Empty : departureGoodsItem.EffectiveCountryCodeOfDestination;

	ZString GetCountryOfDestinationV5()
	{
		var country = departureGoodsItem.EffectiveCountryCodeOfDestination;
		if (departureGoodsItem.Bill.GoodsItems.Cast<NctsDepartureCargoDesc>().All(x => x.EffectiveCountryCodeOfDestination == country))
		{
			country = ZString.Empty;
		}
		return country;
	}

	public string ReferenceNumberUCR
	{
		get
		{
			if (departureGoodsItem?.Header.MovementHeader is NctsDepartureMovementHeader movementHeader)
			{
				return (movementHeader.BM_UniqueConsignmentReference.IsEmpty && goodsItem.Bill.B0_ReferenceID.IsEmpty) ? goodsItem.BY_CommercialReferenceNumber.ReturnNullIfEmpty() : null;
			}
			return goodsItem.BY_CommercialReferenceNumber.ReturnNullIfEmpty();
		}
	}

	public string DeclarationType => goodsItem.BY_Type;

	public ICommodity Commodity => commodity ??= GetCommodityDataProviderCore();
	ICommodity commodity;

	protected virtual ICommodity GetCommodityDataProviderCore() => CommodityDataProvider.New(goodsItem);

	public IReadOnlyCollection<IPackaging> Packagings => packagings ??= GetPackagingDataProvidersCore(goodsItem.Packages).ToArray();
	IReadOnlyCollection<IPackaging> packagings;

	protected virtual IEnumerable<IPackaging> GetPackagingDataProvidersCore(INctsPackageCollection<EU.NCTS.Business.NctsPackage, NctsCommonCargoDesc> packages) => ConsignmentItemPackagingDataProvider.NewCollection(packages);

	public IReadOnlyCollection<IDocument> PreviousDocuments => previousDocuments ??= departureGoodsItem != null ? DocumentDataProvider.NewCollection(departureGoodsItem.PreviousDocuments).ToArray() : null;
	IReadOnlyCollection<IDocument> previousDocuments;

	public IReadOnlyCollection<IDocument> SupportingDocuments => supportingDocuments ??= DocumentDataProvider.NewCollection(goodsItem.SupportingDocuments).ToArray();
	IReadOnlyCollection<IDocument> supportingDocuments;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= AdditionalInformationDataProvider.NewCollection(goodsItem.AdditionalInfos, AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public IReadOnlyCollection<IAdditionalReference> AdditionalReferences => additionalReferences ??= AdditionalReferenceDataProvider.NewCollection(goodsItem.AdditionalInfos).ToArray();
	IReadOnlyCollection<IAdditionalReference> additionalReferences;

	public IReadOnlyCollection<IAdditionalSupplyChainActor> AdditionalSupplyChainActors => additionalSupplyChainActors ??= IsNationalTransit || departureGoodsItem == null ? null : AdditionalSupplyChainActorDataProvider.NewCollection(departureGoodsItem.CusSupplyChainActorReferences).ToArray();
	IReadOnlyCollection<IAdditionalSupplyChainActor> additionalSupplyChainActors;

	public virtual string UnloadingRemarkCode => string.Empty;

	public virtual string UnloadingRemarkText => string.Empty;

	public IRefinement Refinement => null;

	public IRefund Refund => null;

	public bool IsReleased => false;

	public bool IsBlocked => false;

	public IConsignee Consignee => null;

	public IReadOnlyCollection<IDocument> TransportDocuments => null;

	public ITransportCharges TransportCharges => null;
}
