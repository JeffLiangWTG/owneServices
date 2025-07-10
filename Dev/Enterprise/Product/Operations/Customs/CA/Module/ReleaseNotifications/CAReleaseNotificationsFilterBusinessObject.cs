using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	class CAReleaseNotificationsFilterBusinessObject : EDIMessageFilterBusinessObject
	{
		protected GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get { return simpleQueryHelper ?? (simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(EDIMessage))); }
		}
		GenAddOnColumnQueryHelper simpleQueryHelper;

		internal new static class Constants
		{
			internal const string WarehouseCode = "Warehouse Code";
			internal const string CargoControlNumber = "Cargo Control Number";
			internal const string TransactionNumber = "Transaction Number";
			internal const string ReleaseDate = "Release Date";
			internal const string ProcessingDate = "Processing Date";
			internal const string CBSAOffice = "CBSA Office";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();

			var nkFiler = result.AddNkFilter(Constants.WarehouseCode, (c, v) => SimpleQueryHelper.GetQueryHandlingBlanks(ReleaseStatus.Schema.RL_WarehouseCode, c, v), ModuleIDs.Customs.CA.SubLocation, new CACSubLocationCollection(Factory));
			nkFiler.Category = FilterCategories.Locations;
			nkFiler.MaxLength = ReleaseStatus.Schema.RL_WarehouseCodeMaxLength;
			nkFiler = result.AddNkFilter(Constants.CBSAOffice, (c, v) => SimpleQueryHelper.GetQueryHandlingBlanks(ReleaseStatus.Schema.RL_ReleaseOffice, c, v), ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetOffices());
			nkFiler.Category = FilterCategories.Locations;
			nkFiler.MaxLength = ReleaseStatus.Schema.RL_ReleaseOfficeMaxLength;
			result.AddNumberFilter(Constants.CargoControlNumber, GetCargoControlNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(EDIMessageSchema.EM_ApplicationReference);
			var transactionFilter = result.AddTextFilter(Constants.TransactionNumber, GetTransactionNumberQuery)
				.WithMaxLengthOf<ModuleTextFilter>(EDIMessageSchema.EM_ApplicationReference);
			transactionFilter.Category = FilterCategories.NumbersAndReferences;
			result.AddDateFilter(Constants.ReleaseDate, GetReleaseDateQuery);
			result.AddDateFilter(Constants.ProcessingDate, GetProcessingDateQuery);
			return result;
		}

		ZQuery GetCargoControlNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter_PossiblyCommaSeparated(EDIMessageSchema.EM_ApplicationReference, comparisonOperator, value.Replace(" ", ""));
			return query;
		}

		ZQuery GetTransactionNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return EDIMessageQueryHelper.GetSegmentQueryWithSchema(comparisonOperator, value);
		}

		public static ZDBOnlySubQuery GetGenAddOnTextQuery(SQLComparisonOperator comparisonOperator, ZString value, string columnName)
		{
			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			return genAddOnQuery;
		}

		ZQuery GetReleaseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return SimpleQueryHelper.GetQueryOnGenAddOnColumn(EDIMessage.Schema.RNSReleaseDate, comparisonOperator, startDate, endDate);
		}

		ZQuery GetProcessingDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			return SimpleQueryHelper.GetQueryOnGenAddOnColumn(EDIMessage.Schema.RNSProcessingDate, comparisonOperator, startDate, endDate);
		}

		protected override ZBool ShouldAddEHubIDFilters
		{
			get { return false; }
		}

		ZZRefCusCodeListCombinedCollection GetOffices()
		{
			return ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
		}
	}
}
