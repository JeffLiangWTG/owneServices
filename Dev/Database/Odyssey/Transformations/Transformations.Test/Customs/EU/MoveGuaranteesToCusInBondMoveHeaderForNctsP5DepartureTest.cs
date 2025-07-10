using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU
{
	[TestedType(typeof(MoveGuaranteesToCusInBondMoveHeaderForNctsP5Departure))]
	class MoveGuaranteesToCusInBondMoveHeaderForNctsP5DepartureTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				var utcNow = DateTime.UtcNow;
				var cleanedUtcNow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 00);
				var unmodifiedDateTime = new DateTime(2023, 1, 1, 1, 1, 0);

				var createdMovementHeaderResultList = new List<Tuple<Guid, Guid, string, DateTime, string, string>>();
				TestConnection.ExecuteReader("SELECT * FROM CusInBondMoveHeader WHERE BM_TypeOfSecurity = 'NON'",
					reader => createdMovementHeaderResultList.Add(Tuple.Create((Guid)reader["BM_PK"], (Guid)reader["BM_BH"], (string)reader["BM_SubApplicationCode"], (DateTime)reader["BM_SystemCreateTimeUtc"], (string)reader["BM_SystemCreateUser"], (string)reader["BM_SystemLastEditUser"])));
				var singleMovementHeaderResult = createdMovementHeaderResultList
					.Single(x => cleanedUtcNow.AddMinutes(-1) <= x.Item4 && cleanedUtcNow.AddMinutes(+1) >= x.Item4);

				AssertEquals("Missing DepartureMovementHeader created", $"{cusInBondHeaderDepartureWithoutMovementHeaderPK}, D, ~UK, ~UK", $"{singleMovementHeaderResult.Item2}, {singleMovementHeaderResult.Item3}, {singleMovementHeaderResult.Item5}, {singleMovementHeaderResult.Item6}");

				var cusBondDetailResultList = new List<Tuple<Guid, string, Guid, DateTime, string>>();
				TestConnection.ExecuteReader("SELECT * FROM CusBondDetail",
					reader => cusBondDetailResultList.Add(Tuple.Create((Guid)reader["PW_PK"], (string)reader["PW_ParentTableCode"], (Guid)reader["PW_ParentID"], (DateTime)reader["PW_SystemLastEditTimeUtc"], (string)reader["PW_SystemLastEditUser"])));

				var modifiedCusBondDetailRecords = cusBondDetailResultList.Where(x => cleanedUtcNow.AddMinutes(-1) <= x.Item4 && cleanedUtcNow.AddMinutes(+1) >= x.Item4);
				var unmodifiedCusBondDetailRecords = cusBondDetailResultList.Except(modifiedCusBondDetailRecords);
				AssertContainsExactElementsInAnyOrder("parent remains", new[]
				{
					$"{cusBondDetailArrivalP5PK}, BH, {cusInBondHeaderArrivalP5PK}, {unmodifiedDateTime}, KCH",
					$"{cusBondDetailDepartureP4PK}, BH, {cusInBondHeaderDepartureP4PK}, {unmodifiedDateTime}, KCH",
					$"{cusBondDetailNonNctsPK}, JE, {jobDeclarationPK}, {unmodifiedDateTime}, KCH"
				}, unmodifiedCusBondDetailRecords.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item4}, {x.Item5}").ToArray());
				AssertContainsExactElementsInAnyOrder("parent changed", new[]
				{
					$"{cusBondDetailDepartureP5PK}, BM, {cusInBondMoveHeaderDepartureP5OlderPK}, E",
					$"{cusBondDetailForMissingMovementHeaderPK}, BM, {singleMovementHeaderResult.Item1}, E",
					$"{cusBondDetailForMissingMovementHeaderPK2}, BM, {singleMovementHeaderResult.Item1}, E",
				}, modifiedCusBondDetailRecords.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item5}").ToArray());
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new MoveGuaranteesToCusInBondMoveHeaderForNctsP5Departure();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @companyPK											UNIQUEIDENTIFIER = NEWID(),
						@branchPK											UNIQUEIDENTIFIER = NEWID(),
						@jobDeclarationPK									UNIQUEIDENTIFIER = '{jobDeclarationPK}',	
						@cusInBondHeaderArrivalP5PK							UNIQUEIDENTIFIER = '{cusInBondHeaderArrivalP5PK}',
						@cusInBondHeaderDepartureP5PK						UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDepartureP4PK						UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureP4PK}',
						@cusInBondHeaderDepartureWithoutMovementHeaderPK	UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureWithoutMovementHeaderPK}',
						@cusInBondMoveHeaderArrivalP5PK						UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderDepartureP5OlderPK				UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDepartureP5OlderPK}',
						@cusInBondMoveHeaderDepartureP5NewerPK				UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderDepartureP4PK					UNIQUEIDENTIFIER = NEWID(),
						@cusBondDetailArrivalP5PK							UNIQUEIDENTIFIER = '{cusBondDetailArrivalP5PK}',
						@cusBondDetailDepartureP5PK							UNIQUEIDENTIFIER = '{cusBondDetailDepartureP5PK}',
						@cusBondDetailDepartureP4PK							UNIQUEIDENTIFIER = '{cusBondDetailDepartureP4PK}',
						@cusBondDetailNonNctsPK								UNIQUEIDENTIFIER = '{cusBondDetailNonNctsPK}',
						@cusBondDetailForMissingMovementHeaderPK			UNIQUEIDENTIFIER = '{cusBondDetailForMissingMovementHeaderPK}',
						@cusBondDetailForMissingMovementHeaderPK2			UNIQUEIDENTIFIER = '{cusBondDetailForMissingMovementHeaderPK2}'

				DELETE FROM GlbCompany WHERE GC_PK = @companyPK
				DELETE FROM GlbBranch WHERE GB_PK = @branchPK
				DELETE FROM CusInBondHeader WHERE BH_PK IN (@cusInBondHeaderArrivalP5PK, @cusInBondHeaderDepartureP5PK, @cusInBondHeaderDepartureP4PK)
				DELETE FROM CusBondDetail WHERE PW_PK IN(@cusBondDetailArrivalP5PK, @cusBondDetailDepartureP5PK, @cusBondDetailDepartureP4PK, @cusBondDetailNonNctsPK)

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)	VALUES
					(@companyPK, 'DE', 'EUR', 'DDE', 'DE company', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@branchPK, @companyPK, 'BRN', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@cusInBondHeaderArrivalP5PK,	@branchPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondHeaderDepartureP5PK,	@branchPK, 'BH02', 'NC5', 'D', '2022-12-15', 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondHeaderDepartureWithoutMovementHeaderPK, @branchPK, 'BH03', 'NC5', 'D', '2023-03-02', 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondHeaderDepartureP4PK, @branchPK, 'BH04', 'NC4', 'D', '2023-03-02', 1, GetUtcDate(), 'KCH', 'KCH')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SubApplicationCode, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate) VALUES
					(@cusInBondMoveHeaderArrivalP5PK,			@cusInBondHeaderArrivalP5PK,	'D', '2022-07-30', 'KCH', GetUtcDate(), 'KCH','2022-07-30','2022-07-30'),
					(@cusInBondMoveHeaderDepartureP5OlderPK,	@cusInBondHeaderDepartureP5PK,	'D', '2022-12-15', 'KCH', GetUtcDate(), 'KCH','2022-07-30','2022-07-30'),
					(@cusInBondMoveHeaderDepartureP5NewerPK,	@cusInBondHeaderDepartureP5PK,	'D', '2023-07-30', 'KCH', GetUtcDate(), 'KCH','2022-12-15','2022-12-15'),
					(@cusInBondMoveHeaderDepartureP4PK,			@cusInBondHeaderDepartureP4PK,	'D', '2023-03-02', 'KCH', GetUtcDate(), 'KCH','2023-03-02','2023-03-02')

				INSERT INTO CusBondDetail (PW_PK, PW_ParentTableCode, PW_ParentID, PW_SystemCreateTimeUtc, PW_SystemLastEditTimeUtc, PW_SystemCreateUser, PW_SystemLastEditUser) VALUES
					(@cusBondDetailArrivalP5PK,		'BH', @cusInBondHeaderArrivalP5PK,		'2023-01-01', '2023-01-01 01:01:00', 'KCH', 'KCH'),
					(@cusBondDetailDepartureP5PK,	'BH', @cusInBondHeaderDepartureP5PK,	'2023-01-01', '2023-01-01 01:01:00', 'KCH', 'KCH'),
					(@cusBondDetailDepartureP4PK,	'BH', @cusInBondHeaderDepartureP4PK,	'2023-01-01', '2023-01-01 01:01:00', 'KCH', 'KCH'),
					(@cusBondDetailNonNctsPK,		'JE', @jobDeclarationPK,				'2023-01-01', '2023-01-01 01:01:00', 'KCH', 'KCH'),
					(@cusBondDetailForMissingMovementHeaderPK, 'BH', @cusInBondHeaderDepartureWithoutMovementHeaderPK, '2023-01-01', '2023-01-01 01:01:00', 'KCH', 'KCH'),
					(@cusBondDetailForMissingMovementHeaderPK2, 'BH', @cusInBondHeaderDepartureWithoutMovementHeaderPK, '2023-01-01', '2023-01-01 01:01:00', 'KCH', 'KCH')";
			TestConnection.ExecuteNonQuery(sql);
		}
		Guid jobDeclarationPK = Guid.NewGuid();
		Guid cusInBondHeaderArrivalP5PK = Guid.NewGuid();
		Guid cusInBondHeaderDepartureP4PK = Guid.NewGuid();
		Guid cusInBondHeaderDepartureWithoutMovementHeaderPK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDepartureP5OlderPK = Guid.NewGuid();
		Guid cusBondDetailArrivalP5PK = Guid.NewGuid();
		Guid cusBondDetailDepartureP5PK = Guid.NewGuid();
		Guid cusBondDetailDepartureP4PK = Guid.NewGuid();
		Guid cusBondDetailNonNctsPK = Guid.NewGuid();
		Guid cusBondDetailForMissingMovementHeaderPK = Guid.NewGuid();
		Guid cusBondDetailForMissingMovementHeaderPK2 = Guid.NewGuid();
	}
}
