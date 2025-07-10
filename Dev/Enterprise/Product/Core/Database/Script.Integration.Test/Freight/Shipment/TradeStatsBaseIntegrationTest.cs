using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment.Testing
{
	class TradeStatsBaseIntegrationTest : TransactionedTestCase
	{
		[TestDate(2019, 2, 17, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestTradeStatsBase_AustralianEasternStandardTime()
		{
			AssertResults(string.Empty, 0, 1, 3);
		}

		[TestDate(2019, 2, 18, 10, 0, 0)]
		[TestUtcOffset(-10, 0, 0)]
		public void TestTradeStatsBase_CookIslandTime()
		{
			AssertResults(string.Empty, 0, 3, 1);
		}

		[TestDate(2019, 2, 17, 14, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestTradeStatsBase_ZoneInfoIsNotRequired()
		{
			AssertResults(null, 0, 1, 3);
		}

		#region Implementation

		void AssertResults(string zoneCode, int expectedFCLCount, int expectedLCLCount, int expectedAirCount)
		{
			AssertEquals("Precondition: Local time is 18/2/2019 00:00:00", new ZDateTime(2019, 2, 18), ZDateTime.Now);

			var dateFrom = ZDateTime.Now.ToDateTime();
			var dateTo = dateFrom.AddDays(2);
			var utcTimeOffset = (int)ZDateTimeOffset.Now.Offset.TotalMinutes;

			using (var command = TestConnection.Command("TradeStatsBase"))
			{
				command.CommandType = CommandType.StoredProcedure;
				command.AddParameter("@CompanyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@BranchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@DateFrom", SqlDbType.SmallDateTime, dateFrom);
				command.AddParameter("@DateTo", SqlDbType.SmallDateTime, dateTo);
				command.AddParameter("@TradeDirection", SqlDbType.Char, string.Empty);
				command.AddParameter("@UseJobRegistrationDate", SqlDbType.Char, "Y");
				command.AddParameter("@CountryCode", SqlDbType.Char, string.Empty);
				command.AddParameter("@ZoneCode", SqlDbType.Char, zoneCode ?? (object)DBNull.Value);
				command.AddParameter("@ConsolPort", SqlDbType.Char, string.Empty);
				command.AddParameter("@UTCTimeOffset", SqlDbType.Int, utcTimeOffset);

				using (var reader = command.ExecuteReader())
				{
					var zoneCodes = new List<object>();
					while (reader.Read())
					{
						AssertEquals("FCL Count", expectedFCLCount, reader["FCLJobCount"]);
						AssertEquals("LCL Count", expectedLCLCount, reader["LCLJobCount"]);
						AssertEquals("AIR Count", expectedAirCount, reader["AIRJobCount"]);
						zoneCodes.Add(reader["ZoneCode"]);
					}

					if (zoneCode == null)
					{
						AssertEquals("When a shipment's origin/destination is linked to multiple zones, and zone info is not required in the report, the shipment should not be duplicated.", 1, zoneCodes.Count);
						AssertEquals(DBNull.Value, zoneCodes[0]);
					}
					else
					{
						AssertCollectionContains("AAAA", zoneCodes);
						AssertCollectionContains("BBBB", zoneCodes);
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var zoneHeaderPk1 = InsertZoneHeader("RPT", "AAAA", "Test Zone 1");
			var zoneHeaderPk2 = InsertZoneHeader("ALL", "BBBB", "Test Zone 2");

			var factory = new BusinessObjectFactory();
			var port = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
			AssertNotNull(port);

			var portPK = port.PK.ToGuid();
			InsertZonePivot(zoneHeaderPk1, portPK);
			InsertZonePivot(zoneHeaderPk2, portPK);

			companyPK = Guid.NewGuid();
			branchPK = Guid.NewGuid();
			InsertCompanyAndBranch(companyPK, branchPK);

			departmentPK = Guid.NewGuid();
			InsertDepartment(departmentPK);

			var shipment1PK = Guid.NewGuid();
			var shipment2PK = Guid.NewGuid();
			var shipment3PK = Guid.NewGuid();
			var shipment4PK = Guid.NewGuid();
			var shipment5PK = Guid.NewGuid();
			var shipment6PK = Guid.NewGuid();
			var shipment7PK = Guid.NewGuid();
			var shipment8PK = Guid.NewGuid();

			InsertShipment(shipment1PK, "S00050001", Constants.TransportModes.Sea, Constants.ContainerModes.FCL, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 17, 12, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment2PK, "S00050002", Constants.TransportModes.Air, Constants.ContainerModes.Loose, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 17, 15, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment3PK, "S00050003", Constants.TransportModes.Air, Constants.ContainerModes.Loose, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 18, 09, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment4PK, "S00050004", Constants.TransportModes.Sea, Constants.ContainerModes.LCL, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 18, 11, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment5PK, "S00050005", Constants.TransportModes.Air, Constants.ContainerModes.Loose, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 19, 13, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment6PK, "S00050006", Constants.TransportModes.Sea, Constants.ContainerModes.LCL, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 19, 15, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment7PK, "S00050007", Constants.TransportModes.Sea, Constants.ContainerModes.LCL, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 20, 09, 0, 0, DateTimeKind.Utc));
			InsertShipment(shipment8PK, "S00050008", Constants.TransportModes.Sea, Constants.ContainerModes.FCL, companyPK, branchPK, departmentPK, new ZDateTime(2019, 2, 20, 11, 0, 0, DateTimeKind.Utc));
		}

		void InsertShipment(Guid pk, string shipmentRef, string transportMode, string packingMode, Guid companyPk, Guid branchPk, Guid departmentPk, ZDateTime jobHeaderSystemCreateTimeUtc)
		{
			using (var sqlCmd = TestConnection.Command(@"INSERT INTO dbo.JobShipment
(JS_PK, JS_UniqueConsignRef, JS_RL_NKOrigin, JS_RL_NKDestination, JS_ShipmentType, JS_TransportMode, JS_PackingMode, JS_SystemCreateTimeUtc, JS_SystemCreateUser, JS_SystemLastEditTimeUtc, JS_SystemLastEditUser)
VALUES (@JS_PK, @JS_UniqueConsignRef, 'AUSYD', 'HKHKG', 'STD', @JS_TransportMode, @JS_PackingMode, GetUtcDate(), '~BP', GetUtcDate(), '~BP')"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@JS_PK", pk, JobShipmentSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@JS_UniqueConsignRef", shipmentRef, JobShipmentSchema.JS_UniqueConsignRef);
				sqlCmd.AddParameterBasedOnDbColumn("@JS_TransportMode", transportMode, JobShipmentSchema.JS_TransportMode);
				sqlCmd.AddParameterBasedOnDbColumn("@JS_PackingMode", packingMode, JobShipmentSchema.JS_PackingMode);
				sqlCmd.ExecuteNonQuery();
			}

			using (var sqlCmd = TestConnection.Command(@"INSERT dbo.JobHeader (JH_PK, JH_GC, JH_GB, JH_GE, JH_JobNum, JH_ParentID, JH_ParentTableCode, JH_Status, JH_SystemCreateTimeUtc, JH_SystemCreateUser, JH_SystemLastEditTimeUtc, JH_SystemLastEditUser) VALUES (newid(), @JH_GC, @JH_GB, @JH_GE, @JH_JobNum, @JH_ParentID, @JH_ParentTableCode, @JH_Status, @JH_SystemCreateTimeUtc, '~BP', GetUtcDate(), '~BP')"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GC", companyPk, JobHeaderSchema.JH_GC);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GB", branchPk, JobHeaderSchema.JH_GB);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_GE", departmentPk, JobHeaderSchema.JH_GE);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_JobNum", shipmentRef, JobHeaderSchema.JH_JobNum);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_ParentID", pk, JobHeaderSchema.JH_ParentID);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_ParentTableCode", "JS", JobHeaderSchema.JH_ParentTableCode);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_Status", "WRK", JobHeaderSchema.JH_GS_NKRepSales);
				sqlCmd.AddParameterBasedOnDbColumn("@JH_SystemCreateTimeUtc", jobHeaderSystemCreateTimeUtc.ToDateTime(), JobHeaderSchema.JH_SystemCreateTimeUtc);
				sqlCmd.ExecuteNonQuery();
			}
		}

		void InsertCompanyAndBranch(Guid companyPk, Guid branchPk)
		{
			using (var sqlCmd = TestConnection.Command(@"INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@GC_PK, 'DAN', 'AU company', 'AU', 'AUD')"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GC_PK", companyPk, GlbCompanySchema.PK);
				sqlCmd.ExecuteNonQuery();
			}

			using (var sqlCmd = TestConnection.Command(@"INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@GB_PK, @GB_GC)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GB_PK", branchPk, GlbBranchSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@GB_GC", companyPk, GlbBranchSchema.GB_GC);
				sqlCmd.ExecuteNonQuery();
			}
		}

		void InsertDepartment(Guid pk)
		{
			using (var sqlCmd = TestConnection.Command(@"INSERT dbo.GlbDepartment (GE_PK) VALUES (@GE_PK)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@GE_PK", pk, GlbDepartmentSchema.PK);
				sqlCmd.ExecuteNonQuery();
			}
		}

		Guid InsertZoneHeader(string zoneType, string zoneCode, string description)
		{
			var pk = Guid.NewGuid();
			using (var sqlCmd = TestConnection.Command($"INSERT dbo.RefZoneHeader (FZ_PK, FZ_ZoneType, FZ_Code, FZ_Description, FZ_SystemCreateTimeUtc, FZ_SystemCreateUser, FZ_SystemLastEditTimeUtc, FZ_SystemLastEditUser) VALUES (@FZ_PK, @FZ_ZoneType, @FZ_Code, @FZ_Description, @FZ_SystemCreateTimeUtc, @FZ_SystemCreateUser, @FZ_SystemLastEditTimeUtc, @FZ_SystemLastEditUser)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_PK", pk, RefZoneHeaderSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_ZoneType", zoneType, RefZoneHeaderSchema.FZ_ZoneType);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_Code", zoneCode, RefZoneHeaderSchema.FZ_Code);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_Description", description, RefZoneHeaderSchema.FZ_Description);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_SystemCreateTimeUtc", DateTime.UtcNow, RefZoneHeaderSchema.FZ_SystemCreateTimeUtc);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_SystemCreateUser", "~BP", RefZoneHeaderSchema.FZ_SystemCreateUser);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_SystemLastEditTimeUtc", DateTime.UtcNow, RefZoneHeaderSchema.FZ_SystemLastEditTimeUtc);
				sqlCmd.AddParameterBasedOnDbColumn("@FZ_SystemLastEditUser", "~BP", RefZoneHeaderSchema.FZ_SystemLastEditUser);
				sqlCmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid InsertZonePivot(Guid zoneHeaderPk, Guid parentID)
		{
			var pk = Guid.NewGuid();
			using (var sqlCmd = TestConnection.Command($"INSERT dbo.RefZonePivot (F2_PK, F2_FZ, F2_ParentID, F2_ParentTableCode, F2_SystemCreateTimeUtc, F2_SystemCreateUser, F2_SystemLastEditTimeUtc, F2_SystemLastEditUser) VALUES (@F2_PK, @F2_FZ, @F2_ParentID, @F2_ParentTableCode, @F2_SystemCreateTimeUtc, @F2_SystemCreateUser, @F2_SystemLastEditTimeUtc, @F2_SystemLastEditUser)"))
			{
				sqlCmd.AddParameterBasedOnDbColumn("@F2_PK", pk, RefZonePivotSchema.PK);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_FZ", zoneHeaderPk, RefZonePivotSchema.F2_FZ);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_ParentID", parentID, RefZonePivotSchema.F2_ParentID);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_ParentTableCode", "RL", RefZonePivotSchema.F2_ParentTableCode);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_SystemCreateTimeUtc", DateTime.UtcNow, RefZonePivotSchema.F2_SystemCreateTimeUtc);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_SystemCreateUser", "~BP", RefZonePivotSchema.F2_SystemCreateUser);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_SystemLastEditTimeUtc", DateTime.UtcNow, RefZonePivotSchema.F2_SystemLastEditTimeUtc);
				sqlCmd.AddParameterBasedOnDbColumn("@F2_SystemLastEditUser", "~BP", RefZonePivotSchema.F2_SystemLastEditUser);
				sqlCmd.ExecuteNonQuery();
			}
			return pk;
		}

		Guid companyPK;
		Guid branchPK;
		Guid departmentPK;
		#endregion
	}
}
