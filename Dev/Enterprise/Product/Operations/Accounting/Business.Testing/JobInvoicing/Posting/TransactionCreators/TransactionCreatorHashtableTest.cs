using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class TransactionCreatorHashtableTest : TestCaseWithFactory
	{
		public void TestAddAPInvoiceDuplicated()
		{
			var creatorHashtable = new TransactionCreatorHashtable();
			APInvoice invoice = Factory.New<APInvoice>();
			creatorHashtable.AddAPInvoice(invoice, "CLIENT1", "1");
			AssertExceptionThrown<DuplicatedInvoiceException>(() => creatorHashtable.AddAPInvoice(invoice, "CLIENT1", "1"));
		}

		public void TestCompany()
		{
			var creatorHashtable = new TransactionCreatorHashtable();
			AssertEquals("if there is no transaction, company fallback to current login company", GlbCompany.CurrentCompany.PK, creatorHashtable.Company.PK);

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_GC = newCompany.PK;
			creatorHashtable.AddAPInvoice(invoice, "CLIENT1", "1");
			AssertEquals("company should use the transaction company", newCompany.PK, creatorHashtable.Company.PK);
		}

		#region AP Invoice Tests

		#region TEST: Add AP Invoice

		public void TestUATransactionCount()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			UAInvoice invoice1 = Factory.New<UAInvoice>();
			UAInvoice invoice2 = Factory.New<UAInvoice>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";

			AssertEquals("Key Count", 0, creatorHashtable.UATransactionsCount);

			creatorHashtable.AddAPInvoice(invoice1, client1, "1");
			creatorHashtable.AddAPInvoice(invoice2, client2, "1");

			AssertEquals("Key Count", 2, creatorHashtable.UATransactionsCount);
		}

		public void TestAddAPInvoice()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APInvoice invoice1 = Factory.New<APInvoice>();
			APInvoice invoice2 = Factory.New<APInvoice>();
			APInvoice invoice3 = Factory.New<APInvoice>();
			APInvoice invoice4 = Factory.New<APInvoice>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client4 = "CLIENT4";

			AssertEquals("Key Count", 0, creatorHashtable.APTransactionsCount);

			creatorHashtable.AddAPInvoice(invoice1, client1, "1");
			creatorHashtable.AddAPInvoice(invoice2, client2, "2");
			creatorHashtable.AddAPInvoice(invoice3, client4, "3");
			creatorHashtable.AddAPInvoice(invoice4, client1, "2");

			AssertEquals("Key Count", 4, creatorHashtable.APTransactionsCount);
		}

		#endregion

		#region TEST: Retrieve AP Invoice

		public void TestRetrieveAPInvoice()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APInvoice invoice1 = Factory.New<APInvoice>();
			APInvoice invoice2 = Factory.New<APInvoice>();
			APInvoice invoice3 = Factory.New<APInvoice>();
			APInvoice invoice4 = Factory.New<APInvoice>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client3 = "CLIENT3";
			string client4 = "CLIENT4";

			creatorHashtable.AddAPInvoice(invoice1, client1, "1");
			creatorHashtable.AddAPInvoice(invoice2, client2, "2");
			creatorHashtable.AddAPInvoice(invoice3, client4, "3");
			creatorHashtable.AddAPInvoice(invoice4, client1, "2");

			APInvoice retrievedInvoice = creatorHashtable.RetrieveAPInvoice(client4, "3");
			AssertEquals("Same Invoice", invoice3.PK, retrievedInvoice.PK);

			retrievedInvoice = creatorHashtable.RetrieveAPInvoice(client1, "1");
			AssertEquals("Same Invoice", invoice1.PK, retrievedInvoice.PK);

			retrievedInvoice = creatorHashtable.RetrieveAPInvoice(client2, "2");
			AssertEquals("Same Invoice", invoice2.PK, retrievedInvoice.PK);

			retrievedInvoice = creatorHashtable.RetrieveAPInvoice(client1, "2");
			AssertEquals("Same Invoice", invoice4.PK, retrievedInvoice.PK);

			retrievedInvoice = creatorHashtable.RetrieveAPInvoice(client3, "9");
			AssertNull("No Invoice Returned", retrievedInvoice);
		}

		#endregion

		#region TEST: Remove AP Invoice

		public void TestRemoveAPInvoice()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APInvoice invoice1 = Factory.New<APInvoice>();
			APInvoice invoice2 = Factory.New<APInvoice>();
			APInvoice invoice3 = Factory.New<APInvoice>();
			APInvoice invoice4 = Factory.New<APInvoice>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client4 = "CLIENT4";

			AssertEquals("Key Count", 0, creatorHashtable.APTransactionsCount);

			creatorHashtable.AddAPInvoice(invoice1, client1, "1");
			creatorHashtable.AddAPInvoice(invoice2, client2, "2");
			creatorHashtable.AddAPInvoice(invoice3, client4, "3");
			creatorHashtable.AddAPInvoice(invoice4, client1, "2");

			AssertEquals("Key Count", 4, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPInvoice(client1, "2");
			AssertEquals("Key Count", 3, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPInvoice(client2, "2");
			AssertEquals("Key Count", 2, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPInvoice(client4, "3");
			AssertEquals("Key Count", 1, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPInvoice(client1, "1");
			AssertEquals("Key Count", 0, creatorHashtable.APTransactionsCount);
		}

		#endregion

		#region TEST: Contains AP Invoice

		public void TestContainsAPInvoice()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APInvoice invoice1 = Factory.New<APInvoice>();
			APInvoice invoice2 = Factory.New<APInvoice>();
			APInvoice invoice3 = Factory.New<APInvoice>();
			APInvoice invoice4 = Factory.New<APInvoice>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client4 = "CLIENT4";

			creatorHashtable.AddAPInvoice(invoice1, client1, "1");
			creatorHashtable.AddAPInvoice(invoice2, client2, "2");
			creatorHashtable.AddAPInvoice(invoice3, client4, "3");
			creatorHashtable.AddAPInvoice(invoice4, client1, "2");

			AssertEquals("Contains AP Invoice", true, creatorHashtable.ContainsAPInvoice(client1, "2"));
			AssertEquals("Contains AP Invoice", true, creatorHashtable.ContainsAPInvoice(client2, "2"));
			AssertEquals("Contains AP Invoice", true, creatorHashtable.ContainsAPInvoice(client4, "3"));
			AssertEquals("Contains AP Invoice", true, creatorHashtable.ContainsAPInvoice(client1, "1"));

			AssertEquals("Contains AP Invoice", false, creatorHashtable.ContainsAPInvoice(client1, "3"));
			AssertEquals("Contains AP Invoice", false, creatorHashtable.ContainsAPInvoice(client4, "2"));
		}

		#endregion

		#endregion

		#region AP Credit Note Tests

		#region TEST: Add AP Credit Note

		public void TestAddAPCreditNote()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APCreditNote creditNote1 = Factory.New<APCreditNote>();
			APCreditNote creditNote2 = Factory.New<APCreditNote>();
			APCreditNote creditNote3 = Factory.New<APCreditNote>();
			APCreditNote creditNote4 = Factory.New<APCreditNote>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client4 = "CLIENT4";

			AssertEquals("Key Count", 0, creatorHashtable.APTransactionsCount);

			creatorHashtable.AddAPCreditNote(creditNote1, client1, "1");
			creatorHashtable.AddAPCreditNote(creditNote2, client2, "2");
			creatorHashtable.AddAPCreditNote(creditNote3, client4, "3");
			creatorHashtable.AddAPCreditNote(creditNote4, client1, "2");

			AssertEquals("Key Count", 4, creatorHashtable.APTransactionsCount);
		}

		#endregion

		#region TEST: Retrieve AP Credit Note

		public void TestRetrieveAPCreditNote()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APCreditNote creditNote1 = Factory.New<APCreditNote>();
			APCreditNote creditNote2 = Factory.New<APCreditNote>();
			APCreditNote creditNote3 = Factory.New<APCreditNote>();
			APCreditNote creditNote4 = Factory.New<APCreditNote>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client3 = "CLIENT3";
			string client4 = "CLIENT4";

			creatorHashtable.AddAPCreditNote(creditNote1, client1, "1");
			creatorHashtable.AddAPCreditNote(creditNote2, client2, "2");
			creatorHashtable.AddAPCreditNote(creditNote3, client4, "3");
			creatorHashtable.AddAPCreditNote(creditNote4, client1, "2");

			APCreditNote retrievedCreditNote = creatorHashtable.RetrieveAPCreditNote(client4, "3");
			AssertEquals("Same CreditNote", creditNote3.PK, retrievedCreditNote.PK);

			retrievedCreditNote = creatorHashtable.RetrieveAPCreditNote(client1, "1");
			AssertEquals("Same CreditNote", creditNote1.PK, retrievedCreditNote.PK);

			retrievedCreditNote = creatorHashtable.RetrieveAPCreditNote(client2, "2");
			AssertEquals("Same CreditNote", creditNote2.PK, retrievedCreditNote.PK);

			retrievedCreditNote = creatorHashtable.RetrieveAPCreditNote(client1, "2");
			AssertEquals("Same CreditNote", creditNote4.PK, retrievedCreditNote.PK);

			retrievedCreditNote = creatorHashtable.RetrieveAPCreditNote(client3, "9");
			AssertNull("No CreditNote Returned", retrievedCreditNote);
		}

		#endregion

		#region TEST: Remove AP Credit Note

		public void TestRemoveAPCreditNote()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APCreditNote creditNote1 = Factory.New<APCreditNote>();
			APCreditNote creditNote2 = Factory.New<APCreditNote>();
			APCreditNote creditNote3 = Factory.New<APCreditNote>();
			APCreditNote creditNote4 = Factory.New<APCreditNote>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client4 = "CLIENT4";

			AssertEquals("Key Count", 0, creatorHashtable.APTransactionsCount);

			creatorHashtable.AddAPCreditNote(creditNote1, client1, "1");
			creatorHashtable.AddAPCreditNote(creditNote2, client2, "2");
			creatorHashtable.AddAPCreditNote(creditNote3, client4, "3");
			creatorHashtable.AddAPCreditNote(creditNote4, client1, "2");

			AssertEquals("Key Count", 4, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPCreditNote(client1, "2");
			AssertEquals("Key Count", 3, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPCreditNote(client2, "2");
			AssertEquals("Key Count", 2, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPCreditNote(client4, "3");
			AssertEquals("Key Count", 1, creatorHashtable.APTransactionsCount);

			creatorHashtable.RemoveAPCreditNote(client1, "1");
			AssertEquals("Key Count", 0, creatorHashtable.APTransactionsCount);
		}

		#endregion

		#region TEST: Contains AP Credit Note

		public void TestContainsAPCreditNote()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();

			APCreditNote creditNote1 = Factory.New<APCreditNote>();
			APCreditNote creditNote2 = Factory.New<APCreditNote>();
			APCreditNote creditNote3 = Factory.New<APCreditNote>();
			APCreditNote creditNote4 = Factory.New<APCreditNote>();

			string client1 = "CLIENT1";
			string client2 = "CLIENT2";
			string client4 = "CLIENT4";

			creatorHashtable.AddAPCreditNote(creditNote1, client1, "1");
			creatorHashtable.AddAPCreditNote(creditNote2, client2, "2");
			creatorHashtable.AddAPCreditNote(creditNote3, client4, "3");
			creatorHashtable.AddAPCreditNote(creditNote4, client1, "2");

			AssertEquals("Contains AP CreditNote", true, creatorHashtable.ContainsAPCreditNote(client1, "2"));
			AssertEquals("Contains AP CreditNote", true, creatorHashtable.ContainsAPCreditNote(client2, "2"));
			AssertEquals("Contains AP CreditNote", true, creatorHashtable.ContainsAPCreditNote(client4, "3"));
			AssertEquals("Contains AP CreditNote", true, creatorHashtable.ContainsAPCreditNote(client1, "1"));

			AssertEquals("Contains AP CreditNote", false, creatorHashtable.ContainsAPCreditNote(client1, "3"));
			AssertEquals("Contains AP CreditNote", false, creatorHashtable.ContainsAPCreditNote(client2, "1"));
			AssertEquals("Contains AP CreditNote", false, creatorHashtable.ContainsAPCreditNote(client4, "2"));
		}

		#endregion

		#endregion

		public void TestRemoveAndDeleteAll()
		{
			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();
			string client = "CLIENT1";

			APInvoice invoice = Factory.New<APInvoice>();
			creatorHashtable.AddAPInvoice(invoice, client, "1");

			APPaymentApprovalWithoutAuthorisation payment = Factory.New<APPaymentApprovalWithoutAuthorisation>();
			creatorHashtable.AddAPPaymentApproval(payment, client, "BANK", ReceiptTypes.Cash, "REF", "JOB");

			TransactionMatchLinkCollection matchLinkCollection = new TransactionMatchLinkCollection(Factory);
			TransactionMatchLink matchLink = matchLinkCollection.AddNew();
			creatorHashtable.MatchLinks.Add(ZGuid.NewZGuid(), matchLinkCollection);

			AssertEquals("Precondition: Transaction count.", 1, creatorHashtable.Count);
			AssertEquals("Precondition: Payment Approvals count.", 1, creatorHashtable.GetAllAPPaymentApprovals().Length);
			AssertEquals("Precondition: MatchLinks count.", 1, creatorHashtable.MatchLinks.Count);

			creatorHashtable.RemoveAndDeleteAll();
			AssertEquals("Transaction count.", 0, creatorHashtable.Count);
			AssertEquals("Invoice must be deleted.", true, invoice.IsDeleted);
			AssertEquals("Payment Approvals count.", 0, creatorHashtable.GetAllAPPaymentApprovals().Length);
			AssertEquals("Payment Approval must be deleted.", true, payment.IsDeleted);
			AssertEquals("Match Links count.", 0, creatorHashtable.MatchLinks.Count);
			AssertEquals("Transaction Match Link must be deleted.", true, matchLink.IsDeleted);
		}

		public void TestGetAllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes()
		{
			var apInvoice = Factory.New<APInvoice>();
			var apCreditNote = Factory.New<APCreditNote>();
			var apInvoiceConvertedToUA = Factory.New<APInvoice>();
			apInvoiceConvertedToUA.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			var apCreditNoteConvertedToUA = Factory.New<APCreditNote>();
			apCreditNoteConvertedToUA.AH_Ledger = LedgerTypes.UnapprovedPayableTransactions;
			var uaInvoice = Factory.New<UAInvoice>();
			var uaCreditNote = Factory.New<UACreditNote>();

			TransactionCreatorHashtable creatorHashtable = new TransactionCreatorHashtable();
			creatorHashtable.AddAPInvoice(apInvoice, "Client1", "INV1");
			creatorHashtable.AddAPInvoice(apInvoiceConvertedToUA, "Client2", "INV1");
			creatorHashtable.AddAPCreditNote(apCreditNote, "Client3", "INV1");
			creatorHashtable.AddAPCreditNote(apCreditNoteConvertedToUA, "Client4", "INV1");
			creatorHashtable.AddAPInvoice(uaInvoice, "Client5", "INV1");
			creatorHashtable.AddAPCreditNote(uaCreditNote, "Client6", "INV1");

			AssertEquals("AllAPInvoicesAndCreditNotes count", creatorHashtable.Count, creatorHashtable.GetAllAPInvoicesAndCreditNotes().Length);
			AssertContainsExactElementsInAnyOrder("AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes",
				new InvoicingBase[] { apInvoice, apCreditNote },
				creatorHashtable.GetAllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes());
		}

		public void TestContainsARTransaction()
		{
			var creatorHashtable = new TransactionCreatorHashtable();

			var arInvoice1 = Factory.New<ARInvoice>();
			var arInvoice2 = Factory.New<ARInvoice>();
			var arCreditNote1 = Factory.New<ARCreditNote>();
			var arCreditNote2 = Factory.New<ARCreditNote>();

			creatorHashtable.AddARInvoice(arInvoice1);
			creatorHashtable.AddARInvoice(arInvoice2);
			creatorHashtable.AddARInvoice(arCreditNote1);
			creatorHashtable.AddARInvoice(arCreditNote2);

			AssertEquals("Contains AR Invoice", true, creatorHashtable.ContainsARTransaction(arInvoice1.PK));
			AssertEquals("Contains AR Invoice", true, creatorHashtable.ContainsARTransaction(arInvoice2.PK));
			AssertEquals("Contains AR CreditNote", true, creatorHashtable.ContainsARTransaction(arCreditNote1.PK));
			AssertEquals("Contains AR CreditNote", true, creatorHashtable.ContainsARTransaction(arCreditNote2.PK));
		}
	}
}
