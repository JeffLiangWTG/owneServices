namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System.Data;
	using CargoWise.Schema;
	using Enterprise.AuditDataServices.Subscription.Subscribers;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(DummyRefCountrySubscriber))]
	class DummyRefCountrySubscriberTest : ActualDataChangesAuditSubscriberTest
	{
		public override void TestCustomFilter()
		{
			var subscriber = new DummyRefCountrySubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			if (shouldBeFiltered)
			{
				row[RefCountrySchema.RN_IsActive.Name] = 0;
			}
			else
			{
				row[RefCountrySchema.RN_IsActive.Name] = 1;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { RefCountrySchema.RN_IsActive };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
