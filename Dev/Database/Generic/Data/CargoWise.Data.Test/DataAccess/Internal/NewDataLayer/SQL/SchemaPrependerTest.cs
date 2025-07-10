using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess.Internal.NewDataLayer.SQL
{
	class SchemaPrependerTest : TestCase
	{
		public void TestAddSchemaNameAlreadyHasSchemaName()
		{
			var result = SchemaPrepender.AddSchemaName("dbo.DummyBizo");
			AssertEquals("dbo.DummyBizo", result);
		}

		public void TestAddSchemaNameDidntHaveSchemaName()
		{
			var result = SchemaPrepender.AddSchemaName("DummyBizo");
			AssertEquals("dbo.DummyBizo", result);
		}

		public void TestAddSchemaNameInvalidTableName()
		{
			var result = SchemaPrepender.AddSchemaName("NotATable");
			AssertEquals("dbo.NotATable", result);
		}

		public void TestAddSchemaNameNonDbo()
		{
			var result = SchemaPrepender.AddSchemaName("GlbStaffRemuneration");
			AssertEquals("hrm.GlbStaffRemuneration", result);
		}
	}
}
