using System;
using System.Data;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(WIPACRSubscriber))]
	public class WIPACRSubscriberTest : TransactionWithLineSubscriberBaseTest
	{
		protected override string ExpectedCode => "WAR";

		protected override string ExpectedDescription => "WIP and Accrual Subscriber";

		public override void TestProcessChanges()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				var subscriber = NewDataChangeSubscriber();
				var changeTable = CreateProcessHeaderTable();
				using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
				{
					changeTable.Columns.Remove(AccTransactionLinesSchema.Constants.AL_AH);
					var dataColumn = new DataColumn(AccTransactionLinesSchema.Constants.AL_AH, typeof(Guid));
					dataColumn.AllowDBNull = true;
					changeTable.Columns.Add(dataColumn);
					CreateRowForTransactionLineWithoutHeader(changeTable, DateTime.Now, DateTime.Now);
					subscriber.ProcessChanges(loggerMock.Object, changeTable);

					generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionLinesSchema.PK.Name, 1))), Times.Exactly(1));
				}
			}
		}

		[TestDate(2023, 5, 14)]
		public override void TestProcessChanges_BeforeGenerateJournalEntriesCDCStartDate()
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				var subscriber = NewDataChangeSubscriber();
				var changeTable = CreateProcessHeaderTable();

				CreateRowForTransactionLineWithoutHeader(changeTable, new DateTime(2023, 5, 9));
				CreateRowForTransactionLineWithoutHeader(changeTable, ZDateTime.Today.ToDateTime());

				subscriber.ProcessChanges(loggerMock.Object, changeTable);
				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionLinesSchema.PK.Name, 1))), Times.Exactly(1));
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new WIPACRSubscriber();

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
				row[AccTransactionLinesSchema.AL_LineType.Name] = TransactionLineTypes.Revenue;
			}
			else
			{
				row[AccTransactionLinesSchema.AL_LineType.Name] = TransactionLineTypes.Accrual;
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
