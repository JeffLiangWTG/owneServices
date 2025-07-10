using System;
using System.Data;
using CargoWise.Schema;
using Enterprise.AuditDataServices.Accounting.Subscribers;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Accounting.Test
{
	[TestedType(typeof(AccTransactionHeaderWithARAPLedgerSubscriber))]
	public class AccTransactionHeaderWithARAPLedgerSubscriberTest : TransactionWithoutLineSubscriberBaseTest
	{
		protected override string ExpectedCode => "HRP";

		protected override string ExpectedDescription => "AccTransactionHeader added for AH_Ledger in ('AR','AP') and AH_TransactionType in ('JNL','TRF','CTR','REC','PAY','OVP','DSC','EXX')";

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_PAY()
		{
			AssertProcessChanges(TransactionTypes.Payment);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_REC()
		{
			AssertProcessChanges(TransactionTypes.Receipt);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_TRF()
		{
			AssertProcessChanges(TransactionTypes.Transfer);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_CTR()
		{
			AssertProcessChanges(TransactionTypes.Contra);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_JNL()
		{
			AssertProcessChanges(TransactionTypes.Journal);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_DSC()
		{
			AssertProcessChanges(TransactionTypes.Discount, true);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_OVP()
		{
			AssertProcessChanges(TransactionTypes.Overpayment, true);
		}

		[TestDate(2023, 5, 14)]
		public void TestProcessChanges_EXX()
		{
			AssertProcessChanges(TransactionTypes.ExchangeDifference, true);
		}

		void AssertProcessChanges(string transactionType, bool isNeedMatchLink = false)
		{
			using (AccountingMasterFilesRegistry.Instance.GenerateJournalEntriesCDCStartDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new DateTime(2023, 5, 10)))
			{
				var changeTable = CreateProcessHeaderTable();

				CreateRow(changeTable, DateTime.Now, LedgerTypes.AccountsPayable, transactionType, isNeedMatchLink);
				CreateRow(changeTable, DateTime.Now, LedgerTypes.AccountsReceivable, transactionType, isNeedMatchLink);

				NewDataChangeSubscriber().ProcessChanges(loggerMock.Object, changeTable);
				loggerMock.Verify(m => m.Log(LogType.Debug, It.Is<string>(s => s.StartsWith("AccTransactionHeader Received DataRows: 2"))), Times.Once);
				generalLedgerDataProcessorMock.Verify(m => m.ProcessData(It.Is<DataRow[]>(x => AssertChangeRow(x, AccTransactionHeaderSchema.PK.Name, 2))), Times.Exactly(1));
			}
		}

		public override void TestCustomFilter()
		{
			var subscriber = new AccTransactionHeaderWithARAPLedgerSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			var jCJNLRow = table.NewRow();
			jCJNLRow[AccTransactionHeaderSchema.AH_Ledger.Name] = LedgerTypes.JobCosting;
			jCJNLRow[AccTransactionHeaderSchema.AH_TransactionType.Name] = TransactionTypes.Journal;
			jCJNLRow[AccTransactionHeaderSchema.AH_TransactionCategory.Name] = TransactionCategory.SingleTransaction;
			table.Rows.Add(jCJNLRow);
			table.AcceptChanges();

			AssertEquals(3, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			RunCustomFilter(jCJNLRow, subscriber);
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
			AssertCollectionNotContains(jCJNLRow, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			if (shouldBeFiltered)
			{
				row[AccTransactionHeaderSchema.AH_Ledger.Name] = LedgerTypes.General;
				row[AccTransactionHeaderSchema.AH_TransactionType.Name] = TransactionTypes.Contra;
				row[AccTransactionHeaderSchema.AH_TransactionCategory.Name] = TransactionCategory.SingleTransaction;
			}
			else
			{
				row[AccTransactionHeaderSchema.AH_Ledger.Name] = LedgerTypes.AccountsPayable;
				row[AccTransactionHeaderSchema.AH_TransactionType.Name] = TransactionTypes.Contra;
				row[AccTransactionHeaderSchema.AH_TransactionCategory.Name] = TransactionCategory.SingleTransaction;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { AccTransactionHeaderSchema.AH_Ledger, AccTransactionHeaderSchema.AH_TransactionType, AccTransactionHeaderSchema.AH_TransactionCategory };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
