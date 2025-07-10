using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZDBOnlyUnionQueryTest : TestCaseWithFactory
	{
		public void TestGetAsCompleteSqlStatementWithCombine()
		{
			ZDBOnlyUnionQuery query = new ZDBOnlyUnionQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, false);
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			AssertEquals(" UNION SELECT Z0_Code FROM dbo.DummyBizo WHERE Z0_Code = '123'", query.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, true));
		}

		public void TestGetAsCompleteSqlStatementWithoutCombine()
		{
			ZDBOnlyUnionQuery query = new ZDBOnlyUnionQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, false);
			query.AddToFilter(DummyBizoSchema.Z0_Code, "123");
			AssertEquals(" UNION SELECT Z0_Code FROM dbo.DummyBizo WHERE Z0_Code = " + ParameterNameFactory.GetParameterName(1), query.GetAsCompleteSQLStatement(DummyBizoSchema.Constants.TableName, false));
		}

		public void TestLiteralTextADONoParameters()
		{
			ZDBOnlyUnionQuery query = new ZDBOnlyUnionQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, false);
			AssertEquals(" UNION SELECT Z0_Code FROM dbo.DummyBizo", query.LiteralTextADO);
		}

		public void TestLiteralTextADOWithParameters()
		{
			ZDBOnlyUnionQuery query = new ZDBOnlyUnionQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, false);
			query.AddToFilter(DummyBizoSchema.Z0_Decimal, 123m);
			AssertEquals(" UNION SELECT Z0_Code FROM dbo.DummyBizo WHERE Z0_Decimal = 123", query.LiteralTextADO);
		}

		public void TestUnionAllQuery()
		{
			ZDBOnlyUnionQuery query = new ZDBOnlyUnionQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, true);
			query.AddToFilter(DummyBizoSchema.Z0_Decimal, 123m);
			AssertEquals(" UNION ALL SELECT Z0_Code FROM dbo.DummyBizo WHERE Z0_Decimal = 123", query.LiteralTextADO);
		}
	}
}
