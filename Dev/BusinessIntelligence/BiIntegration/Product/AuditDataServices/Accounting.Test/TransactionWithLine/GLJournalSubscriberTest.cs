using System.Data;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(GLJournalSubscriber))]
	class GLJournalSubscriberTest : TransactionWithLineSubscriberBaseTest
	{
		protected override string ExpectedCode => "GJS";

		protected override string ExpectedDescription => "GL Journal Subscriber";

		protected override bool ExpectedNotifyUpdate => true;

		public override void TestSpecificColumns()
		{
			var expectedSpecificColumns = new SchemaColumn[10] {
				AccTransactionLinesSchema.AL_LineAmount,
				AccTransactionLinesSchema.AL_OSAmount,
				AccTransactionLinesSchema.AL_OH,
				AccTransactionLinesSchema.AL_RX_NKTransactionCurrency,
				AccTransactionLinesSchema.AL_ExchangeRate,
				AccTransactionLinesSchema.AL_AG,
				AccTransactionLinesSchema.AL_GB,
				AccTransactionLinesSchema.AL_GE,
				AccTransactionLinesSchema.AL_PostDate,
				AccTransactionLinesSchema.AL_ReverseDate
			};

			AssertContainsExactElementsInAnyOrder(expectedSpecificColumns, NewDataChangeSubscriber().SpecificColumns);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GLJournalSubscriber();

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
				row[AccTransactionLinesSchema.AL_LineType.Name] = "GJL";
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
