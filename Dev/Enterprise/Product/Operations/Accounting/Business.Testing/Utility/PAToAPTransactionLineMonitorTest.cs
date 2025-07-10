using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Testing
{
	public class PAToAPTransactionLineMonitorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var header = CreateTransactionPendingAllocation();

			AccountingConfigurationRegistry.Instance.EnableTransactionLineMonitor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			PAToAPTransactionLineMonitor.Create(header);

			AssertNull("The feature registry is not enabled.", PAToAPTransactionLineMonitor.GetInstance(header));

			AccountingConfigurationRegistry.Instance.EnableTransactionLineMonitor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			PAToAPTransactionLineMonitor.Create(header);

			AssertNotNull(PAToAPTransactionLineMonitor.GetInstance(header));
		}

		public void TestCreate_InvalidLedgerOrTransactionType()
		{
			var header = new TransactionCreator().CreateTransaction(Factory, LedgerTypes.CashBook, TransactionTypes.DirectPayment);
			PAToAPTransactionLineMonitor.Create(header);

			AssertNull("The ledger or transaction type is not valid for monitoring.", PAToAPTransactionLineMonitor.GetInstance(header));
		}

		public void TestGetInstance()
		{
			var header1 = CreateTransactionPendingAllocation();
			var header2 = CreateTransactionPendingAllocation();

			AssertNull("The moniter of header 1 is not created", PAToAPTransactionLineMonitor.GetInstance(header1));
			AssertNull("The moniter of header 2 is not created", PAToAPTransactionLineMonitor.GetInstance(header2));

			PAToAPTransactionLineMonitor.Create(header1);

			AssertNotNull(PAToAPTransactionLineMonitor.GetInstance(header1));
			AssertNull("The moniter of header 2 is not created", PAToAPTransactionLineMonitor.GetInstance(header2));

			PAToAPTransactionLineMonitor.Create(header2);

			AssertNotNull(PAToAPTransactionLineMonitor.GetInstance(header1));
			AssertNull("The moniter of header 1 is already created", PAToAPTransactionLineMonitor.GetInstance(header2));
		}

		public void TestRecordLineCount()
		{
			var header = CreateTransactionPendingAllocation();
			PAToAPTransactionLineMonitor.Create(header);
			var monitor = PAToAPTransactionLineMonitor.GetInstance(header);

			AssertEquals("Precondition",
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:
N/A

Total calls of adding line: 0
Total calls of removing line: 0", monitor.GetInfo().Trim());

			monitor.RecordLineCount("Test1", 1);

			AssertEquals("Add one line count info",
@"Last 10 counts of lines:
Test1: 1

Stack trace of last line remove:
N/A

Total calls of adding line: 0
Total calls of removing line: 0", monitor.GetInfo().Trim());

			for (int i = 2; i <= 12; i++)
			{
				monitor.RecordLineCount("Test" + i, i);
			}

			AssertEquals("Test exceeding the line count capacity",
@"Last 10 counts of lines:
Test3: 3
Test4: 4
Test5: 5
Test6: 6
Test7: 7
Test8: 8
Test9: 9
Test10: 10
Test11: 11
Test12: 12

Stack trace of last line remove:
N/A

Total calls of adding line: 0
Total calls of removing line: 0", monitor.GetInfo().Trim());
		}

		public void TestRecordLineItemAdd()
		{
			var header = CreateTransactionPendingAllocation();
			PAToAPTransactionLineMonitor.Create(header);
			var monitor = PAToAPTransactionLineMonitor.GetInstance(header);

			monitor.RecordLineItemChange(new CargoWise.EntityFramework.CollectionCountChangedEventArgs(true, null));
			monitor.RecordLineItemChange(new CargoWise.EntityFramework.CollectionCountChangedEventArgs(true, null));

			AssertEquals("Test exceeding the line count capacity",
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:
N/A

Total calls of adding line: 2
Total calls of removing line: 0", monitor.GetInfo().Trim());
		}

		public void TestRecordLineItemRemove()
		{
			var header = CreateTransactionPendingAllocation();
			PAToAPTransactionLineMonitor.Create(header);
			var monitor = PAToAPTransactionLineMonitor.GetInstance(header);

			monitor.RecordLineItemChange(new CargoWise.EntityFramework.CollectionCountChangedEventArgs(false, null));

			AssertEquals(true, Enterprise.Accounting.Business.TestObjectCreator.IsContainSubStrings(monitor.GetInfo().Trim(),
@"Last 10 counts of lines:
N/A

Stack trace of last line remove:",
				"at Enterprise.Accounting.Business.PAToAPTransactionLineMonitor.RecordLineItemChange(CollectionCountChangedEventArgs e)",
@"Total calls of adding line: 0
Total calls of removing line: 1"));
		}

		public void TestGetInfo()
		{
			var header = CreateTransactionPendingAllocation();
			PAToAPTransactionLineMonitor.Create(header);
			var monitor = PAToAPTransactionLineMonitor.GetInstance(header);

			monitor.RecordLineCount("Test1", 1);
			monitor.RecordLineItemChange(new CargoWise.EntityFramework.CollectionCountChangedEventArgs(true, null));

			AssertEquals("Test exceeding the line count capacity", @"
Last 10 counts of lines:
Test1: 1

Stack trace of last line remove:
N/A

Total calls of adding line: 1
Total calls of removing line: 0".Trim(), monitor.GetInfo().Trim());
		}

		protected override void SetUp()
		{
			base.SetUp();
			AccountingConfigurationRegistry.Instance.EnableTransactionLineMonitor.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		TransactionPendingAllocation CreateTransactionPendingAllocation() => TestObjectCreator.CreateTransactionPendingAllocation("000001", TestObjectCreator.AALSHI, 100m);

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
