using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(TransactionLineSubAccountCollection))]
	public abstract class TransactionLineSubAccountCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSubAccountsSequence()
		{
			var line = CreateDependentTransactionLine();

			var subAccount1 = line.SubAccounts.AddNew();
			var subAccount2 = line.SubAccounts.AddNew();
			var subAccount3 = line.SubAccounts.AddNew();
			var subAccount4 = line.SubAccounts.AddNew();

			subAccount1.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			subAccount3.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			subAccount4.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;

			line.SubAccounts.Load();
			AssertEquals("The first sub account sequence should be 4", 4, subAccount1.AL1_Calc_Sequence);
			AssertEquals("The second sub account sequence should be 3", 3, subAccount2.AL1_Calc_Sequence);
			AssertEquals("The third sub account sequence should be 2", 2, subAccount3.AL1_Calc_Sequence);
			AssertEquals("The fourth sub account sequence should be 1", 1, subAccount4.AL1_Calc_Sequence);

			subAccount3.Delete();

			line.SubAccounts.Load();
			AssertEquals("The first sub account sequence should be 4 when table code is 'GG'", 4, subAccount1.AL1_Calc_Sequence);
			AssertEquals("The second sub account sequence should be 3 when table code is 'GS'", 3, subAccount2.AL1_Calc_Sequence);
			AssertNull("The third sub account has been deleted", line.SubAccounts.Cast<TransactionLineSubAccount>().FirstOrDefault(x => x == subAccount3));
			AssertEquals("The fourth sub account sequence should be 1 when table code is 'OH'", 1, subAccount4.AL1_Calc_Sequence);
		}

		public void TestFirstSubAccount()
		{
			var line = CreateDependentTransactionLine();
			var subAccount1 = line.SubAccounts.AddNew();
			var subAccount2 = line.SubAccounts.AddNew();

			subAccount1.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;

			AssertEquals("Pre-condition", 1, subAccount1.AL1_Calc_Sequence);
			AssertEquals("Pre-condition", 2, subAccount2.AL1_Calc_Sequence);
			AssertEquals("Pre-condition", subAccount1, line.SubAccounts.FirstSubAccount);

			subAccount1.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("First Sub Account", subAccount2, line.SubAccounts.FirstSubAccount);
		}

		public void TestSecondSubAccount()
		{
			var line = CreateDependentTransactionLine();
			var subAccount1 = line.SubAccounts.AddNew();
			var subAccount2 = line.SubAccounts.AddNew();

			subAccount1.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;

			AssertEquals("Pre-condition", 1, subAccount1.AL1_Calc_Sequence);
			AssertEquals("Pre-condition", 2, subAccount2.AL1_Calc_Sequence);
			AssertEquals("Pre-condition", subAccount2, line.SubAccounts.SecondSubAccount);

			subAccount1.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertEquals("Second Sub Account", subAccount1, line.SubAccounts.SecondSubAccount);
		}

		public void TestUpdateSubAccountCacheKey()
		{
			var line = CreateDependentTransactionLine();
			var subAccount1 = line.SubAccounts.AddNew();
			var subAccount2 = line.SubAccounts.AddNew();

			subAccount1.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			subAccount2.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;

			var subAccountCacheKey = GetValue("SubAccountCacheKey");

			line.SubAccounts.UpdateSubAccountCacheKey();

			Assert("SubAccountCacheKey will be changed", subAccountCacheKey != GetValue("SubAccountCacheKey"));

			object GetValue(String proprtryName)
			{
				return line.SubAccounts.GetType().GetProperty(proprtryName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase).GetValue(line.SubAccounts);
			}
		}

		protected abstract Type GetExpectedTransactionHeader();

		DependentTransactionLine CreateDependentTransactionLine()
		{
			var header = (TransactionHeaderWithLines)Factory.New(GetExpectedTransactionHeader());
			header.FillWithValidTestData();

			var line = header.Lines.AddNew();
			line.FillWithValidTestData();
			return line;
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class ARInvoiceTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<ARInvoiceLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARInvoice);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class APInvoiceTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<APInvoiceLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APInvoice);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class ARAdjustmentNoteTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<ARAdjustmentNoteLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARAdjustmentNote);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class APAdjustmentNoteTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<APAdjustmentNoteLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APAdjustmentNote);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class ARCreditNoteTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<ARCreditNoteLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARCreditNote);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class APCreditNoteTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<APCreditNoteLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APCreditNote);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class UACreditNoteTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<UACreditNoteLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(UACreditNote);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class UAInvoiceTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<UAInvoiceLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(UAInvoice);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class DirectReceiptTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<DirectReceiptLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(DirectReceipt);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class DirectPaymentTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<DirectPaymentLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(DirectPayment);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class GLJournalTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<GLJournalLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(GLJournal);
			}
		}

		[TestedType(typeof(TransactionLineSubAccountCollection))]
		public class FCBAdjustmentJournalTransactionLineSubAccountCollectionTest : TransactionLineSubAccountCollectionTest
		{
			protected override BusinessObjectCollection GetCollectionToTest()
			{
				return new TransactionLineSubAccountCollection(Factory.NewWithValidTestData<FCBAdjustmentJournalLine>());
			}

			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(FCBAdjustmentJournal);
			}
		}
	}
}
