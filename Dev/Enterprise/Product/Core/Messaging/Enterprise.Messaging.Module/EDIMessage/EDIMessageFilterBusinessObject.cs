using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.Module
{
	public class EDIMessageFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "it's an identifier")]
		public static class Constants
		{
			public const string ApplicationCode = "Application Code";
			public const string MessageNum = "Message Number";
			public const string Direction = "Direction";
			public const string TransportType = "TransportType";
			public const string MessageType = "Message Type";
			public const string MessageSubType = "Message Sub Type";
			public const string Status = "Status";
			public const string DateTimeCreated = "Date Time Created";
			public const string UserCreated = "User Created";
			public const string MessageText = "Message Text";
			public const string MessageTime = "Message Time";
			public const string HeldUntilDate = "Held Until Date";
			public const string ExternalReferenceNumber = "External Reference Number";

			public const string InterchangeNumber = "Interchange Number";
			public const string InterchangeStatus = "Interchange Status";
			public const string InterchangeDateTimeSent = "Interchange Date Time Sent";
			public const string Sender = "Sender";
			public const string Receiver = "Receiver";
			public const string EHubID = "eHub ID";

			public const string AllExceptAcknowledgments = "AEA";
			public const string Queued = "QUE";
		}

		#region Module Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection result = new ModuleFilterCollection();

			if (ShouldAddApplicationCodeFilter)
			{
				var filterPart = result.AddTextFilter(Constants.ApplicationCode, EDIMessageSchema.EM_ApplicationCode, new ApplicationCodeList());
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|ApplicationCode", "Application Code");
				filterPart.MaxLength = EDIInterchangeSchema.EI_ApplicationCode.MaxLength;
			}

			if (ShouldAddApplicationReferenceFilter)
			{
				result.AddTextFilter(GetApplicationReferenceFilterName(), EDIMessageSchema.EM_ApplicationReference).MultilingualDescription = GetApplicationReferenceFilterCaption();
			}

			if (ShouldAddMessageNumFilter)
			{
				result.AddNumberFilter(Constants.MessageNum, EDIMessageSchema.EM_MessageNum).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|MessageNum", "Message Number");
			}

			if (ShouldAddInterchageDetailFilters)
			{
				ModuleFilter interchangeNumFilter = result.AddNumberFilter(Constants.InterchangeNumber, EDIInterchangeSchema.EI_InterchangeNum);
				interchangeNumFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|InterchangeNumber", "Interchange Number");
				interchangeNumFilter.SubGroup = MessageSubGroup;
				result.AddDateFilter(Constants.InterchangeDateTimeSent, GetInterchangeDateTimeSentQuery).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|InterchangeDateTimeSent", "Interchange Date Time Sent");
			}

			if (ShouldAddDirectionFilter)
			{
				var filterPart = result.AddTextFilter(Constants.Direction, EDIMessageSchema.EM_ReceiveTransmit, EM_ReceiveTransmit_List);
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|Direction", "Direction");
				filterPart.Category = FilterCategories.ModesAndTypes;
			}

			if (ShouldAddTransportTypeFilter)
			{
				var filterPart = result.AddTextFilter(Constants.TransportType, EDIMessageSchema.EM_TransportType, new EDIInterchangeTransportTypeList());
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|TransportType", "Transport Type");
				filterPart.Category = FilterCategories.ModesAndTypes;
			}

			if (ShouldAddMessageTypeSubTypeFilter)
			{
				var filterPart = result.AddTextFilter(Constants.MessageType, EDIMessageSchema.EM_MessageType);
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|MessageType", "Message Type");
				filterPart.Category = FilterCategories.ModesAndTypes;
				filterPart = result.AddTextFilter(Constants.MessageSubType, GetMessageSubTypeTextQuery);
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|MessageSubType", "Message Sub Type");
				filterPart.MaxLength = EDIMessageSchema.EM_MessageSubType.MaxLength;
				filterPart.Category = FilterCategories.ModesAndTypes;
			}

			if (ShouldAddStatusFilter)
			{
				var filterPart = result.AddTextFilter(Constants.Status, GetStatusQuery, EM_Status_List);
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|Status", "Status");
				filterPart.Category = FilterCategories.StatusAndFlags;
			}

			if (ShouldAddMessageTimeFilter)
			{
				result.AddDateFilter(Constants.MessageTime, EDIMessageSchema.EM_SystemCreateTimeUtc, true).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|MessageTime", "Message Time");
			}

			if (ShouldAddEHubIDFilters)
			{
				var eHubIdFilter = new EDIInterchangeEHubIdFilter(Constants.EHubID);
				result.AddFilter(eHubIdFilter);
				eHubIdFilter.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|eHubTrackingID", "eHub ID");
				eHubIdFilter.SubGroup = MessageSubGroup;
				UpdateComparisionOperator(eHubIdFilter);
			}

			if (ShouldAddSenderFilters)
			{
				var filterPart = result.AddTextFilter(Constants.Sender, GetInterchangeSenderQuery);
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|Sender", "Sender");
				filterPart.MaxLength = EDIInterchangeSchema.EI_From.MaxLength;
			}

			if (ShouldAddReceiverFilters)
			{
				var filterPart = result.AddTextFilter(Constants.Receiver, GetInterchangeReceiverQuery);
				filterPart.MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|Receiver", "Receiver");
				filterPart.MaxLength = EDIInterchangeSchema.EI_To.MaxLength;
			}

			result.AddFilter(new EDIMessageTextFilter("Message Text", this)
			{
				MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|MessageText", "Message Text"),
				SubGroup = MessageTextSubGroup
			});

			result.AddDateFilter(Constants.HeldUntilDate, EDIMessageSchema.EM_HeldUntilDate, true).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|HeldUntilDate", "Held Until Date");

			result.AddNumberFilter(Constants.ExternalReferenceNumber, EDIMessageSchema.EM_ExternalReferenceNumber).MultilingualDescription = ResString.GetMultilingualString("Messaging|EDIMessageFilter|ExternalReferenceNumber", "External Reference Number");

			return result;
		}

		protected virtual ZQuery GetMessageSubTypeTextQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(EDIMessageSchema.EM_MessageSubType, comparisonOperator, value);
			return query;
		}

		internal void UpdateComparisionOperator(ModuleTextFilter eHubFilter)
		{
			eHubFilter.ComparisonOperator_List.Clear();
			eHubFilter.ComparisonOperator_List.AddPair(Res.GetString("Messaging|EDIMessageFilter|eHubIDFilter|Option|Exact", "exact"), Res.GetString("Messaging|EDIMessageFilter|eHubIDFilter|ComparisonConstraintDescription|Exact", "Search for an Exact match"));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
		protected virtual string GetApplicationReferenceFilterName()
		{
			return "Application Reference";
		}

		protected virtual MultilingualString GetApplicationReferenceFilterCaption()
		{
			return ResString.GetMultilingualString("Messaging|EDIMessageFilter|ApplicationReference", "Application Reference");
		}

		#endregion

		#region ZBool Properties 'Should Add an Individual Filter Strip ?'

		protected virtual ZBool ShouldAddApplicationCodeFilter
		{
			get { return true; }
		}

		protected virtual bool ShouldAddApplicationReferenceFilter
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddDirectionFilter
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddTransportTypeFilter
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddMessageTypeSubTypeFilter
		{
			get { return true; }
		}

		protected virtual bool ShouldAddStatusFilter
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddInterchageDetailFilters
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddMessageNumFilter
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddMessageTimeFilter
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddEHubIDFilters
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddSenderFilters
		{
			get { return true; }
		}

		protected virtual ZBool ShouldAddReceiverFilters
		{
			get { return true; }
		}

		#endregion

		#region Query

		ZQuery GetInterchangeDateTimeSentQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(EDIMessage));

			ZDBOnlySubQuery interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIMessageSchema.EM_EI);
			AddDateTimeRange(interchangeQuery, comparisonOperator, JoinCondition.And, EDIInterchangeSchema.EI_SystemCreateTimeUtc, fromDate, toDate, true);
			result.AddSubQuery(interchangeQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetInterchangeSenderQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetInterchangeValueQuery(comparisonOperator, EDIInterchangeSchema.EI_From, value);
		}

		ZQuery GetInterchangeReceiverQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetInterchangeValueQuery(comparisonOperator, EDIInterchangeSchema.EI_To, value);
		}

		ZQuery GetInterchangeValueQuery(SQLComparisonOperator comparisonOperator, SchemaStringColumn columnName, ZString value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(EDIMessage));

			if (comparisonOperator.IsNegativeSQLOperator() || comparisonOperator == SpecialComparisonOperator.IsBlank && value.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_EI, SQLComparisonOperator.Equal, null);
			}

			ZDBOnlySubQuery interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIMessageSchema.EM_EI);
			interchangeQuery.AddToFilter(JoinCondition.And, columnName, comparisonOperator, value);
			result.AddSubQuery(interchangeQuery, JoinCondition.Or);

			return result;
		}

		ZQuery GetStatusQuery(ZString value)
		{
			ZQuery result = new ZQuery();
			if (value == Constants.AllExceptAcknowledgments)
			{
				result.AddToFilter(EDIMessageSchema.EM_Status, SQLComparisonOperator.NotEqual, EDIMessage.Status.Acknowledged);
			}
			else
			{
				result.AddToFilter(EDIMessageSchema.EM_Status, value);
			}
			return result;
		}

		#endregion

		#region Code Lists

		public CodeDescriptionPairList EM_Status_List
		{
			get
			{
				if (fEM_Status_List == null)
				{
					fEM_Status_List = new CodeDescriptionPairList();
					fEM_Status_List.AddPair(Constants.AllExceptAcknowledgments, Res.GetString("Messaging|EDIMessageFilter|Status|AllExceptAcknowledgments", "All Except Acknowledgments"));
					var eDIMessageStatus = new EDIMessageStatusList();
					var statusPairs = from statusPair in typeof(EDIMessageStatusList.Codes).GetFields()
									  select new { Code = (string)statusPair.GetValue(statusPair), Description = eDIMessageStatus.GetDescriptionFromCode((string)statusPair.GetValue(statusPair)), Identifier = statusPair.Name }
										 ;
					foreach (var status in statusPairs)
					{
						fEM_Status_List.AddPair(status.Code, status.Description);
					}
				}
				return fEM_Status_List;
			}
		}
		CodeDescriptionPairList fEM_Status_List;

		public CodeDescriptionPairList EM_ReceiveTransmit_List => Factory.GetCachedValue<ReceiveTransmitList>();

		#endregion

		public override ZQuery Filter
		{
			get
			{
				ZQuery baseFilter = base.Filter;
				if (AreSystemMessagesHidden)
				{
					baseFilter.AddToFilter(JoinCondition.And, EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.NotEqual, ApplicationCodeList.Codes.SYS);
				}
				return baseFilter;
			}
		}

		protected virtual bool AreSystemMessagesHidden
		{
			get { return !Env.CurrentUser.LoggedInWithMasterPassword; }
		}

		#region SubGroups
		ModuleFilterSubGroup messageSubGroup;
		ModuleFilterSubGroup messageTextSubGroup;

		public ModuleFilterSubGroup MessageSubGroup
		{
			get { return messageSubGroup ?? (messageSubGroup = new EDIMessageSubGroup()); }
		}

		public ModuleFilterSubGroup MessageTextSubGroup
		{
			get { return messageTextSubGroup ?? (messageTextSubGroup = new EDIMessageTextSubGroup()); }
		}

		class EDIMessageSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(EDIMessage));
				ZDBOnlySubQuery interchangeQuery = new ZDBOnlySubQuery(typeof(EDIInterchange), EDIMessageSchema.EM_EI);
				interchangeQuery.AddToFilter(filter);
				result.AddSubQuery(interchangeQuery, JoinCondition.And);

				return result;
			}
		}

		class EDIMessageTextSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(EDIMessage));
				ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.PK);
				subQuery.AddToFilter(filter);
				result.IgnoreBlobFieldsCheck = true;
				result.AddSubQuery(subQuery, JoinCondition.Or);
				return result;
			}
		}

		#endregion
	}
}
