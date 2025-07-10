using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Freight.Business;
using Enterprise.Integration.ZArchitecture;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using IFilterControl = Enterprise.Integration.ZArchitecture.IFilterControl;

namespace Enterprise.Customs.CA.Module
{
	partial class CACFSShipmentModuleColumnsAndFiltersProvider
	{
		[SuppressStaticMethodsAreLocatedOnCorrectClassMessage]
		class ArrivalCertificationColumnsAndFiltersProvider : Integration.Customs.CA.IExternalModuleColumnsAndFiltersProvider
		{
			#region Implementation of IExternalModuleColumnsAndFiltersProvider

			#region AddFilters

			public void AddFilters(IModuleFilterCollection filters, BusinessObjectFactory factory)
			{
				var collection = (ModuleFilterCollection)filters;
				collection.AddFilter(new EqualModuleStatusFilter(
															ArrivalColumnsHelper.Captions.ArrivalCertificationStatusId,
															ArrivalColumnsHelper.Captions.ArrivalCertificationStatusMultilingualDescription,
										GetArrivalCertificationStatusQuery,
										GetArrivalCertificationStatusList));

				var releaseDateFilter = collection.AddDateFilter(ArrivalColumnsHelper.Captions.ArrivalCertificationDateId, GetArrivalCertificationDateQuery);
				releaseDateFilter.MultilingualDescription = ArrivalColumnsHelper.Captions.ArrivalCertificationDateMultilingualDescription;
			}

			#region Arrival Certification Status

			static ZQuery GetArrivalCertificationStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
			{
				var result = new ZDBOnlyQuery(typeof(CommonShipment));
				if (value == CAExternalColumnsHelper.Constants.NotSentCode)
				{
					var notIn = comparisonOperator == SQLComparisonOperator.Equal;
					var subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID, notIn);
					subQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
					subQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
					subQuery.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
					subQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					subQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, RNSMessageTypes.Codes.ArrivalCertification);
					result.AddSubQuery(subQuery, JoinCondition.And);
				}
				else
				{
					var notIn = comparisonOperator == SQLComparisonOperator.NotEqual;
					var subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID, notIn);
					subQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
					subQuery.AddToFilter(EDIMessageSchema.EM_Status, value);

					var acmSubQuery = new ZDBOnlyQuery(typeof(EDIMessage));
					acmSubQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
					acmSubQuery.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
					acmSubQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
					acmSubQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, RNSMessageTypes.Codes.ArrivalCertification);

					ZString filter = string.Format(@"{0} IN (SELECT {0} FROM (SELECT {0}, ROW_NUMBER() OVER (PARTITION BY {1} ORDER BY {2} DESC) RowNumber from {3} {4}) ACM WHERE RowNumber=1)",
						EDIMessageSchema.PK.Name, EDIMessageSchema.EM_LinkUniqueID.Name, EDIMessageSchema.EM_SystemCreateTimeUtc.Name, EDIMessageSchema.PK.TableName,
						acmSubQuery.GetAsWhereClause(true));

					subQuery.AddFilterAndZSQLParameterCollection(filter, new ZSqlParameterCollection());
					result.AddSubQuery(subQuery, JoinCondition.And);
				}
				return result;
			}

			static IList GetArrivalCertificationStatusList()
			{
				var result = new EDIMessageStatusList();
				result.AddPair(CAExternalColumnsHelper.Constants.NotSentCode, CAExternalColumnsHelper.Constants.NotSentDescription);
				return result;
			}

			#endregion

			#region Arrival Certification Date

			static ZQuery GetArrivalCertificationDateQuery(DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
			{
				var result = new ZDBOnlyQuery(typeof(CommonShipment));

				var notIn = comparisonOperator == DateComparisonOperator.HasNoDateEntered;
				var subQuery = new ZDBOnlySubQuery(typeof(EDIMessage), EDIMessageSchema.EM_LinkUniqueID, notIn);
				subQuery.AddToFilter(EDIMessageSchema.EM_LinkTable, JobShipmentSchema.Constants.TableName);
				subQuery.AddToFilter(EDIMessageSchema.EM_Status, MessageStatusList.Codes.Sent);

				if (comparisonOperator == DateComparisonOperator.HasDateInRange)
				{
					if (value1.IsValid)
					{
						subQuery.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, value1);
					}
					if (value2.IsValid)
					{
						subQuery.AddToFilter(EDIMessageSchema.EM_SystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, value2);
					}
				}

				var acmSubQuery = new ZDBOnlyQuery(typeof(EDIMessage));
				acmSubQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, EDIMessage.ApplicationCodes.CAIMP);
				acmSubQuery.AddToFilter(EDIMessageSchema.EM_MessageType, MessageTypeList.Codes.RNSRequest);
				acmSubQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				acmSubQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, RNSMessageTypes.Codes.ArrivalCertification);

				ZString filter = string.Format(@"{0} IN (SELECT {0} FROM (SELECT {0}, ROW_NUMBER() OVER (PARTITION BY {1} ORDER BY {2} DESC) RowNumber from {3} {4}) ACM WHERE RowNumber=1)",
					EDIMessageSchema.PK.Name, EDIMessageSchema.EM_LinkUniqueID.Name, EDIMessageSchema.EM_SystemCreateTimeUtc.Name, EDIMessageSchema.PK.TableName,
					acmSubQuery.GetAsWhereClause(true));

				subQuery.AddFilterAndZSQLParameterCollection(filter, new ZSqlParameterCollection());
				result.AddSubQuery(subQuery, JoinCondition.And);

				return result;
			}

			#endregion

			#endregion

			#region AddColumns
			readonly ArrivalColumnsHelper helper = new ArrivalColumnsHelper();

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
