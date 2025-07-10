using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class ChequeFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public ChequeFilterBusinessObject()
			: base()
		{
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			filters.AddNumberFilter("Cheque #", AccReceivedChequeSchema.RCH_ChequeNumber).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_ChequeNumber", "Cheque #");

			filters.AddNumberFilter("Cheque Portfolio #", AccReceivedChequeSchema.RCH_ChequeReference).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_ChequeReference", "Cheque Portfolio #");

			filters.AddDateFilter("Due Date", AccReceivedChequeSchema.RCH_DueDate).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_DueDate", "Due Date");

			var filter = (ModuleFilter)filters.AddTextFilter("Status", AccReceivedChequeSchema.RCH_Status, StatusList);
			filter.Category = FilterCategories.StatusAndFlags;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccHotChequeFilter|Status", "Status");

			filter = filters.AddGuidFilter("Received Debtor", ModuleIDs.Organisation, AccReceivedChequeSchema.RCH_OH_ReceivedFrom, AQ_OHList);
			filter.Category = FilterCategories.Organisations;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_OH_ReceivedFrom", "Received Debtor");

			filters.AddTextFilter("Cheque Drawer", AccReceivedChequeSchema.RCH_ChequeDrawer).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_ChequeDrawer", "Cheque Drawer");

			filters.AddTextFilter("Cheque Bank Account", AccReceivedChequeSchema.RCH_BankName).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_BankName", "Cheque Bank Account");

			filters.AddTextFilter("Place of Payment", AccReceivedChequeSchema.RCH_PaymentLocation).MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_PaymentLocation", "Place of Payment");

			filter = filters.AddNumberRangeFilter("Amount", AccReceivedChequeSchema.RCH_Amount);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_Amount", "Amount");

			filter = filters.AddNkFilter("Currency", AccReceivedChequeSchema.RCH_RX_NKChequeCurrency, ModuleIDs.RefCurrency, CurrencyList);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_RX_NKChequeCurrency", "Currency");

			filter = filters.AddGuidFilter("Given Bank Account", ModuleIDs.AccBankAccount, AccReceivedChequeSchema.RCH_AB_GivenBankAccount, BankAccounts);
			filter.Category = FinancialDetailsCategory;
			filter.MultilingualDescription = ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|RCH_AB_GivenBankAccount", "Given Bank Account");

			return filters;
		}

		#region FinancialDetails Category

		protected FilterCategory FinancialDetailsCategory => financialDetailsCategory ??= new FilterCategory(ResString.GetMultilingualString("Accounting|AccReceivedChequeFilter|FinancialDetails", "Financial Details"));
		FilterCategory financialDetailsCategory;

		#endregion

		#region BankList

		public AccBankAccountCollection BankAccounts => FindboxLookupCollections.GetBankAccounts(Factory);

		#endregion

		#region StatusList

		public CodeDescriptionPairList StatusList => statusList ??= new CodeDescriptionPairList() { new CodeDescriptionPair("COH", Res.GetString("Accounting|AccReceivedChequeFilter|COH", "Cheque on Hold")) };
		CodeDescriptionPairList statusList;

		#endregion

		#region AQ_OHList

		public OrgHeaderCollection AQ_OHList => aq_OHList ??= new OrgHeaderCollection(Factory);
		OrgHeaderCollection aq_OHList;

		#endregion

		#region CurrencyList

		public RefCurrencyCollection CurrencyList => currencies ??= new RefCurrencyCollection(Factory);
		RefCurrencyCollection currencies;

		#endregion
	}
}
