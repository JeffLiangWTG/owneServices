using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU;

[TestedType(typeof(ChangeCFR_ParentTableCodeAndCFR_ParentIDFromHeaderToMovementHeaderPhase5))]
public class ChangeCFR_ParentTableCodeAndCFR_ParentIDFromHeaderToMovementHeaderPhase5Test : DataTransformationTestCase
{
	protected override DataTransformation GetNewTestTransformationInstance() => new ChangeCFR_ParentTableCodeAndCFR_ParentIDFromHeaderToMovementHeaderPhase5();

	protected override void PrepareTestData()
	{
		var sql = $@"
				DECLARE @companyPK										UNIQUEIDENTIFIER = NEWID(),
						@branchPK										UNIQUEIDENTIFIER = NEWID(),
						@jobDeclarationPK								UNIQUEIDENTIFIER = NEWID(),
						@cusEntryInstructionPK							UNIQUEIDENTIFIER = '{cusEntryInstructionPK}',
						@cusInBondHeaderArrivalP5PK						UNIQUEIDENTIFIER = '{cusInBondHeaderArrivalP5PK}',
						@cusInBondHeaderDepartureP5PK					UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDepartureP5PK2					UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureP5PK2}',
						@cusInBondHeaderDepartureP4PK					UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureP4PK}',
						@cusInBondHeaderDepartureWithArrivalMoveHeaderPK UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureWithArrivalMoveHeaderPK}',
						@cusInBondHeaderDepartureWithNoMoveHeaderPK		UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureWithNoMoveHeaderPK}',
						@cusInBondHeaderDepartureWithNonScaCusRefPK		UNIQUEIDENTIFIER = '{cusInBondHeaderDepartureWithNonScaCusRefPK}',
						@cusInBondMoveHeaderArrivalP5PK					UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderDepartureP5OlderPK			UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDepartureP5OlderPK}',
						@cusInBondMoveHeaderDepartureP5NewerPK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderDepartureP4PK				UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderDepartureP5PK2				UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveHeaderArrivalP5PK2				UNIQUEIDENTIFIER = NEWID(),
						@cusReferenceArrivalP5PK						UNIQUEIDENTIFIER = '{cusReferenceArrivalP5PK}',
						@cusReferenceDepartureP5PK						UNIQUEIDENTIFIER = '{cusReferenceDepartureP5PK}',
						@cusReferenceDepartureP4PK						UNIQUEIDENTIFIER = '{cusReferenceDepartureP4PK}',
						@cusReferenceNonNctsPK							UNIQUEIDENTIFIER = '{cusReferenceNonNctsPK}',
						@cusReferenceNonScaPK							UNIQUEIDENTIFIER = '{cusReferenceNonScaPK}',
						@cusReferenceArrivalMoveHeaderPK				UNIQUEIDENTIFIER = '{cusReferenceArrivalMoveHeaderPK}',
						@cusReferenceNoMoveHeaderPK						UNIQUEIDENTIFIER = '{cusReferenceNoMoveHeaderPK}',
						@cusReferenceNoMoveHeaderPK2					UNIQUEIDENTIFIER = '{cusReferenceNoMoveHeaderPK2}',
						@cusReferenceNoMoveHeaderNonScaPK				UNIQUEIDENTIFIER = '{cusReferenceNoMoveHeaderNonScaPK}'
				DELETE FROM GlbCompany WHERE GC_PK = @companyPK
				DELETE FROM GlbBranch WHERE GB_PK = @branchPK
				DELETE FROM CusInBondHeader WHERE BH_PK IN (@cusInBondHeaderArrivalP5PK, @cusInBondHeaderDepartureP5PK, @cusInBondHeaderDepartureP4PK)
				DELETE FROM CusBondDetail WHERE PW_PK IN(@cusReferenceArrivalP5PK, @cusReferenceDepartureP5PK, @cusReferenceDepartureP4PK, @cusReferenceNonNctsPK, @cusReferenceNonScaPK)

INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)	VALUES
					(@companyPK, 'DE', 'EUR', 'DDE', 'DE company', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@branchPK, @companyPK, 'BRN', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@cusInBondHeaderArrivalP5PK,	@branchPK, 'BH01', 'NC5', 'A', '2022-07-30', 1, GetUtcDate(), 'CM1', 'CM1'),
					(@cusInBondHeaderDepartureP5PK,	@branchPK, 'BH02', 'NC5', 'D', '2022-12-15', 1, GetUtcDate(), 'CM1', 'CM1'),
					(@cusInBondHeaderDepartureP4PK, @branchPK, 'BH03', 'NC4', 'D', '2023-03-02', 1, GetUtcDate(), 'CM1', 'CM1'),
					(@cusInBondHeaderDepartureP5PK2,	@branchPK, 'BH04', 'NC5', 'D', '2022-12-16', 1, GetUtcDate(), 'CM1', 'CM1'),
					(@cusInBondHeaderDepartureWithArrivalMoveHeaderPK, @branchPK, 'BH05', 'NC5', 'D', '2023-12-17', 1, GetUtcDate(), 'CM1', 'CM1'),
					(@cusInBondHeaderDepartureWithNoMoveHeaderPK, @branchPK, 'BH06', 'NC5', 'D', '2023-12-18', 1, GetUtcDate(), 'CM1', 'CM1'),
					(@cusInBondHeaderDepartureWithNonScaCusRefPK, @branchPK, 'BH07', 'NC5', 'D', '2023-12-18', 1, GetUtcDate(), 'CM1', 'CM1')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SystemCreateTimeUtc,	BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate, BM_SubApplicationCode) VALUES
					(@cusInBondMoveHeaderArrivalP5PK,			@cusInBondHeaderArrivalP5PK,	'2022-07-30', 'CM1', GetUtcDate(), 'CM1','2022-07-30','2022-07-30', 'A'),
					(@cusInBondMoveHeaderDepartureP5OlderPK,	@cusInBondHeaderDepartureP5PK,	'2022-12-15', 'CM1', GetUtcDate(), 'CM1','2022-07-30','2022-07-30', 'D'),
					(@cusInBondMoveHeaderDepartureP5NewerPK,	@cusInBondHeaderDepartureP5PK,	'2023-07-30', 'CM1', GetUtcDate(), 'CM1','2022-12-15','2022-12-15', 'D'),
					(@cusInBondMoveHeaderDepartureP4PK,			@cusInBondHeaderDepartureP4PK,	'2023-03-02', 'CM1', GetUtcDate(), 'CM1','2023-03-02','2023-03-02', 'D'),
					(@cusInBondMoveHeaderDepartureP5PK2,		@cusInBondHeaderDepartureP5PK2,	'2023-07-29', 'CM1', GetUtcDate(), 'CM1','2022-12-16','2022-12-16', 'D'),
					(@cusInBondMoveHeaderArrivalP5PK2,			@cusInBondHeaderDepartureWithArrivalMoveHeaderPK,	'2023-12-17', 'CM1', GetUtcDate(), 'CM1', '2023-12-17', '2023-12-17', 'A')

				INSERT INTO CusReference (CFR_PK, CFR_ParentTableCode, CFR_ParentID, CFR_Type, CFR_Reference, CFR_Code, CFR_SystemCreateTimeUtc, CFR_SystemLastEditTimeUtc, CFR_SystemCreateUser, CFR_SystemLastEditUser) VALUES
					(@cusReferenceArrivalP5PK,			'BH', @cusInBondHeaderArrivalP5PK,		'SCA', 'REF1', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceDepartureP5PK,		'BH', @cusInBondHeaderDepartureP5PK,	'SCA', 'REF2', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceDepartureP4PK,		'BH', @cusInBondHeaderDepartureP4PK,	'SCA', 'REF3', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceNonNctsPK,			'CEI', @cusEntryInstructionPK,			'SCA', 'REF4', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceNonScaPK,				'BH', @cusInBondHeaderDepartureP5PK2,	'OLC', 'REF5', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceArrivalMoveHeaderPK,	'BH', @cusInBondHeaderDepartureWithArrivalMoveHeaderPK, 'SCA', 'REF6', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceNoMoveHeaderPK,		'BH', @cusInBondHeaderDepartureWithNoMoveHeaderPK, 'SCA', 'REF7', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceNoMoveHeaderPK2,		'BH', @cusInBondHeaderDepartureWithNoMoveHeaderPK, 'SCA', 'REF7', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1'),
					(@cusReferenceNoMoveHeaderNonScaPK,	'BH', @cusInBondHeaderDepartureWithNonScaCusRefPK, 'OLC', 'REF8', 'FW', '2023-01-01', '2023-01-01 01:01:00', 'CM1', 'CM1')";
		TestConnection.ExecuteNonQuery(sql);
	}

	Guid cusEntryInstructionPK = Guid.NewGuid();
	Guid cusInBondHeaderArrivalP5PK = Guid.NewGuid();
	Guid cusInBondHeaderDepartureP4PK = Guid.NewGuid();
	Guid cusInBondHeaderDepartureP5PK2 = Guid.NewGuid();
	Guid cusInBondHeaderDepartureWithArrivalMoveHeaderPK = Guid.NewGuid();
	Guid cusInBondHeaderDepartureWithNoMoveHeaderPK = Guid.NewGuid();
	Guid cusInBondHeaderDepartureWithNonScaCusRefPK = Guid.NewGuid();
	Guid cusInBondMoveHeaderDepartureP5OlderPK = Guid.NewGuid();
	Guid cusReferenceArrivalP5PK = Guid.NewGuid();
	Guid cusReferenceDepartureP5PK = Guid.NewGuid();
	Guid cusReferenceNonScaPK = Guid.NewGuid();
	Guid cusReferenceDepartureP4PK = Guid.NewGuid();
	Guid cusReferenceNonNctsPK = Guid.NewGuid();
	Guid cusReferenceArrivalMoveHeaderPK = Guid.NewGuid();
	Guid cusReferenceNoMoveHeaderPK = Guid.NewGuid();
	Guid cusReferenceNoMoveHeaderPK2 = Guid.NewGuid();
	Guid cusReferenceNoMoveHeaderNonScaPK = Guid.NewGuid();

	protected override void AssertTransformationResults()
	{
		var utcNow = DateTime.UtcNow;
		var cleanedUtcNow = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 00);
		var unmodifiedDateTime = new DateTime(2023, 1, 1, 1, 1, 0);

		var createdMovementHeaderResultList = new List<Tuple<Guid, Guid, string, DateTime, string, string>>();
		TestConnection.ExecuteReader("SELECT * FROM CusInBondMoveHeader WHERE BM_TypeOfSecurity = 'NON'",
			reader => createdMovementHeaderResultList.Add(Tuple.Create((Guid)reader["BM_PK"], (Guid)reader["BM_BH"], (string)reader["BM_SubApplicationCode"], (DateTime)reader["BM_SystemCreateTimeUtc"], (string)reader["BM_SystemCreateUser"], (string)reader["BM_SystemLastEditUser"])));
		var singleMovementHeaderResult = createdMovementHeaderResultList
			.Single(x => cleanedUtcNow.AddMinutes(-1) <= x.Item4 && cleanedUtcNow.AddMinutes(+1) >= x.Item4);

		AssertEquals("Missing DepartureMovementHeader created", $"{cusInBondHeaderDepartureWithNoMoveHeaderPK}, D, E, E", $"{singleMovementHeaderResult.Item2}, {singleMovementHeaderResult.Item3}, {singleMovementHeaderResult.Item5}, {singleMovementHeaderResult.Item6}");
		AssertEquals("Missing DepartureMovementHeader created", $"{cusInBondHeaderDepartureWithNoMoveHeaderPK}, D, E, E", $"{singleMovementHeaderResult.Item2}, {singleMovementHeaderResult.Item3}, {singleMovementHeaderResult.Item5}, {singleMovementHeaderResult.Item6}");

		var cusReferenceResultList = new List<Tuple<Guid, string, Guid, DateTime, string>>();
		TestConnection.ExecuteReader("SELECT * FROM CusReference",
			reader => cusReferenceResultList.Add(Tuple.Create((Guid)reader["CFR_PK"], (string)reader["CFR_ParentTableCode"], (Guid)reader["CFR_ParentID"], (DateTime)reader["CFR_SystemLastEditTimeUtc"], (string)reader["CFR_SystemLastEditUser"])));
		var modifiedRecords = cusReferenceResultList.Where(x => cleanedUtcNow.AddMinutes(-1) <= x.Item4 && cleanedUtcNow.AddMinutes(+1) >= x.Item4);
		var unmodifiedRecords = cusReferenceResultList.Except(modifiedRecords);
		AssertContainsExactElementsInAnyOrder("parent remains", new[]
		{
			$"{cusReferenceArrivalP5PK}, BH, {cusInBondHeaderArrivalP5PK}, {unmodifiedDateTime}, CM1",
			$"{cusReferenceDepartureP4PK}, BH, {cusInBondHeaderDepartureP4PK}, {unmodifiedDateTime}, CM1",
			$"{cusReferenceNonNctsPK}, CEI, {cusEntryInstructionPK}, {unmodifiedDateTime}, CM1",
			$"{cusReferenceNonScaPK}, BH, {cusInBondHeaderDepartureP5PK2}, {unmodifiedDateTime}, CM1",
			$"{cusReferenceArrivalMoveHeaderPK}, BH, {cusInBondHeaderDepartureWithArrivalMoveHeaderPK}, {unmodifiedDateTime}, CM1",
			$"{cusReferenceNoMoveHeaderNonScaPK}, BH, {cusInBondHeaderDepartureWithNonScaCusRefPK}, {unmodifiedDateTime}, CM1",
		}, unmodifiedRecords.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item4}, {x.Item5}").ToArray());
		AssertContainsExactElementsInAnyOrder("parent changed", new[]
		{
			$"{cusReferenceDepartureP5PK}, BM, {cusInBondMoveHeaderDepartureP5OlderPK}, E",
			$"{cusReferenceNoMoveHeaderPK}, BM, {singleMovementHeaderResult.Item1}, E",
			$"{cusReferenceNoMoveHeaderPK2}, BM, {singleMovementHeaderResult.Item1}, E"
		}, modifiedRecords.Select(x => $"{x.Item1}, {x.Item2}, {x.Item3}, {x.Item5}").ToArray());
	}
}
