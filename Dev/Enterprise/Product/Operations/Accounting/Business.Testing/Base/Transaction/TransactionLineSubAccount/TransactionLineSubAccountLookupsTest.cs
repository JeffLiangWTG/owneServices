using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class TransactionLineSubAccountLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubAccountList()
		{
			TransactionLineSubAccount.AL1_SubClassParentTableCode = OrgHeaderSchema.Constants.Prefix;
			AssertType("SubAccountList should be OrgHeaderCollection when type is 'Organization'", typeof(OrgHeaderCollection), Lookups.SubAccountList);

			TransactionLineSubAccount.AL1_SubClassParentTableCode = AccGroupsSchema.Constants.Prefix;
			AssertType("SubAccountList should be AccGroupsCollection when type is 'Sales/Expense Groups'", typeof(AccGroupsCollection), Lookups.SubAccountList);

			TransactionLineSubAccount.AL1_SubClassParentTableCode = GlbStaffSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbStaffAndResourceCollection when type is 'Staff and Resources'", typeof(GlbStaffAndResourceCollection), Lookups.SubAccountList);

			TransactionLineSubAccount.AL1_SubClassParentTableCode = GlbGroupSchema.Constants.Prefix;
			AssertType("SubAccountList should be GlbGroupCollection when type is 'Staff Group'", typeof(GlbGroupCollection), Lookups.SubAccountList);
		}

		public void TestSubAccountTypeList()
		{
			AssertEquals(@"ORG - Organization
SEG - Sales/Expense Groups
STR - Staff and Resources
SGP - Staff Group", Lookups.SubAccountTypeList.ElementsAsString);
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

		protected override void SetUp()
		{
			base.SetUp();
			var line = CreateDependentTransactionLine();
			TransactionLineSubAccount = line.SubAccounts.AddNew();
			Lookups = TransactionLineSubAccount.Lookups;
		}
		TransactionLineSubAccountLookups Lookups;
		TransactionLineSubAccount TransactionLineSubAccount;

		public class ARInvoiceTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARInvoice);
			}
		}

		public class APInvoiceTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APInvoice);
			}
		}

		public class ARAdjustmentNoteTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARAdjustmentNote);
			}
		}

		public class APAdjustmentNoteTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APAdjustmentNote);
			}
		}

		public class ARCreditNoteTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(ARCreditNote);
			}
		}

		public class APCreditNoteTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(APCreditNote);
			}
		}

		public class UACreditNoteTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(UACreditNote);
			}
		}

		public class UAInvoiceTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(UAInvoice);
			}
		}

		public class DirectReceiptTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(DirectReceipt);
			}
		}

		public class DirectPaymentTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(DirectPayment);
			}
		}

		public class GLJournalTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(GLJournal);
			}
		}

		public class FCBAdjustmentJournalTransactionLineSubAccountLookupsTest : TransactionLineSubAccountLookupsTest
		{
			protected override Type GetExpectedTransactionHeader()
			{
				return typeof(FCBAdjustmentJournal);
			}
		}
	}
}
