using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using QuoteStatusCodes = Enterprise.MasterFiles.Business.EPaymentStatusCodes.Quote;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APPaymentBatchPosterEPaymentQuoteFilterBusinessObject : FilterStripBusinessObject
	{
		public APPaymentBatchPosterEPaymentQuoteFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = "PaymentBatchForm";
			QueryObjectType = typeof(AccEPaymentQuote);
		}

		#region Filters

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			ModuleFilter filter = filters.AddFlagsFilter("Include Discarded Quotes", new string[] { ResString.GetMultilingualString("32a95235-6951-47a7-b7c7-768fa087ec17", "Include Discarded Quotes") }, new GetFlagsQuery[] { IncludeDiscardedQuery });
			filter.MultilingualDescription = ResString.GetMultilingualString("7a0b47ba-1504-4a58-9b95-2ff7ff544649", "Include Discarded Quotes");
			filter.Visibility = FilterVisibility.AlwaysVisible;

			filter = filters.AddTextFilter("Provider", AccEPaymentQuoteSchema.QU_ProviderCode, EPaymentProviderCodes.CodesList);
			filter.MultilingualDescription = ResString.GetMultilingualString("50b6ab0d-fff7-4ec0-a960-eb638a1e3b62", "Provider");

			filter = filters.AddNkFilter("Currency", AccEPaymentQuoteSchema.QU_RX_NKToCurrency, ModuleIDs.RefCurrency, new RefCurrencyCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("71ed4564-b4e9-4059-9a10-3f5cb659e80c", "Currency");

			filter = filters.AddDateFilter("Received(Date / Time range)", AccEPaymentQuoteSchema.QU_LastResponseReceivedUtc);
			filter.MultilingualDescription = ResString.GetMultilingualString("dee63c2a-7236-4efd-abaa-edf2572731e3", "Received(Date / Time range)");

			filter = filters.AddTextFilter("Status", AccEPaymentQuoteSchema.QU_Status, QuoteStatusCodes.CodesList);
			filter.MultilingualDescription = ResString.GetMultilingualString("256c9832-b402-48c6-b326-608b86d579c5", "Status");

			filter = filters.AddTextFilter("Quote Number", AccEPaymentQuoteSchema.QU_InternalReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("d7880894-8720-4b9f-8061-e041057c82d5", "Quote Number");

			filter = filters.AddTextFilter("Provider Reference", AccEPaymentQuoteSchema.QU_ProviderReference);
			filter.MultilingualDescription = ResString.GetMultilingualString("993b7201-b0fa-483e-87a9-a514550c9327", "Provider Reference");

			filter = filters.AddTextFilter("Error Message", AccEPaymentQuoteSchema.QU_ErrorDescription);
			filter.MultilingualDescription = ResString.GetMultilingualString("2f13cade-660e-45a0-98c8-5bcf8916c200", "Error Message");

			filter = filters.AddGuidFilter("Creditor", ModuleIDs.Organisation, GetCreditorQuery, new OrgHeaderCollection(Factory));
			filter.MultilingualDescription = ResString.GetMultilingualString("0d25f9c5-7b0d-42a7-93aa-8ac68cb8a3c9", "Creditor");

			return filters;
		}

		ZQuery IncludeDiscardedQuery(ZBool flag) => flag ? new ZQuery() : new ZQuery(AccEPaymentQuoteSchema.QU_Status, SQLComparisonOperator.NotEqual, QuoteStatusCodes.Discarded);

		ZQuery GetCreditorQuery(ZGuid value)
		{
			if (value.IsValid)
			{
				var result = new ZDBOnlyQuery(typeof(AccEPaymentQuote));
				var orderSubQuery = new ZDBOnlySubQuery(typeof(AccPaymentApproval), AccEPaymentQuoteSchema.QU_AV);
				orderSubQuery.AddToFilter(AccPaymentApprovalSchema.AV_OH, value);
				result.AddSubQuery(orderSubQuery, JoinCondition.And);
				return result;
			}

			return null;
		}

		#endregion Filters
	}
}
