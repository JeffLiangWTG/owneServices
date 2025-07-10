using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Module
{
	public class DocumentListMessagesFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string MessageNumber = "Message Number";
			public const string Status = "Status";
			public const string MessageTime = "Message Time";

			public const string InterchangeNumber = "Interchange Number";
			public const string InterchangeDateTimeSent = "Interchange Date Time Sent";
			public const string EHubID = "eHub ID";

			public const string DocumentType = "Document Type";
			public const string RequestStatus = "Request Status";
		}
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var messageNumberFilter = result.AddNumberFilter(Schema.MessageNumber, EDIMessageSchema.EM_MessageNum);
			messageNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|MessageNumber", Schema.MessageNumber);

			var statusFilter = result.AddTextFilter(Schema.Status, EDIMessageSchema.EM_Status, Factory.GetCachedValue<EDIMessageStatusList>());
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|Status", Schema.Status);
			statusFilter.Category = FilterCategories.StatusAndFlags;

			var messageTimeFilter = result.AddDateFilter(Schema.MessageTime, EDIMessageSchema.EM_SystemCreateTimeUtc, true);
			messageTimeFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|MessageTime", Schema.MessageTime);

			var interchangeNumberFilter = result.AddNumberFilter(Schema.InterchangeNumber, GetInterchangeNumberQuery);
			interchangeNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|InterchangeNumber", Schema.InterchangeNumber);

			var interchangeDateTimeSentFilter = result.AddDateFilter(Schema.InterchangeDateTimeSent, GetInterchangeSentDateQuery);
			interchangeDateTimeSentFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|InterchangeDateTimeSent", Schema.InterchangeDateTimeSent);

			var eHubIDFilter = result.AddTextFilter(Schema.EHubID, GetEHubIDQuery);
			eHubIDFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|EHubID", Schema.EHubID);
			eHubIDFilter.ComparisonOperator_List.Clear();
			eHubIDFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));

			var documentTypeFilter = result.AddTextFilter(Schema.DocumentType, GetCusPollingTransactionTypeQuery, Factory.GetCachedValue<ElectronicDocumentTypeList>());
			documentTypeFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|DocumentType", Schema.DocumentType);
			documentTypeFilter.Category = FilterCategories.ModesAndTypes;

			var requestStatusFilter = result.AddTextFilter(Schema.RequestStatus, GetCusPollingTransactionStatusQuery, Factory.GetCachedValue<CusPollingTransactionStatusList>());
			requestStatusFilter.MultilingualDescription = ResString.GetMultilingualString("DocumentListMessagesFilterStripBusinessObject|RequestStatus", Schema.RequestStatus);
			requestStatusFilter.Category = FilterCategories.StatusAndFlags;

			return result;
		}
		ZQuery GetInterchangeNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

			var interchangeQuery = new ZDBOnlySubQuery(typeof(Enterprise.Messaging.Business.EDIInterchange), EDIMessageSchema.EM_EI);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, comparisonOperator, value.KeepAlphanumericCharacters());
			messageQuery.AddSubQuery(interchangeQuery, JoinCondition.And);

			return messageQuery;
		}
		ZQuery GetInterchangeSentDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

			var interchangeQuery = new ZDBOnlySubQuery(typeof(Enterprise.Messaging.Business.EDIInterchange), EDIMessageSchema.EM_EI);
			AddDateTimeRange(interchangeQuery, comparisonOperator, JoinCondition.And, EDIInterchangeSchema.EI_SystemCreateTimeUtc, value1, value2, true);
			messageQuery.AddSubQuery(interchangeQuery, JoinCondition.And);

			return messageQuery;
		}
		ZQuery GetEHubIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

			var interchangeQuery = new ZDBOnlySubQuery(typeof(Enterprise.Messaging.Business.EDIInterchange), EDIMessageSchema.EM_EI);
			interchangeQuery.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, System.Guid.TryParse(value, out System.Guid guid) ? guid : System.Guid.Empty);
			messageQuery.AddSubQuery(interchangeQuery, JoinCondition.And);

			return messageQuery;
		}

		ZQuery GetCusPollingTransactionTypeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

			var transactionQuery = new ZDBOnlySubQuery(typeof(CusPollingTransaction), CusPollingTransactionSchema.CPT_ParentID);
			transactionQuery.AddToFilter(CusPollingTransactionSchema.CPT_Reference, comparisonOperator, value.KeepAlphanumericCharacters());
			messageQuery.AddSubQuery(transactionQuery, JoinCondition.And);

			return messageQuery;
		}
		ZQuery GetCusPollingTransactionStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));

			var transactionQuery = new ZDBOnlySubQuery(typeof(CusPollingTransaction), CusPollingTransactionSchema.CPT_ParentID);
			transactionQuery.AddToFilter(CusPollingTransactionSchema.CPT_Status, comparisonOperator, value.KeepAlphanumericCharacters());
			messageQuery.AddSubQuery(transactionQuery, JoinCondition.And);

			return messageQuery;
		}
	}
}
