using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU
{
	[TestedType(typeof(CreateCusInBondMoveDetailForNcts5Departure))]
	class CreateCusInBondMoveDetailForNcts5DepartureTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				var sqlUtcNow = DateTime.UtcNow;
				var sqlUtcNowFrom = sqlUtcNow.AddMinutes(-2);
				var sqlUtcNowTo = sqlUtcNow.AddMinutes(+2);

				var bills = new List<Tuple<Guid, DateTime, string>>();
				TestConnection.ExecuteReader($"SELECT * FROM CusInBondBill WHERE B0_PK IN('{cusInBondBillDeparture5WithDetall_PK}', '{cusInBondBillDeparture5TwoLinesWithoutDetall1_PK}', '{cusInBondBillDeparture5TwoLinesWithoutDetall2_PK}', '{cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK}', '{cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK}', '{cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK}', '{cusInBondBillDeparture4_PK}')",
					reader => bills.Add(Tuple.Create((Guid)reader["B0_PK"], (DateTime)reader["B0_SystemLastEditTimeUtc"], (string)reader["B0_SystemLastEditUser"])));
				var existBills = bills.Where(x => sqlUtcNowFrom <= x.Item2 && sqlUtcNowTo >= x.Item2);
				AssertContainsExactElementsInAnyOrder("All Bills exist", new[]
				{
					$"{cusInBondBillDeparture5WithDetall_PK}, KCH",
					$"{cusInBondBillDeparture5TwoLinesWithoutDetall1_PK}, KCH",
					$"{cusInBondBillDeparture5TwoLinesWithoutDetall2_PK}, KCH",
					$"{cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK}, KCH",
					$"{cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK}, KCH",
					$"{cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK}, KCH",
					$"{cusInBondBillDeparture4_PK}, KCH",
				}, existBills.Select(x => $"{x.Item1}, {x.Item3}").ToArray());

				var moveDetail = new List<Tuple<Guid, Guid, DateTime, string, string, Guid>>();
				TestConnection.ExecuteReader($"SELECT * FROM CusInBondMoveDetail WHERE B9_B0 IN('{cusInBondBillDeparture5WithDetall_PK}', '{cusInBondBillDeparture5TwoLinesWithoutDetall1_PK}', '{cusInBondBillDeparture5TwoLinesWithoutDetall2_PK}', '{cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK}', '{cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK}', '{cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK}')",
					reader => moveDetail.Add(Tuple.Create((Guid)reader["B9_PK"], (Guid)reader["B9_B0"], (DateTime)reader["B9_SystemLastEditTimeUtc"], (string)reader["B9_SystemLastEditUser"], (string)reader["B9_SeqNo"], (Guid)reader["B9_BM"])));
				var createMoveDetail = moveDetail.Where(x => sqlUtcNowFrom <= x.Item3 && sqlUtcNowTo >= x.Item3);
				AssertContainsExactElementsInAnyOrder("Movements Details (4 new)", new[]
				{
					$"{cusInBondMoveHeaderDeparture5WithDetall_PK}, 1, KCH",
					$"{cusInBondMoveHeaderDeparture5TwoLinesDetall_PK}, 1, E",
					$"{cusInBondMoveHeaderDeparture5TwoLinesDetall_PK}, 2, E",
					$"{cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK}, 1, E",
					$"{cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK}, 2, E",
					$"{cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK}, 3, E",
				}, createMoveDetail.Select(d => $"{d.Item6}, {d.Item5}, {d.Item4}").ToArray());
			});
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new CreateCusInBondMoveDetailForNcts5Departure();

		public override string[] expectedIndex => new string[]
		{
			"NONCLUSTERED INDEX [_IDX_Transformation_CusInBondHeader_1] ON [dbo].[CusInBondHeader] ([BH_ApplicationCode], [BH_HeaderType]) INCLUDE ([BH_PK]) WHERE ([BH_ApplicationCode]='NC5' AND [BH_HeaderType]='D') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_IDX_Transformation_CusInBondMoveDetail_1] ON [dbo].[CusInBondMoveDetail] ([B9_SeqNo], [B9_PreviousITNumber], [B9_SystemLastEditTimeUtc], [B9_SystemLastEditUser]) INCLUDE ([B9_PK]) WHERE ([B9_SeqNo]='0' AND [B9_PreviousITNumber]='PUPD') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)"
		};

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @company_PK											UNIQUEIDENTIFIER = NEWID(),
						@branch_PK											UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDeparture5WithDetall_PK				UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDeparture5TwoLinesDetall_PK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDeparture5ThreeLinesDetall_PK		UNIQUEIDENTIFIER = NEWID(),
						@cusInBondHeaderDeparture4_PK						UNIQUEIDENTIFIER = NEWID(),
						@cusInBondBillDeparture5WithDetall_PK				UNIQUEIDENTIFIER = '{cusInBondBillDeparture5WithDetall_PK}',
						@cusInBondBillDeparture5TwoLinesWithoutDetall1_PK	UNIQUEIDENTIFIER = '{cusInBondBillDeparture5TwoLinesWithoutDetall1_PK}',
						@cusInBondBillDeparture5TwoLinesWithoutDetall2_PK	UNIQUEIDENTIFIER = '{cusInBondBillDeparture5TwoLinesWithoutDetall2_PK}',
						@cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK	UNIQUEIDENTIFIER = '{cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK}',
						@cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK	UNIQUEIDENTIFIER = '{cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK}',
						@cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK	UNIQUEIDENTIFIER = '{cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK}',
						@cusInBondBillDeparture4_PK							UNIQUEIDENTIFIER = '{cusInBondBillDeparture4_PK}',
						@cusInBondMoveHeaderDeparture5WithDetall_PK			UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDeparture5WithDetall_PK}',
						@cusInBondMoveHeaderDeparture5TwoLinesDetall_PK		UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDeparture5TwoLinesDetall_PK}',
						@cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK	UNIQUEIDENTIFIER = '{cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK}',
						@cusInBondMoveHeaderDeparture4_PK					UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveDetailDeparture5WithDetall_PK			UNIQUEIDENTIFIER = NEWID(),
						@cusInBondMoveDetailDeparture5TwoLinesWithDetall_PK	UNIQUEIDENTIFIER = NEWID()

				DELETE FROM GlbCompany WHERE GC_PK = @company_PK
				DELETE FROM GlbBranch WHERE GB_PK = @branch_PK
				DELETE FROM CusInBondHeader WHERE BH_PK IN (@cusInBondHeaderDeparture5WithDetall_PK, @cusInBondHeaderDeparture5TwoLinesDetall_PK, @cusInBondHeaderDeparture5ThreeLinesDetall_PK, @cusInBondHeaderDeparture4_PK)
				DELETE FROM CusInBondBill WHERE B0_PK IN (@cusInBondBillDeparture5WithDetall_PK, @cusInBondBillDeparture5TwoLinesWithoutDetall1_PK, @cusInBondBillDeparture5TwoLinesWithoutDetall2_PK, @cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK, @cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK, @cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK, @cusInBondBillDeparture4_PK)
				DELETE FROM CusInBondMoveHeader WHERE BM_PK IN(@cusInBondMoveHeaderDeparture5WithDetall_PK, @cusInBondMoveHeaderDeparture5TwoLinesDetall_PK, @cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK, @cusInBondMoveHeaderDeparture4_PK)
				DELETE FROM CusInBondMoveDetail WHERE B9_BM IN(@cusInBondMoveHeaderDeparture5WithDetall_PK, @cusInBondMoveHeaderDeparture5TwoLinesDetall_PK, @cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK, @cusInBondMoveHeaderDeparture4_PK)

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser)	VALUES
					(@company_PK, 'DE', 'EUR', 'DDE', 'DE company', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@branch_PK, @company_PK, 'BRN', GetUtcDate(), GetUtcDate(), 'E', 'E')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_SystemCreateTimeUtc, BH_IsActive, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@cusInBondHeaderDeparture5WithDetall_PK,		@branch_PK, 'BH02', 'NC5', 'D', GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondHeaderDeparture5TwoLinesDetall_PK,	@branch_PK, 'BH02', 'NC5', 'D', GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondHeaderDeparture5ThreeLinesDetall_PK,	@branch_PK, 'BH02', 'NC5', 'D', GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondHeaderDeparture4_PK,					@branch_PK, 'BH04', 'NC4', 'D', GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SubApplicationCode, BM_SystemCreateTimeUtc, BM_SystemCreateUser, BM_SystemLastEditTimeUtc, BM_SystemLastEditUser, BM_EntryDate, BM_ValuationDate) VALUES
					(@cusInBondMoveHeaderDeparture5WithDetall_PK,		@cusInBondHeaderDeparture5WithDetall_PK,		'D', GetUtcDate(), 'KCH', GetUtcDate(), 'KCH', GetUtcDate(),GetUtcDate()),
					(@cusInBondMoveHeaderDeparture5TwoLinesDetall_PK,	@cusInBondHeaderDeparture5TwoLinesDetall_PK,	'D', GetUtcDate(), 'KCH', GetUtcDate(), 'KCH', GetUtcDate(),GetUtcDate()),
					(@cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK,	@cusInBondHeaderDeparture5ThreeLinesDetall_PK,	'D', GetUtcDate(), 'KCH', GetUtcDate(), 'KCH', GetUtcDate(),GetUtcDate()),
					(@cusInBondMoveHeaderDeparture4_PK,					@cusInBondHeaderDeparture4_PK,					'D', GetUtcDate(), 'KCH', GetUtcDate(), 'KCH', GetUtcDate(),GetUtcDate())

				INSERT INTO CusInBondBill (B0_PK, B0_BH, B0_SystemCreateTimeUtc, B0_IsValid, B0_SystemLastEditTimeUtc, B0_SystemLastEditUser, B0_SystemCreateUser) VALUES
					(@cusInBondBillDeparture5WithDetall_PK,					@cusInBondHeaderDeparture5WithDetall_PK,		GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondBillDeparture5TwoLinesWithoutDetall1_PK,			@cusInBondHeaderDeparture5TwoLinesDetall_PK,	GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondBillDeparture5TwoLinesWithoutDetall2_PK,		@cusInBondHeaderDeparture5TwoLinesDetall_PK,	GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK,	@cusInBondHeaderDeparture5ThreeLinesDetall_PK,	GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK,	@cusInBondHeaderDeparture5ThreeLinesDetall_PK,	GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK,	@cusInBondHeaderDeparture5ThreeLinesDetall_PK,	GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH'),
					(@cusInBondBillDeparture4_PK,							@cusInBondHeaderDeparture4_PK,					GetUtcDate(), 1, GetUtcDate(), 'KCH', 'KCH')

				INSERT INTO CusInBondMoveDetail (B9_PK, B9_BM, B9_B0, B9_SeqNo, B9_SystemCreateTimeUtc, B9_SystemLastEditTimeUtc, B9_SystemCreateUser, B9_SystemLastEditUser, B9_UnloadedState) VALUES
					(@cusInBondMoveDetailDeparture5WithDetall_PK,			@cusInBondMoveHeaderDeparture5WithDetall_PK,		@cusInBondBillDeparture5WithDetall_PK,			'1', GetUtcDate(), GetUtcDate(), 'KCH', 'KCH', 'NEW')";
			TestConnection.ExecuteNonQuery(sql);
		}

		Guid cusInBondBillDeparture5WithDetall_PK = Guid.NewGuid();
		Guid cusInBondBillDeparture5TwoLinesWithoutDetall1_PK = Guid.NewGuid();
		Guid cusInBondBillDeparture5TwoLinesWithoutDetall2_PK = Guid.NewGuid();
		Guid cusInBondBillDeparture5ThreeLinesWithoutDetall1_PK = Guid.NewGuid();
		Guid cusInBondBillDeparture5ThreeLinesWithoutDetall2_PK = Guid.NewGuid();
		Guid cusInBondBillDeparture5ThreeLinesWithoutDetall3_PK = Guid.NewGuid();
		Guid cusInBondBillDeparture4_PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDeparture5WithDetall_PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDeparture5TwoLinesDetall_PK = Guid.NewGuid();
		Guid cusInBondMoveHeaderDeparture5ThreeLinesDetall_PK = Guid.NewGuid();
	}
}
