using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Telematics;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	[TestedType(typeof(Report_TelPreDriveChecklistSummary))]
	class Report_TelPreDriveChecklistSummaryTest : DbCreateScriptTest
	{
		[UseSnapshotProtection]
		public void TestRetrievedChecklists()
		{
			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				// Arrange
				CreateData(connection);
				CreateChecklists(connection);

				CombineAssertions(() =>
				{
					Test(
						"PDR",
						"2021-03-14",
						"2021-03-15",
						Array.Empty<(string, string, string, string, string, string, string)>());
					Test(
						"PDR",
						"2021-03-15",
						"2021-03-17",
						new[]
						{
							("BOB", "Mr. Bob", "ASD-123", "16/03/2021 12:00:00 AM", "Why is my internet still not working?", "2,3", "=CONCATENATE(\"2: 2\",CHAR(10),\"3: 3\")"),
							("ASD", "Mrs. Bob", "JJL-012", "16/03/2021 12:00:00 AM", "WHY is my internet still not working?", "1,2,3", "=CONCATENATE(\"1: 1\",CHAR(10),\"2: 2\",CHAR(10),\"3: 3\")"),
						});
					Test(
						"FTD",
						"2021-03-15",
						"2021-03-17",
						new[]
						{
							("QWE", "A Terrible Driver", "ACF-111", "16/03/2021 12:00:00 AM", "Tired of working from mobile hotspot....", "1", "=CONCATENATE(\"1: 1\")"),
							("AAA", "An ok driver", "QWE-444", "16/03/2021 12:00:00 AM", "Damned NBN!", "2", "=CONCATENATE(\"2: 2\")"),
						});
				});

				void Test(string type, string startTime, string endTime, IEnumerable<(string, string, string, string, string, string, string)> expectedElements)
				{
					// Act
					var result = DataUtils.GetDataTableFromQuery(connection, $@"
						SELECT
							*
						FROM
							dbo.Report_TelPreDriveChecklistSummary('{startTime}', '{endTime}', '{type}')
						")
						.Rows.Cast<DataRow>()
						.Select(row => (row["DriverCode"].ToString(), row["DriverName"].ToString(), row["VehicleRegistration"].ToString(), row["ReportCompletiontime"].ToString(), row["ReportNotes"].ToString(), row["ReportCodes"].ToString(), row["ReportFailures"].ToString()))
						.ToArray();

					// Assert
					AssertContainsExactElementsInAnyOrder(expectedElements.ToArray(), result);
				}
			}
		}

		void CreateChecklists(DbConnection dbConnection)
		{
			using (var command = dbConnection.Command(FormattableString.Invariant($@"
INSERT INTO dbo.{TelPreDriveChecklistHeaderSchema.Constants.TableName}({TelPreDriveChecklistHeaderSchema.Constants.PK}, {TelPreDriveChecklistHeaderSchema.Constants.TPH_GS_NKDriver}, {TelPreDriveChecklistHeaderSchema.Constants.TPH_ChecklistCreateTimeUtc}, {TelPreDriveChecklistHeaderSchema.Constants.TPH_SystemCreateUser}, {TelPreDriveChecklistHeaderSchema.Constants.TPH_SystemCreateTimeUtc}, {TelPreDriveChecklistHeaderSchema.Constants.TPH_Type}, {TelPreDriveChecklistHeaderSchema.Constants.TPH_Notes})
VALUES
('{Headers[0]}', '{Drivers[0].code}', '2021-03-16', '{Drivers[0].code}', '2021-03-16', 'PDR', 'Why is my internet still not working?'),
('{Headers[1]}', '{Drivers[1].code}', '2021-03-16', '{Drivers[1].code}', '2021-03-16', 'PDR', 'WHY is my internet still not working?'),
('{Headers[2]}', '{Drivers[2].code}', '2021-03-16', '{Drivers[2].code}', '2021-03-16', 'FTD', 'Tired of working from mobile hotspot....'),
('{Headers[3]}', '{Drivers[3].code}', '2021-03-16', '{Drivers[3].code}', '2021-03-16', 'FTD', 'Damned NBN!')
;

INSERT INTO dbo.{TelPreDriveChecklistEntrySchema.Constants.TableName}({TelPreDriveChecklistEntrySchema.Constants.PK}, {TelPreDriveChecklistEntrySchema.Constants.TPE_TPH_ChecklistHeader}, {TelPreDriveChecklistEntrySchema.Constants.TPE_Index}, {TelPreDriveChecklistEntrySchema.Constants.TPE_Description}, {TelPreDriveChecklistEntrySchema.Constants.TPE_IsAgreed})
VALUES
(NEWID(), '{Headers[0]}', 1, '1', 'Y'),
(NEWID(), '{Headers[0]}', 2, '2', 'N'),
(NEWID(), '{Headers[0]}', 3, '3', ''),
(NEWID(), '{Headers[1]}', 1, '1', 'N'),
(NEWID(), '{Headers[1]}', 2, '2', 'N'),
(NEWID(), '{Headers[1]}', 3, '3', ''),
(NEWID(), '{Headers[2]}', 1, '1', ''),
(NEWID(), '{Headers[2]}', 2, '2', 'Y'),
(NEWID(), '{Headers[2]}', 3, '3', 'Y'),
(NEWID(), '{Headers[3]}', 1, '1', 'Y'),
(NEWID(), '{Headers[3]}', 2, '2', 'N'),
(NEWID(), '{Headers[3]}', 3, '3', 'Y')
")))
			{
				command.ExecuteNonQuery();
			}
		}

		void CreateData(DbConnection dbConnection)
		{
			using (var command = dbConnection.Command(FormattableString.Invariant($@"
INSERT INTO dbo.GlbDevice({GlbDeviceSchema.Constants.PK}, {GlbDeviceSchema.Constants.V3_Model}, {GlbDeviceSchema.Constants.V3_SystemCreateTimeUtc}, {GlbDeviceSchema.Constants.V3_SystemCreateUser}, {GlbDeviceSchema.Constants.V3_IsActive}, {GlbDeviceSchema.Constants.V3_MobileServicesIdentifier}, {GlbDeviceSchema.Constants.V3_HumanReadableIdentifier}, {GlbDeviceSchema.Constants.V3_SystemLastEditTimeUtc}, {GlbDeviceSchema.Constants.V3_SystemLastEditUser})
VALUES
('{Devices[0]}', 'Model1', '2016-03-16', 'U', 1, 0x1, '1', GetUtcDate(), 'U'),
('{Devices[1]}', 'Model1', '2016-03-16', 'U', 1, 0x2, '2', GetUtcDate(), 'U'),
('{Devices[2]}', 'Model1', '2016-03-16', 'U', 1, 0x3, '3', GetUtcDate(), 'U'),
('{Devices[3]}', 'Model1', '2016-03-16', 'U', 1, 0x4, '4', GetUtcDate(), 'U'),
('{Devices[4]}', 'Model1', '2016-03-16', 'U', 1, 0x5, '5', GetUtcDate(), 'U')
;

INSERT INTO dbo.{RefEquipmentSchema.Constants.TableName}({RefEquipmentSchema.Constants.PK}, {RefEquipmentSchema.Constants.RQ_Description}, {RefEquipmentSchema.Constants.RQ_ShortCode}, {RefEquipmentSchema.Constants.RQ_Registration})
VALUES
('{RefEquipments[0]}', 'Truck', 'ASD-123', 'ASD-123'),
('{RefEquipments[1]}', 'Truck', 'JJL-012', 'JJL-012'),
('{RefEquipments[2]}', 'Truck', 'ACF-111', 'ACF-111'),
('{RefEquipments[3]}', 'Truck', 'QWE-444', 'QWE-444'),
('{RefEquipments[4]}', 'Truck', 'DSA-987', 'DSA-987')

INSERT INTO dbo.GlbDeviceAssignmentDivot ({GlbDeviceAssignmentDivotSchema.Constants.PK}, {GlbDeviceAssignmentDivotSchema.Constants.V7_ParentTableCode}, {GlbDeviceAssignmentDivotSchema.Constants.V7_ParentID}, {GlbDeviceAssignmentDivotSchema.Constants.V7_V3_Device}, {GlbDeviceAssignmentDivotSchema.Constants.V7_StartTimeUtc}, {GlbDeviceAssignmentDivotSchema.Constants.V7_EndTimeUtc})
VALUES
(NEWID(), 'RQ', '{Entities[0]}', '{Devices[4]}', '2016-01-01', '2017-01-01'),
(NEWID(), 'RQ', '{Entities[0]}', '{Devices[0]}', '2017-01-01', NULL),

(NEWID(), 'RQ', '{Entities[1]}', '{Devices[1]}', '2017-02-01', NULL),

(NEWID(), 'RQ', '{Entities[2]}', '{Devices[2]}', '2017-01-01', '2017-03-01'),
(NEWID(), 'RQ', '{Entities[2]}', '{Devices[3]}', '2017-03-01', NULL),

(NEWID(), 'RQ', '{Entities[3]}', '{Devices[0]}', '2016-01-01', '2017-01-01'),
(NEWID(), 'RQ', '{Entities[3]}', '{Devices[1]}', '2017-01-01', '2017-02-01'),
(NEWID(), 'RQ', '{Entities[3]}', '{Devices[3]}', '2017-02-01', '2017-02-10'),
(NEWID(), 'RQ', '{Entities[3]}', '{Devices[4]}', '2017-02-10', '2017-03-01')
;

INSERT INTO dbo.GlbStaff ({GlbStaffSchema.Constants.PK}, {GlbStaffSchema.Constants.GS_LoginName}, {GlbStaffSchema.Constants.GS_Code}, {GlbStaffSchema.Constants.GS_FullName}, {GlbStaffSchema.Constants.GS_SystemCreateTimeUtc}, {GlbStaffSchema.Constants.GS_SystemCreateUser}, {GlbStaffSchema.Constants.GS_SystemLastEditTimeUtc}, {GlbStaffSchema.Constants.GS_SystemLastEditUser})
VALUES
('{Drivers[0].pk}', '{Drivers[0].code}', '{Drivers[0].code}', '{Drivers[0].fullname}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Drivers[1].pk}', '{Drivers[1].code}', '{Drivers[1].code}', '{Drivers[1].fullname}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Drivers[2].pk}', '{Drivers[2].code}', '{Drivers[2].code}', '{Drivers[2].fullname}', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{Drivers[3].pk}', '{Drivers[3].code}', '{Drivers[3].code}', '{Drivers[3].fullname}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
;

INSERT INTO dbo.TelEdge ({TelEdgeSchema.Constants.PK}, {TelEdgeSchema.Constants.TE_EntityTableCodeFrom}, {TelEdgeSchema.Constants.TE_EntityIdFrom}, {TelEdgeSchema.Constants.TE_EntityTableCodeTo}, {TelEdgeSchema.Constants.TE_EntityIdTo}, {TelEdgeSchema.Constants.TE_RelationshipType}, {TelEdgeSchema.Constants.TE_StartTime})
VALUES
(NEWID(), 'RQ', '{RefEquipments[0]}', 'GS', '{Drivers[0].pk}', 'OPT', '2016-01-01'),
(NEWID(), 'RQ', '{RefEquipments[1]}', 'GS', '{Drivers[1].pk}', 'OPT', '2016-01-01'),
(NEWID(), 'RQ', '{RefEquipments[2]}', 'GS', '{Drivers[2].pk}', 'OPT', '2016-01-01'),
(NEWID(), 'RQ', '{RefEquipments[3]}', 'GS', '{Drivers[3].pk}', 'OPT', '2016-01-01')
")))
			{
				command.ExecuteNonQuery();
			}
		}

		protected readonly Guid[] Devices =
		{
			Guid.Parse("00000000-0000-0001-0000-000000000001"),
			Guid.Parse("00000000-0000-0002-0000-000000000002"),
			Guid.Parse("00000000-0000-0003-0000-000000000003"),
			Guid.Parse("00000000-0000-0004-0000-000000000004"),
			Guid.Parse("00000000-0000-0005-0000-000000000005")
		};

		protected readonly Guid[] RefEquipments =
		{
			Guid.Parse("00000000-0000-0001-0000-000000000001"),
			Guid.Parse("00000000-0000-0002-0000-000000000002"),
			Guid.Parse("00000000-0000-0003-0000-000000000003"),
			Guid.Parse("00000000-0000-0004-0000-000000000004"),
			Guid.Parse("00000000-0000-0005-0000-000000000005")
		};

		protected readonly Guid[] Entities =
		{
			Guid.Parse("00000000-0001-0000-0000-000000000000"),
			Guid.Parse("00000000-0002-0000-0000-000000000000"),
			Guid.Parse("00000000-0003-0000-0000-000000000000"),
			Guid.Parse("00000000-0004-0000-0000-000000000000")
		};

		protected readonly (Guid pk, string code, string fullname)[] Drivers =
		{
			(Guid.Parse("00000000-0000-0001-0000-000000000000"), "BOB", "Mr. Bob"),
			(Guid.Parse("00000000-0000-0002-0000-000000000000"), "ASD", "Mrs. Bob"),
			(Guid.Parse("00000000-0000-0003-0000-000000000000"), "QWE", "A Terrible Driver"),
			(Guid.Parse("00000000-0000-0004-0000-000000000000"), "AAA", "An ok driver"),
		};

		protected readonly Guid[] Headers =
		{
			Guid.Parse("00000000-0001-0000-0000-000000000000"),
			Guid.Parse("00000000-0002-0000-0000-000000000000"),
			Guid.Parse("00000000-0003-0000-0000-000000000000"),
			Guid.Parse("00000000-0004-0000-0000-000000000000")
		};
	}
}
