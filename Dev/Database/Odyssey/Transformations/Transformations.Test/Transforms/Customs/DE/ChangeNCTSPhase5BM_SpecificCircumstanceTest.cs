using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.DE;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.DE
{
	[TestedType(typeof(ChangeNCTSPhase5BM_SpecificCircumstance))]
	sealed class ChangeNCTSPhase5BM_SpecificCircumstanceTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<Guid, DateTime, string, string>>();
			TestConnection.ExecuteReader("SELECT * FROM CusInBondMoveHeader",
				reader => resultList.Add(Tuple.Create((Guid)reader["BM_PK"]
				, (DateTime)reader["BM_SystemLastEditTimeUtc"]
				, (string)reader["BM_SystemLastEditUser"]
				, (string)reader["BM_SpecificCircumstance"])));

			var utcNow = DateTime.UtcNow;
			var cleanedUtcNow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 00);
			var unmodifiedDateTime = new DateTime(2023, 1, 1, 1, 1, 0);

			var modifiedRecords = resultList.Where(x => cleanedUtcNow.AddMinutes(-1) <= x.Item2 && cleanedUtcNow.AddMinutes(+1) >= x.Item2);
			var unmodifiedRecords = resultList.Except(modifiedRecords);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"{cusInBondMoveHeaderFRArrivalPK}, {unmodifiedDateTime}, KCH, 1",
				$"{cusInBondMoveHeaderFRDeparturePK}, {unmodifiedDateTime}, KCH, 1",
				$"{cusInBondMoveHeaderDEDepartureP4PK}, {unmodifiedDateTime}, KCH, 1",
				$"{cusInBondMoveHeaderDEArrivalP4PK}, {unmodifiedDateTime}, KCH, 2",
				$"{cusInBondMoveHeaderDEArrivalPK}, {unmodifiedDateTime}, KCH, 1",
			}, unmodifiedRecords.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item4}").ToArray());

			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"{cusInBondMoveHeaderDEDeparture1PK}, E, A20",
				$"{cusInBondMoveHeaderDEDeparture2PK}, E, XXX",
			}, modifiedRecords.Select(x => $"{x.Item1}, {x.Item3}, {x.Item4}").ToArray());
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ChangeNCTSPhase5BM_SpecificCircumstance();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @CompanyDEPK							UNIQUEIDENTIFIER = '{CompanyDEPK}',
						@CompanyFRPK							UNIQUEIDENTIFIER = '{CompanyFRPK}',
						@BranchDEPK								UNIQUEIDENTIFIER = '{BranchDEPK}',
						@BranchFRPK								UNIQUEIDENTIFIER = '{BranchFRPK}',
						@cusInBondHeaderDEArrivalPK				UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDEDeparturePK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDEArrivalP4PK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderFRArrivalPK				UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderFRDeparturePK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDEDepartureP4PK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderDEArrivalPK			UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDEArrivalPK}',
						@cusInBondMoveHeaderDEDeparture1PK		UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDEDeparture1PK}',
						@cusInBondMoveHeaderDEDeparture2PK		UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDEDeparture2PK}',
						@cusInBondMoveHeaderFRArrivalPK			UNIQUEIDENTIFIER = '{cusInBondMoveHeaderFRArrivalPK}',
						@cusInBondMoveHeaderFRDeparturePK		UNIQUEIDENTIFIER = '{cusInBondMoveHeaderFRDeparturePK}',
						@cusInBondMoveHeaderDEDepartureP4PK		UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDEDepartureP4PK}',
						@cusInBondMoveHeaderDEArrivalP4PK		UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDEArrivalP4PK}'

				DELETE FROM GlbCompany WHERE GC_PK IN (@CompanyDEPK, @CompanyFRPK)
				DELETE FROM GlbBranch WHERE GB_PK IN (@BranchDEPK, @BranchFRPK)
				DELETE FROM CusInBondHeader WHERE BH_PK IN (@cusInBondHeaderDEArrivalPK, @cusInBondHeaderFRArrivalPK, @cusInBondHeaderFRDeparturePK, @cusInBondHeaderDEDeparturePK, @cusInBondHeaderDEArrivalP4PK, @cusInBondHeaderDEDepartureP4PK)
				DELETE FROM CusInBondMoveHeader WHERE BM_PK IN (@cusInBondMoveHeaderDEArrivalPK, @cusInBondMoveHeaderDEDeparture1PK, @cusInBondMoveHeaderDEDeparture2PK, @cusInBondMoveHeaderFRArrivalPK, @cusInBondMoveHeaderFRDeparturePK, @cusInBondMoveHeaderDEDepartureP4PK, @cusInBondMoveHeaderDEArrivalP4PK)

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)	VALUES
					(@CompanyDEPK, 'DE', 'EUR', 'TDE', 'DE company', '2022-07-30T00:00:00', 'E', '2022-07-30T00:00:00', 'E'),
					(@CompanyFRPK, 'FR', 'EUR', 'TFR', 'FR company', '2022-07-30T00:00:00', 'E', '2022-07-30T00:00:00', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES
					(@BranchDEPK, @CompanyDEPK, 'TDD', 'DE', '2022-07-30T00:00:00', 'E', '2022-07-30T00:00:00', 'E'),
					(@BranchFRPK, @CompanyFRPK, 'TFF', 'FR', '2022-07-30T00:00:00', 'E', '2022-07-30T00:00:00', 'E')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@cusInBondHeaderDEArrivalPK,		@BranchDEPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'KCH','KCH'),
					(@cusInBondHeaderFRArrivalPK,		@BranchFRPK, 'BH02', 'NC5', 'A', '2022-12-15', 1, GetUtcDate(), 'KCH','KCH'),
					(@cusInBondHeaderFRDeparturePK,		@BranchFRPK, 'BH02', 'NC5', 'D', '2022-12-15', 1, GetUtcDate(), 'KCH','KCH'),
					(@cusInBondHeaderDEDeparturePK,		@BranchDEPK, 'BH03', 'NC5', 'D', '2023-03-02', 1, GetUtcDate(), 'KCH','KCH'),
					(@cusInBondHeaderDEDepartureP4PK,	@BranchDEPK, 'BH03', 'NCT', 'D', '2023-03-02', 1, GetUtcDate(), 'KCH','KCH'),
					(@cusInBondHeaderDEArrivalP4PK,		@BranchDEPK, 'BH04', 'NCT', 'A', '2023-03-02', 1, GetUtcDate(), 'KCH','KCH')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SystemCreateTimeUtc,	BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate, BM_SpecificCircumstance) VALUES
					(@cusInBondMoveHeaderDEArrivalPK,		@cusInBondHeaderDEArrivalPK,		'2022-07-30', 'KCH', '2023-01-01 01:01:00', 'KCH','2022-07-30','2022-07-30', '1'),
					(@cusInBondMoveHeaderDEDeparture1PK,	@cusInBondHeaderDEDeparturePK,		'2023-07-30', 'KCH', '2023-01-01 01:01:00', 'KCH','2022-07-30','2022-07-30', '1'),
					(@cusInBondMoveHeaderDEDeparture2PK,	@cusInBondHeaderDEDeparturePK,		'2023-07-30', 'KCH', '2023-01-01 01:01:00', 'KCH','2022-07-30','2022-07-30', '2'),
					(@cusInBondMoveHeaderFRArrivalPK,		@cusInBondHeaderFRArrivalPK,		'2022-12-15', 'KCH', '2023-01-01 01:01:00', 'KCH','2022-12-15','2022-12-15', '1'),
					(@cusInBondMoveHeaderFRDeparturePK,		@cusInBondHeaderFRDeparturePK,		'2022-12-15', 'KCH', '2023-01-01 01:01:00', 'KCH','2022-12-15','2022-12-15', '1'),
					(@cusInBondMoveHeaderDEDepartureP4PK,	@cusInBondHeaderDEDepartureP4PK,	'2023-03-02', 'KCH', '2023-01-01 01:01:00', 'KCH','2023-03-02','2023-03-02', '1'),
					(@cusInBondMoveHeaderDEArrivalP4PK,		@cusInBondHeaderDEArrivalP4PK,		'2024-02-01', 'KCH', '2023-01-01 01:01:00', 'KCH','2024-02-01','2024-02-01', '2')";

			TestConnection.ExecuteNonQuery(sql);
		}
		Guid cusInBondMoveHeaderFRArrivalPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderFRDeparturePK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDEArrivalPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDEDeparture1PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDEDeparture2PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDEDepartureP4PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDEArrivalP4PK = Guid.NewGuid();

		Guid CompanyDEPK = Guid.NewGuid();
		Guid CompanyFRPK = Guid.NewGuid();
		Guid BranchDEPK = Guid.NewGuid();
		Guid BranchFRPK = Guid.NewGuid();
	}
}
