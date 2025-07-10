using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU
{
	[TestedType(typeof(PopulateBM_GrossWeightUnloadedAndB0_GrossWeightUnloadedForNctsP5Arrival))]
	sealed class PopulateBM_GrossWeightUnloadedAndB0_GrossWeightUnloadedForNctsP5ArrivalTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			CombineAssertions(() =>
			{
				var sqlUtcNow = DateTime.UtcNow;
				var sqlUtcNowFrom = sqlUtcNow.AddMinutes(-2);
				var sqlUtcNowTo = sqlUtcNow.AddMinutes(+2);
				var moveHeaders = new List<Tuple<Guid, decimal, DateTime, string>>();
				TestConnection.ExecuteReader($"SELECT * FROM CusInBondMoveHeader WHERE BM_PK IN('{moveHeaderArrivalP5_PK}', '{moveHeaderDepartureP5_PK}', '{moveHeaderArrivalP4_PK}', '{moveHeader3ArrivalP5_PK}', '{moveHeader4ArrivalP5_PK}', '{moveHeader5ArrivalP5_PK}')",
					reader => moveHeaders.Add(Tuple.Create((Guid)reader["BM_PK"], (decimal)reader["BM_GrossWeightUnloaded"], (DateTime)reader["BM_SystemLastEditTimeUtc"], (string)reader["BM_SystemLastEditUser"])));

				var bills = new List<Tuple<Guid, decimal, DateTime, string>>();
				TestConnection.ExecuteReader($"SELECT * FROM CusInBondBill WHERE B0_PK IN('{billArrivalP5DIF_PK}', '{billArrivalP5DEC_PK}', '{bill2ArrivalP5DIF_PK}', '{billArrivalP5MIS_PK}', '{billDepartureP5_PK}', '{billArrivalP4_PK}', '{bill3ArrivalP5_PK}', '{bill4ArrivalP5_PK}')",
					reader => bills.Add(Tuple.Create((Guid)reader["B0_PK"], (decimal)reader["B0_GrossWeightUnloaded"], (DateTime)reader["B0_SystemLastEditTimeUtc"], (string)reader["B0_SystemLastEditUser"])));

				var modifiedMoveHeaders = moveHeaders.Where(x => sqlUtcNowFrom <= x.Item3 && sqlUtcNowTo >= x.Item3);
				var unmodifiedMoveHeaders = moveHeaders.Except(modifiedMoveHeaders);
				var modifiedBills = bills.Where(x => sqlUtcNowFrom <= x.Item3 && sqlUtcNowTo >= x.Item3);
				var unmodifiedBills = bills.Except(modifiedBills);

				AssertContainsExactElementsInAnyOrder("Modified MoveHeaders", new[]
				{
					$"{moveHeaderArrivalP5_PK}, 360.000000, E"
				}, modifiedMoveHeaders.Select(x => $"{x.Item1}, {x.Item2}, {x.Item4}").ToArray());
				AssertContainsExactElementsInAnyOrder("Unmodified  MoveHeaders", new[]
				{
					$"{moveHeaderDepartureP5_PK}, 0.000000, KCH",
					$"{moveHeaderArrivalP4_PK}, 0.000000, KCH",
					$"{moveHeader3ArrivalP5_PK}, 0.000000, KCH",
					$"{moveHeader4ArrivalP5_PK}, 0.000000, KCH",
					$"{moveHeader5ArrivalP5_PK}, 0.000000, KCH"
				}, unmodifiedMoveHeaders.Select(x => $"{x.Item1}, {x.Item2}, {x.Item4}").ToArray());

				AssertContainsExactElementsInAnyOrder("Modified Bills", new[]
				{
					$"{billArrivalP5DIF_PK}, 190.000000, E",
					$"{bill2ArrivalP5DIF_PK}, 70.000000, E",
				}, modifiedBills.Select(x => $"{x.Item1}, {x.Item2}, {x.Item4}").ToArray());
				AssertContainsExactElementsInAnyOrder("Unmodified  Bills", new[]
				{
					$"{billArrivalP5DEC_PK}, 0.000000, KCH",
					$"{billArrivalP5MIS_PK}, 0.000000, KCH",
					$"{billDepartureP5_PK}, 0.000000, KCH",
					$"{billArrivalP4_PK}, 0.000000, KCH",
					$"{bill3ArrivalP5_PK}, 0.000000, KCH",
					$"{bill4ArrivalP5_PK}, 0.000000, KCH"
				}, unmodifiedBills.Select(x => $"{x.Item1}, {x.Item2}, {x.Item4}").ToArray());
			});
		}
 
		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateBM_GrossWeightUnloadedAndB0_GrossWeightUnloadedForNctsP5Arrival();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @companyPK								UNIQUEIDENTIFIER = NEWID(),
						@branchPK								UNIQUEIDENTIFIER = NEWID(),
						@headerArrivalP5_PK						UNIQUEIDENTIFIER = '{headerArrivalP5_PK}',
						@moveHeaderArrivalP5_PK					UNIQUEIDENTIFIER = '{moveHeaderArrivalP5_PK}',
						@billArrivalP5DIF_PK					UNIQUEIDENTIFIER = '{billArrivalP5DIF_PK}',
						@bill2ArrivalP5DIF_PK					UNIQUEIDENTIFIER = '{bill2ArrivalP5DIF_PK}',
						@billArrivalP5DEC_PK					UNIQUEIDENTIFIER = '{billArrivalP5DEC_PK}',
						@billArrivalP5MIS_PK					UNIQUEIDENTIFIER = '{billArrivalP5MIS_PK}',
						@goodsItemArrivalP5DIF_PK				UNIQUEIDENTIFIER = '{goodsItemArrivalP5DIF_PK}',
						@goodsItem2ArrivalP5DIF_PK				UNIQUEIDENTIFIER = '{goodsItem2ArrivalP5DIF_PK}',
						@goodsItem3ArrivalP5DIF_PK				UNIQUEIDENTIFIER = '{goodsItem3ArrivalP5DIF_PK}',
						@goodsItemArrivalP5DEC_PK				UNIQUEIDENTIFIER = '{goodsItemArrivalP5DEC_PK}',
						@goodsItemArrivalP5NEW_PK				UNIQUEIDENTIFIER = '{goodsItemArrivalP5NEW_PK}',
						@goodsItemArrivalP5MIS_PK				UNIQUEIDENTIFIER = '{goodsItemArrivalP5MIS_PK}',
						@goodsItem2ArrivalP5DEC_PK				UNIQUEIDENTIFIER = '{goodsItem2ArrivalP5DEC_PK}',
						@differenceGoodsItemArrivalP5DIF_PK		UNIQUEIDENTIFIER = '{differenceGoodsItemArrivalP5DIF_PK}',
						@differenceGoodsItem2ArrivalP5DIF_PK	UNIQUEIDENTIFIER = '{differenceGoodsItem2ArrivalP5DIF_PK}',
						@differenceGoodsItem3ArrivalP5DIF_PK	UNIQUEIDENTIFIER = '{differenceGoodsItem3ArrivalP5DIF_PK}',
						@movementDetailArrivalP5DIF_PK			UNIQUEIDENTIFIER = '{movementDetailArrivalP5DIF_PK}',
						@movementDetail2ArrivalP5DIF_PK			UNIQUEIDENTIFIER = '{movementDetail2ArrivalP5DIF_PK}',
						@movementDetailArrivalP5DEC_PK			UNIQUEIDENTIFIER = '{movementDetailArrivalP5DEC_PK}',
						@movementDetailArrivalP5MIS_PK			UNIQUEIDENTIFIER = '{movementDetailArrivalP5MIS_PK}',

						@headerDepartureP5_PK					UNIQUEIDENTIFIER = '{headerDepartureP5_PK}',
						@moveHeaderDepartureP5_PK				UNIQUEIDENTIFIER = '{moveHeaderDepartureP5_PK}',
						@billDepartureP5_PK						UNIQUEIDENTIFIER = '{billDepartureP5_PK}',
						@movementDetailDepartureP5DIF_PK		UNIQUEIDENTIFIER = '{movementDetailDepartureP5DIF_PK}',
						@goodsItemDepartureP5DEC_PK				UNIQUEIDENTIFIER = '{goodsItemDepartureP5DEC_PK}',

						@headerArrivalP4_PK						UNIQUEIDENTIFIER = '{headerArrivalP4_PK}',
						@moveHeaderArrivalP4_PK					UNIQUEIDENTIFIER = '{moveHeaderArrivalP4_PK}',
						@billArrivalP4_PK						UNIQUEIDENTIFIER = '{billArrivalP4_PK}',
						@movementDetailArrivalP4DIF_PK			UNIQUEIDENTIFIER = '{movementDetailArrivalP4DIF_PK}',
						@goodsItemArrivalP4DEC_PK				UNIQUEIDENTIFIER = '{goodsItemArrivalP4DEC_PK}',

						@header3ArrivalP5_PK					UNIQUEIDENTIFIER = '{header3ArrivalP5_PK}',
						@moveHeader3ArrivalP5_PK				UNIQUEIDENTIFIER = '{moveHeader3ArrivalP5_PK}',
						@bill3ArrivalP5_PK						UNIQUEIDENTIFIER = '{bill3ArrivalP5_PK}',
						@movementDetail3ArrivalP5DIF_PK			UNIQUEIDENTIFIER = '{movementDetail3ArrivalP5DIF_PK}',
						@goodsItem3ArrivalP5DEC_PK				UNIQUEIDENTIFIER = '{goodsItem3ArrivalP5DEC_PK}',

						@header4ArrivalP5_PK					UNIQUEIDENTIFIER = '{header4ArrivalP5_PK}',
						@moveHeader4ArrivalP5_PK				UNIQUEIDENTIFIER = '{moveHeader4ArrivalP5_PK}',
						@bill4ArrivalP5_PK						UNIQUEIDENTIFIER = '{bill4ArrivalP5_PK}',
						@movementDetail4ArrivalP5DIF_PK			UNIQUEIDENTIFIER = '{movementDetail4ArrivalP5IF_PK}',
						@goodsItem4ArrivalP5DEC_PK				UNIQUEIDENTIFIER = '{goodsItem4ArrivalP5DEC_PK}',

						@header5ArrivalP5_PK					UNIQUEIDENTIFIER = '{header5ArrivalP5_PK}',
						@moveHeader5ArrivalP5_PK				UNIQUEIDENTIFIER = '{moveHeader5ArrivalP5_PK}',
						@goodsItem5ArrivalP5DEC_PK				UNIQUEIDENTIFIER = '{goodsItem5ArrivalP5DEC_PK}'

				DELETE FROM GlbCompany WHERE GC_PK = @companyPK
				DELETE FROM GlbBranch WHERE GB_PK = @branchPK
				DELETE FROM CusInBondHeader WHERE BH_PK IN (@headerArrivalP5_PK, @headerDepartureP5_PK, @headerArrivalP4_PK, @header3ArrivalP5_PK, @header4ArrivalP5_PK, @header5ArrivalP5_PK)

				INSERT INTO GlbCompany (GC_PK, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency, GC_Code, GC_Name, GC_SystemCreateTimeUtc, GC_SystemLastEditTimeUtc, GC_SystemLastEditUser, GC_SystemCreateUser) VALUES
					(@companyPK, 'DE', 'EUR', 'DDE', 'DE company', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')

				INSERT INTO GlbBranch (GB_PK, GB_GC, GB_Code, GB_SystemCreateTimeUtc, GB_SystemLastEditTimeUtc, GB_SystemLastEditUser, GB_SystemCreateUser) VALUES
					(@branchPK, @companyPK, 'BRN', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')

				INSERT INTO CusInBondHeader (BH_PK, BH_GB, BH_JobReference, BH_ApplicationCode, BH_HeaderType, BH_IsActive, BH_SystemCreateTimeUtc, BH_SystemLastEditTimeUtc, BH_SystemLastEditUser, BH_SystemCreateUser) VALUES
					(@headerArrivalP5_PK,	@branchPK, 'BH01', 'NC5', 'A', '1', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@headerDepartureP5_PK,	@branchPK, 'BH02', 'NC5', 'D', '1', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@headerArrivalP4_PK,	@branchPK, 'BH03', 'NC4', 'A', '1', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@header3ArrivalP5_PK,	@branchPK, 'BH04', 'NC5', 'A', '1', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@header4ArrivalP5_PK,	@branchPK, 'BH05', 'NC5', 'A', '1', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@header5ArrivalP5_PK,	@branchPK, 'BH06', 'NC5', 'A', '1', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')

				INSERT INTO CusInBondMoveHeader (BM_PK, BM_BH, BM_SubApplicationCode, BM_NoChangesToReport, BM_CustomsStatus, BM_SystemCreateTimeUtc, BM_SystemLastEditTimeUtc, BM_SystemCreateUser, BM_SystemLastEditUser) VALUES
					(@moveHeaderArrivalP5_PK,	@headerArrivalP5_PK,	'A', '0', 'CLR',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@moveHeaderDepartureP5_PK,	@headerDepartureP5_PK,	'D', '0', 'CLR',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@moveHeaderArrivalP4_PK,	@headerArrivalP4_PK,	'A', '0', 'CLR',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@moveHeader3ArrivalP5_PK,	@header3ArrivalP5_PK,	'A', '1', 'CLR',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@moveHeader4ArrivalP5_PK,	@header4ArrivalP5_PK,	'A', '0', '',		'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@moveHeader5ArrivalP5_PK,	@header5ArrivalP5_PK,	'A', '0', 'CLR',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')

				INSERT INTO CusInBondBill (B0_PK, B0_BH, B0_SystemCreateTimeUtc, B0_SystemLastEditTimeUtc, B0_SystemCreateUser, B0_SystemLastEditUser) VALUES
					(@billArrivalP5DIF_PK,	@headerArrivalP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@bill2ArrivalP5DIF_PK,	@headerArrivalP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@billArrivalP5DEC_PK,	@headerArrivalP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@billArrivalP5MIS_PK,	@headerArrivalP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@billDepartureP5_PK,	@headerDepartureP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@billArrivalP4_PK,		@headerArrivalP4_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@bill3ArrivalP5_PK,	@header3ArrivalP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@bill4ArrivalP5_PK,	@header4ArrivalP5_PK,	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')

				INSERT INTO CusInBondMoveDetail (B9_PK, B9_BM, B9_B0, B9_UnloadedState, B9_SystemCreateTimeUtc, B9_SystemLastEditTimeUtc, B9_SystemCreateUser, B9_SystemLastEditUser) VALUES
					(@movementDetailArrivalP5DIF_PK,	@moveHeaderArrivalP5_PK,	@billArrivalP5DIF_PK,	'DIF', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetail2ArrivalP5DIF_PK,	@moveHeaderArrivalP5_PK,	@bill2ArrivalP5DIF_PK,	'DIF', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetailArrivalP5DEC_PK,	@moveHeaderArrivalP5_PK,	@billArrivalP5DEC_PK,	'DEC', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetailArrivalP5MIS_PK,	@moveHeaderArrivalP5_PK,	@billArrivalP5MIS_PK,	'MIS', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetailDepartureP5DIF_PK,	@moveHeaderDepartureP5_PK,	@billDepartureP5_PK,	'DIF', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetailArrivalP4DIF_PK,	@moveHeaderArrivalP4_PK,	@billArrivalP4_PK,		'DIF', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetail3ArrivalP5DIF_PK,	@moveHeader3ArrivalP5_PK,	@bill3ArrivalP5_PK,		'DIF', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@movementDetail4ArrivalP5DIF_PK,	@moveHeader4ArrivalP5_PK,	@bill4ArrivalP5_PK,		'DIF', '{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')

				INSERT INTO CusInBondCargoDesc (BY_PK, BY_ParentTableCode, BY_ParentID, BY_UnloadedState, BY_BY_Commodity, BY_GrossWeight, BY_SystemCreateTimeUtc, BY_SystemLastEditTimeUtc, BY_SystemCreateUser, BY_SystemLastEditUser) VALUES
					(@goodsItemArrivalP5DIF_PK,				'B0', @billArrivalP5DIF_PK,			'DIF',	@differenceGoodsItemArrivalP5DIF_PK,	'10.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItem2ArrivalP5DIF_PK,			'B0', @billArrivalP5DIF_PK,			'DIF',	@differenceGoodsItem2ArrivalP5DIF_PK,	'10.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItem3ArrivalP5DIF_PK,			'B0', @bill2ArrivalP5DIF_PK,		'DIF',	@differenceGoodsItem3ArrivalP5DIF_PK,	'9.000000',		'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItemArrivalP5DEC_PK,				'B0', @billArrivalP5DEC_PK,			'DEC',	null,									'100.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItemArrivalP5NEW_PK,				'B0', @billArrivalP5DIF_PK,			'NEW',	null,									'80.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItemArrivalP5MIS_PK,				'B0', @billArrivalP5MIS_PK,			'MIS',	null,									'600.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItemDepartureP5DEC_PK,			'B0', @billDepartureP5_PK,			'DEC',	null,									'600.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItemArrivalP4DEC_PK,				'B0', @billArrivalP4_PK,			'DEC',	null,									'600.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItem3ArrivalP5DEC_PK,			'B0', @bill3ArrivalP5_PK,			'DEC',	null,									'600.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItem4ArrivalP5DEC_PK,			'B0', @bill4ArrivalP5_PK,			'DEC',	null,									'600.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@goodsItem5ArrivalP5DEC_PK,			'BM', @moveHeader5ArrivalP5_PK,		'DEC',	null,									'600.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),

					(@differenceGoodsItemArrivalP5DIF_PK,	'BY', @goodsItemArrivalP5DIF_PK,	'',		null,									'50.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@differenceGoodsItem2ArrivalP5DIF_PK,	'BY', @goodsItem2ArrivalP5DIF_PK,	'',		null,									'60.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH'),
					(@differenceGoodsItem3ArrivalP5DIF_PK,	'BY', @goodsItem3ArrivalP5DIF_PK,	'',		null,									'70.000000',	'{creationDateTime}', '{creationDateTime}', 'KCH', 'KCH')
				";
			TestConnection.ExecuteNonQuery(sql);
		}

		const string creationDateTime = "2024 - 06 - 01 01:01:00";
		//Matching
		Guid headerArrivalP5_PK = Guid.NewGuid();
		Guid moveHeaderArrivalP5_PK = Guid.NewGuid();
		Guid billArrivalP5DIF_PK = Guid.NewGuid();
		Guid billArrivalP5DEC_PK = Guid.NewGuid();
		Guid billArrivalP5MIS_PK = Guid.NewGuid();
		Guid bill2ArrivalP5DIF_PK = Guid.NewGuid();
		Guid movementDetailArrivalP5DIF_PK = Guid.NewGuid();
		Guid movementDetailArrivalP5DEC_PK = Guid.NewGuid();
		Guid movementDetailArrivalP5MIS_PK = Guid.NewGuid(); //Non matching
		Guid movementDetail2ArrivalP5DIF_PK = Guid.NewGuid();
		Guid goodsItemArrivalP5DIF_PK = Guid.NewGuid();
		Guid goodsItem2ArrivalP5DIF_PK = Guid.NewGuid();
		Guid goodsItem3ArrivalP5DIF_PK = Guid.NewGuid();
		Guid goodsItemArrivalP5DEC_PK = Guid.NewGuid();
		Guid goodsItemArrivalP5NEW_PK = Guid.NewGuid();
		Guid goodsItemArrivalP5MIS_PK = Guid.NewGuid(); //Non Matching
		Guid goodsItem2ArrivalP5DEC_PK = Guid.NewGuid();
		Guid differenceGoodsItemArrivalP5DIF_PK = Guid.NewGuid();
		Guid differenceGoodsItem2ArrivalP5DIF_PK = Guid.NewGuid();
		Guid differenceGoodsItem3ArrivalP5DIF_PK = Guid.NewGuid();

		//Non Matching Header D
		Guid headerDepartureP5_PK = Guid.NewGuid();
		Guid moveHeaderDepartureP5_PK = Guid.NewGuid();
		Guid billDepartureP5_PK = Guid.NewGuid();
		Guid movementDetailDepartureP5DIF_PK = Guid.NewGuid();
		Guid goodsItemDepartureP5DEC_PK = Guid.NewGuid();

		//Non Matching Header P4
		Guid headerArrivalP4_PK = Guid.NewGuid();
		Guid moveHeaderArrivalP4_PK = Guid.NewGuid();
		Guid billArrivalP4_PK = Guid.NewGuid();
		Guid movementDetailArrivalP4DIF_PK = Guid.NewGuid();
		Guid goodsItemArrivalP4DEC_PK = Guid.NewGuid();

		//Non Matching MoveHeader ChangesToReport
		Guid header3ArrivalP5_PK = Guid.NewGuid();
		Guid moveHeader3ArrivalP5_PK = Guid.NewGuid();
		Guid bill3ArrivalP5_PK = Guid.NewGuid();
		Guid movementDetail3ArrivalP5DIF_PK = Guid.NewGuid();
		Guid goodsItem3ArrivalP5DEC_PK = Guid.NewGuid();

		//Non Matching MoveHeader Customs Status
		Guid header4ArrivalP5_PK = Guid.NewGuid();
		Guid moveHeader4ArrivalP5_PK = Guid.NewGuid();
		Guid bill4ArrivalP5_PK = Guid.NewGuid();
		Guid movementDetail4ArrivalP5IF_PK = Guid.NewGuid();
		Guid goodsItem4ArrivalP5DEC_PK = Guid.NewGuid();

		//Non Matching GoodsItem non matching ParentTableCode
		Guid header5ArrivalP5_PK = Guid.NewGuid();
		Guid moveHeader5ArrivalP5_PK = Guid.NewGuid();
		Guid goodsItem5ArrivalP5DEC_PK = Guid.NewGuid();
	}
}
