using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	class K84MessageFilterBusinessObject : EDIMessageFilterBusinessObject
	{
		#region Constants

		internal new static class Constants
		{
			internal static MultilingualString K84AccountingDateMultilingualDescription
			{
				get { return ResString.GetMultilingualString("6B066F8C-24D4-4085-B767-E91ABB8F26D6", K84AccountingDateID); }
			}
			internal const string K84AccountingDateID = "Accounting Date";

			internal static MultilingualString K84StatementDateMultilingualDescription
			{
				get { return ResString.GetMultilingualString("17F82F82-47D3-4270-9303-995F10A0C0AB", K84StatementDateID); }
			}
			internal const string K84StatementDateID = "Statement Date";
		}

		#endregion

		protected override ZQuery GetMessageSubTypeTextQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = base.GetMessageSubTypeTextQuery(comparisonOperator, value);
			var messageQuery = new ZDBOnlyQuery(typeof(EDIMessage));
			var isNegativeOperator = GenAddOnColumnHelper.OperatorsDictionary.ContainsKey(comparisonOperator);
			var messageSubQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.PK, isNegativeOperator);
			if (isNegativeOperator)
			{
				comparisonOperator = GenAddOnColumnHelper.OperatorsDictionary[comparisonOperator];
			}

			var genAddOnQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, EDIMessageSchema.Constants.Prefix);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, EDIMessage.Schema.XMLCustomsMessageType);
			genAddOnQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, comparisonOperator, value);
			messageSubQuery.AddSubQuery(genAddOnQuery, JoinCondition.And);
			messageSubQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch);
			messageQuery.AddSubQuery(messageSubQuery, JoinCondition.And);
			return query.AddToFilter(messageQuery, isNegativeOperator ? JoinCondition.And : JoinCondition.Or);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			result.AddDateFilter(Constants.K84AccountingDateID, GetAccountingDateQuery, isNullable: false).MultilingualDescription = Constants.K84AccountingDateMultilingualDescription;
			result.AddDateFilter(Constants.K84StatementDateID, GetStatementDateQuery, isNullable: false).MultilingualDescription = Constants.K84StatementDateMultilingualDescription;
			return result;
		}

		ZQuery GetAccountingDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetNonNullableDateQuery(fromDate, toDate, EDIMessage.Schema.K84AccountingDate);
		}

		ZQuery GetStatementDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			return GetNonNullableDateQuery(fromDate, toDate, EDIMessage.Schema.K84StatementDate);
		}

		ZQuery GetNonNullableDateQuery(ZDateTime fromDate, ZDateTime toDate, string addOnColumnSchemaName)
		{
			if (fromDate.IsEmpty)
			{
				fromDate = ZDateTime.MinSmallDateTimeValue;
			}

			if (toDate.IsEmpty)
			{
				toDate = ZDateTime.MaxSmallDateTimeValue;
			}

			var result = new ZDBOnlyQuery(typeof(EDIMessage));
			var queryText = string.Format(CultureInfo.CurrentCulture, @"{0} IN (SELECT XA_ParentID FROM dbo.GenAddOnColumn WHERE XA_ParentTableCode = 'EM' AND
											XA_Name = '{1}' AND CONVERT(DATETIME, XA_Data) >= '{2}' AND CONVERT(DATETIME, XA_Data) <= '{3}')",
						EDIMessageSchema.Constants.PK, addOnColumnSchemaName,
						fromDate.ToString("yyyyMMdd", CultureInfo.CurrentCulture),
						toDate.ToString("yyyyMMdd", CultureInfo.CurrentCulture));

			result.AddFilterAndZSQLParameterCollection(queryText, null);
			return result;
		}

		protected override ZBool ShouldAddEHubIDFilters
		{
			get { return false; }
		}

		protected GenAddOnColumnQueryHelper GenAddOnColumnHelper
		{
			get { return helper ?? (helper = new GenAddOnColumnHelper().SimpleQueryHelperForEDIMessage); }
		}
		GenAddOnColumnQueryHelper helper;
	}
}
