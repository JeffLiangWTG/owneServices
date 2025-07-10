using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public static class EDIMessageQueryHelper
	{
		public static ZQuery GetSegmentQuery(SQLComparisonOperator comparisonOperator, ZString value, IEnumerable<string> segmentPatterns, int valueExactLength = 0)
		{
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.IncludeBlob(EDIMessageSchema.EM_MessageText);
			foreach (var filter in from pattern in segmentPatterns select GetSegmentFilter(comparisonOperator, value, pattern.TrimEnd('\'') + "''", valueExactLength))
			{
				var joinCondition = query.IsEmpty || IsNegativeComparisonOperator(comparisonOperator) ? JoinCondition.And : JoinCondition.Or;
				query.AddFilterAndZSQLParameterCollection(filter, null, joinCondition);
			}

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				var noSegmentQuery = new ZDBOnlyQuery(typeof(EDIMessage));
				noSegmentQuery.IncludeBlob(EDIMessageSchema.EM_MessageText);

				foreach (var filter in from pattern in segmentPatterns select GetSegmentFilter(SQLComparisonOperator.NotEqual, "%", pattern.TrimEnd('\'') + "''", valueExactLength))
				{
					noSegmentQuery.AddFilterAndZSQLParameterCollection(filter, null, JoinCondition.And);
				}
				query.AddToFilter(noSegmentQuery, JoinCondition.Or);
			}
			return query;
		}

		public static ZQuery GetSegmentQueryWithSchema(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var joinCondition = IsNegativeComparisonOperator(comparisonOperator) ? JoinCondition.And : JoinCondition.Or;
			var gen = SimpleQueryHelper.GetQueryHandlingBlanks(EDIReleaseMessage.Schema.TransactionNumber, comparisonOperator, value);
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			query.AddToFilter(EDIMessageSchema.EM_ApplicationReference, comparisonOperator, value);
			query.AddToFilter(gen, joinCondition);
			return query;
		}

		public static GenAddOnColumnQueryHelper SimpleQueryHelper
		{
			get { return simpleQueryHelper ?? (simpleQueryHelper = new GenAddOnColumnQueryHelper(typeof(EDIMessage))); }
		}

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static GenAddOnColumnQueryHelper simpleQueryHelper;

		static string GetSegmentFilter(SQLComparisonOperator comparisonOperator, ZString value, string segmentPattern, int valueExactLength)
		{
			var filterBuilder = new ZStringBuilder(EDIMessageSchema.EM_MessageText.Name);

			if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				filterBuilder.Append("LIKE");
				filterBuilder.Append(string.Format("'%{0}%'", string.Format(segmentPattern, value.IsEmpty ? "_%" : value.ToString())));
				filterBuilder.Append("AND");
				filterBuilder.Append(EDIMessageSchema.EM_MessageText.Name);
				filterBuilder.Append("NOT LIKE");
				filterBuilder.Append(string.Format("'%{0}%'", string.Format(segmentPattern, "")));
			}
			else
			{
				if (IsNegativeComparisonOperator(comparisonOperator))
				{
					filterBuilder.Append("NOT");
				}

				filterBuilder.Append("LIKE");

				if (comparisonOperator == SpecialComparisonOperator.IsBlank)
				{
					value = ZString.Empty;
				}
				else
				{
					value = new ZString(comparisonOperator.EscapedSqlValue(value));
					if (valueExactLength > 0 && IsStartsWithOrDoesNotStartWithComparisonOperator(comparisonOperator))
					{
						value = value.TrimEnd('%').PadRight(valueExactLength, '_');
					}
				}

				filterBuilder.Append(string.Format("'%{0}%'", string.Format(segmentPattern, value)));
			}

			return filterBuilder.ToStringWithDelimiterBetweenAppends(" ");
		}

		static bool IsStartsWithOrDoesNotStartWithComparisonOperator(SQLComparisonOperator comparisonOperator)
		{
			return new[]
					{
						SQLComparisonOperator.StartsWith,
						SQLComparisonOperator.DoesNotStartWith
					}.Contains(comparisonOperator);
		}

		static bool IsNegativeComparisonOperator(SQLComparisonOperator comparisonOperator)
		{
			return new[]
					{
						SQLComparisonOperator.DoesNotStartWith,
						SQLComparisonOperator.NotEqual,
						SQLComparisonOperator.NotContains
					}.Contains(comparisonOperator);
		}

		public static ZDBOnlySubQuery GetGenAddOnTextQuery(SQLComparisonOperator comparisonOperator, ZString value, string columnName)
		{
			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, columnName);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			return genAddOnQuery;
		}

		public static ZQuery GetEDIMessageGenPivotQuery(IEnumerable<ZGuid> parentPKs, IEnumerable<ZString> subTypes)
		{
			var result = new ZDBOnlyQuery(typeof(EDIMessage));
			result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.UniversalDataMessaging);
			result.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			result.AddToFilter(EDIMessageSchema.EM_MessageType, EDIMessageTypeList.Codes.XDC);
			result.AddToFilter(EDIMessageSchema.EM_MessageSubType, subTypes);
			var logFilter = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.PK);
			logFilter.AddToFilter(StmALogSchema.SL_Parent, parentPKs);
			var genPivotFilter = new ZDBOnlySubQuery(typeof(GenPivot), GenPivotSchema.XX_Relation2ID);
			genPivotFilter.AddToFilter(GenPivotSchema.XX_RelationType, Constants.GenPivotTypes.XmlEdiMessage);
			genPivotFilter.AddSubQuery(GenPivotSchema.XX_Relation1ID, logFilter, JoinCondition.And);
			result.AddSubQuery(genPivotFilter, JoinCondition.And);
			return result;
		}
	}
}
