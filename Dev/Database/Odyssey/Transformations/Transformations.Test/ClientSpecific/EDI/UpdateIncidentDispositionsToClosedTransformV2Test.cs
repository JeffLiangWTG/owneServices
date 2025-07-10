using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Transforms.ClientSpecific.EDI.Testing
{
	[TestedType(typeof(UpdateIncidentDispositionsToClosedTransformV2))]
	public class UpdateIncidentDispositionsToClosedTransformV2Test : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var indexCountQuery = @"SELECT COUNT(Name)
FROM sys.indexes 
WHERE name='NR_RX__IM_SystemCreateTimeUtc_Closed' AND object_id = OBJECT_ID('dbo.IncidentMain')";

			var indexCount = Db.Connection.ExecuteScalar(indexCountQuery);
			AssertEquals("Index should be deleted once transformation completes", 0, indexCount);

			var loadResult = new List<(string IM_ResolutionCode, string IM_ClosureResolution)>();
			var loadOpenIncidents = $"SELECT IM_ResolutionCode, IM_ClosureResolution FROM dbo.IncidentMain WHERE IM_PK IN ({string.Join(",", nonClosedIncidentGuids.Select(x => $"'{x}'"))});";
			using (var reader = Db.Connection.Command(loadOpenIncidents).ExecuteReader())
			{
				while (reader.Read())
				{
					var resolutionCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
					var closureResolution = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
					loadResult.Add((resolutionCode, closureResolution));
				}
			}

			AssertEquals("Should have 4 open incidents", 4, loadResult.Count);

			foreach (var row in loadResult)
			{
				Assert("Non-closed incident resolution code should be unchanged", string.IsNullOrWhiteSpace(row.IM_ResolutionCode));
				Assert("Non-closed incident closure resolution should be unchanged", string.IsNullOrWhiteSpace(row.IM_ClosureResolution));
			}

			loadResult = new List<(string IM_ResolutionCode, string IM_ClosureResolution)>();
			var loadClosedButNotTransformedIncidents = $"SELECT IM_ResolutionCode, IM_ClosureResolution FROM dbo.IncidentMain WHERE IM_PK IN ({string.Join(",", closedIncidentsToNotTransformGuids.Select(x => $"'{x}'"))});";
			using (var reader = Db.Connection.Command(loadClosedButNotTransformedIncidents).ExecuteReader())
			{
				while (reader.Read())
				{
					var resolutionCode = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
					var closureResolution = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
					loadResult.Add((resolutionCode, closureResolution));
				}
			}

			AssertEquals("Should have 4 untransformed closed incidents", 5, loadResult.Count);

			foreach (var row in loadResult)
			{
				AssertNotEquals("Incident resolution code should be unchanged", "CLS", row.IM_ResolutionCode);
				Assert("Incident closure resolution should be unchanged", string.IsNullOrWhiteSpace(row.IM_ClosureResolution));
			}

			var loadTransformedResult = new List<(Guid IM_PK, string IM_ResolutionCode, string IM_ClosureResolution, DateTime IM_ResolveTimeUtc)>();
			var loadClosedTransformedIncidents = $"SELECT IM_PK, IM_ResolutionCode, IM_ClosureResolution, IM_ResolveTimeUtc FROM dbo.IncidentMain WHERE IM_PK IN ({string.Join(",", closedIncidentsToTransformGuids.Select(x => $"'{x.PK}'"))});";
			using (var reader = Db.Connection.Command(loadClosedTransformedIncidents).ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = reader.GetGuid(0);
					var resolutionCode = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
					var closureResolution = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
					var resolvedTime = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
					loadTransformedResult.Add((pk, resolutionCode, closureResolution, resolvedTime));
				}
			}

			AssertEquals("Should have 27 transformed incidents", 33, loadTransformedResult.Count);

			var expectedResolvedTime = new DateTime(2023, 06, 01, 12, 0, 0);
			var expectedResolvedTimeJuly = new DateTime(2023, 07, 01, 12, 0, 0);
			//Offset unloco time by SGSIN
			var expectedEventTime = new DateTime(2023, 06, 01, 12, 0, 0).AddMinutes(480);

			foreach (var row in loadTransformedResult)
			{
				AssertEquals("Incident resolution code should be updated", "CLS", row.IM_ResolutionCode);
				var expectedClosureResolution = closedIncidentsToTransformGuids.Find(x => x.PK == row.IM_PK).Resolution;
				AssertEquals("Incident closure resolution should be updated", expectedClosureResolution, row.IM_ClosureResolution);

				if (row.IM_PK == closedIncidentsToTransformGuids[25].PK)
				{
					AssertEquals("Resolved Time should be updated to match latest STC event to CLS", expectedResolvedTimeJuly, row.IM_ResolveTimeUtc);
				}
				else
				{
					AssertEquals("Resolved Time should be updated to match latest STC event to CLS", expectedResolvedTime, row.IM_ResolveTimeUtc);
				}
			}

			var loadIRSEventResult = new List<(Guid Parent, DateTime EventTime, DateTime EventTimeUtc, string BranchCode)>();
			var loadIRSEvents = @"
SELECT SL_Parent, SL_EventTime, SL_EventTimeUtc, SL_GB_NKBranch FROM dbo.StmALog
WHERE
SL_SE_NKEvent = 'IRS'
AND SL_Reference = 'Data Transform'
AND SL_Table = 'IncidentMain'
AND SL_GS_NKUser = '~BP'
;";
			using (var reader = Db.Connection.Command(loadIRSEvents).ExecuteReader())
			{
				while (reader.Read())
				{
					var pk = reader.GetGuid(0);
					var eventTime = reader.GetDateTime(1);
					var eventTimeUtc = reader.GetDateTime(2);
					var branchCode = reader.GetString(3);
					loadIRSEventResult.Add((pk, eventTime, eventTimeUtc, branchCode));
				}
			}

			AssertEquals("Should have created 26 IRS events", 32, loadIRSEventResult.Count);

			foreach (var row in loadIRSEventResult)
			{
				if (row.Parent == closedIncidentsToTransformGuids[25].PK)
				{
					AssertEquals("Event time should be set to utc time since the event occurred outside any applicable RefUNLOCOUtcOffset ranges", expectedResolvedTimeJuly, row.EventTime);
					AssertEquals("Event time should be set to utc time", expectedResolvedTimeJuly, row.EventTimeUtc);
				}
				else
				{
					AssertEquals("Event time should be updated based on the SL_GB_NKBranch => UNLOCO => offset", expectedEventTime, row.EventTime);
					AssertEquals("Event time should be set to utc time", expectedResolvedTime, row.EventTimeUtc);
				}

				AssertEquals("Branch code should be copied from STC/JCL log", branchCode, row.BranchCode);
			}

			Assert("Should not add IRS event for incident with no STC or JCL events", loadIRSEventResult.All(x => x.Parent != closedIncidentsToTransformGuids[5].PK));

			var matchGuids = loadIRSEventResult.Select(x => x.Parent).Append(closedIncidentsToTransformGuids[5].PK).OrderBy(x => x).ToArray();
			var expectedGuids = closedIncidentsToTransformGuids.Select(x => x.PK).OrderBy(x => x).ToArray();
			AssertArrayEqualsByElements("One event should have been added for all the transformed incidents except the one with no STC or JCL event", expectedGuids, matchGuids);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			return new UpdateIncidentDispositionsToClosedTransformV2ForTest();
		}

		class UpdateIncidentDispositionsToClosedTransformV2ForTest : UpdateIncidentDispositionsToClosedTransformV2
		{
			protected override int BatchSize => 4;
		}

		readonly List<Guid> nonClosedIncidentGuids = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
		};

		readonly List<(Guid PK, string Resolution)> closedIncidentsToTransformGuids = new List<(Guid PK, string Resolution)>
		{
			(Guid.NewGuid(), "URP"),
			(Guid.NewGuid(), "UPD"),
			(Guid.NewGuid(), "TSP"),
			(Guid.NewGuid(), "TRN"),
			(Guid.NewGuid(), "TRM"),
			(Guid.NewGuid(), "SYS"),
			(Guid.NewGuid(), "SRS"),
			(Guid.NewGuid(), "REF"),
			(Guid.NewGuid(), "OTH"),
			(Guid.NewGuid(), "NSC"),
			(Guid.NewGuid(), "DUP"),
			(Guid.NewGuid(), "DTF"),
			(Guid.NewGuid(), "COM"),
			(Guid.NewGuid(), "CIG"),
			(Guid.NewGuid(), "QDC"),
			(Guid.NewGuid(), "NFR"),
			(Guid.NewGuid(), "CAN"),
			(Guid.NewGuid(), "NDF"),
			(Guid.NewGuid(), "DBR"),
			(Guid.NewGuid(), "CLI"),
			(Guid.NewGuid(), "SAL"),
			(Guid.NewGuid(), "NCS"),
			(Guid.NewGuid(), "NCR"),
			(Guid.NewGuid(), "CRA"),
			(Guid.NewGuid(), "NCN"),
			(Guid.NewGuid(), "NRC"),//25
			(Guid.NewGuid(), "PDM"),
			(Guid.NewGuid(), "PFR"),
			(Guid.NewGuid(), "PSL"),
			(Guid.NewGuid(), "PTI"),
			(Guid.NewGuid(), "PTT"),
			(Guid.NewGuid(), "REI"),
			(Guid.NewGuid(), "SFR"),
		};

		readonly List<Guid> closedIncidentsToNotTransformGuids = new List<Guid>
		{
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
			Guid.NewGuid(),
		};

		const string branchCode = "BWO";

		protected override void PrepareTestData()
		{
			//insert incidents with and without closed status
			//join to events with STC and non STC events
			//make sure it only uses the latest STC event with reference ending in "to CLS"
			//Ensure it only applies to incidents with the specified dispositions from the registry

			DbObjectCreator.CreateTableIfNotExists(Db.Connection, "IncidentMain", @"
CREATE TABLE dbo.IncidentMain
(
	[IM_PK] UNIQUEIDENTIFIER NOT NULL,
	[IM_IncidentNumber] VARCHAR(20) NOT NULL DEFAULT '' ,
	[IM_IncidentType] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_Status] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_ResolutionCode] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_ClosureResolution] CHAR(3) NOT NULL DEFAULT '' ,
	[IM_ResolveTimeUtc] DATETIME NULL,
	[IM_SystemCreateTimeUtc] SMALLDATETIME NULL,
	[IM_SystemCreateUser] VARCHAR(3) NOT NULL DEFAULT '' ,
	[IM_SystemLastEditTimeUtc] DATETIME NULL,
	[IM_SystemLastEditUser] VARCHAR(3) NOT NULL DEFAULT ''
);");

			var creator = new TransformationTestDataCreator();
			var companyPK = creator.CreateCompany("TES", "AU");
			creator.CreateBranch(branchCode, "SGSIN", companyPK);

			var dataQuery = $@"
INSERT INTO dbo.RefUNLOCOUtcOffset(
	[RLO_PK],
	[RLO_RL_NKCode],
	[RLO_StartTimeUtc],
	[RLO_EndTimeUtc],
	[RLO_OffsetMinutesFromUtc],
	[RLO_SystemCreateTimeUtc],
	[RLO_SystemCreateUser],
	[RLO_SystemLastEditTimeUtc],
	[RLO_SystemLastEditUser])
VALUES
(NEWID(), 'SGSIN', '2023-05-30', '2023-06-03', 480, '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E')

INSERT INTO dbo.IncidentMain(
	[IM_PK],
	[IM_IncidentNumber],
	[IM_IncidentType],
	[IM_Status],
	[IM_ResolutionCode],
	[IM_SystemCreateTimeUtc],
	[IM_SystemCreateUser],
	[IM_SystemLastEditTimeUtc],
	[IM_SystemLastEditUser])
VALUES
('{nonClosedIncidentGuids[0]}', 'CS99991111', 'INC', 'OPN', '', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{nonClosedIncidentGuids[1]}', 'CS99991112', 'INC', 'WRK', '', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{nonClosedIncidentGuids[2]}', 'CS99991113', 'INC', 'NCL', '', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{nonClosedIncidentGuids[3]}', 'CS99991114', 'INC', 'SUS', '', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[0].PK}', 'CS99991115', 'INC', 'CLS', '{closedIncidentsToTransformGuids[0].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[1].PK}', 'CS99991116', 'INC', 'CLS', '{closedIncidentsToTransformGuids[1].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[2].PK}', 'CS99991117', 'INC', 'CLS', '{closedIncidentsToTransformGuids[2].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[3].PK}', 'CS99991118', 'INC', 'CLS', '{closedIncidentsToTransformGuids[3].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[4].PK}', 'CS99991119', 'INC', 'CLS', '{closedIncidentsToTransformGuids[4].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[5].PK}', 'CS99991120', 'INC', 'CLS', '{closedIncidentsToTransformGuids[5].Resolution}', '2020-05-01 12:00:00', 'E', '2023-06-01 12:00:00', 'E'),
('{closedIncidentsToTransformGuids[6].PK}', 'CS99991121', 'INC', 'CLS', '{closedIncidentsToTransformGuids[6].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[7].PK}', 'CS99991122', 'INC', 'CLS', '{closedIncidentsToTransformGuids[7].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[8].PK}', 'CS99991123', 'INC', 'CLS', '{closedIncidentsToTransformGuids[8].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[9].PK}', 'CS99991124', 'INC', 'CLS', '{closedIncidentsToTransformGuids[9].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[10].PK}', 'CS99991125', 'INC', 'CLS', '{closedIncidentsToTransformGuids[10].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[11].PK}', 'CS99991126', 'INC', 'CLS', '{closedIncidentsToTransformGuids[11].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[12].PK}', 'CS99991127', 'INC', 'CLS', '{closedIncidentsToTransformGuids[12].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[13].PK}', 'CS99991128', 'INC', 'CLS', '{closedIncidentsToTransformGuids[13].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[14].PK}', 'CS99991129', 'INC', 'CLS', '{closedIncidentsToTransformGuids[14].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[15].PK}', 'CS99991130', 'INC', 'CLS', '{closedIncidentsToTransformGuids[15].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[16].PK}', 'CS99991131', 'INC', 'CLS', '{closedIncidentsToTransformGuids[16].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[17].PK}', 'CS99991132', 'INC', 'CLS', '{closedIncidentsToTransformGuids[17].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[18].PK}', 'CS99991133', 'INC', 'CLS', '{closedIncidentsToTransformGuids[18].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[19].PK}', 'CS99991134', 'INC', 'CLS', '{closedIncidentsToTransformGuids[19].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[20].PK}', 'CS99991135', 'INC', 'CLS', '{closedIncidentsToTransformGuids[20].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[21].PK}', 'CS99991136', 'INC', 'CLS', '{closedIncidentsToTransformGuids[21].Resolution}', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[22].PK}', 'CS99991137', 'INC', 'CLS', '{closedIncidentsToTransformGuids[22].Resolution}', '2020-06-01 11:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[23].PK}', 'CS99991138', 'INC', 'CLS', '{closedIncidentsToTransformGuids[23].Resolution}', '2020-05-02 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[24].PK}', 'CS99991139', 'INC', 'CLS', '{closedIncidentsToTransformGuids[24].Resolution}', '2020-04-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[25].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[25].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[26].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[26].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[27].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[27].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[28].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[28].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[29].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[29].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[30].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[30].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[31].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[31].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToTransformGuids[32].PK}', 'CS99991140', 'INC', 'CLS', '{closedIncidentsToTransformGuids[32].Resolution}', '2020-01-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToNotTransformGuids[0]}', 'CS99991141', 'INC', 'CLS', 'UPO', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToNotTransformGuids[1]}', 'CS99991142', 'INC', 'CLS', 'FQP', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToNotTransformGuids[2]}', 'CS99991143', 'INC', 'CLS', 'DEP', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToNotTransformGuids[3]}', 'CS99991144', 'INC', 'CLS', 'UDO', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E'),
('{closedIncidentsToNotTransformGuids[4]}', 'CS99991145', 'INC', 'CLS', 'AUT', '2020-06-01 12:00:00', 'E', '2023-07-25 02:43:44.503', 'E')

INSERT INTO dbo.StmALog(
	[SL_PK],
	[SL_Table],
	[SL_SE_NKEvent],
	[SL_Parent],
	[SL_Reference],
	[SL_EventTime],
	[SL_PostedTimeUtc],
	[SL_GS_NKUser],
	[SL_GB_NKBranch])
VALUES
(NEWID(), 'IncidentMain', 'STC', '{nonClosedIncidentGuids[0]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{nonClosedIncidentGuids[1]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{nonClosedIncidentGuids[2]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{nonClosedIncidentGuids[3]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[0].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-05-03 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[0].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[1].PK}', 'Status - CLS to WRK', '2023-01-01 12:00:00', '2023-05-03 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[1].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'EDT', '{closedIncidentsToTransformGuids[2].PK}', '', '2023-01-01 12:00:00', '2023-07-21 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[2].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[3].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[3].PK}', 'Disposition - AUC to COM', '2023-01-01 12:00:00', '2023-06-02 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[4].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'JCL', '{closedIncidentsToTransformGuids[4].PK}', '', '2023-01-01 12:00:00', '2023-07-02 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'JCL', '{closedIncidentsToTransformGuids[6].PK}', '', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[7].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[8].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[9].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[10].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[11].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[12].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[13].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[14].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[15].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[16].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[17].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[18].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[19].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[20].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[21].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[22].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[23].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[24].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[25].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-07-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[26].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[27].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[28].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[29].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[30].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[31].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToTransformGuids[32].PK}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToNotTransformGuids[0]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToNotTransformGuids[1]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToNotTransformGuids[2]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO'),
(NEWID(), 'IncidentMain', 'STC', '{closedIncidentsToNotTransformGuids[3]}', 'Status - WRK to CLS', '2023-01-01 12:00:00', '2023-06-01 12:00:00', 'E', 'BWO')"
;
			using (var cmd = Db.Connection.Command(dataQuery))
			{
				cmd.ExecuteNonQuery();
			}
		}
	}
}
