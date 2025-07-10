using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	abstract class TelDeviceTestCase : DbCreateScriptTest
	{
		protected abstract string ArrangeQuery();

		protected void CreateData(DbConnection dbConnection = null)
		{
			dbConnection = dbConnection ?? Db.Connection;
			using (var command = dbConnection.Command(FormattableString.Invariant($@"
INSERT INTO dbo.GlbDevice (V3_PK, V3_Model, V3_SystemCreateTimeUtc, V3_SystemCreateUser, V3_IsActive, V3_MobileServicesIdentifier, V3_HumanReadableIdentifier, V3_HardwareKind, V3_SystemLastEditTimeUtc, V3_SystemLastEditUser)
VALUES
('{Devices[0]}', 'Model1', '2016-03-16', 'U', 1, 0x1, '1', 'AND', GetUtcDate(), 'U'),
('{Devices[1]}', 'Model1', '2016-03-16', 'U', 1, 0x2, '2', 'UNK', GetUtcDate(), 'U'),
('{Devices[2]}', 'Model1', '2016-03-16', 'U', 1, 0x3, '3', 'EMB', GetUtcDate(), 'U'),
('{Devices[3]}', 'Model1', '2016-03-16', 'U', 1, 0x4, '4', 'EMB', GetUtcDate(), 'U'),
('{Devices[4]}', 'Model1', '2016-03-16', 'U', 1, 0x5, '5', 'IOS', GetUtcDate(), 'U')
;

INSERT INTO dbo.GlbDeviceAssignmentDivot (V7_PK, V7_ParentTableCode, V7_ParentID, V7_V3_Device, V7_StartTimeUtc, V7_EndTimeUtc, V7_SystemCreateTimeUtc, V7_SystemCreateUser, V7_SystemLastEditTimeUtc, V7_SystemLastEditUser)
VALUES
(NEWID(), 'RQ', '{Entities[0]}', '{Devices[4]}', '2016-01-01', '2017-01-01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'RQ', '{Entities[0]}', '{Devices[0]}', '2017-01-01', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

(NEWID(), 'RQ', '{Entities[1]}', '{Devices[1]}', '2017-02-01', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

(NEWID(), 'RQ', '{Entities[2]}', '{Devices[2]}', '2017-01-01', '2017-03-01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'RQ', '{Entities[2]}', '{Devices[3]}', '2017-03-01', NULL, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

(NEWID(), 'RQ', '{Entities[3]}', '{Devices[0]}', '2016-01-01', '2017-01-01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'RQ', '{Entities[3]}', '{Devices[1]}', '2017-01-01', '2017-02-01', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'RQ', '{Entities[3]}', '{Devices[3]}', '2017-02-01', '2017-02-10', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
(NEWID(), 'RQ', '{Entities[3]}', '{Devices[4]}', '2017-02-10', '2017-03-01', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
;

INSERT INTO dbo.{TelSubEquipmentSchema.Constants.TableName} ({TelSubEquipmentSchema.Constants.PK}, {TelSubEquipmentSchema.Constants.TSE_Id}, {TelSubEquipmentSchema.Constants.TSE_Configuration}, {TelSubEquipmentSchema.Constants.TSE_Name}, {TelSubEquipmentSchema.Constants.TSE_Type}, {TelSubEquipmentSchema.Constants.TSE_SystemCreateTimeUtc}, {TelSubEquipmentSchema.Constants.TSE_SystemCreateUser}, {TelSubEquipmentSchema.Constants.TSE_SystemLastEditTimeUtc}, {TelSubEquipmentSchema.Constants.TSE_SystemLastEditUser})
VALUES
('{SubEquipments[0]}', 0x01020304, CONVERT(XML, N'', 2), '111', 'O', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{SubEquipments[1]}', 0x04030201, CONVERT(XML, N'', 2), '222', 'O', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{SubEquipments[2]}', 0xFFFFFFFF, CONVERT(XML, N'', 2), '333', 'O', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{SubEquipments[3]}', 0x11111111, CONVERT(XML, N'', 2), '444', 'O', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('{SubEquipments[4]}', 0x12345678, CONVERT(XML, N'', 2), '555', 'O', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
")))
			{
				command.ExecuteNonQuery();
			}

			using (var command = dbConnection.Command(ArrangeQuery()))
			{
				command.ExecuteNonQuery();
			}
		}

		protected readonly Guid[] Devices =
		{
			Guid.Parse("00000000-0000-0001-0000-000000000000"),
			Guid.Parse("00000000-0000-0002-0000-000000000000"),
			Guid.Parse("00000000-0000-0003-0000-000000000000"),
			Guid.Parse("00000000-0000-0004-0000-000000000000"),
			Guid.Parse("00000000-0000-0005-0000-000000000000")
		};

		protected readonly Guid[] SubEquipments =
		{
			Guid.Parse("00000000-0000-0001-0000-000000000000"),
			Guid.Parse("00000000-0000-0002-0000-000000000000"),
			Guid.Parse("00000000-0000-0003-0000-000000000000"),
			Guid.Parse("00000000-0000-0004-0000-000000000000"),
			Guid.Parse("00000000-0000-0005-0000-000000000000")
		};

		protected readonly Guid[] Entities =
		{
			Guid.Parse("00000000-0001-0000-0000-000000000000"),
			Guid.Parse("00000000-0002-0000-0000-000000000000"),
			Guid.Parse("00000000-0003-0000-0000-000000000000"),
			Guid.Parse("00000000-0004-0000-0000-000000000000")
		};
	}
}
