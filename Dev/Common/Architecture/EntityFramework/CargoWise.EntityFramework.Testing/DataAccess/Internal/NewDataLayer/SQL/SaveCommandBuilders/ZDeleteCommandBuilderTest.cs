using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZDeleteCommandBuilderTest : ZSqlCommandBuilderTest
	{
		public override void TestWithDummyRow()
		{
			DataRow row = CreateDummyRow();
			row[DummyBizoSchema.Z0_Description.Name] = "something else";

			StringBuilder commandTextBuilder = new StringBuilder();
			ZSqlCommandBuilder builder = GetBuilder(row);
			builder.AppendCommandTextAndBlobSaver(commandTextBuilder);

			string text = commandTextBuilder.ToString();
			AssertContains("Command", "DELETE FROM db.dbo.tablename", text);
			AssertContains("Value description", "AND ISNULL(NULLIF(Z0_Description, @16), NULLIF(@16, Z0_Description)) is NULL", text);
			AssertContains("Value description", "@16 = 'hello'", text);
		}

		public void TestIsConcurrencyCheckRequired()
		{
			ZSqlCommandBuilder builder = GetBuilder(CreateDummyRow());
			Assert(builder.IsConcurrencyCheckRequired);

			var schema = ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema("OrgPatternMatch");
			builder = new ZDeleteCommandBuilder(CreateDummyRow(), "db.dbo.OrgPatternMatch", true, 1, schema);
			Assert(!builder.IsConcurrencyCheckRequired);

			builder = new ZDeleteCommandBuilder(CreateDummyRow(), "OrgPatternMatch", true, 1, schema);
			Assert(!builder.IsConcurrencyCheckRequired);
		}

		protected override DataRow CreateDummyRow()
		{
			DataRow row = base.CreateDummyRow();
			row.AcceptChanges();
			return row;
		}

		protected override ZSqlCommandBuilder GetBuilder(DataRow row)
		{
			return new ZDeleteCommandBuilder(row, "db.dbo.tablename", true, 1, ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchema(row.Table.TableName));
		}
	}

	sealed class ZDeleteCommandBuilderConcurrencyTest : ConcurrencyCheckerTest
	{
		protected override ZSqlCommandBuilder GetBuilderForConcurrencyChecks(DataRow row, string tableName, IApplicationSchemaResolver schemaResolver)
		{
			return new ZDeleteCommandBuilder(row, tableName, true, 1, schemaResolver.GetTableSchema(row.Table.TableName));
		}
	}
}
