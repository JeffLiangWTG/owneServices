using System.Data;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(DPYDRCSubscriber))]
	class DPYDRSubscriberTest : TransactionWithLineSubscriberBaseTest
	{
		protected override string ExpectedCode => "DPR";

		protected override string ExpectedDescription => "Direct Payment and Direct Receipt Subscriber";

		public override void TestCustomFilter()
		{
			var subscriber = new DPYDRCSubscriber();

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
				row[AccTransactionLinesSchema.AL_LineType.Name] = TransactionLineTypes.Accrual;
			}
			else
			{
				row[AccTransactionLinesSchema.AL_LineType.Name] = "DPY";
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { AccTransactionLinesSchema.AL_LineType };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
