using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Build.Database.Script.Public.Telematics
{
	static class TestDataQueries
	{
		public static string BatteryQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceBattery (GDB_PK, GDB_V3_Device, GDB_MeasurementTimeUtc, GDB_IsCharging, GDB_Voltage, GDB_CurrentA, GDB_TemperatureC, GDB_ChargeRemaining, GDB_SystemCreateTimeUtc, GDB_SystemCreateUser, GDB_SystemLastEditTimeUtc, GDB_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 0, 3, 2, 3, 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string BatteryBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceBattery", "GDB", pksToBlock);
		}

		public static string ExternalVoltageQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceExternalVoltage (GDV_PK, GDV_V3_Device, GDV_MeasurementTimeUtc, GDV_Voltage, GDV_SystemCreateTimeUtc, GDV_SystemCreateUser, GDV_SystemLastEditTimeUtc, GDV_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string ExternalVoltageBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceExternalVoltage", "GDV", pksToBlock);
		}

		public static string IgnitionQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceIgnition (GDI_PK, GDI_V3_Device, GDI_MeasurementTimeUtc, GDI_State, GDI_SystemCreateTimeUtc, GDI_SystemCreateUser, GDI_SystemLastEditTimeUtc, GDI_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string IgnitionBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceIgnition", "GDI", pksToBlock);
		}

		public static string CombinationReportQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceCombinationReport (GDC_PK, GDC_V3_Device, GDC_MeasurementTimeUtc, GDC_CombinationCode, GDC_SystemCreateTimeUtc, GDC_SystemCreateUser, GDC_SystemLastEditTimeUtc, GDC_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 'A12', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string CombinationReportBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceCombinationReport", "GDC", pksToBlock);
		}

		public static string LocationQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceLocation (V2_PK, V2_V3_Device, V2_MeasurementTimeUtc, V2_Location, V2_AccuracyInMetres, V2_Speedkmh, V2_CompassHeadingDegrees, V2_SatelliteQuantity, V2_HDOP, V2_SystemCreateTimeUtc, V2_SystemCreateUser, V2_SystemLastEditTimeUtc, V2_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', geography::STGeomFromText('POINT(0 1 2)', 4326), 1, 2, 3, 4, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string LocationBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceLocation", "V2", pksToBlock);
		}

		public static string LogQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceLog (GDL_PK, GDL_V3_Device, GDL_MeasurementTimeUtc, GDL_MessageString, GDL_SystemCreateTimeUtc, GDL_SystemCreateUser, GDL_SystemLastEditTimeUtc, GDL_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 'Message', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string LogBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceLog", "GDL", pksToBlock);
		}

		public static string OdometerQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceOdometer (GDO_PK, GDO_V3_Device, GDO_MeasurementTimeUtc, GDO_OdometerKM, GDO_SystemCreateTimeUtc, GDO_SystemCreateUser, GDO_SystemLastEditTimeUtc, GDO_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 6, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 7, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 8, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 9, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 10, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 12, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 13, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 14, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 6, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 7, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 8, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 9, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 10, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 12, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 13, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 14, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 6, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 7, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 8, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 9, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 10, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 12, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 13, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 14, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 6, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 7, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 8, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 9, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 10, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 12, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 13, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 14, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 2, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 3, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 4, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 6, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 7, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 8, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 9, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 10, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 12, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 13, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 14, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 15, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string OdometerBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceOdometer", "GDO", pksToBlock);
		}

		public static string OnboardMassQuery(Guid[] devices, Guid[] subEquipments)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceOnboardMass (GDM_PK, GDM_V3_Device, GDM_TSE_SubEquipment, GDM_MeasurementTimeUtc, GDM_PressureKPa, GDM_DeviationPercent, GDM_SystemCreateTimeUtc, GDM_SystemCreateUser, GDM_SystemLastEditTimeUtc, GDM_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '{subEquipments[0]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '{subEquipments[0]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '{subEquipments[0]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '{subEquipments[0]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '{subEquipments[0]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '{subEquipments[0]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '{subEquipments[0]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '{subEquipments[0]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '{subEquipments[0]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '{subEquipments[0]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '{subEquipments[0]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '{subEquipments[0]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '{subEquipments[0]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '{subEquipments[0]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '{subEquipments[0]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '{subEquipments[0]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '{subEquipments[1]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '{subEquipments[1]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '{subEquipments[1]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '{subEquipments[1]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '{subEquipments[1]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '{subEquipments[1]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '{subEquipments[1]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '{subEquipments[1]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '{subEquipments[1]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '{subEquipments[1]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '{subEquipments[1]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '{subEquipments[1]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '{subEquipments[1]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '{subEquipments[1]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '{subEquipments[1]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '{subEquipments[1]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '{subEquipments[2]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '{subEquipments[2]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '{subEquipments[2]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '{subEquipments[2]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '{subEquipments[2]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '{subEquipments[2]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '{subEquipments[2]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '{subEquipments[2]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '{subEquipments[2]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '{subEquipments[2]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '{subEquipments[2]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '{subEquipments[2]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '{subEquipments[2]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '{subEquipments[2]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '{subEquipments[2]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '{subEquipments[2]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '{subEquipments[3]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '{subEquipments[3]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '{subEquipments[3]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '{subEquipments[3]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '{subEquipments[3]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '{subEquipments[3]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '{subEquipments[3]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '{subEquipments[3]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '{subEquipments[3]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '{subEquipments[3]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '{subEquipments[3]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '{subEquipments[3]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '{subEquipments[3]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '{subEquipments[3]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '{subEquipments[3]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '{subEquipments[3]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '{subEquipments[4]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '{subEquipments[4]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '{subEquipments[4]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '{subEquipments[4]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '{subEquipments[4]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '{subEquipments[4]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '{subEquipments[4]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '{subEquipments[4]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '{subEquipments[4]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '{subEquipments[4]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '{subEquipments[4]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '{subEquipments[4]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '{subEquipments[4]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '{subEquipments[4]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '{subEquipments[4]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '{subEquipments[4]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string OnboardMassBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceOnboardMass", "GDM", pksToBlock);
		}

		public static string TemperatureQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceTemperature (GDT_PK, GDT_V3_Device, GDT_MeasurementTimeUtc, GDT_TemperatureC, GDT_SystemCreateTimeUtc, GDT_SystemCreateUser, GDT_SystemLastEditTimeUtc, GDT_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string TemperatureBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceTemperature", "GDT", pksToBlock);
		}

		public static string TyreAlertQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceTyreAlert (GDA_PK, GDA_V3_Device, GDA_MeasurementTimeUtc, GDA_OperationMode, GDA_IsADCOverflow, GDA_IsLowBatteryVoltage, GDA_SpecificHardwareFault, GDA_PressureKPa, GDA_TemperatureC, GDA_SystemCreateTimeUtc, GDA_SystemCreateUser, GDA_SystemLastEditTimeUtc, GDA_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 1, 0, 1, 2, 10, 11, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string TyreAlertBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceTyreAlert", "GDA", pksToBlock);
		}

		public static string TyreReportQuery(Guid[] devices)
		{
			return FormattableString.Invariant($@"
INSERT dbo.GlbDeviceTyreReport (GDR_PK, GDR_V3_Device, GDR_MeasurementTimeUtc, GDR_PressureKPa, GDR_TemperatureC, GDR_SystemCreateTimeUtc, GDR_SystemCreateUser, GDR_SystemLastEditTimeUtc, GDR_SystemLastEditUser)
VALUES
('00000001-0000-0000-0000-000000000000', '{devices[0]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000001', '{devices[0]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000002', '{devices[0]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000003', '{devices[0]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000004', '{devices[0]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000005', '{devices[0]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000006', '{devices[0]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000007', '{devices[0]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000008', '{devices[0]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-000000000009', '{devices[0]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000A', '{devices[0]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000B', '{devices[0]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000C', '{devices[0]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000D', '{devices[0]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000E', '{devices[0]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000001-0000-0000-0000-00000000000F', '{devices[0]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000002-0000-0000-0000-000000000000', '{devices[1]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000001', '{devices[1]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000002', '{devices[1]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000003', '{devices[1]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000004', '{devices[1]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000005', '{devices[1]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000006', '{devices[1]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000007', '{devices[1]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000008', '{devices[1]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-000000000009', '{devices[1]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000A', '{devices[1]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000B', '{devices[1]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000C', '{devices[1]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000D', '{devices[1]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000E', '{devices[1]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000002-0000-0000-0000-00000000000F', '{devices[1]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000003-0000-0000-0000-000000000000', '{devices[2]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000001', '{devices[2]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000002', '{devices[2]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000003', '{devices[2]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000004', '{devices[2]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000005', '{devices[2]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000006', '{devices[2]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000007', '{devices[2]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000008', '{devices[2]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-000000000009', '{devices[2]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000A', '{devices[2]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000B', '{devices[2]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000C', '{devices[2]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000D', '{devices[2]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000E', '{devices[2]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000003-0000-0000-0000-00000000000F', '{devices[2]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000004-0000-0000-0000-000000000000', '{devices[3]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000001', '{devices[3]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000002', '{devices[3]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000003', '{devices[3]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000004', '{devices[3]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000005', '{devices[3]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000006', '{devices[3]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000007', '{devices[3]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000008', '{devices[3]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-000000000009', '{devices[3]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000A', '{devices[3]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000B', '{devices[3]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000C', '{devices[3]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000D', '{devices[3]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000E', '{devices[3]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000004-0000-0000-0000-00000000000F', '{devices[3]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),

('00000005-0000-0000-0000-000000000000', '{devices[4]}', '2016-12-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000001', '{devices[4]}', '2016-12-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000002', '{devices[4]}', '2016-12-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000003', '{devices[4]}', '2016-12-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000004', '{devices[4]}', '2017-01-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000005', '{devices[4]}', '2017-01-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000006', '{devices[4]}', '2017-01-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000007', '{devices[4]}', '2017-01-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000008', '{devices[4]}', '2017-02-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-000000000009', '{devices[4]}', '2017-02-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000A', '{devices[4]}', '2017-02-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000B', '{devices[4]}', '2017-02-28', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000C', '{devices[4]}', '2017-03-01', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000D', '{devices[4]}', '2017-03-10', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000E', '{devices[4]}', '2017-03-20', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
('00000005-0000-0000-0000-00000000000F', '{devices[4]}', '2017-03-30', 10, 5, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
");
		}

		public static string TyreReportBlockQuery(IEnumerable<Guid> pksToBlock)
		{
			return BlockRecordsQuery("GlbDeviceTyreReport", "GDR", pksToBlock);
		}

		public static string BlockRecordsQuery(string table, string tablePrefix, IEnumerable<Guid> devicePksToBlock)
		{
			return string.Join(";\r\n", devicePksToBlock.Select(guid => FormattableString.Invariant($@"
UPDATE dbo.{table} WITH(XLOCK) SET {tablePrefix}_PK = {tablePrefix}_PK, {tablePrefix}_MeasurementTimeUtc = {tablePrefix}_MeasurementTimeUtc, {tablePrefix}_SystemLastEditUser = 'E', {tablePrefix}_SystemLastEditTimeUtc = GetDate() WHERE {tablePrefix}_PK = '{guid}'
")));
		}

		public static string BlockAllRecordsForDeviceQuery(string table, string tablePrefix, IEnumerable<Guid> devicePksToBlock)
		{
			return string.Join(";\r\n", devicePksToBlock.Select(guid => FormattableString.Invariant($@"
UPDATE dbo.{table}
SET {tablePrefix}_PK = {tablePrefix}_PK, {tablePrefix}_MeasurementTimeUtc = {tablePrefix}_MeasurementTimeUtc, {tablePrefix}_SystemLastEditUser = 'E', {tablePrefix}_SystemLastEditTimeUtc = GetDate()
WHERE {tablePrefix}_PK IN
(
	SELECT {tablePrefix}_PK FROM dbo.{table} WHERE {tablePrefix}_V3_Device = '{guid}'
)")));
		}
	}
}
