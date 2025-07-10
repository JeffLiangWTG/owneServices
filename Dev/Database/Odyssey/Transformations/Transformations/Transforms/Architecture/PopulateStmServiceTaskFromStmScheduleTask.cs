using System;
using System.Data;
using System.Xml;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	public class PopulateStmServiceTaskFromStmScheduleTask : DataTransformation
	{
		public override string UserDescription => "Populate StmServiceTask table using data from StmScheduleTask table.";

		protected override void OfflinePostUpgradeTransform()
		{
			var proceed = PerformBulkConversion();

			if (!proceed)
			{
				return;
			}

			var convertConfigStringXmlTable = GetConvertedServiceTaskData();

			if (convertConfigStringXmlTable.Rows.Count > 0)
			{
				foreach (DataRow row in convertConfigStringXmlTable.Rows)
				{
					var configXml = row["UncompressedScheduleState"] as string;
					var xmlDoc = new XmlDocument();

					if (string.IsNullOrWhiteSpace(configXml))
					{
						continue;
					}

					try
					{
						xmlDoc.LoadXml(configXml);
					}
					catch (XmlException)
					{
						manager.ShowInfoMessage($"Service Task {row[StmScheduleTaskSchema.Constants.S5_ScheduleType] as string} has invalid schedule configuration. Using defaults for SecondaryProcessesMaxCount and Additional Configuration");
					}

					var secondaryProcessesMaxCount = xmlDoc.GetElementsByTagName("SecondaryProcessesMaxCount")[0]?.InnerXml;
					var configString = xmlDoc.GetElementsByTagName("ConfigString")[0]?.InnerXml;

					var newScheduleConfig = row[StmServiceTaskSchema.Constants.SST_Configuration] as string;
					var newScheduleConfigXml = new XmlDocument();
					newScheduleConfigXml.LoadXml(newScheduleConfig);

					bool updateNeeded = false;
					if (!string.IsNullOrEmpty(secondaryProcessesMaxCount))
					{
						AppendXml(newScheduleConfigXml, "SecondaryProcessesMaxCount", secondaryProcessesMaxCount);
						updateNeeded = true;
					}

					if (!string.IsNullOrEmpty(configString))
					{
						AppendXml(newScheduleConfigXml, "ConfigString", configString);
						updateNeeded = true;
					}

					if (updateNeeded)
					{
						UpdateStmServiceTaskRow(row[StmScheduleTaskSchema.Constants.S5_ScheduleType] as string, newScheduleConfigXml.OuterXml);
					}
				}
			}
		}

		static bool PerformBulkConversion()
		{
			var localOffset = DateTimeOffset.Now.Offset;
			var localOffsetText = (localOffset >= TimeSpan.Zero ? '+' : '-') + localOffset.ToString(@"hh\:mm");

			Db.Connection.ExecuteNonQuery(sqlBulkTransferText, cmd => cmd.AddParameter("@LocalOffset", SqlDbType.VarChar, localOffsetText));

			return true;
		}

		DataTable GetConvertedServiceTaskData()
		{
			var result = new DataTable();

			var sqlConfigStringText = @"-- GetStmScheduleTask XML config
SELECT
	SCH.S5_ScheduleType, dbo.CLRUncompressAsString(SCH.S5_ScheduleState) as UncompressedScheduleState, SST.SST_Configuration
FROM
	dbo.StmScheduleTask SCH
JOIN
	dbo.StmServiceTask SST
ON
	SCH.S5_ScheduleType = SST.SST_ServiceTaskCode
WHERE
	SCH.S5_ParentTableCode = 'SH'
AND
	S5_ScheduleState IS NOT NULL
;";

			using var cmd = Db.Connection.Command(sqlConfigStringText);
			using var adapter = cmd.NewDataAdapter();
			adapter.Fill(result);

			return result;
		}

		void UpdateStmServiceTaskRow(string serviceTaskCode, string configurationXmlString)
		{
			var sql = @"-- Update StmServiceTask with latest config
UPDATE
	dbo.StmServiceTask
SET
	SST_Configuration = @SST_Configuration,
	SST_SystemLastEditTimeUtc = SST_SystemLastEditTimeUtc,
	SST_SystemLastEditUser = SST_SystemLastEditUser
WHERE
	SST_ServiceTaskCode = @SST_ServiceTaskCode
;";

			using var cmd = Db.Connection.Command(sql);
			cmd.AddParameterBasedOnDbColumn("@SST_ServiceTaskCode", serviceTaskCode, StmServiceTaskSchema.SST_ServiceTaskCode);
			cmd.AddParameterBasedOnDbColumn("@SST_Configuration", configurationXmlString, StmServiceTaskSchema.SST_Configuration);
			cmd.ExecuteNonQuery();
		}

		static void AppendXml(XmlDocument document, string elementName, string value)
		{
			var elementToAppend = document.CreateElement(elementName);
			elementToAppend.InnerText = value;
			document.DocumentElement.AppendChild(elementToAppend);
		}

		const string sqlBulkTransferText = @"-- Bulk conversion of service task schedules
DELETE dbo.StmServiceTask;

IF NOT EXISTS (SELECT 1 FROM dbo.StmServiceTask)
BEGIN
	DECLARE @DefaultTime smalldatetime;
	SET @DefaultTime = '1970-01-01 00:00:00';

	DECLARE @LocalTimeZone nvarchar(128);

	SELECT TOP(1) @LocalTimeZone = name
	FROM sys.time_zone_info
	WHERE current_utc_offset = @LocalOffset
	;

	WITH CTE AS
	(
		SELECT ROW_NUMBER()
		OVER (PARTITION BY S5_ScheduleType ORDER BY S5_SystemLastEditTimeUtc DESC) AS RowNum
		FROM dbo.StmScheduleTask
		WHERE S5_ParentTableCode = 'SH'
	)
	DELETE
	FROM CTE
	WHERE RowNum > 1;

	WITH WEEKDAYMAP AS
	(
		SELECT 1 WeekDayNumber, 'Sunday' WeekDay UNION ALL
		SELECT 2, 'Monday' UNION ALL
		SELECT 3, 'Tuesday' UNION ALL
		SELECT 4, 'Wednesday' UNION ALL
		SELECT 5, 'Thursday' UNION ALL
		SELECT 6, 'Friday' UNION ALL
		SELECT 7, 'Saturday'
	),
	OLDSCHEDULES AS
	(
		SELECT
			S5_IsActive,
			S5_GB,
			ISNULL(S5_NextScheduledPrintRunTimeUtc, @DefaultTime) As S5_NextScheduledPrintRunTimeUtc,
			S5_ScheduleType,
			ISNULL(S5_SystemCreateTimeUtc, @DefaultTime) AS S5_SystemCreateTimeUtc,
			ISNULL(NULLIF(S5_SystemCreateUser, ''), '~BP') AS S5_SystemCreateUser,
			ISNULL(S5_SystemLastEditTimeUtc, @DefaultTime) AS S5_SystemLastEditTimeUtc,
			ISNULL(NULLIF(S5_SystemLastEditUser, ''), '~BP') AS S5_SystemLastEditUser,
			S5_TaskPeriod,
			CASE WHEN S5_TaskPeriodCount <= 0 THEN 1 ELSE S5_TaskPeriodCount END AS S5_TaskPeriodCount,
			S5_DailyStartTime,
			S5_DailyEndTime,
			S5_WeekDaysOnly,
			S5_DayList,
			S5_DayNumber,
			S5_WeekDayOccurrenceNumber,
			S5_StartDate,
			S5_MonthNumber,
			MAP.WeekDay AS WeekDay,
			FORMAT(S5_DailyStartTime AT TIME ZONE @LocalTimeZone AT TIME ZONE 'UTC', 'HH:mm:00') AS S5_DailyStartTimeUtcString,
			FORMAT(S5_DailyEndTime AT TIME ZONE @LocalTimeZone AT TIME ZONE 'UTC', 'HH:mm:00') AS S5_DailyEndTimeUtcString,
			DATEDIFF(
				DAY, 
				ISNULL(
					CAST(S5_DailyStartTime AT TIME ZONE @LocalTimeZone AS DATETIME),
					CAST(ISNULL(S5_NextScheduledPrintRunTimeUtc, @DefaultTime) AT TIME ZONE 'UTC' AT TIME ZONE @LocalTimeZone AS DATETIME)),
				ISNULL(
					CAST(S5_DailyStartTime AT TIME ZONE @LocalTimeZone AT TIME ZONE 'UTC' AS DATETIME),
					CAST(ISNULL(S5_NextScheduledPrintRunTimeUtc, @DefaultTime) AT TIME ZONE 'UTC' AS DATETIME))) AS WeekDayOffset,
			'00:00:00' AS PreviousDayTruncatedUtcTime,
			'23:59:00' AS NextDayTruncatedUtcTime
		FROM 
			dbo.StmScheduleTask SCH
		LEFT JOIN
			WEEKDAYMAP MAP
		ON
			MAP.WeekDayNumber = SCH.S5_DayNumber
		WHERE
			S5_ParentTableCode = 'SH'
	)
	INSERT INTO dbo.StmServiceTask
	(
		SST_PK,
		SST_Active,
		SST_GB_Branch,
		SST_NextRunTime,
		SST_ServiceTaskCode,
		SST_SystemCreateTimeUtc,
		SST_SystemCreateUser,
		SST_SystemLastEditTimeUtc,
		SST_SystemLastEditUser,
		SST_Configuration,
		SST_LastRunTime
	)
	SELECT
	NEWID(),
	S5_IsActive,
	S5_GB,
	S5_NextScheduledPrintRunTimeUtc,
	S5_ScheduleType,
	S5_SystemCreateTimeUtc,
	S5_SystemCreateUser,
	S5_SystemLastEditTimeUtc,
	S5_SystemLastEditUser,
	CASE S5_TaskPeriod
		WHEN 'S'
		THEN CONCAT(
			N'<ScheduleConfig><NextRunTimeCalculatorSeconds Period=""', S5_TaskPeriodCount,
			CASE WHEN S5_DailyStartTime IS NULL OR S5_DailyEndTime IS NULL THEN '' ELSE CONCAT(N'"" StartTime=""', S5_DailyStartTimeUtcString) END,
			CASE WHEN S5_DailyEndTime IS NULL OR S5_DailyStartTime IS NULL THEN '' ELSE CONCAT(N'"" EndTime=""', S5_DailyEndTimeUtcString) END,
			N'""/></ScheduleConfig>')

		WHEN 'T'
		THEN CONCAT(
			N'<ScheduleConfig><NextRunTimeCalculatorMinutes Period=""', S5_TaskPeriodCount,
			CASE WHEN S5_DailyStartTime IS NULL OR S5_DailyEndTime IS NULL THEN '' ELSE CONCAT(N'"" StartTime=""', S5_DailyStartTimeUtcString) END,
			CASE WHEN S5_DailyEndTime IS NULL OR S5_DailyStartTime IS NULL THEN '' ELSE CONCAT(N'"" EndTime=""', S5_DailyEndTimeUtcString) END,
			N'""/></ScheduleConfig>')

		WHEN 'H'
		THEN CONCAT(
			N'<ScheduleConfig><NextRunTimeCalculatorHours Period=""', S5_TaskPeriodCount,
			CASE WHEN S5_DailyStartTime IS NULL OR S5_DailyEndTime IS NULL THEN '' ELSE CONCAT(N'"" StartTime=""', S5_DailyStartTimeUtcString) END,
			CASE WHEN S5_DailyEndTime IS NULL OR S5_DailyStartTime IS NULL THEN '' ELSE CONCAT(N'"" EndTime=""', S5_DailyEndTimeUtcString) END,
			N'""/></ScheduleConfig>')

		WHEN 'D'
		THEN CONCAT(
			N'<ScheduleConfig><NextRunTimeCalculatorDays Period=""',
			S5_TaskPeriodCount,
			N'"" ScheduledRunTime=""', ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00'),
			N'""/></ScheduleConfig>')

		WHEN 'W' THEN 
			CASE 
				WHEN S5_WeekDaysOnly = 1
				THEN CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime=""', ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00'),
					N'"" /></ScheduleConfig>')

				ELSE CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorWeeks Period=""',
					S5_TaskPeriodCount,
					'"" ScheduledRunTime=""', ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00'),
					N'""><DaysOfOccurrence>',
					CASE WHEN SUBSTRING(S5_DayList, 1, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 1 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					CASE WHEN SUBSTRING(S5_DayList, 2, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 2 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					CASE WHEN SUBSTRING(S5_DayList, 3, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 3 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					CASE WHEN SUBSTRING(S5_DayList, 4, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 4 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					CASE WHEN SUBSTRING(S5_DayList, 5, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 5 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					CASE WHEN SUBSTRING(S5_DayList, 6, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 6 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					CASE WHEN SUBSTRING(S5_DayList, 7, 1) = 'Y' THEN CONCAT(N'<DayOfWeek>', DATENAME(WEEKDAY, DATEADD(DAY, 7 + WeekdayOffset, '2022-01-01')), '</DayOfWeek>') ELSE '' END,
					N'</DaysOfOccurrence></NextRunTimeCalculatorWeeks></ScheduleConfig>')
			END

		WHEN 'M' THEN
			CASE
				WHEN S5_DayNumber = 99
				THEN CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorMonthsByLastDay Period=""',
					S5_TaskPeriodCount,
					N'"" ScheduledRunTime=""', 
					CASE
						WHEN WeekDayOffset < 0 THEN PreviousDayTruncatedUtcTime
						WHEN WeekDayOffset > 0 THEN NextDayTruncatedUtcTime
						ELSE ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00')
					END,
					N'""></NextRunTimeCalculatorMonthsByLastDay></ScheduleConfig>')

				WHEN S5_WeekDayOccurrenceNumber = 0
				THEN CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorMonthsByDate Period=""',
					S5_TaskPeriodCount,
					N'"" DayOfOccurrence=""', DAY(S5_StartDate),
					N'"" ScheduledRunTime=""', 
					CASE
						WHEN WeekDayOffset < 0 THEN PreviousDayTruncatedUtcTime
						WHEN WeekDayOffset > 0 THEN NextDayTruncatedUtcTime
						ELSE ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00')
					END,
					N'""></NextRunTimeCalculatorMonthsByDate></ScheduleConfig>')

				ELSE CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorMonthsByDayOfWeek WeekOfTheMonth=""', S5_WeekDayOccurrenceNumber,
					N'"" DayOfTheWeek=""', WeekDay,
					N'"" ScheduledRunTime=""',
					CASE
						WHEN WeekDayOffset < 0 THEN PreviousDayTruncatedUtcTime
						WHEN WeekDayOffset > 0 THEN NextDayTruncatedUtcTime
						ELSE ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00')
					END,
					N'""></NextRunTimeCalculatorMonthsByDayOfWeek></ScheduleConfig>')
			END

		WHEN 'Y' THEN
			CASE
				WHEN S5_WeekDayOccurrenceNumber = 0
				THEN CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorYearsByDate Month=""', S5_MonthNumber,
					N'"" Day=""', DAY(S5_StartDate),
					N'"" ScheduledRunTime=""',
					CASE
						WHEN WeekDayOffset < 0 THEN PreviousDayTruncatedUtcTime
						WHEN WeekDayOffset > 0 THEN NextDayTruncatedUtcTime
						ELSE ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00')
					END,
					N'""/></ScheduleConfig>')

				ELSE CONCAT(
					N'<ScheduleConfig><NextRunTimeCalculatorYearsByDayOfMonth DayOfTheWeek=""', WeekDay,
					N'"" WeekOfTheMonth=""', S5_WeekDayOccurrenceNumber,
					N'"" MonthOfTheYear=""', S5_MonthNumber,
					N'"" ScheduledRunTime=""',
					CASE
						WHEN WeekDayOffset < 0 THEN PreviousDayTruncatedUtcTime
						WHEN WeekDayOffset > 0 THEN NextDayTruncatedUtcTime
						ELSE ISNULL(ISNULL(S5_DailyStartTimeUtcString, FORMAT(S5_NextScheduledPrintRunTimeUtc, 'HH:mm:00')), '00:00:00')
					END,
					N'""/></ScheduleConfig>')
			END
		ELSE NULL
	END,
	NULL
FROM
	OLDSCHEDULES
;
END
";
	}
}
