using System.Collections;
using System.Linq;
using System.Text;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	partial class CAShipmentModuleColumnsAndFiltersProvider
	{
		[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
		protected class RNSColumnsAndFiltersProvider : Integration.Customs.CA.IShipmentModuleColumnsAndFiltersProvider
		{
			#region Implementation of IShipmentModuleColumnsAndFiltersProvider

			#region AddFilters

			public void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory)
			{
				var collection = (ModuleFilterCollection)filters;
				collection.AddFilter(new EqualModuleStatusFilter(
															RNSColumnsHelper.Captions.RNSReleaseStatusFilterId,
															RNSColumnsHelper.Captions.RNSReleaseStatusMultilingualDescription,
															GetRNSStatusQuery,
															GetRNSStatusList));

				var releaseDateFilter = collection.AddDateFilter(RNSColumnsHelper.Captions.RNSReleaseDateFilterId, GetRNSReleaseDateQuery);
				releaseDateFilter.MultilingualDescription = RNSColumnsHelper.Captions.RNSReleaseDateMultilingualDescription;
			}

			#region RNS Release Status

			static ZQuery GetRNSStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var result = new ZDBOnlyQuery(typeof(CommonShipment));
				if (value == CAExternalColumnsHelper.Constants.NotSentCode)
				{
					var messageTypes = new[] { MessageTypeList.Codes.RNSRequest, MessageTypeList.Codes.EDIRelease };
					var notIn = comparisonOperator == SQLComparisonOperator.Equal;
					var subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID, notIn);
					subQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
					subQuery.AddToFilter(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, messageTypes);
					subQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, SQLComparisonOperator.Equal, EDIMessage.ApplicationCodes.CAIMP);
					result.AddSubQuery(subQuery, JoinCondition.And);
				}
				else
				{
					#region Filter

					var inOperator = comparisonOperator == SQLComparisonOperator.NotEqual ? " NOT IN " : " IN ";
					string excludeSubTypesFilter = string.Empty;
					if (!RNSMessagingBO.ReleaseSubTypesToIgnore.Contains(value) && value != CAExternalColumnsHelper.Constants.AwaitingResponseCode)
					{
						var stringBuilder = new StringBuilder();
						foreach (string subType in RNSMessagingBO.ReleaseSubTypesToIgnore)
						{
							stringBuilder.AppendFormat("'{0}',", subType);
						}
						excludeSubTypesFilter = @"
			AND " + EDIMessageSchema.EM_MessageSubType.Name + " NOT IN (" + stringBuilder.ToString().TrimEnd(',') + ")";
					}

					var filter =
JobShipmentSchema.PK.Name + inOperator + @"
(
	SELECT " + EDIMessageSchema.EM_LinkUniqueID.Name + @"
	FROM 
	(
		SELECT 
			*,
			ROW_NUMBER() OVER(PARTITION BY " + EDIMessageSchema.EM_LinkUniqueID.Name +
						" ORDER BY " + EDIMessageSchema.EM_ReceiveTransmit.Name + " ASC, " + EDIMessageSchema.EM_SystemCreateTimeUtc.Name + @" DESC) AS RowNumber
		FROM " + EDIMessageSchema.Constants.SqlSchemaName + "." + EDIMessageSchema.Constants.TableName + @" 
		WHERE " + EDIMessageSchema.EM_MessageType.Name + " IN ('" + MessageTypeList.Codes.EDIRelease + "', '" + MessageTypeList.Codes.RNSRequest + "')" + excludeSubTypesFilter + @"
		AND " + EDIMessageSchema.EM_ApplicationCode.Name + " = '" + EDIMessage.ApplicationCodes.CAIMP + @"'
		AND " + EDIMessageSchema.EM_LinkUniqueID.Name + @" IS NOT NULL
		AND " + EDIMessageSchema.EM_LinkTable.Name + " = '" + JobShipmentSchema.Constants.TableName + @"'
	) INNERSELECT
	WHERE RowNumber = 1 AND {0}
)";

					#endregion

					if (value == CAExternalColumnsHelper.Constants.AwaitingResponseCode)
					{
						var subFilter = EDIMessageSchema.EM_ReceiveTransmit.Name + " = '" + EDIMessage.Direction.Transmit + "'";
						result.AddFilterAndZSQLParameterCollection(string.Format(filter, subFilter), null);
					}
					else
					{
						var subFilter = EDIMessageSchema.EM_ReceiveTransmit.Name + " = '" + EDIMessage.Direction.Receive + "' AND " +
														EDIMessageSchema.EM_MessageSubType.Name + " = @SubType";
						var parameters = new ZSqlParameterCollection { { "@SubType", value, EDIMessageSchema.EM_MessageSubType } };
						result.AddFilterAndZSQLParameterCollection(string.Format(filter, subFilter), parameters);
					}
				}
				return result;
			}

			static IList GetRNSStatusList()
			{
				var result = new EDIReleaseImportEntryStatusList();
				result.RemoveCode(EDIReleaseImportEntryStatusList.Codes.MultipleCargoControlNumber);
				result.AddPair(CAExternalColumnsHelper.Constants.AwaitingResponseCode, CAExternalColumnsHelper.Constants.AwaitingResponseDescription);
				result.AddPair(CAExternalColumnsHelper.Constants.NotSentCode, CAExternalColumnsHelper.Constants.NotSentDescription);
				return result;
			}

			#endregion

			#region RNS Release Date

			static ZQuery GetRNSReleaseDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
			{
				#region Filter

				var inOperator = comparisonOperator == DateComparisonOperator.HasNoDateEntered ? " NOT IN " : " IN ";
				var dateRangeFilter = comparisonOperator == DateComparisonOperator.HasDateInRange && value1.IsValid ? " AND ReleaseDate > @Date1" : string.Empty;
				dateRangeFilter += comparisonOperator == DateComparisonOperator.HasDateInRange && value2.IsValid ? " AND ReleaseDate <= @Date2" : string.Empty;
				var filter =
JobShipmentSchema.PK.Name + inOperator + @"
(
	SELECT " + EDIMessageSchema.EM_LinkUniqueID.Name + @"
	FROM 
	(
		SELECT
			" + EDIMessageSchema.EM_LinkUniqueID.Name + @",
			CAST(" + GenAddOnColumnSchema.XA_Data.Name + @" AS datetime) AS ReleaseDate,
			ROW_NUMBER() OVER(PARTITION BY " + EDIMessageSchema.EM_LinkUniqueID.Name +
					" ORDER BY " + EDIMessageSchema.EM_SystemCreateTimeUtc.Name + @" DESC) AS RowNumber
		FROM " + EDIMessageSchema.Constants.SqlSchemaName + "." + EDIMessageSchema.Constants.TableName + @" 
		INNER JOIN " + GenAddOnColumnSchema.Constants.SqlSchemaName + "." + GenAddOnColumnSchema.Constants.TableName + @" 
		ON " + GenAddOnColumnSchema.XA_ParentID.Name + " = " + EDIMessageSchema.PK.Name + @"
		AND " + GenAddOnColumnSchema.XA_ParentTableCode.Name + " = '" + EDIMessageSchema.Constants.Prefix + @"'
		AND " + GenAddOnColumnSchema.XA_Name.Name + " = '" + EDIReleaseMessage.Schema.RNSReleaseDate + @"'
		AND " + EDIMessageSchema.EM_MessageType.Name + " IN ('" + MessageTypeList.Codes.EDIRelease + "', '" + MessageTypeList.Codes.RNSRequest + @"')
		AND " + EDIMessageSchema.EM_ApplicationCode.Name + " = '" + EDIMessage.ApplicationCodes.CAIMP + @"'
		AND " + EDIMessageSchema.EM_ReceiveTransmit.Name + " = '" + EDIMessage.Direction.Receive + @"'
		AND " + EDIMessageSchema.EM_LinkUniqueID.Name + @" IS NOT NULL
		AND " + EDIMessageSchema.EM_LinkTable.Name + " = '" + JobShipmentSchema.Constants.TableName + @"'
	) INNERSELECT
	WHERE RowNumber = 1 " + dateRangeFilter + @"
	
)";

				#endregion

				var result = new ZDBOnlyQuery(typeof(CommonShipment));
				var parameters = comparisonOperator != DateComparisonOperator.HasDateInRange ? null
									: new ZSqlParameterCollection
									{
										{ "@Date1", value1, EDIMessageSchema.EM_SystemCreateTimeUtc },
										{ "@Date2", value2, EDIMessageSchema.EM_SystemCreateTimeUtc }
									};
				result.AddFilterAndZSQLParameterCollection(filter, parameters);
				return result;
			}

			#endregion

			#endregion

			#region AddColumns
			readonly RNSColumnsHelper helper = new RNSColumnsHelper();

			public void AddColumns(IFilterControl filterControl)
			{
				var control = (ZFilterStripControl)filterControl;
				helper.AddColumns(control.FilteredGrid, control.GridCollection);
			}

			#endregion

			#endregion
		}
	}
}
