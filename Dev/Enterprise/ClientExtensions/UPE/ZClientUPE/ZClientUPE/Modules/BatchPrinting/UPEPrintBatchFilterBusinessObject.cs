using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class UPEPrintBatchFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			result.AddDateFilter("Last Printed", ClientPrintBatchSchema.T7_LastPrintedDate);
			result.AddTextFilter("Document Type", ClientPrintBatchSchema.T7_BatchType, new UPEPrintBatchTypes()).Visibility = FilterVisibility.AlwaysVisible;

			ModuleTextFilter statusFilter = result.AddTextFilter("Print Status", GetPrintStatusQuery, PrintStatusList);
			statusFilter.Property = "Unprinted";
			statusFilter.Visibility = FilterVisibility.AlwaysVisible;

			return result;
		}

		protected override ModuleFilter GetModuleFilterThatOverridesAllOtherFiltersCore()
		{
			return new ModuleNumberFilter("Batch Number", GetBatchNumberQuery);
		}

		#region Lookups

		CodeDescriptionPairList PrintStatusList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair("Unprinted", "Unprinted");
				result.AddPair("Failed", "Failed");
				return result;
			}
		}

		#endregion

		#region Filters

		ZQuery GetBatchNumberQuery(SQLComparisonOperator @operator, ZString value)
		{
			ZQuery result = new ZQuery();

			ZInt intValue;
			if (ZInt.TryParse(value, out intValue) && intValue > 0)
			{
				result.AddToFilter_PossiblyCommaSeparated(ClientPrintBatchSchema.T7_BatchNumber, intValue);
			}

			return result;
		}

		ZQuery GetPrintStatusQuery(ZString value)
		{
			ZQuery query = new ZQuery();

			if (value == "Unprinted")
			{
				query.AddToFilter(JoinCondition.And, ClientPrintBatchSchema.T7_LastPrintedDate, null);
			}
			else if (value == "Failed")
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(UPEPrintBatch));

				ZDBOnlySubQuery printBatchItemSubQuery = new ZDBOnlySubQuery(typeof(UPEPrintBatchItem), ClientPrintBatchItemSchema.T6_T7);
				ZDBOnlySubQuery stmALogSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);

				stmALogSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, new[] { Events.DocumentNotDeliveredCode, Events.DocumentFailedToBeDeliveredReplacedByDNDEventCode });
				stmALogSubQuery.AddFilterAndZSQLParameterCollection(StmALogSchema.SL_EventTime.Name + ">=" + ClientPrintBatchSchema.T7_LastPrintedDate.Name, new ZSqlParameterCollection());
				stmALogSubQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, new UPEPrintBatchTypes().GetDocumentEmailSubjectContainsString(value));

				printBatchItemSubQuery.AddSubQuery(ClientPrintBatchItemSchema.T6_ParentID, stmALogSubQuery, JoinCondition.And);
				dBOnlyQuery.AddSubQuery(printBatchItemSubQuery, JoinCondition.And);

				dBOnlyQuery.AddToFilter(JoinCondition.And, ClientPrintBatchSchema.T7_LastPrintedDate, SQLComparisonOperator.NotEqual, null);
				query.AddToFilter(dBOnlyQuery);
			}

			return query;
		}

		#endregion
	}
}
