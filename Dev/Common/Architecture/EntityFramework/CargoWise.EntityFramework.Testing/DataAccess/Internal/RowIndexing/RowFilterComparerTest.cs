using System.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class RowFilterComparerTest : TestCase
	{
		public void TestEmptyFilterMatches()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row);
			table.Rows.Add(row);
			Assert(new RowFilterComparer(table, new ZQuery()).IsMatch(row));
		}

		public void TestNoResultQuery()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row);
			table.Rows.Add(row);
			AssertEquals(false, new RowFilterComparer(table, ZQuery.NoResultQuery).IsMatch(row));
		}

		public void TestMatchingFilterMatches()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row[DummyBizoSchema.Z0_Number.Name] = 1;
			table.Rows.Add(row);
			AssertEquals(true, new RowFilterComparer(table, new ZQuery(DummyBizoSchema.Z0_Number, 1)).IsMatch(row));
		}

		public void TestNonMatchingFilter()
		{
			RowFactory factory = new RowFactory();
			DataTable table = factory.GetTable(DummyBizoSchema.Constants.TableName);
			DataRow row = table.NewRow();
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row[DummyBizoSchema.Z0_Number.Name] = 2;
			table.Rows.Add(row);
			AssertEquals(false, new RowFilterComparer(table, new ZQuery(DummyBizoSchema.Z0_Number, 1)).IsMatch(row));
		}
	}
}
