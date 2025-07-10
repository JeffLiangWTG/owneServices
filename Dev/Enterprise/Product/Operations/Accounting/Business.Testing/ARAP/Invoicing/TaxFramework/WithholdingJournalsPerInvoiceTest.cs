using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	public class WithholdingJournalsPerInvoiceTest : TestCaseWithFactory
	{
		public void TestConstructor_ParentInvoiceIsInitialized()
		{
			var parentInvoice = Factory.New<APInvoice>();
			var journalForDisplay = new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty);

			var result = new WithholdingJournalsPerInvoice(parentInvoice, new[] { journalForDisplay });

			AssertEquals(parentInvoice, result.ParentInvoice);
		}

		public void TestConstructor_JournalsAreInitialized()
		{
			var parentInvoice = Factory.New<APInvoice>();
			var journalForDisplay1 = new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Today);
			var journalForDisplay2 = new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.Empty);
			var journalForDisplay3 = new WithholdingJournalForDisplay(Factory.New<APJournal>(), ZDate.BrettsBirthday);

			var expectedJournals = new[] { journalForDisplay1, journalForDisplay2, journalForDisplay3 };

			var result = new WithholdingJournalsPerInvoice(parentInvoice, expectedJournals);

			AssertContainsExactElementsInAnyOrder(expectedJournals, result.Journals);
		}

		public void TestConstructor_ThrowsArgumentNullException_WhenParentNull()
		{
			var withholdingJournals = new List<WithholdingJournalForDisplayCollection>().AsReadOnly();

			var exception = AssertExceptionThrown<ArgumentNullException>("", "Value cannot be null.\r\nParameter name: parentInvoice", () => new WithholdingJournalsPerInvoice(null, Array.Empty<WithholdingJournalForDisplay>()));
		}

		public void TestConstructor_ThrowsArgumentNullException_WhenJournalsNull()
		{
			var parentInvoice = Factory.NewWithValidTestData<APInvoice>();

			var exception = AssertExceptionThrown<ArgumentNullException>("", "Value cannot be null.\r\nParameter name: journals", () => new WithholdingJournalsPerInvoice(parentInvoice, null));
		}
	}
}
