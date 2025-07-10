using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business.BISI
{
	public partial class ShipmentStatusDataSelector
	{
		public enum DeliveryArea { Metro, Other }

		public IReadOnlyList<IShipmentStatusData> StatusesForExport
		{
			get
			{
				IShipmentStatusData[] result = new IShipmentStatusData[StatusForExportList.Count];
				StatusForExportList.Values.CopyTo(result, 0);
				return result;
			}
		}

		public int GetStatusForExportListCount()
		{
			return StatusForExportList.Count;
		}

		public int AddEveryDayStatuses(ZDateTime startDate, ZDateTime endDate)
		{
			return AddEveryDayStatusesCore(startDate, endDate);
		}

		public int AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrival(ZDateTime startDate, ZDateTime endDate)
		{
			if (!UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime)
			{
				return AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrivalCore(startDate, endDate);
			}
			return 0;
		}

		public int AddWorkingDayStatuses(ZDateTime startDate, ZDateTime endDate, DeliveryArea deliveryArea)
		{
			if (!UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime)
			{
				return AddWorkingDayStatusesCore(startDate, endDate, deliveryArea);
			}
			return 0;
		}

		public int AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrival(ZDateTime startDate, ZDateTime endDate, DeliveryArea deliveryArea)
		{
			if (!UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime)
			{
				return AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrivalCore(startDate, endDate, deliveryArea);
			}
			return 0;
		}

		#region Add Everyday Statuses

		int AddEveryDayStatusesCore(ZDateTime startDate, ZDateTime endDate)
		{
			IShipmentStatusData[] statusDataArray = FilteredShipmentStatusData(startDate, endDate);
			if (statusDataArray != null)
			{
				foreach (IShipmentStatusData statusData in statusDataArray)
				{
					if (UPEDataRegistry.Instance.ForceAllXPLDsToBeUploadedEveryTime || IsEveryDayStatus(statusData) || (IsAnEverydayDateofArrivalStatus(statusData) && IsValidArrivalDate(statusData, ZDateTime.Now.Date.ToDateTime())))
					{
						TryAddStatusDataToExportList(statusData);
					}
				}
				return statusDataArray.Length;
			}
			return 0;
		}

		int AddPriorEverydayShipmentStatusesToBeExportedOnDateOfArrivalCore(ZDateTime startDate, ZDateTime endDate)
		{
			IShipmentStatusData[] statusDataArray = FilteredShipmentStatusDataPriorToDateOfArrival(UPEDataRegistry.Instance.XPLDForNonWorkingDaysDateOfArrivalPassed, startDate, endDate);
			if (statusDataArray != null)
			{
				foreach (IShipmentStatusData statusData in statusDataArray)
				{
					if (IsAnEverydayDateofArrivalStatus(statusData))
					{
						TryAddStatusDataToExportList(statusData);
					}
				}
				return statusDataArray.Length;
			}
			return 0;
		}

		#endregion

		#region Working Day Statuses

		int AddWorkingDayStatusesCore(ZDateTime startDate, ZDateTime endDate, DeliveryArea deliveryArea)
		{
			IShipmentStatusData[] statusDataArray = FilteredShipmentStatusData(startDate, endDate);
			if (statusDataArray != null)
			{
				foreach (IShipmentStatusData statusData in statusDataArray)
				{
					if (IsAWorkingDayStatus(statusData) || (IsAWorkingDayDateofArrivalStatus(statusData) && IsValidArrivalDate(statusData, statusData.StatusChangeDate)))
					{
						if (IsPostCodeInDeliveryArea(deliveryArea, statusData.ConsigneePostCode))
						{
							TryAddStatusDataToExportList(statusData);
						}
					}
					else if (statusData.ShipmentStatusType != ShipmentStatusType.Unknown && statusData.HasReasonOrResolutionCode)
					{
						ZString shipmentRef = statusData.ShipmentRef;
						if (StatusForExportList.ContainsKey(shipmentRef))
						{
							if (((IShipmentStatusData)StatusForExportList[shipmentRef]).StatusChangeDate < statusData.StatusChangeDate)
							{
								StatusForExportList.Remove(shipmentRef);
							}
						}
					}
				}
				return statusDataArray.Length;
			}
			return 0;
		}

		int AddPriorWorkingdayShipmentStatusesToBeExportedOnDateOfArrivalCore(ZDateTime startDate, ZDateTime endDate, DeliveryArea deliveryArea)
		{
			IShipmentStatusData[] statusDataArray = FilteredShipmentStatusDataPriorToDateOfArrival(UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed, startDate, endDate);
			if (statusDataArray != null)
			{
				foreach (IShipmentStatusData statusData in statusDataArray)
				{
					if (IsAWorkingDayDateofArrivalStatus(statusData))
					{
						if (IsPostCodeInDeliveryArea(deliveryArea, statusData.ConsigneePostCode))
						{
							TryAddStatusDataToExportList(statusData);
						}
					}
				}
				return statusDataArray.Length;
			}
			return 0;
		}

		bool IsValidArrivalDate(IShipmentStatusData statusData, ZDateTime date)
		{
			bool flightNumberExistInRegistry = (Array.IndexOf<string>(UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne, statusData.FlightNo) != -1);
			if (flightNumberExistInRegistry)
			{
				return statusData.ImportDate.Date.AddDays(1) <= date;
			}
			else
			{
				return statusData.ImportDate.Date <= date;
			}
		}

		#region Zones

		bool IsPostCodeInDeliveryArea(DeliveryArea deliveryArea, ZString postCode)
		{
			bool result = false;

			if (deliveryArea == DeliveryArea.Metro && IsMetroPostCode(postCode))
			{
				result = true;
			}
			else if (deliveryArea == DeliveryArea.Other && !IsMetroPostCode(postCode))
			{
				result = true;
			}

			return result;
		}

		public bool IsMetroPostCode(ZString postCode)
		{
			var zoneItem = RateTransportZoneHelper.GetZoneItemForPostCode(UPSTransportProvider, postCode);
			if (zoneItem != null)
			{
				return zoneItem.Zone.TZ_ZoneName.ToLower().Contains(UPERateTransportZone.MetroKeyword.ToLower());
			}

			return false;
		}

		RateTransportProvider UPSTransportProvider
		{
			get
			{
				if (fUPSTransportProvider == null)
				{
					ZQuery filter = new ZQuery(RateTransportProviderSchema.TP_OH_RelatedParty, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
					fUPSTransportProvider = (RateTransportProvider)Factory.LoadTop1(typeof(RateTransportProvider), filter);
				}

				return fUPSTransportProvider;
			}
		}
		RateTransportProvider fUPSTransportProvider;

		#endregion

		#endregion

		#region Is

		bool IsEveryDayStatus(IShipmentStatusData statusData)
		{
			return
				!IsAnEverydayDateofArrivalStatus(statusData)
				&& !IsAWorkingDayStatus(statusData)
				&& !IsAWorkingDayDateofArrivalStatus(statusData);
		}

		bool IsAnEverydayDateofArrivalStatus(IShipmentStatusData statusData)
		{
			return
				UPEDataRegistry.Instance.XPLDForNonWorkingDaysDateOfArrivalPassed.ContainsCode(statusData.ExceptionResolutionCode)
				|| UPEDataRegistry.Instance.XPLDForNonWorkingDaysDateOfArrivalPassed.ContainsCode(statusData.HoldReasonCode);
		}

		bool IsAWorkingDayDateofArrivalStatus(IShipmentStatusData statusData)
		{
			return
				UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed.ContainsCode(statusData.ExceptionResolutionCode)
				|| UPEDataRegistry.Instance.XPLDForWorkingDaysDateOfArrivalPassed.ContainsCode(statusData.HoldReasonCode);
		}

		bool IsAWorkingDayStatus(IShipmentStatusData statusData)
		{
			return
				UPEDataRegistry.Instance.XPLDForWorkingDays.ContainsCode(statusData.ExceptionResolutionCode)
				|| UPEDataRegistry.Instance.XPLDForWorkingDays.ContainsCode(statusData.HoldReasonCode);
		}

		void TryAddStatusDataToExportList(IShipmentStatusData statusData)
		{
			ZString shipmentRef = statusData.ShipmentRef;

			if (!shipmentRef.IsEmpty && statusData.ShipmentStatusType != ShipmentStatusType.Unknown && statusData.HasReasonOrResolutionCode)
			{
				if (!StatusForExportList.ContainsKey(shipmentRef))
				{
					StatusForExportList.Add(shipmentRef, statusData);
				}
				else if (CanOverrideShipmentStatusData((IShipmentStatusData)StatusForExportList[shipmentRef], statusData))
				{
					StatusForExportList[shipmentRef] = statusData;
				}
			}
		}

		#endregion

		#region Date Of Arrival

		IShipmentStatusData[] FilteredShipmentStatusDataPriorToDateOfArrival(ReadOnlyCodeDescriptionPairList reasonResolutionList, ZDateTime startDate, ZDateTime endDate)
		{
			List<DynamicBusinessObject> objectList = new List<DynamicBusinessObject>();

			objectList.AddRange(StmALogListOfPriorStatusesForUpload(reasonResolutionList, startDate, endDate).ToArray());

			if (objectList.Count > 0)
			{
				var minPostedTime = objectList.Min(o => o[StmALogSchema.Constants.SL_PostedTimeUtc]);
				var maxPostedTime = objectList.Max(o => o[StmALogSchema.Constants.SL_PostedTimeUtc]);
				ZQuery shipmentStatusFilter = new ZQuery();
				shipmentStatusFilter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, minPostedTime);
				shipmentStatusFilter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, maxPostedTime);
				shipmentStatusFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.QueueChanged.Code);

				List<object> stmALogPKs = new List<object>();
				foreach (DynamicBusinessObject dynamicBusinessObject in objectList)
				{
					stmALogPKs.Add(dynamicBusinessObject[StmALogSchema.Constants.PK]);
				}
				shipmentStatusFilter.AddToFilter(StmALogSchema.PK, stmALogPKs);

				var query = FilteredShipmentStatusQuery(shipmentStatusFilter);

				return GetShipmentStatusData(query);
			}
			return null;
		}

		DynamicBusinessObjectCollection StmALogListOfPriorStatusesForUpload(ReadOnlyCodeDescriptionPairList reasonResolutionList, ZDateTime startDate, ZDateTime endDate)
		{
			ZSqlParameterCollection @params = new ZSqlParameterCollection();
			ZString flightNumbersSql = GetFlightNumbersSql(@params);

			#region SqlText

			var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
			var jobDecQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
			jobDecQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);
			var cusMawbQuery = new ZDBOnlyQuery(typeof(UPECusMAWB));
			cusMawbQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

			ZString sqlText = ZString.Format(@"
;WITH QueueChangedLogs AS 
(
	SELECT {5}, {6}, {8}, Housebill, ArrivalDate, {9}
	FROM {0}
	INNER JOIN (
		{21}
		SELECT {11}, {14} AS Housebill, {15} AS ArrivalDate
		FROM {1}
		INNER JOIN {2}
			ON {12} = {13}
			AND {15} >= @JE_DateOfArrivalStart
			AND {15} < DATEADD(DAY, 1, @JE_DateOfArrivalEnd)
			{22}
            AND {25}
		UNION ALL 
		SELECT {11}, {17} AS Housebill, {20} AS ArrivalDate
		FROM {1}
		INNER JOIN {3}
			ON {12} = {16}
		INNER JOIN {4}
			ON {18} = {19}
			AND {20} >= @CM_ArrivalDateStart
			AND {20} < DATEADD(DAY, 1, @CM_ArrivalDateEnd)
			{23}
            AND {26}
	) ValidProcessQueue
		ON {10} = {11}
	WHERE {6} >= DATEADD(MONTH, -6, @SL_PostedTimeUtcStart)
	AND {7} = @SL_SE_NKEvent
)
SELECT {5}, {6}
FROM QueueChangedLogs
INNER JOIN (
	SELECT MAX({6}) PostedTime, Housebill
	FROM QueueChangedLogs
	GROUP BY Housebill
) LatestLogs
	ON LatestLogs.Housebill = QueueChangedLogs.Housebill
WHERE LatestLogs.PostedTime = {6}
AND {9} < convert(datetime, floor(convert(float, ArrivalDate)))
{24}
"
					 , StmALogSchema.Constants.TableName                                    //0
					 , ProcessQueueSchema.Constants.TableName                           //1
					 , JobDeclarationSchema.Constants.TableName                     //2
					 , CusHAWBSchema.Constants.TableName                                    //3
					 , CusMAWBSchema.Constants.TableName                                    //4
					 , StmALogSchema.Constants.PK                                                   //5
					 , StmALogSchema.Constants.SL_PostedTimeUtc                     //6
					 , StmALogSchema.Constants.SL_SE_NKEvent                            //7
					 , StmALogSchema.Constants.SL_Reference                             //8
					 , StmALogSchema.Constants.SL_EventTime                             //9
					 , StmALogSchema.Constants.SL_Parent                                    //10
					 , ProcessQueueSchema.Constants.PK                                      //11
					 , ProcessQueueSchema.Constants.P4_ParentID                     //12
					 , JobDeclarationSchema.Constants.PK                                    //13
					 , JobDeclarationSchema.Constants.JE_HouseBill              //14
					 , JobDeclarationSchema.Constants.JE_DateOfArrival      //15
					 , CusHAWBSchema.Constants.PK                                                   //16
					 , CusHAWBSchema.Constants.CS_HAWB                                      //17
					 , CusHAWBSchema.Constants.CS_CM                                            //18
					 , CusMAWBSchema.Constants.PK                                                   //19
					 , CusMAWBSchema.Constants.CM_ArrivalDate                           //20
					 , GetValidProcessQueuWithFlightNumberInRegistrySql(flightNumbersSql)                       //21
					 , GetAndFlightNumbersSql(false, JobDeclarationSchema.Constants.JE_VoyageFlightNo, flightNumbersSql)        //22
					 , GetAndFlightNumbersSql(false, CusMAWBSchema.Constants.CM_FlightNo, flightNumbersSql)                                 //23
					 , GetAndHoldReasonSql(reasonResolutionList, @params)                                                                                                       //24
					 , jobDecQuery.LiteralTextSqlFormatted      //25
					 , cusMawbQuery.LiteralTextSqlFormatted     //26
					 );

			#endregion

			@params.Add("@SL_SE_NKEvent", Events.QueueChanged.Code, StmALogSchema.SL_SE_NKEvent);
			@params.Add("@CM_ArrivalDateStart", startDate.Date, CusMAWBSchema.CM_ArrivalDate);
			@params.Add("@CM_ArrivalDateEnd", endDate.Date, CusMAWBSchema.CM_ArrivalDate);
			@params.Add("@JE_DateOfArrivalStart", startDate.Date, JobDeclarationSchema.JE_DateOfArrival);
			@params.Add("@JE_DateOfArrivalEnd", endDate.Date, JobDeclarationSchema.JE_DateOfArrival);
			@params.Add("@SL_PostedTimeUtcStart", startDate.Date, StmALogSchema.SL_PostedTimeUtc);

			DynamicBusinessObjectCollection result = new DynamicBusinessObjectCollection(Factory);
			result.Load(sqlText, @params);
			return result;
		}

		ZString GetValidProcessQueuWithFlightNumberInRegistrySql(ZString flightNumbersSqlText)
		{
			if (!flightNumbersSqlText.IsEmpty)
			{
				var branches = UPETools.Instance.UPECustomisationBranches(true).ToArray();
				var jobDecQuery = new ZDBOnlyQuery(typeof(UPEJobDeclaration));
				jobDecQuery.FilterByForeignKey(JobDeclarationSchema.JE_GB, branches);
				var cusMawbQuery = new ZDBOnlyQuery(typeof(UPECusMAWB));
				cusMawbQuery.FilterByForeignKey(CusMAWBSchema.CM_GB, branches);

				ZString sqlText = ZString.Format(@"SELECT {4}, {7} AS Housebill, DATEADD(DAY, 1, {8}) AS ArrivalDate
		FROM {0}
		INNER JOIN {1}
			ON {5} = {6}
			AND {8} >= DATEADD(DAY, -1, @JE_DateOfArrivalStart)
			AND {8} < @JE_DateOfArrivalEnd
			{14}
            AND {16}
		UNION ALL 
		SELECT {4}, {10} AS Housebill, DATEADD(DAY, 1, {13}) AS ArrivalDate
		FROM {0}
		INNER JOIN {2}
			ON {5} = {9}
		INNER JOIN {3}
			ON {11} = {12}
			AND {13} >= DATEADD(DAY, -1, @CM_ArrivalDateStart)
			AND {13} < @CM_ArrivalDateEnd
			{15}
            AND {17}
		UNION ALL"
					, ProcessQueueSchema.Constants.TableName                            //0
					, JobDeclarationSchema.Constants.TableName                      //1
					, CusHAWBSchema.Constants.TableName                                     //2
					, CusMAWBSchema.Constants.TableName                                     //3
					, ProcessQueueSchema.Constants.PK                                           //4
					, ProcessQueueSchema.Constants.P4_ParentID                      //5
					, JobDeclarationSchema.Constants.PK                                     //6
					, JobDeclarationSchema.Constants.JE_HouseBill                   //7
					, JobDeclarationSchema.Constants.JE_DateOfArrival           //8
					, CusHAWBSchema.Constants.PK                                                    //9
					, CusHAWBSchema.Constants.CS_HAWB                                           //10
					, CusHAWBSchema.Constants.CS_CM                                             //11
					, CusMAWBSchema.Constants.PK                                                    //12
					, CusMAWBSchema.Constants.CM_ArrivalDate                            //13
					, GetAndFlightNumbersSql(true, JobDeclarationSchema.Constants.JE_VoyageFlightNo, flightNumbersSqlText)          //14
					, GetAndFlightNumbersSql(true, CusMAWBSchema.Constants.CM_FlightNo, flightNumbersSqlText)                                       //15
					, jobDecQuery.LiteralTextSqlFormatted       //16
					, cusMawbQuery.LiteralTextSqlFormatted      //17
				);

				return sqlText;
			}

			return ZString.Empty;
		}

		internal ZString GetFlightNumbersSql(ZSqlParameterCollection @params)
		{
			string[] flightNumbers = UPEDataRegistry.Instance.FlightNumbersForUploadOnDayOfArrivalPlusOne;
			string flightNumberSqlText = string.Empty;

			if (flightNumbers != null && flightNumbers.Length > 0)
			{
				string[] paramNames = flightNumbers.Select((s, i) => "@FN_" + i).ToArray();
				string delimiter = ",";
				flightNumberSqlText = string.Format("({0})", string.Join(delimiter, paramNames));

				for (int i = 0; i < paramNames.Length; i++)
				{
					@params.Add(paramNames[i], flightNumbers[i], CargoWise.Schema.Schema.GenericStringSchemaColumn);
				}
			}

			return flightNumberSqlText;
		}

		internal ZString GetAndFlightNumbersSql(bool shouldGetStatuesFlightNumberInRegistry, ZString columnName, ZString flightNumbersSqlText)
		{
			if (!flightNumbersSqlText.IsEmpty)
			{
				if (shouldGetStatuesFlightNumberInRegistry)
				{
					return ZString.Format("AND {0} IN {1}", columnName, flightNumbersSqlText);
				}
				else
				{
					return ZString.Format("AND {0} NOT IN {1}", columnName, flightNumbersSqlText);
				}
			}

			return ZString.Empty;
		}

		string GetAndHoldReasonSql(ReadOnlyCodeDescriptionPairList reasonResolutionList, ZSqlParameterCollection @params)
		{
			string result = string.Empty;

			if (reasonResolutionList.Count > 0)
			{
				result = "AND (";
				foreach (ICodeDescription pair in reasonResolutionList)
				{
					string paramName = "@HR_" + pair.Code;
					result += "(substring(" + StmALogSchema.Constants.SL_Reference + ", 11, 2) = " + paramName + " OR ";
					result += "substring(" + StmALogSchema.Constants.SL_Reference + ", 8, 2) = " + paramName + ") OR ";
					@params.Add(paramName, pair.Code, StmALogSchema.SL_Reference);
				}
				result = result.Substring(0, result.Length - 4) + ")";
			}
			return result;
		}

		IShipmentStatusData[] GetShipmentStatusData(ZQuery shipmentStatusFilter)
		{
			List<IShipmentStatusData> result = new List<IShipmentStatusData>();
			int originalTimeOut = Db.Connection.DefaultCommandTimeOutInSeconds;

			try
			{
				Db.Connection.DefaultCommandTimeOutInSeconds = 7200;

				FilteredBusinessObjectReader reader = new FilteredBusinessObjectReader<UPEProcessQueueLog>(FactoryProvider, shipmentStatusFilter) { BatchSize = UPEDataRegistry.Instance.BISIUploadLoadProcessQueueLogBatchSize };
				BusinessObject lastBizoRead = null;
				IEnumerable<UPEProcessQueueLog> logs;
				while ((logs = reader.LoadNextBatchInANewFactory(lastBizoRead).Cast<UPEProcessQueueLog>()).Any())
				{
					result.AddRange(logs);
					FactoryProvider.SaveCurrentAndCreateNew();
					lastBizoRead = logs.LastOrDefault();
				}

				return result.ToArray();
			}
			finally
			{
				Db.Connection.DefaultCommandTimeOutInSeconds = originalTimeOut;
			}
		}

		#endregion

		#region Date Range Filter

		bool CanOverrideShipmentStatusData(IShipmentStatusData existingStatusData, IShipmentStatusData newStatusData)
		{
			int existingStatusPriority = StatusProrityList.IndexOf(existingStatusData.ShipmentStatusType);
			int newStatusPriority = StatusProrityList.IndexOf(newStatusData.ShipmentStatusType);

			return (newStatusPriority > existingStatusPriority ||
				(newStatusPriority == existingStatusPriority && existingStatusData.StatusChangeDate < newStatusData.StatusChangeDate));
		}

		ZDBOnlyQuery FilteredShipmentStatusQuery(ZQuery shipmentStatusFilter)
		{
			var subQueryProcessQueueCS = new ZDBOnlySubQuery(typeof(ProcessQueue), StmALogSchema.SL_Parent);
			var subQueryHawbCS = new ZDBOnlySubQuery(typeof(UPECusHAWB), ProcessQueueSchema.P4_ParentID);
			var subQueryMawbCS = new ZDBOnlySubQuery(typeof(UPECusMAWB), CusHAWBSchema.CS_CM);
			var subQueryBranchCS = new ZDBOnlySubQuery(typeof(GlbBranch), CusMAWBSchema.CM_GB);

			subQueryBranchCS.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			subQueryBranchCS.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			subQueryMawbCS.AddSubQuery(subQueryBranchCS, JoinCondition.And);
			subQueryHawbCS.AddSubQuery(subQueryMawbCS, JoinCondition.And);
			subQueryProcessQueueCS.AddToFilter(ProcessQueueSchema.P4_ParentTableCode, CusHAWBSchema.Constants.Prefix);
			subQueryProcessQueueCS.AddSubQuery(subQueryHawbCS, JoinCondition.And);

			var subQueryProcessQueueJE = new ZDBOnlySubQuery(typeof(ProcessQueue), StmALogSchema.SL_Parent);
			var subQueryJobDeclarationJE = new ZDBOnlySubQuery(typeof(UPEJobDeclaration), ProcessQueueSchema.P4_ParentID);
			var subQueryBranchJE = new ZDBOnlySubQuery(typeof(GlbBranch), JobDeclarationSchema.JE_GB);

			subQueryBranchJE.AddToFilter(GlbBranchSchema.GB_IsActive, true);
			subQueryBranchJE.AddToFilter(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
			subQueryJobDeclarationJE.AddSubQuery(subQueryBranchJE, JoinCondition.And);
			subQueryProcessQueueJE.AddToFilter(ProcessQueueSchema.P4_ParentTableCode, JobDeclarationSchema.Constants.Prefix);
			subQueryProcessQueueJE.AddSubQuery(subQueryJobDeclarationJE, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(StmALog));
			subQueryProcessQueueJE.AddToFilter(shipmentStatusFilter);
			query.AddSubQuery(subQueryProcessQueueJE, JoinCondition.And);
			subQueryProcessQueueCS.AddToFilter(shipmentStatusFilter);
			query.AddSubQuery(subQueryProcessQueueCS, JoinCondition.Or);

			return query;
		}

		internal IShipmentStatusData[] FilteredShipmentStatusData(ZDateTime startDate, ZDateTime endDate)
		{
			ZQuery shipmentStatusFilter = new ZQuery(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.GreaterThanOrEqualTo, Env.Time.GetUtcFromLocalTime(startDate.ToDateTime()));
			shipmentStatusFilter.AddToFilter(StmALogSchema.SL_PostedTimeUtc, SQLComparisonOperator.LessThan, Env.Time.GetUtcFromLocalTime(endDate.ToDateTime()));
			shipmentStatusFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.QueueChanged.Code);
			shipmentStatusFilter.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc;

			var query = FilteredShipmentStatusQuery(shipmentStatusFilter);

			return (IShipmentStatusData[])Factory.Load(typeof(UPEProcessQueueLog), query);
		}

		#endregion

		#region Implementation

		Hashtable StatusForExportList
		{
			get
			{
				if (fStatusForExportList == null)
				{
					fStatusForExportList = new Hashtable();
				}

				return fStatusForExportList;
			}
		}
		Hashtable fStatusForExportList;

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}
				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		BusinessObjectFactoryProvider FactoryProvider
		{
			get { return factoryProvider ?? (factoryProvider = new BusinessObjectFactoryProvider()); }
		}
		BusinessObjectFactoryProvider factoryProvider;

		#region Status Priority List

		IList StatusProrityList
		{
			get
			{
				if (fStatusProrityList == null)
				{
					fStatusProrityList = new ShipmentStatusType[]
						{
							ShipmentStatusType.Unknown,
							ShipmentStatusType.CargoReport,
							ShipmentStatusType.Commercial,
							ShipmentStatusType.Declaration,
							ShipmentStatusType.Special
						};
				}
				return fStatusProrityList;
			}
		}

		IList fStatusProrityList;

		#endregion

		#endregion

		#region For Testing

		internal BusinessObjectFactory FactoryForTesting
		{
			get { return Factory; }
		}

		#endregion
	}
}
