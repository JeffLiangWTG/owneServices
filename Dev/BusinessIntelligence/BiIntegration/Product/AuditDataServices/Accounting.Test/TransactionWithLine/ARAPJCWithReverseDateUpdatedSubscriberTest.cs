using System;
using System.Data;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(ARAPJCWithReverseDateUpdatedSubscriber))]
	public class ARAPJCWithReverseDateUpdatedSubscriberTest : TransactionWithLineSubscriberBaseTest
	{
		protected override string ExpectedCode => "URD";

		protected override string ExpectedDescription => "A subscriber responsible for updating AL_ReverseDate for AR/AR INV/ADJ/CRD JC WIP/ACR/JNL/JRJ";

		protected override int ExpectedRowNums_BeforeGenerateJournalEntriesCDCStartDate => 2;

		protected override bool ExpectedNotifyInsert => false;

		protected override bool ExpectedNotifyUpdate => true;

		public override void TestSpecificColumns() => AssertContainsExactElementsInAnyOrder(new SchemaColumn[] { AccTransactionLinesSchema.AL_ReverseDate }, NewDataChangeSubscriber().SpecificColumns);

		public override void TestCustomFilter()
		{
			var subscriber = new ARAPJCWithReverseDateUpdatedSubscriber();

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
				row[AccTransactionLinesSchema.AL_LineType.Name] = TransactionLineTypes.UnapprovedCost;
				row[AccTransactionLinesSchema.AL_ReverseDate.Name] = DBNull.Value;
			}
			else
			{
				row[AccTransactionLinesSchema.AL_LineType.Name] = TransactionLineTypes.Revenue;
				row[AccTransactionLinesSchema.AL_ReverseDate.Name] = DateTime.Today;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { AccTransactionLinesSchema.AL_LineType, AccTransactionLinesSchema.AL_ReverseDate };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
