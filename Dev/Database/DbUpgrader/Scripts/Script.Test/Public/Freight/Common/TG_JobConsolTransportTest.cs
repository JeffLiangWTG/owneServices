using System;
using System.Globalization;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Common;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	[TestedType(typeof(TG_JobConsolTransport))]
	internal class TG_JobConsolTransportTest : DbCreateScriptTest
	{
		readonly Guid sailingPK = Guid.NewGuid();
		readonly Guid transportPK = Guid.NewGuid();
		const string vesselName = "VESSEL 1";
		const string voyageNo = "Voy1";
		const bool isChartered = true;
		const bool isCargoOnly = true;
		const string aircraftType = "XXX";
		const string origin = "AUMEL";
		Guid departureCTOAddress;
		const string destination = "SGSIN";
		Guid arrivalCTOAddress;
		static readonly DateTime today = DateTime.Today;
		readonly DateTime etd = today.AddDays(25);
		readonly DateTime eta = today.AddDays(26);
		const string onlineScheduleStatus = "MTD";
		readonly DateTime depotReceivalCommences = today.AddDays(1);
		readonly DateTime depotCutOff = today.AddDays(2);
		readonly DateTime depotAvailabilityDate = today.AddDays(3);
		readonly DateTime depotStorageDate = today.AddDays(4);
		readonly DateTime documentaryCutOff = today.AddDays(5);
		readonly DateTime vgmCutOff = today.AddDays(6);
		readonly DateTime terminalReceivalCommences = today.AddDays(7);
		readonly DateTime terminalCutOff = today.AddDays(8);
		readonly DateTime terminalAvailabilityDate = today.AddDays(9);
		readonly DateTime terminalStorageDate = today.AddDays(10);
		readonly DateTime emptyReceivalCommences = today.AddDays(11);
		readonly DateTime emptyCutOff = today.AddDays(12);
		readonly DateTime reeferReceivalCommences = today.AddDays(13);
		readonly DateTime reeferCutOff = today.AddDays(14);
		readonly DateTime dGReceivalCommences = today.AddDays(15);
		readonly DateTime dGCutOff = today.AddDays(16);
		const string serviceString = "myServiceString";

		public void TestJobConsolTransportTrigger_Insert()
		{
			CreateSailingSchedule();
			var sql = @"
				declare @consolPK uniqueidentifier = NEWID();

				INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser)
				VALUES (@consolPK, 'C11000088', 1, GetUtcDate(), '~BP')

				INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_IsLinked, JW_JX, JW_SystemLastEditTimeUtc, JW_SystemLastEditUser)
				VALUES ('{0}', @consolPK, 1, '{1}', GetUtcDate(), '~BP')";

			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sql, transportPK, sailingPK));
			AssertResult();
		}

		public void TestJobConsolTransportTrigger_Update()
		{
			CreateSailingSchedule();

			var sql = @"
				declare @consolPK uniqueidentifier = NEWID();

				INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_IsForwarding, JK_SystemLastEditTimeUtc, JK_SystemLastEditUser)
				VALUES (@consolPK, 'C11000088', 1, GetUtcDate(), '~BP')

				INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_IsLinked, JW_JX, JW_SystemLastEditTimeUtc, JW_SystemLastEditUser)
				VALUES ('{0}', @consolPK, 1, '{1}', GetUtcDate(), '~BP')";

			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sql, transportPK, sailingPK));

			var updateSql = @"
				UPDATE dbo.JobConsolTransport
				SET
					JW_JX = '{0}',
					JW_SystemLastEditTimeUtc = GETUTCDATE(),
					JW_SystemLastEditUser = '~BP'
				WHERE
					JW_PK = '{1}'";

			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, updateSql, sailingPK, transportPK));
			AssertResult();

			updateSql = @"
				UPDATE dbo.JobConsolTransport
				SET
					JW_JX = null,
					JW_SystemLastEditTimeUtc = GETUTCDATE(),
					JW_SystemLastEditUser = '~BP'
				WHERE
					JW_PK = '{1}'";

			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, updateSql, sailingPK, transportPK));
			AssertResult();
		}

		protected override void SetUp()
		{
			base.SetUp();
			departureCTOAddress = CreateOrgHeaderAndAddress("DEPCTO1", "Departure CTO 1", "Departure CTO Address 1");
			arrivalCTOAddress = CreateOrgHeaderAndAddress("ARVCTO1", "Arrival CTO 1", "Arrival CTO Address 1");
		}

		#region Create

		void CreateSailingSchedule()
		{
			var sqlCreateSailing = $@"
				declare @VoyagePK uniqueidentifier
				declare @OriginPK uniqueidentifier
				declare @DestinationPK uniqueidentifier

				set @VoyagePK = NEWID()
				set @OriginPK = NEWID()
				set @DestinationPK = NEWID()

				insert into dbo.JobVoyage(JV_PK, JV_RV_NKVessel, JV_VoyageFlight, JV_IsChartered, JV_AircraftType, JV_IsCargoOnly)
				values(@VoyagePK, '{vesselName}', '{voyageNo}', {isChartered.ToSqlFormat()}, '{aircraftType}', {isCargoOnly.ToSqlFormat()})

				insert into dbo.JobVoyOrigin(
					JA_PK, JA_JV, JA_RL_NKPortOfLoading, JA_OA_DepartureCTOAddress,
					JA_S_DEP, JA_E_DEP, JA_A_DEP, JA_DocumentaryCutoff,
					JA_VGMCutOff, JA_ReceivalCommences, JA_CutOff, JA_EmptyReceivalCommences,
					JA_EmptyCutOff, JA_ReeferReceivalCommences, JA_ReeferCutOff, JA_DGReceivalCommences,
					JA_DGCutOff)
				values(
					@OriginPK, @VoyagePK, '{origin}', '{departureCTOAddress}',
					'{etd.ToSqlFormat()}', '{etd.ToSqlFormat()}', '{etd.ToSqlFormat()}', '{documentaryCutOff.ToSqlFormat()}',
					'{vgmCutOff.ToSqlFormat()}', '{terminalReceivalCommences.ToSqlFormat()}', '{terminalCutOff.ToSqlFormat()}', '{emptyReceivalCommences.ToSqlFormat()}',
					'{emptyCutOff.ToSqlFormat()}', '{reeferReceivalCommences.ToSqlFormat()}', '{reeferCutOff.ToSqlFormat()}', '{dGReceivalCommences.ToSqlFormat()}',
					'{dGCutOff.ToSqlFormat()}')

				insert into dbo.JobVoyDestination(
					JB_PK, JB_JV, JB_RL_NKPortOfDischarge, JB_OA_ArrivalCTOAddress,
					JB_S_ARV, JB_E_ARV, JB_A_ARV, JB_AvailabilityDate,
					JB_StorageDate)
				values(
					@DestinationPK, @VoyagePK, '{destination}', '{arrivalCTOAddress}',
					'{eta.ToSqlFormat()}', '{eta.ToSqlFormat()}', '{eta.ToSqlFormat()}', '{terminalAvailabilityDate.ToSqlFormat()}',
					'{terminalStorageDate.ToSqlFormat()}')

				insert into dbo.JobSailing(
					JX_PK, JX_JA, JX_JB, JX_OnlineScheduleStatus,
					JX_DepotReceivalCommences, JX_DepotCutOff, JX_DepotAvailabilityDate, JX_DepotStorageDate,
					JX_ServiceString, JX_SystemLastEditTimeUtc, JX_SystemLastEditUser)
				values(
					'{sailingPK}', @OriginPK, @DestinationPK, '{onlineScheduleStatus}',
					'{depotReceivalCommences.ToSqlFormat()}', '{depotCutOff.ToSqlFormat()}', '{depotAvailabilityDate.ToSqlFormat()}','{depotStorageDate.ToSqlFormat()}',
					'{serviceString}', GetUtcDate(), '~BP')";

			TestConnection.ExecuteNonQuery(sqlCreateSailing);
		}

		Guid CreateOrgHeaderAndAddress(string orgCode, string orgName, string address1)
		{
			var orgHeaderPk = Guid.NewGuid();
			var addressPK = Guid.NewGuid();

			var sql = @"
				INSERT INTO dbo.OrgHeader(OH_PK, OH_Code, OH_FullName)
				VALUES('{0}', '{1}', '{2}')

				INSERT INTO dbo.OrgAddress(OA_PK, OA_OH, OA_Address1)
				VALUES('{3}', '{0}', '{4}')";

			TestConnection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, sql, orgHeaderPk, orgCode, orgName, addressPK, address1));

			return addressPK;
		}

		#endregion

		#region Assert

		void AssertResult()
		{
			AssertDbColumnEquals("JW_Vessel", vesselName);
			AssertDbColumnEquals("JW_VoyageFlight", voyageNo);
			AssertDbColumnEquals("JW_IsCharter", isChartered);
			AssertDbColumnEquals("JW_IsCargoOnly", isCargoOnly);

			AssertDbColumnEquals("JW_AircraftType", aircraftType);

			AssertDbColumnEquals("JW_RL_NKLoadPort", origin);
			AssertDbColumnEquals("JW_OA_DepartureLocation", departureCTOAddress);
			AssertDbColumnEquals("JW_STD", etd);
			AssertDbColumnEquals("JW_ETD", etd);
			AssertDbColumnEquals("JW_ATD", etd);

			AssertDbColumnEquals("JW_RL_NKDiscPort", destination);
			AssertDbColumnEquals("JW_OA_ArrivalLocation", arrivalCTOAddress);
			AssertDbColumnEquals("JW_STA", eta);
			AssertDbColumnEquals("JW_ETA", eta);
			AssertDbColumnEquals("JW_ATA", eta);
			AssertDbColumnEquals("JW_OnlineScheduleStatus", onlineScheduleStatus);

			AssertDbColumnEquals("JW_DocumentaryCutOff", documentaryCutOff);
			AssertDbColumnEquals("JW_VGMCutOff", vgmCutOff);
			AssertDbColumnEquals("JW_TerminalReceivalCommences", terminalReceivalCommences);
			AssertDbColumnEquals("JW_TerminalCutOff", terminalCutOff);

			AssertDbColumnEquals("JW_EmptyReceivalCommences", emptyReceivalCommences);
			AssertDbColumnEquals("JW_EmptyCutOff", emptyCutOff);
			AssertDbColumnEquals("JW_ReeferReceivalCommences", reeferReceivalCommences);
			AssertDbColumnEquals("JW_ReeferCutOff", reeferCutOff);
			AssertDbColumnEquals("JW_DGReceivalCommences", dGReceivalCommences);
			AssertDbColumnEquals("JW_DGCutOff", dGCutOff);

			AssertDbColumnEquals("JW_TerminalAvailabilityDate", terminalAvailabilityDate);
			AssertDbColumnEquals("JW_TerminalStorageDate", terminalStorageDate);

			AssertDbColumnEquals("JW_DepotReceivalCommences", depotReceivalCommences);
			AssertDbColumnEquals("JW_DepotCutOff", depotCutOff);
			AssertDbColumnEquals("JW_DepotAvailabilityDate", depotAvailabilityDate);
			AssertDbColumnEquals("JW_DepotStorageDate", depotStorageDate);
			AssertDbColumnEquals("JW_ServiceString", serviceString);
		}

		void AssertDbColumnEquals<T>(string dbColumn, T expectedValue)
		{
			var selectSql = @"SELECT {0} FROM dbo.JobConsolTransport WHERE JW_PK = '{1}'";
			AssertEquals(dbColumn, expectedValue, (T)TestConnection.ExecuteScalar(string.Format(selectSql, dbColumn, transportPK)));
		}

		#endregion
	}
}
