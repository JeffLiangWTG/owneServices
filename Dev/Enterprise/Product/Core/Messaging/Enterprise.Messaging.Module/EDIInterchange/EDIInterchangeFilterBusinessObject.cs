using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDIInterchangeFilterBusinessObject : FilterStripBusinessObject
	{
		#region Descriptions

		public static class Descriptions
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter Name Constant")]
			public static readonly string eHubFilter = "eHub ID";
		}

		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			ModuleFilter filter = result.AddNumberFilter(Constants.NumberTypes.InterchangeNum, EDIInterchangeSchema.EI_InterchangeNum);
			filter.Visibility = FilterVisibility.AlwaysVisible;
			filter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|InterchangeNum", "Interchange Number");

			ModuleNumberFilter messageNumFilter = result.AddNumberFilter(Constants.NumberTypes.MessageNum, EDIMessageSchema.EM_MessageNum);
			messageNumFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|MessageNum", "Message Number");
			messageNumFilter.SubGroup = InterchangeSubGroup;

			ModuleTextFilter statusFilter = result.AddTextFilter("Status", GetStatusQuery, EI_Status_List);
			statusFilter.Property = Constants.StatusTypes.AllExceptAcknowledgments;
			statusFilter.Visibility = FilterVisibility.AlwaysVisible;
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|Status", "Status");

			result.AddTextFilter("Direction", EDIInterchangeSchema.EI_ReceiveTransmit, EI_ReceiveTransmit_List).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|Direction", "Direction");
			result.AddTextFilter("Application Code", EDIInterchangeSchema.EI_ApplicationCode).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|ApplicationCode", "Application Code");
			result.AddTextFilter("Type", EDIInterchangeSchema.EI_InterchangeType).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|Type", "Type");
			result.AddTextFilter("Sender", EDIInterchangeSchema.EI_From).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|Sender", "Sender");
			result.AddTextFilter("Receiver", EDIInterchangeSchema.EI_To).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|Receiver", "Receiver");
			result.AddTextFilter("Header Text", GetHeaderTextQuery).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|HeaderText", "Header Text");
			result.AddTextFilter("Transport Type", EDIInterchangeSchema.EI_TransportType, EI_TransportType_List).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|TransportType", "Transport Type");
			result.AddDateFilter("Interchange Time", EDIInterchangeSchema.EI_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|InterchangeTime", "Interchange Time");

			result.AddFilter(new EDIInterchangeTextFilter("Body Text", this)
			{
				MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|BodyText", "Body Text"),
				SubGroup = InterchangeBodyTextSubGroup
			});
			var eHubIdFilter = new EDIInterchangeEHubIdFilter(Descriptions.eHubFilter);
			result.AddFilter(eHubIdFilter);
			eHubIdFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIInterchangeFilter|eHubTrackingID", "eHub ID");

			UpdateComparisionOperator(eHubIdFilter);
			return result;
		}

		ZQuery GetHeaderTextQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			if (comparisonOperator == SQLComparisonOperator.DoesNotStartWith
				|| comparisonOperator == SQLComparisonOperator.NotContains
				|| comparisonOperator == SQLComparisonOperator.NotEqual)
			{
				query.AddToFilter(EDIInterchangeSchema.EI_HeaderText, comparisonOperator, value);
				query.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_HeaderNText, comparisonOperator, value);
			}
			else
			{
				query.AddToFilter(EDIInterchangeSchema.EI_HeaderText, comparisonOperator, value);
				query.AddToFilter(JoinCondition.Or, EDIInterchangeSchema.EI_HeaderNText, comparisonOperator, value);
			}
			return query;
		}

		internal void UpdateComparisionOperator(ModuleTextFilter eHubFilter)
		{
			eHubFilter.ComparisonOperator_List.Clear();
			eHubFilter.ComparisonOperator_List.AddPair(Res.GetString("Messaging|EDIInterchangeFilter|eHubIDFilter|Option|Exact", "exact"), Res.GetString("Messaging|EDIInterchangeFilter|eHubIDFilter|ComparisonConstraintDescription|Exact", "Search for an Exact match"));
		}

		#region Constants

		public static class Constants
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's an identifier")]
			public static class NumberTypes
			{
				public const string InterchangeNum = "Interchange Number";
				public const string MessageNum = "Message Number";
			}

			public static class StatusTypes
			{
				public const string AllExceptAcknowledgments = "AEA";
				public const string Queued = "QUE";
			}
		}

		#endregion

		#region Status

		ZQuery GetStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value == Constants.StatusTypes.AllExceptAcknowledgments)
			{
				result.AddToFilter(EDIInterchangeSchema.EI_Status, SQLComparisonOperator.NotEqual, EDIInterchange.Status.Acknowledged);
			}
			else
			{
				result.AddToFilter(EDIInterchangeSchema.EI_Status, value);
			}
			return result;
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList EI_Status_List
		{
			get
			{
				if (status_List == null)
				{
					status_List = new CodeDescriptionPairList();
					status_List.AddPair(Constants.StatusTypes.AllExceptAcknowledgments, Res.GetString("Messaging|EDIInterchangeFilter|Status|AllExceptAcknowledgments", "All Except Acknowledgments"));
					var eDIInterchangeStatus = new EDIInterchangeStatusList();
					var statusPairs = from statusPair in typeof(EDIInterchangeStatusList.Codes).GetFields()
									  where statusPair.FieldType == typeof(string)
									  select new { Code = (string)statusPair.GetValue(statusPair), Description = eDIInterchangeStatus.GetDescriptionFromCode((string)statusPair.GetValue(statusPair)), Identifier = statusPair.Name }
									;
					foreach (var status in statusPairs)
					{
						status_List.AddPair(status.Code, status.Description);
					}
				}
				return status_List;
			}
		}
		CodeDescriptionPairList status_List;

		CodeDescriptionPairList EI_ReceiveTransmit_List
		{
			get { return receiveTransmit_List ?? (receiveTransmit_List = new ReceiveTransmitList()); }
		}
		CodeDescriptionPairList receiveTransmit_List;

		CodeDescriptionPairList EI_TransportType_List
		{
			get { return transportType_List ?? (transportType_List = new EDIInterchangeTransportTypeList()); }
		}
		CodeDescriptionPairList transportType_List;

		#endregion

		#region Overrides

		public override ZQuery Filter
		{
			get
			{
				ZQuery baseFilter = base.Filter;
				if (AreSystemMessagesHidden)
				{
					baseFilter.AddToFilter(JoinCondition.And, EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.NotEqual, ApplicationCodeList.Codes.SYS);
				}
				return baseFilter;
			}
		}

		protected virtual bool AreSystemMessagesHidden
		{
			get { return !Env.CurrentUser.LoggedInWithMasterPassword; }
		}

		#endregion

		#region SubGroup
		ModuleFilterSubGroup interchangeSubGroup;
		ModuleFilterSubGroup interchangebodyTextSubGroup;

		public ModuleFilterSubGroup InterchangeSubGroup
		{
			get { return interchangeSubGroup ?? (interchangeSubGroup = new EDIInterchangeSubGroup()); }
		}
		public ModuleFilterSubGroup InterchangeBodyTextSubGroup
		{
			get { return interchangebodyTextSubGroup ?? (interchangebodyTextSubGroup = new EDIInterchangeBodyTextSubGroup()); }
		}

		class EDIInterchangeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_EI);
				subQuery.AddToFilter(filter);
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.And);
				return dBOnlyQuery;
			}
		}

		class EDIInterchangeBodyTextSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery dBOnlyQuery = new ZDBOnlyQuery(typeof(EDIInterchange));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIInterchangeSchema.PK);
				subQuery.AddToFilter(filter);
				dBOnlyQuery.IgnoreBlobFieldsCheck = true;
				dBOnlyQuery.AddSubQuery(subQuery, JoinCondition.Or);
				return dBOnlyQuery;
			}
		}
		#endregion

	}
}
