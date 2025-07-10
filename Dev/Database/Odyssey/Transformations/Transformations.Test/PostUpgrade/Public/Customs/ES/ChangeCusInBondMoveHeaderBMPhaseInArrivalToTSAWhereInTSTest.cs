using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.PostUpgrade.Public.Customs.ES;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Customs.ES
{
	[TestedType(typeof(ChangeCusInBondMoveHeaderBMPhaseInArrivalToTSAWhereInTS))]
	class ChangeCusInBondMoveHeaderBMPhaseInArrivalToTSAWhereInTSTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			var resultList = new List<Tuple<Guid, DateTime, string, string>>();
			TestConnection.ExecuteReader("SELECT * FROM CusInBondMoveHeader",
				reader => resultList.Add(Tuple.Create((Guid)reader["BM_PK"]
				, (DateTime)reader["BM_SystemLastEditTimeUtc"]
				, (string)reader["BM_SystemLastEditUser"]
				, (string)reader["BM_Phase"])));

			var utcNow = DateTime.UtcNow;
			var cleanedUtcNow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 00);
			var unmodifiedDateTime = new DateTime(2023, 1, 1, 1, 1, 0);

			var modifiedRecords = resultList.Where(x => cleanedUtcNow.AddMinutes(-1) <= x.Item2 && cleanedUtcNow.AddMinutes(+1) >= x.Item2);
			var unmodifiedRecords = resultList.Except(modifiedRecords);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"{cusInBondMoveHeaderESArrivalWithSumAndNumEqualPKWithTSA}, {unmodifiedDateTime}, XCM, TSA",
				$"{cusInBondMoveHeaderFRArrivalPK}, {unmodifiedDateTime}, XCM, 044",
				$"{cusInBondMoveHeaderFRDeparturePK}, {unmodifiedDateTime}, XCM, 044",
				$"{cusInBondMoveHeaderESDepartureP4PK}, {unmodifiedDateTime}, XCM, 044",
				$"{cusInBondMoveHeaderESArrivalP4PK}, {unmodifiedDateTime}, XCM, 044",
				$"{cusInBondMoveHeaderESArrivalWithSumAndNumDiffPK}, {unmodifiedDateTime}, XCM, 044",
				$"{cusInBondMoveHeaderESArrivalWithOutSumPK}, {unmodifiedDateTime}, XCM, 044",
			}, unmodifiedRecords.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item4}").ToArray());

			AssertContainsExactElementsInAnyOrder(new[]
			{
				$"{cusInBondMoveHeaderESArrivalWithSumAndNumEqualPK}, E, TSA",
			}, modifiedRecords.Select(x => $"{x.Item1}, {x.Item3}, {x.Item4}").ToArray());

			var indexCountQuery = @"SELECT COUNT(Name)
				FROM sys.indexes 
				WHERE name='IX_UpdateBM_CustomsStatusBM_PhaseNCTSPhase5Arrival_BH_ApplicationCode_BH_HeaderType' AND object_id = OBJECT_ID('dbo.CusInbondHeader')";

			var indexCount = Db.Connection.ExecuteScalar(indexCountQuery);
			AssertEquals("Index should be deleted once transformation completes", 0, indexCount);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new ChangeCusInBondMoveHeaderBMPhaseInArrivalToTSAWhereInTS();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @CompanyESPK												UNIQUEIDENTIFIER = '{CompanyESPK}',
						@CompanyFRPK												UNIQUEIDENTIFIER = '{CompanyFRPK}',
						@BranchESPK													UNIQUEIDENTIFIER = '{BranchESPK}',
						@BranchFRPK													UNIQUEIDENTIFIER = '{BranchFRPK}',
						@cusInBondHeaderESArrivalPK1								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderESArrivalPK2								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderESArrivalPK4								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderESArrivalPK3								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderESDeparturePK								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderESArrivalP4PK								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderFRArrivalPK									UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderFRDeparturePK								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderESDepartureP4PK								UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderESArrivalWithOutSumPK					UNIQUEIDENTIFIER = '{cusInBondMoveHeaderESArrivalWithOutSumPK}',
						@cusInBondMoveHeaderESArrivalWithSumAndNumEqualPK			UNIQUEIDENTIFIER = '{cusInBondMoveHeaderESArrivalWithSumAndNumEqualPK}',
						@cusInBondMoveHeaderESArrivalWithSumAndNumEqualPKWithTSA	UNIQUEIDENTIFIER = '{cusInBondMoveHeaderESArrivalWithSumAndNumEqualPKWithTSA}',
						@cusInBondMoveHeaderESArrivalWithSumAndNumDiffPK			UNIQUEIDENTIFIER = '{cusInBondMoveHeaderESArrivalWithSumAndNumDiffPK}',
						@cusInBondMoveHeaderFRArrivalPK								UNIQUEIDENTIFIER = '{cusInBondMoveHeaderFRArrivalPK}',
						@cusInBondMoveHeaderFRDeparturePK							UNIQUEIDENTIFIER = '{cusInBondMoveHeaderFRDeparturePK}',
						@cusInBondMoveHeaderESDepartureP4PK							UNIQUEIDENTIFIER = '{cusInBondMoveHeaderESDepartureP4PK}',
						@cusInBondMoveHeaderESArrivalP4PK							UNIQUEIDENTIFIER = '{cusInBondMoveHeaderESArrivalP4PK}',
						@CusEntryNumCorrect1										UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumCorrect2										UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumCorrect3										UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumCorrect4										UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumCorrect5										UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumCorrect6										UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumCorrectNumNotSum								UNIQUEIDENTIFIER = NEWID(),
						@CusEntryNumIncorrectNum									UNIQUEIDENTIFIER = NEWID(),
						@CusTempStorageRegHeaderCorrect								UNIQUEIDENTIFIER = NEWID(),
						@CusTempStorageRegHeaderIncorrect							UNIQUEIDENTIFIER = NEWID()

				DELETE FROM CusEntryNum WHERE CE_PK IN (@CusEntryNumCorrect1, @CusEntryNumCorrect2, @CusEntryNumCorrect3, @CusEntryNumCorrect4, @CusEntryNumCorrect5, @CusEntryNumCorrect6, @CusEntryNumCorrectNumNotSum, @CusEntryNumIncorrectNum)
				DELETE FROM CusInBondMoveHeader WHERE BM_PK IN (@cusInBondMoveHeaderESArrivalWithOutSumPK, @cusInBondMoveHeaderESArrivalWithSumAndNumEqualPK, @cusInBondMoveHeaderESArrivalWithSumAndNumEqualPKWithTSA, @cusInBondMoveHeaderESArrivalWithSumAndNumDiffPK, @cusInBondMoveHeaderFRArrivalPK, @cusInBondMoveHeaderFRDeparturePK, @cusInBondMoveHeaderESDepartureP4PK, @cusInBondMoveHeaderESArrivalP4PK)
				DELETE FROM CusTempStorageRegHeader WHERE SRH_PK IN (@CusTempStorageRegHeaderCorrect, @CusTempStorageRegHeaderIncorrect)
				DELETE FROM CusInBondHeader WHERE BH_PK IN (@cusInBondHeaderESArrivalPK1, @cusInBondHeaderESArrivalPK2, @cusInBondHeaderESArrivalPK4, @cusInBondHeaderESArrivalPK3, @cusInBondHeaderFRArrivalPK, @cusInBondHeaderFRDeparturePK, @cusInBondHeaderESDeparturePK, @cusInBondHeaderESArrivalP4PK, @cusInBondHeaderESDepartureP4PK)
				DELETE FROM GlbBranch WHERE GB_PK IN (@BranchESPK, @BranchFRPK)
				DELETE FROM GlbCompany WHERE GC_PK IN (@CompanyESPK, @CompanyFRPK)

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemCreateUser, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser)	VALUES
					(@CompanyESPK, 'ES', 'EUR', 'TES', 'ES company', '2022-07-30T00:00:00', 'XCM', '2022-07-30T00:00:00', 'XCM'),
					(@CompanyFRPK, 'FR', 'EUR', 'TFR', 'FR company', '2022-07-30T00:00:00', 'XCM', '2022-07-30T00:00:00', 'XCM')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_RN_NKCountryCode, GB_SystemCreateTimeUtc, GB_SystemCreateUser, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser) VALUES
					(@BranchESPK, @CompanyESPK, 'TDD', 'ES', '2022-07-30T00:00:00', 'XCM', '2022-07-30T00:00:00', 'XCM'),
					(@BranchFRPK, @CompanyFRPK, 'TFF', 'FR', '2022-07-30T00:00:00', 'XCM', '2022-07-30T00:00:00', 'XCM')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@cusInBondHeaderESArrivalPK1,		@BranchESPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderESArrivalPK2,		@BranchESPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderESArrivalPK3,		@BranchESPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderESArrivalPK4,		@BranchESPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderFRArrivalPK,		@BranchFRPK, 'BH02', 'NC5', 'A', '2022-12-15', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderFRDeparturePK,		@BranchFRPK, 'BH02', 'NC5', 'D', '2022-12-15', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderESDeparturePK,		@BranchESPK, 'BH03', 'NC5', 'D', '2023-03-02', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderESDepartureP4PK,	@BranchESPK, 'BH03', 'NCT', 'D', '2023-03-02', 1, GetUtcDate(), 'XCM','XCM'),
					(@cusInBondHeaderESArrivalP4PK,		@BranchESPK, 'BH04', 'NCT', 'A', '2023-03-02', 1, GetUtcDate(), 'XCM','XCM')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SystemCreateTimeUtc,	BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate, BM_Phase, BM_AutoVersion) VALUES
					(@cusInBondMoveHeaderESArrivalWithOutSumPK,					@cusInBondHeaderESArrivalPK1,		'2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-07-30', '2022-07-30', '044', 1),
					(@cusInBondMoveHeaderESArrivalWithSumAndNumEqualPK,			@cusInBondHeaderESArrivalPK2,		'2023-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-07-30', '2022-07-30', '044', 1),
					(@cusInBondMoveHeaderESArrivalWithSumAndNumDiffPK,			@cusInBondHeaderESArrivalPK3,		'2023-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-07-30', '2022-07-30', '044', 1),
					(@cusInBondMoveHeaderESArrivalWithSumAndNumEqualPKWithTSA,	@cusInBondHeaderESArrivalPK4,		'2023-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-07-30', '2022-07-30', 'TSA', 1),
					(@cusInBondMoveHeaderFRArrivalPK,							@cusInBondHeaderFRArrivalPK,		'2022-12-15', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-12-15', '2022-12-15', '044', 1),
					(@cusInBondMoveHeaderFRDeparturePK,							@cusInBondHeaderFRDeparturePK,		'2022-12-15', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-12-15', '2022-12-15', '044', 1),
					(@cusInBondMoveHeaderESDepartureP4PK,						@cusInBondHeaderESDepartureP4PK,	'2023-03-02', 'XCM', '2023-01-01 01:01:00', 'XCM', '2023-03-02', '2023-03-02', '044', 1),
					(@cusInBondMoveHeaderESArrivalP4PK,							@cusInBondHeaderESArrivalP4PK,		'2024-02-01', 'XCM', '2023-01-01 01:01:00', 'XCM', '2024-02-01', '2024-02-01', '044', 1)

				INSERT INTO CusTempStorageRegHeader (SRH_PK, SRH_Reference, SRH_SystemCreateTimeUtc, SRH_SystemCreateUser, SRH_SystemLastEditTimeUtc, SRH_SystemLastEditUser, SRH_ArrivalDate, SRH_AppCode) VALUES
					(@CusTempStorageRegHeaderCorrect, 'CORRECT', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-07-30', 'SUM'),
					(@CusTempStorageRegHeaderIncorrect, 'AAAAA', '2023-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', '2022-07-30', 'ADT')

				INSERT INTO CusEntryNum (CE_PK, CE_ParentID, CE_ParentTable, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser, CE_EntryType, CE_EntryNum) VALUES
					(@CusEntryNumCorrect1, @cusInBondHeaderESArrivalPK2, 'CusInBondHeader', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'CORRECT'),
					(@CusEntryNumCorrect6, @cusInBondHeaderESArrivalPK4, 'CusInBondHeader', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'CORRECT'),
					(@CusEntryNumCorrect2, @cusInBondHeaderFRArrivalPK, 'CusInBondHeader', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'CORRECT'),
					(@CusEntryNumCorrect3, @cusInBondHeaderFRDeparturePK, 'CusInBondHeader', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'CORRECT'),
					(@CusEntryNumCorrect4, @cusInBondHeaderESDepartureP4PK, 'CusInBondHeader', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'CORRECT'),
					(@CusEntryNumCorrect5, @cusInBondHeaderESArrivalP4PK, 'CusInBondHeader', '2022-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'CORRECT'),
					(@CusEntryNumCorrectNumNotSum, @cusInBondHeaderESArrivalPK1, 'CusInBondHeader', '2023-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'MRN', 'CORRECT'),
					(@CusEntryNumIncorrectNum, @cusInBondHeaderESArrivalPK3, 'CusInBondHeader', '2023-07-30', 'XCM', '2023-01-01 01:01:00', 'XCM', 'SUM', 'INCORRECT')";

			TestConnection.ExecuteNonQuery(sql);
		}

		Guid cusInBondMoveHeaderFRArrivalPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderFRDeparturePK = Guid.NewGuid();
		Guid cusInBondMoveHeaderESArrivalWithOutSumPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderESArrivalWithSumAndNumEqualPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderESArrivalWithSumAndNumEqualPKWithTSA = Guid.NewGuid();
		Guid cusInBondMoveHeaderESArrivalWithSumAndNumDiffPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderESDepartureP4PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderESArrivalP4PK = Guid.NewGuid();

		Guid CompanyESPK = Guid.NewGuid();
		Guid CompanyFRPK = Guid.NewGuid();
		Guid BranchESPK = Guid.NewGuid();
		Guid BranchFRPK = Guid.NewGuid();
	}
}
