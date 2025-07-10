using Enterprise.Client.EDI.Billing.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.Billing.ClientLicencePriceHeader;

public class ClientLicencePriceHeaderFilterBusinessObject : FilterStripBusinessObject, IRelatedModuleFilterBusinessObject
{
	public ClientLicencePriceHeaderFilterBusinessObject()
	{
		QueryObjectType = typeof(Enterprise.Client.EDI.Billing.Business.ClientLicencePriceHeader);
		_staffList = new GlbStaffCollection(Factory);
		_currency = new RefCurrencyCollection(Factory);
		_country = new RefCountryCollection(Factory);
	}

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		ModuleFilterCollection filters = new ModuleFilterCollection();

		ModuleNkFilter headerFilter = new ModuleNkFilter("Header Created By",
			ClientLicencePriceHeaderSchema.L6_SystemCreateUser, ModuleIDs.GlbStaff, _staffList);
		filters.AddCustomFilter(headerFilter);

		filters.AddFilter(new ModuleTextFilter("Version", ClientLicencePriceHeaderSchema.L6_PricelistVersion));
		filters.AddFilter(new ModuleTextFilter("Edition", ClientLicencePriceHeaderSchema.L6_LicenceEdition));
		filters.AddFilter(new ModuleTextFilter("Discount Code", ClientLicencePriceHeaderSchema.L6_DiscountCode));
		filters.AddFilter(new ModuleTextFilter("Test Db Code", ClientLicencePriceHeaderSchema.L6_TestDbPriceCode));
		filters.AddFilter(new ModuleTextFilter("Rounding Params", ClientLicencePriceHeaderSchema.L6_Rounding));
		filters.AddFilter(new ModuleFlagsFilter("Is Standard", new[] { "Is Standard" }, new[] { ClientLicencePriceHeaderSchema.L6_IsStandard }));
		filters.AddFilter(new ModuleFlagsFilter("Use Std Discount", new string[] { "Use Std Discount" },
			new[] { ClientLicencePriceHeaderSchema.L6_UseStandardDiscount }));
		filters.AddFilter(new ModuleFlagsFilter("Has Exchange Rates", new string[] { "Has Exchange Rates" },
			new[] { ClientLicencePriceHeaderSchema.L6_HasExchangeRates }));
		filters.AddFilter(new ModuleFlagsFilter("Is Disbursement Bundle", new string[] { "Is Disbursement Bundle" },
			new[] { ClientLicencePriceHeaderSchema.L6_IsDisbursementBundle }));
		filters.AddFilter(new ModuleDateFilter("Valid From", ClientLicencePriceHeaderSchema.L6_ValidFrom, true));
		filters.AddFilter(new ModuleDateFilter("Valid To", ClientLicencePriceHeaderSchema.L6_ValidTo, true));
		filters.AddFilter(new ModuleDateFilter("Header Created", ClientLicencePriceHeaderSchema.L6_SystemCreateTimeUtc, true));
		filters.AddFilter(new ModuleNumberRangeFilter("Unit Rate", ClientLicencePriceHeaderSchema.L6_LicenceUnitRate));
		filters.AddFilter(new ModuleNumberRangeFilter("Live Months (until Test DB Billing)", ClientLicencePriceHeaderSchema.L6_LiveMonthsUntilTestDbBilling));
		filters.AddFilter(new ModuleNkFilter("Header Currency", ClientLicencePriceHeaderSchema.L6_RX_NKCurrency, ModuleIDs.RefCurrency, _currency));
		filters.AddFilter(new ModuleTextFilter("System", ClientLicencePriceHeaderSchema.L6_SystemCode,
			BillingConstants.PriceHeaderType.GetPriceHeaderTypeList()));
		filters.AddFilter(new ModuleTextFilter("Country", ClientLicencePriceHeaderSchema.L6_RN_NKCountry, _country));

		return filters;
	}

	readonly GlbStaffCollection _staffList;
	readonly RefCurrencyCollection _currency;
	readonly RefCountryCollection _country;
}
