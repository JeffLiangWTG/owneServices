using System.Data;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ConcurrencyInfoTest : TransactionedTestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestShouldCheckConcurrency()
		{
			var data = new DataSet();

			var connectionInfo = new ZSqlConnectionInfo(Db.Connection, "");

			var loader = new ZSqlLoader(data, connectionInfo, ObjectFactory.Get<IApplicationSchemaResolver>());

			var table = loader.GetTable(DummyBizoSchema.Constants.TableName);
			var row = table.NewRow();

			foreach (DataColumn column in table.Columns)
			{
				var schemaColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumn(column.ColumnName, DummyBizoSchema.Constants.TableName);

				var shouldCheck = schemaColumn.ShouldCheckConcurrency(row, column);

				AssertEquals(string.Format("concurrency policy check for {0}", schemaColumn.Name),
					!schemaColumn.IsLargeBinaryOrText, shouldCheck);
			}
		}

		public void TestGetConcurrencyPolicyFromTable()
		{
			var dummyBizo = new BusinessObjectFactory().New<DummyBusinessObject>();
			dummyBizo.SetConcurrencyPolicy(dummyBizo.Z0_DescriptionInfo.Name, ConcurrencyPolicy.Strict);
			AssertEquals("Get concurrency policy from row if concurrency policy on table is null", ConcurrencyPolicy.Strict, ConcurrencyInfo.GetConcurrencyPolicy(dummyBizo.Z0_DescriptionInfo, dummyBizo.Row));

			dummyBizo.Table.ExtendedProperties[typeof(ConcurrencyPolicy)] = ConcurrencyPolicy.Ignore;
			AssertEquals("Get concurrency policy from table if concurrency policy on table is not null", ConcurrencyPolicy.Ignore, ConcurrencyInfo.GetConcurrencyPolicy(dummyBizo.Z0_DescriptionInfo, dummyBizo.Row));
		}
	}
}
