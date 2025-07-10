using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NationalTransitCommoditySpecificationDataProvider : ICommoditySpecification
{
	public static ICommoditySpecification New(NctsDepartureCargoDesc goodsItem) => goodsItem != null ? new NationalTransitCommoditySpecificationDataProvider(goodsItem) : null;

	NationalTransitCommoditySpecificationDataProvider(NctsDepartureCargoDesc goodsItem)
	{
		this.goodsItem = goodsItem;
	}
	readonly NctsDepartureCargoDesc goodsItem;

	public int BorderValue => 0;

	public string InvoiceCurrency => null;

	public bool? GoodsReturned => null;

	public bool? NonTradingGoods => null;

	public bool? OwnPropulsion => null;

	public int? CompensationType => 0;

	public bool? Repair => null;

	public bool? RestrictionObligation => Restrictions.Any();

	public IReadOnlyCollection<IRestriction> Restrictions => restrictions ??= RestrictionDataProvider.NewCollection(goodsItem.Restrictions).ToArray();
	IReadOnlyCollection<IRestriction> restrictions;

	public IReadOnlyCollection<IAdditionalInformation> AdditionalInformations => additionalInformations ??= AdditionalInformationDataProvider.NewCollection(goodsItem.AdditionalInfos, AdditionalInfoSubTypeList.Codes.AdditionalInformation).ToArray();
	IReadOnlyCollection<IAdditionalInformation> additionalInformations;

	public string CountryOfOrigin => null;

	public string CountryOfProduction => null;

	public string InvoiceValue => null;

	public bool? NetAssessment => null;

	public bool? Preference => null;

	public IReadOnlyCollection<IAdditionalTax> AdditionalTaxes => null;

	public IReadOnlyCollection<IFee> Fees => null;

	public IAssessment NetWeightAssessment => null;

	public ITaxInformation TaxInformation => null;
}
