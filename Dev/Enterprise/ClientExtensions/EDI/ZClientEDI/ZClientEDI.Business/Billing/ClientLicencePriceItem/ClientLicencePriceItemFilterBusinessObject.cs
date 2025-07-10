using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business;
public class ClientLicencePriceItemFilterBusinessObject : FilterStripBusinessObject, IRelatedModuleFilterBusinessObject
{
	public ClientLicencePriceItemFilterBusinessObject()
	{
		QueryObjectType = typeof(ClientLicencePriceItem);
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		ModuleFilterCollection filters = new ModuleFilterCollection();

		ModuleTextFilter itemFilter = new ModuleTextFilter("Parent Code", ClientLicencePriceItemSchema.L7_ParentCode);
		filters.AddCustomFilter(itemFilter);

		filters.AddFilter(new ModuleTextFilter("Category", ClientLicencePriceItemSchema.L7_Category));
		filters.AddFilter(new ModuleTextFilter("Charge Basis", ClientLicencePriceItemSchema.L7_ChargeBasis));
		filters.AddFilter(new ModuleTextFilter("Charge Code", ClientLicencePriceItemSchema.L7_ChargeCode));
		filters.AddFilter(new ModuleTextFilter("Code", ClientLicencePriceItemSchema.L7_Code));
		filters.AddFilter(new ModuleTextFilter("Country Tier Code", ClientLicencePriceItemSchema.L7_CountryTierCode));
		filters.AddFilter(new ModuleTextFilter("Description", ClientLicencePriceItemSchema.L7_Description));
		filters.AddFilter(new ModuleTextFilter("Discount Charge Code", ClientLicencePriceItemSchema.L7_DiscountChargeCode));
		filters.AddFilter(new ModuleTextFilter("Exchange Rate Group Code", ClientLicencePriceItemSchema.L7_ExchangeRateGroupCode));
		filters.AddFilter(new ModuleTextFilter("Fee Type", ClientLicencePriceItemSchema.L7_FeeType));
		filters.AddFilter(new ModuleTextFilter("Language", ClientLicencePriceItemSchema.L7_Language));
		filters.AddFilter(new ModuleTextFilter("PGM Discount Group Code", ClientLicencePriceItemSchema.L7_PGM_DiscountGroupCode));
		filters.AddFilter(new ModuleTextFilter("Parent Category", ClientLicencePriceItemSchema.L7_ParentCategory));
		filters.AddFilter(new ModuleTextFilter("Product Availability", ClientLicencePriceItemSchema.L7_ProductAvailability));
		filters.AddFilter(new ModuleTextFilter("Deposit Charge Code", ClientLicencePriceItemSchema.L7_DepositChargeCode));
		filters.AddFilter(new ModuleTextFilter("Product Display Category", ClientLicencePriceItemSchema.L7_ProductDisplayCategory));
		filters.AddFilter(new ModuleTextFilter("RX NK Currency", ClientLicencePriceItemSchema.L7_RX_NKCurrency));
		filters.AddFilter(new ModuleTextFilter("Ref4", ClientLicencePriceItemSchema.L7_Ref4));
		filters.AddFilter(new ModuleTextFilter("Unit Break Parent Code", ClientLicencePriceItemSchema.L7_UnitBreakParentCode));
		filters.AddFilter(new ModuleTextFilter("Web Parent Code", ClientLicencePriceItemSchema.L7_WebParentCode));
		filters.AddFilter(new ModuleFlagsFilter("Is Volume Adjustment Eligible", new[] { "Is Standard" }, new[] { ClientLicencePriceItemSchema.L7_IsVolumeAdjustmentEligible }));
		filters.AddFilter(new ModuleNumberRangeFilter("Licence Units", ClientLicencePriceItemSchema.L7_LicenceUnits));
		filters.AddFilter(new ModuleNumberRangeFilter("Order", ClientLicencePriceItemSchema.L7_Order));
		filters.AddFilter(new ModuleNumberRangeFilter("Price", ClientLicencePriceItemSchema.L7_Price));
		filters.AddFilter(new ModuleNumberRangeFilter("Unit Break", ClientLicencePriceItemSchema.L7_UnitBreak));
		filters.AddFilter(new ModuleTextFilter("Disbursement Country", ClientLicencePriceItemSchema.L7_RN_NKDisbursementCountry, new RefCountryCollection(Factory)));
		filters.AddFilter(new ModuleTextFilter("Disbursement Direction", ClientLicencePriceItemSchema.L7_DisbursementDirection, JobConfigurationSelectorLookups.GetBaseDirectionList()));

		return filters;
	}
}
