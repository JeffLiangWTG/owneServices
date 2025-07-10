using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.EU
{
	[TestedType(typeof(MoveCustomsThirdQuantityFromGenAddOnToCusInBondCargoDesc))]
	sealed class MoveCustomsThirdQuantityFromGenAddOnToCusInBondCargoDescTest : DataTransformationTestCase
	{
		protected override DataTransformation GetNewTestTransformationInstance() => new MoveCustomsThirdQuantityFromGenAddOnToCusInBondCargoDesc();

		protected override void PrepareTestData()
		{
			var sql = $@"
				DECLARE @companyPK       AS UNIQUEIDENTIFIER = NEWID();
				DECLARE @branchPK        AS UNIQUEIDENTIFIER = NEWID();
				DECLARE @headerPhase4_PK AS UNIQUEIDENTIFIER = NEWID();
				DECLARE @headerPhase5_PK AS UNIQUEIDENTIFIER = NEWID();
				DECLARE @movePhase4_PK   AS UNIQUEIDENTIFIER = NEWID();
				DECLARE @movePhase5_PK   AS UNIQUEIDENTIFIER = NEWID();

				DELETE FROM [dbo].[GenAddOnColumn] WHERE [XA_PK] IN ('{genAddOnPhase4Value_PK}', '{genAddOnPhase4Unit_PK}', '{genAddOnPhase4Value_differentName_PK}', '{genAddOnPhase4Value_differentTableCode_PK}', '{genAddOnPhase5Value_PK}', '{genAddOnPhase5Unit_PK}');
				DELETE FROM [dbo].[CusInBondCargoDesc] WHERE [BY_PK] IN ('{goodsItemPhase4_match_PK}', '{goodsItemPhase4_differentName_PK}', '{goodsItemPhase4_differentTableCode_PK}', '{goodsItemPhase5_PK}');
				DELETE FROM [dbo].[CusInBondMoveHeader] WHERE [BM_PK] IN (@movePhase4_PK, @movePhase5_PK);
				DELETE FROM [dbo].[CusInBondHeader] WHERE [BH_PK] IN (@headerPhase4_PK, @headerPhase5_PK);
				DELETE FROM [dbo].[GlbBranch] WHERE [GB_PK] = @branchPK;
				DELETE FROM [dbo].[GlbCompany] WHERE [GC_PK] = @companyPK;

				INSERT INTO [dbo].[GlbCompany] ([GC_PK]   , [GC_RN_NKCountryCode], [GC_RX_NKLocalCurrency], [GC_Code], [GC_Name], [GC_SystemCreateTimeUtc], [GC_SystemLastEditTimeUtc], [GC_SystemLastEditUser], [GC_SystemCreateUser])
				                        VALUES (@companyPK, 'ES'                 , 'EUR'                  , 'ESC'    , 'ES company'           , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                  , 'MSB');

				INSERT INTO [dbo].[GlbBranch] ([GB_PK]  , [GB_GC]   , [GB_Code], [GB_SystemCreateTimeUtc], [GB_SystemLastEditTimeUtc], [GB_SystemLastEditUser], [GB_SystemCreateUser])
				                       VALUES (@branchPK, @companyPK, 'BRN'    , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                  , 'MSB');

				INSERT INTO [dbo].[CusInBondHeader] ([BH_PK]         , [BH_GB]  , [BH_JobReference], [BH_ApplicationCode], [BH_HeaderType], [BH_IsActive], [BH_SystemCreateTimeUtc], [BH_SystemLastEditTimeUtc], [BH_SystemLastEditUser], [BH_SystemCreateUser])
				                             VALUES (@headerPhase4_PK, @branchPK, 'BH01'           , 'NCT'               , 'D'            , '1'          , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                  , 'MSB'),
				                                    (@headerPhase5_PK, @branchPK, 'BH03'           , 'NC5'               , 'D'            , '1'          , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                  , 'MSB');

				INSERT INTO [dbo].[CusInBondMoveHeader] ([BM_PK]       , [BM_BH]         , [BM_SubApplicationCode], [BM_SystemCreateTimeUtc], [BM_SystemCreateUser], [BM_SystemLastEditTimeUtc], [BM_SystemLastEditUser], [BM_EntryDate]   , [BM_ValuationDate])
				                                 VALUES (@movePhase4_PK, @headerPhase4_PK, 'D'                    , GETUTCDATE()            , 'MSB'                , GETUTCDATE()              , 'MSB'                  , GETUTCDATE()     , GETUTCDATE()     ),
				                                        (@movePhase5_PK, @headerPhase5_PK, 'D'                    , GETUTCDATE()            , 'MSB'                , GETUTCDATE()              , 'MSB'                  , GETUTCDATE()     , GETUTCDATE()     );

				INSERT INTO [dbo].[CusInBondCargoDesc] ([BY_PK]                                  , [BY_ParentTableCode], [BY_ParentID]  , [BY_UnloadedState], [BY_GrossWeight], [BY_CustomsThirdQuantity], [BY_CustomsThirdUnitQty], [BY_SystemCreateTimeUtc], [BY_SystemLastEditTimeUtc], [BY_SystemCreateUser], [BY_SystemLastEditUser])
				                                VALUES ('{goodsItemPhase4_match_PK}'             , 'BM'                , @movePhase4_PK, 'DIF'             , '10.000000'     , '0.000000'               , ''                      , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                , 'MSB'),
				                                       ('{goodsItemPhase4_coalesche_PK}'         , 'BM'                , @movePhase4_PK, 'DIF'             , '10.000000'     , '0.000000'               , ''                      , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                , 'MSB'),
				                                       ('{goodsItemPhase4_differentName_PK}'     , 'BM'                , @movePhase4_PK, 'DIF'             , '10.000000'     , '{expectedValue}'        , '{expectedUnit}'        , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                , 'MSB'),
				                                       ('{goodsItemPhase4_differentTableCode_PK}', 'BM'                , @movePhase4_PK, 'DIF'             , '10.000000'     , '{expectedValue}'        , '{expectedUnit}'        , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                , 'MSB'),
				                                       ('{goodsItemPhase5_PK}'                   , 'BM'                , @movePhase5_PK, 'DIF'             , '10.000000'     , '{expectedValue}'        , '{expectedUnit}'        , GETUTCDATE()            , GETUTCDATE()              , 'MSB'                , 'MSB');

				INSERT INTO [dbo].[GenAddOnColumn] ([XA_PK]                                      , [XA_Name] , [XA_Type], [XA_Data]        , [XA_ParentTableCode], [XA_ParentID]                           , [XA_SystemCreateTimeUtc], [XA_SystemCreateUser], [XA_SystemLastEditTimeUtc], [XA_SystemLastEditUser], [XA_AutoVersion])
				                            VALUES ('{genAddOnPhase4Value_PK}'                   , 'CTQ'     , 'DEC'    , '{expectedValue}', 'BY'                ,'{goodsItemPhase4_match_PK}'             , GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0),
				                                   ('{genAddOnPhase4Unit_PK}'                    , 'CTUQ'    , 'STR'    , '{expectedUnit}X', 'BY'                ,'{goodsItemPhase4_match_PK}'             , GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0),
				                                   ('{genAddOnPhase4Value_coalesche_PK}'         , 'CTQ'     , 'DEC'    , 'NaN'            , 'BY'                ,'{goodsItemPhase4_coalesche_PK}'         , GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0),
				                                   ('{genAddOnPhase4Value_differentName_PK}'     , 'XYZ'     , 'DEC'    , '10.00000'       , 'BY'                ,'{goodsItemPhase4_differentName_PK}'     , GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0),
				                                   ('{genAddOnPhase4Value_differentTableCode_PK}', 'CTQ'     , 'DEC'    , '10.00000'       , 'B0'                ,'{goodsItemPhase4_differentTableCode_PK}', GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0),
				                                   ('{genAddOnPhase5Value_PK}'                   , 'CTQ'     , 'DEC'    , '10.00000'       , 'BY'                ,'{goodsItemPhase5_PK}'                   , GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0),
				                                   ('{genAddOnPhase5Unit_PK}'                    , 'CTUQ'    , 'STR'    , 'ABCD'           , 'BY'                ,'{goodsItemPhase5_PK}'                   , GETUTCDATE()            , 'MSB'                ,GETUTCDATE()               , 'MSB'                  , 0);
			";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected override void AssertTransformationResults() => CombineAssertions(() =>
		{
			var cusInBondCargoDescResultList = new List<Tuple<Guid, decimal, string>>();
			var genAddOnColumnResultList = new List<Guid>();

			var cusInBondCargoDescSql = $@"
				SELECT [BY_PK], [BY_CustomsThirdQuantity], [BY_CustomsThirdUnitQty] 
				FROM [dbo].[CusInBondCargoDesc]
				WHERE [BY_PK] IN ('{goodsItemPhase4_match_PK}', '{goodsItemPhase4_coalesche_PK}', '{goodsItemPhase4_differentName_PK}', '{goodsItemPhase4_differentTableCode_PK}', '{goodsItemPhase5_PK}');
			";

			var genAddOnColumnSql = $@"
				SELECT [XA_PK]
				FROM [dbo].[GenAddOnColumn]
				WHERE [XA_PK] IN ('{genAddOnPhase4Value_PK}', '{genAddOnPhase4Unit_PK}', '{genAddOnPhase4Value_differentName_PK}', '{genAddOnPhase4Value_differentTableCode_PK}', '{genAddOnPhase5Value_PK}', '{genAddOnPhase5Unit_PK}');
			";

			TestConnection.ExecuteReader(cusInBondCargoDescSql, reader => cusInBondCargoDescResultList.Add(Tuple.Create((Guid)reader["BY_PK"], (decimal)reader["BY_CustomsThirdQuantity"], (string)reader["BY_CustomsThirdUnitQty"])));
			TestConnection.ExecuteReader(genAddOnColumnSql, reader => genAddOnColumnResultList.Add((Guid)reader["XA_PK"]));

			AssertContainsExactElementsInAnyOrder(new[]
			{
					Tuple.Create(goodsItemPhase4_match_PK, expectedValue, expectedUnit),
					Tuple.Create(goodsItemPhase4_coalesche_PK, 0m, ""),
					Tuple.Create(goodsItemPhase4_differentName_PK, expectedValue, expectedUnit),
					Tuple.Create(goodsItemPhase4_differentTableCode_PK, expectedValue, expectedUnit),
					Tuple.Create(goodsItemPhase5_PK, expectedValue, expectedUnit),
			}, cusInBondCargoDescResultList);

			AssertContainsExactElementsInAnyOrder(new[]
			{
				genAddOnPhase4Value_differentName_PK,
				genAddOnPhase4Value_differentTableCode_PK,
				genAddOnPhase5Value_PK,
				genAddOnPhase5Unit_PK
			}, genAddOnColumnResultList);
		});

		const decimal expectedValue = 325.12m;
		const string expectedUnit = "UNIT";

		// Matching CusInBondHeader
		readonly Guid goodsItemPhase4_match_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase4Value_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase4Unit_PK = Guid.NewGuid();
		readonly Guid goodsItemPhase4_coalesche_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase4Value_coalesche_PK = Guid.NewGuid();

		// Matching CusInBondHeader but non matching GenAddOnColumn
		readonly Guid goodsItemPhase4_differentName_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase4Value_differentName_PK = Guid.NewGuid();
		readonly Guid goodsItemPhase4_differentTableCode_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase4Value_differentTableCode_PK = Guid.NewGuid();

		// Non Matching CusInBondHeader
		readonly Guid goodsItemPhase5_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase5Value_PK = Guid.NewGuid();
		readonly Guid genAddOnPhase5Unit_PK = Guid.NewGuid();
	}
}
