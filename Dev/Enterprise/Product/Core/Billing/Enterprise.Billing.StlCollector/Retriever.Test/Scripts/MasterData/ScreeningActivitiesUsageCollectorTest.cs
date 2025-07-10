using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.MasterData;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.MasterData
{
	[TestedType(typeof(ScreeningActivitiesUsageCollector))]
	class ScreeningActivitiesUsageCollectorTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery1 = @"
			INSERT INTO dbo.StmScheduleTask
		   (S5_PK
		   ,S5_ScheduleDescription
		   ,S5_TaskPeriod
		   ,S5_WeekDaysOnly
		   ,S5_TaskPeriodCount
		   ,S5_DayNumber
		   ,S5_DayList
		   ,S5_MonthNumber
		   ,S5_WeekDayOccurrenceNumber
		   ,S5_StartDate
		   ,S5_EndAfterCount
		   ,S5_EndDate
		   ,S5_ScheduleActualRunCount
		   ,S5_AccountingPeriodScheduleFrstRun
		   ,S5_DateScheduleFirstRun
		   ,S5_ScheduleType
		   ,S5_TypeOfDocument
		   ,S5_NextScheduledPrintRunTimeUtc
		   ,S5_CurrentPrintRunTime
		   ,S5_IsActive
		   ,S5_IsPrivate
		   ,S5_ScheduleState
		   ,S5_DailyStartTime
		   ,S5_DailyEndTime
		   ,S5_ParentTableCode
		   ,S5_ParentID
		   ,S5_GB
		   ,S5_RunTimeInMinutes
		   ,S5_GS_NKPrintUser
		   ,S5_SystemCreateTimeUtc
		   ,S5_SystemCreateUser
		   ,S5_SystemLastEditTimeUtc
		   ,S5_SystemLastEditUser
		   ,S5_OverdueDurationInSeconds)
	 VALUES
		   (NEWID()
		   ,'Denied Party Screening - Re-screening Advice Manager'
		   ,'H'
		   ,0
		   ,1
		   ,0
		   ,'NNNNNNN'
		   ,0
		   ,0
		   ,'2024-06-04 00:00:00.000'
		   ,0
		   ,NULL
		   ,0
		   ,0
		   ,NULL
		   ,'DPR'
		   ,'DPS'
		   ,'2024-06-04 05:17:51.683'
		   ,NULL
		   ,0
		   ,1
		   ,NULL
		   ,NULL
		   ,NULL
		   ,'SH'
		   ,NULL
		   ,NULL
		   ,0
		   ,''
		   ,'2024-06-04 05:17:00'
		   ,''
		   ,'2024-06-04 05:17:00'
		   ,''
		   ,0)
";
			var sqlQuery2 = @"
			INSERT INTO dbo.StmScheduleTask
		   (S5_PK
		   ,S5_ScheduleDescription
		   ,S5_TaskPeriod
		   ,S5_WeekDaysOnly
		   ,S5_TaskPeriodCount
		   ,S5_DayNumber
		   ,S5_DayList
		   ,S5_MonthNumber
		   ,S5_WeekDayOccurrenceNumber
		   ,S5_StartDate
		   ,S5_EndAfterCount
		   ,S5_EndDate
		   ,S5_ScheduleActualRunCount
		   ,S5_AccountingPeriodScheduleFrstRun
		   ,S5_DateScheduleFirstRun
		   ,S5_ScheduleType
		   ,S5_TypeOfDocument
		   ,S5_NextScheduledPrintRunTimeUtc
		   ,S5_CurrentPrintRunTime
		   ,S5_IsActive
		   ,S5_IsPrivate
		   ,S5_ScheduleState
		   ,S5_DailyStartTime
		   ,S5_DailyEndTime
		   ,S5_ParentTableCode
		   ,S5_ParentID
		   ,S5_GB
		   ,S5_RunTimeInMinutes
		   ,S5_GS_NKPrintUser
		   ,S5_SystemCreateTimeUtc
		   ,S5_SystemCreateUser
		   ,S5_SystemLastEditTimeUtc
		   ,S5_SystemLastEditUser
		   ,S5_OverdueDurationInSeconds)
	 VALUES
			(NEWID()
			,'Denied Party Screening - Re-screening Advice Manager'
			,'H'
			,0
			,1
			,0
			,'NNNNNNN'
			,0
			,0
			,'2024-06-04 00:00:00.000'
			,0
			,NULL
		   ,0
		   ,0
		   ,NULL
		   ,'DSS'
		   ,'DPS'
		   ,'2024-06-04 05:17:51.683'
		   ,NULL
		   ,0
		   ,1
		   ,NULL
		   ,NULL
		   ,NULL
		   ,'SH'
		   ,NULL
		   ,NULL
		   ,0
		   ,''
		   ,'2024-06-04 05:17:00'
		   ,''
		   ,'2024-06-04 05:17:00'
		   ,''
		   ,0)
";
			var sqlQuery3 = @"
 INSERT INTO dbo.StmData
			(SD_PK
			,SD_Name
			,SD_Type
			,SD_IsLogged
			,SD_BinaryValue
			,SD_GuidValue
			,SD_IsCancelled
			,SD_PreserveTestValue
			,SD_SystemCreateTimeUtc
			,SD_SystemCreateUser
			,SD_SystemLastEditTimeUtc
			,SD_SystemLastEditUser)
	  VALUES
			(NEWID()
			,'DPSFreightMovementRestrictions'
			,'STR'
			,1
			,CAST('NO' AS VARBINARY(MAX))
			,NULL
			,0
			,0
			,'2024-06-03 04:59:00'
			,'E'
			,'2024-06-04 09:51:00'
			,'E')";
			TestConnection.ExecuteNonQuery(sqlQuery1);
			TestConnection.ExecuteNonQuery(sqlQuery2);
			TestConnection.ExecuteNonQuery(sqlQuery3);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 3);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var expectedAdditionalRefs = @"{""DPR"":""{\""TaskPeriod\"":\""H\"",\""TaskPeriodCount\"":1,\""WeeksDaysOnly\"":false,\""DayNumber\"":0,\""DayList\"":\""NNNNNNN\"",\""WeekDayOccurrenceNumber\"":0,\""IsActive\"":0}"",""DSS"":""{\""TaskPeriod\"":\""H\"",\""TaskPeriodCount\"":1,\""WeeksDaysOnly\"":false,\""DayNumber\"":0,\""DayList\"":\""NNNNNNN\"",\""WeekDayOccurrenceNumber\"":0,\""IsActive\"":0}"",""FreightCompanySettings"":""{\""TotalCompanies\"":2,\""ALL\"":0,\""NO\"":0,\""EXP\"":0,\""INT\"":0}""}";
			AssertEquals("Number of Transactions", 1, transactions.Count());
			var transaction1 = transactions.First();
			AssertNotNull(transaction1.Reference1);
			AssertEquals("Additional Reference value",expectedAdditionalRefs, transaction1.AdditionalRefs);
		}
	}
}
