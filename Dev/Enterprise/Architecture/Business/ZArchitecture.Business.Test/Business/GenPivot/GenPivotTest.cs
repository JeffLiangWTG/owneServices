using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(GenPivot))]
	sealed class GenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLoadRelationPivot()
		{
			var biz1 = Factory.New<DummyBusinessObject>();
			var biz2 = Factory.New<DummyBusinessObject>();
			var biz3 = Factory.New<DummyBusinessObject>();
			var pivot = Factory.New<GenPivot>();
			pivot.XX_RelationType = "AAA";
			pivot.XX_Relation1ID = biz1.PK;
			pivot.XX_Relation1TableCode = biz1.TablePrefix;
			pivot.XX_Relation2ID = biz2.PK;
			pivot.XX_Relation2TableCode = biz2.TablePrefix;

			AssertEquals(pivot, GenPivot.LoadRelation1Pivot(biz1, "AAA"));
			AssertEquals(pivot, GenPivot.LoadRelation2Pivot(biz2, "AAA"));
			AssertNull(GenPivot.LoadRelation1Pivot(biz1, "BBB"));
			AssertNull(GenPivot.LoadRelation2Pivot(biz2, "BBB"));
			AssertNull(GenPivot.LoadRelation1Pivot(biz3, "AAA"));
			AssertNull(GenPivot.LoadRelation2Pivot(biz3, "AAA"));
		}

		public void TestAddIncidentLinkFilter_ThreeParties()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			GenPivot.Query.AddPivotFilter(query, DummyBizoSchema.Constants.Prefix, "AAA",
				typeof(StmModuleFilter), StmModuleFilterSchema.Constants.Prefix, new ZQuery(StmModuleFilterSchema.S9_ModuleID, "MyFilter"),
				typeof(StmNote), StmNoteSchema.Constants.Prefix, new ZQuery(StmNoteSchema.ST_SystemCreateUser, "E"));

			string expected =
@"(
	Z0_PK IN 
	(
		SELECT XX_Relation2ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
		AND
		XX_Relation1TableCode = 'S9' 
		AND
		XX_Relation2TableCode = 'Z0' 
		AND
		(
			XX_Relation1ID IN 
			(
				SELECT S9_PK FROM dbo.StmModuleFilter WHERE S9_ModuleID = 'MyFilter' 
				AND
				(
					S9_PK IN 
					(
						SELECT XX_Relation2ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
						AND
						XX_Relation1TableCode = 'ST' 
						AND
						XX_Relation2TableCode = 'S9' 
						AND
						XX_Relation1ID IN 
						(
							SELECT ST_PK FROM dbo.StmNote WHERE ST_SystemCreateUser = 'E'
						)
					)
				)
			)
		)
	)
)
OR
(
	Z0_PK IN 
	(
		SELECT XX_Relation2ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
		AND
		XX_Relation1TableCode = 'S9' 
		AND
		XX_Relation2TableCode = 'Z0' 
		AND
		(
			XX_Relation1ID IN 
			(
				SELECT S9_PK FROM dbo.StmModuleFilter WHERE S9_ModuleID = 'MyFilter' 
				AND
				(
					S9_PK IN 
					(
						SELECT XX_Relation1ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
						AND
						XX_Relation2TableCode = 'ST' 
						AND
						XX_Relation1TableCode = 'S9' 
						AND
						XX_Relation2ID IN 
						(
							SELECT ST_PK FROM dbo.StmNote WHERE ST_SystemCreateUser = 'E'
						)
					)
				)
			)
		)
	)
)
OR
(
	Z0_PK IN 
	(
		SELECT XX_Relation1ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
		AND
		XX_Relation2TableCode = 'S9' 
		AND
		XX_Relation1TableCode = 'Z0' 
		AND
		(
			XX_Relation2ID IN 
			(
				SELECT S9_PK FROM dbo.StmModuleFilter WHERE S9_ModuleID = 'MyFilter' 
				AND
				(
					S9_PK IN 
					(
						SELECT XX_Relation2ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
						AND
						XX_Relation1TableCode = 'ST' 
						AND
						XX_Relation2TableCode = 'S9' 
						AND
						XX_Relation1ID IN 
						(
							SELECT ST_PK FROM dbo.StmNote WHERE ST_SystemCreateUser = 'E'
						)
					)
				)
			)
		)
	)
)
OR
(
	Z0_PK IN 
	(
		SELECT XX_Relation1ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
		AND
		XX_Relation2TableCode = 'S9' 
		AND
		XX_Relation1TableCode = 'Z0' 
		AND
		(
			XX_Relation2ID IN 
			(
				SELECT S9_PK FROM dbo.StmModuleFilter WHERE S9_ModuleID = 'MyFilter' 
				AND
				(
					S9_PK IN 
					(
						SELECT XX_Relation1ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
						AND
						XX_Relation2TableCode = 'ST' 
						AND
						XX_Relation1TableCode = 'S9' 
						AND
						XX_Relation2ID IN 
						(
							SELECT ST_PK FROM dbo.StmNote WHERE ST_SystemCreateUser = 'E'
						)
					)
				)
			)
		)
	)
)
";
			AssertEquals(expected, query.LiteralTextADOFormatted);
		}

		public void TestAddIncidentLinkFilter_TwoParties()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			GenPivot.Query.AddPivotFilter(query, DummyBizoSchema.Constants.Prefix, "AAA",
					typeof(StmNote), StmNoteSchema.Constants.Prefix, new ZQuery(StmNoteSchema.ST_SystemCreateUser, "E"));

			string expected =
@"(
	Z0_PK IN 
	(
		SELECT XX_Relation2ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
		AND
		XX_Relation1TableCode = 'ST' 
		AND
		XX_Relation2TableCode = 'Z0' 
		AND
		XX_Relation1ID IN 
		(
			SELECT ST_PK FROM dbo.StmNote WHERE ST_SystemCreateUser = 'E'
		)
	)
)
OR
(
	Z0_PK IN 
	(
		SELECT XX_Relation1ID FROM dbo.GenPivot WHERE XX_RelationType = 'AAA' 
		AND
		XX_Relation2TableCode = 'ST' 
		AND
		XX_Relation1TableCode = 'Z0' 
		AND
		XX_Relation2ID IN 
		(
			SELECT ST_PK FROM dbo.StmNote WHERE ST_SystemCreateUser = 'E'
		)
	)
)
";
			AssertEquals(expected, query.LiteralTextSqlFormatted);
		}
	}
}