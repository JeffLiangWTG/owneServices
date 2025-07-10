using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ARInvoiceNumberGeneratorTest : TestCaseWithFactory
	{
		#region TestAllocationOfInvoiceNumber Invoice Tests
		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_ARInvoice()
		{
			var nonClashingInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, null);
			nonClashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			nonClashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_APCreditNote()
		{
			var nonClashingInvoice = TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.AUD, 1, null);
			nonClashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			nonClashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_APInvoice()
		{
			var clashingInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1, null);
			clashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			clashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_IncompleteInvoice()
		{
			var clashingInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1, null);
			clashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			clashingInvoice.AH_TransactionNum = "CASSAUD121101";
			clashingInvoice.SaveAsIncomplete();
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_UnapprovedInvoice()
		{
			var clashingInvoice = TestObjectCreator.CreateInvoice(typeof(UAInvoice), TestObjectCreator.AUD, 1, null);
			clashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			clashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_InvoicePendingAllocation()
		{
			var unallocatedTransaction = Factory.New<TransactionPendingAllocation>();
			unallocatedTransaction.AH_TransactionNum = "CASSAUD121101";
			unallocatedTransaction.AH_OH = TestObjectCreator.AALSHI.PK;
			unallocatedTransaction.AH_OSExTaxAmount = 100m;
			unallocatedTransaction.AH_PostDate = ZDateTime.Today;
			unallocatedTransaction.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
			unallocatedTransaction.AH_DueDate = ZDateTime.Today.AddDays(1);
			unallocatedTransaction.AH_OSTaxAmount = 10m;
			unallocatedTransaction.AH_Desc = "Description";
			AssertEquals("Pending allocation is for an invoice", TransactionTypes.InvoicePendingAllocation, unallocatedTransaction.AH_TransactionType);
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_JobInvoicing()
		{
			var job = TestObjectCreator.CreateJob("123", TestObjectCreator.AALSHI, 1M, TestObjectCreator.AALSHI, 1M);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, -100M,
				TestObjectCreator.AALSHI, "CASSAUD121101", TestObjectCreator.AUD, -100M, TestObjectCreator.ABIGAS);
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		#endregion

		#region TestAllocationOfInvoiceNumber Credit Note Tests

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_ARCreditNote()
		{
			var nonClashingInvoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.AUD, 1, null);
			nonClashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			nonClashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_APInvoice()
		{
			var nonClashingInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1, null);
			nonClashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			nonClashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_APCreditNote()
		{
			var clashingInvoice = TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.AUD, 1, null);
			clashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			clashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_IncompleteCreditNote()
		{
			var clashingInvoice = TestObjectCreator.CreateInvoice(typeof(APCreditNote), TestObjectCreator.AUD, 1, null);
			clashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			clashingInvoice.AH_TransactionNum = "CASSAUD121101";
			clashingInvoice.SaveAsIncomplete();
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_UnapprovedCreditNote()
		{
			var clashingInvoice = TestObjectCreator.CreateInvoice(typeof(UACreditNote), TestObjectCreator.AUD, 1, null);
			clashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			clashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_CreditNotePendingAllocation()
		{
			var unallocatedTransaction = Factory.New<TransactionPendingAllocation>();
			unallocatedTransaction.AH_TransactionNum = "CASSAUD121101";
			unallocatedTransaction.AH_OH = TestObjectCreator.AALSHI.PK;
			unallocatedTransaction.AH_OSExTaxAmount = -100m;
			unallocatedTransaction.AH_PostDate = ZDateTime.Today;
			unallocatedTransaction.AH_InvoiceDate = ZDateTime.Today.AddDays(-1);
			unallocatedTransaction.AH_DueDate = ZDateTime.Today.AddDays(1);
			unallocatedTransaction.AH_OSTaxAmount = -10m;
			unallocatedTransaction.AH_Desc = "Description";
			Factory.Save();
			AssertEquals("Pending allocation is for a credit note", TransactionTypes.CreditNotePendingAllocation, unallocatedTransaction.AH_TransactionType);
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_JobInvoicing()
		{
			var job = TestObjectCreator.CreateJob("123", TestObjectCreator.AALSHI, 1M, TestObjectCreator.AALSHI, 1M);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "CC1", TestObjectCreator.AUD, -100M,
				TestObjectCreator.AALSHI, "CASSAUD121101", TestObjectCreator.AUD, -100M, TestObjectCreator.ABIGAS);
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101/A", transactionNumber);
		}
		#endregion

		public void TestGetNextLiteralInvoiceNumber_WithSuffix()
		{
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.AUD, 1, null);
			apInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			apInvoice.AH_TransactionNum = "XYZ1-C";
			var arInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, null);
			arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			var apInvoice2 = TestObjectCreator.CreateAPInvoice<APInvoice>("", TestObjectCreator.AUD, 1, 100, 10, 0, 100, 10, 0);
			apInvoice2.AH_OH = TestObjectCreator.AALSHI.PK;
			string result = InvoiceLiteralNumberGenerator.GetNextLiteralInvoiceNumber_WithSuffix(Factory, apInvoice2, "XYZ1", "-C");
			AssertEquals("Invoice Number Correct", "XYZ1-C/A", result);
			result = InvoiceLiteralNumberGenerator.GetNextLiteralInvoiceNumber_WithSuffix(Factory, apInvoice2, "ABC1", "-C");
			AssertEquals("Invoice Number Correct", "ABC1-C", result);
		}

		public void TestGetLetterRepresentation()
		{
			AssertEquals("", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(-1));
			AssertEquals("", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(0));
			AssertEquals("/A", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(1));
			AssertEquals("/Z", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(26));
			AssertEquals("/AA", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(27));
			AssertEquals("/BA", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(53));
			AssertEquals("/BZ", InvoiceLiteralNumberGenerator.GetLetterRepresentation_ForTestOnly(78));
		}

		public void TestGetNextJobInvoiceNumber()
		{
			AssertEquals("", InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(null, null));

			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_JobNum = "TestNum";

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice.AH_JH = testJob.PK;
			AssertEquals("TestNum", InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(invoice, testJob));
		}

		public void TestGetInvoiceFilter()
		{
			Job testJob = Factory.NewJobForTesting<Job>();
			testJob.JH_JobNum = "TestNum";
			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			invoice.AH_JH = testJob.PK;

			var query = InvoiceLiteralNumberGenerator.GetInvoiceFilter_ForTestOnly(invoice, testJob);
			Assert(query.AddOptionRecompileConditionally);
			AssertContains("OPTION (RECOMPILE)", query.GetAsWhereClause(true));
		}

		public void TestGetNextJobInvoiceNumberWhenAH_ConsolidatedInvoiceRefIsLiteralize()
		{
			using (var settings = TestEntityFrameworkSettings.Get())
			{
				settings.FieldsToLiteralize = AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.Name;

				AssertCollectionContains("AH_ConsolidatedInvoiceRef", ParameterSettingsCache.FieldsToLiteralize);

				var testJob = Factory.NewJobForTesting<Job>();
				testJob.JH_JobNum = "TestNum";

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
				invoice.AH_JH = testJob.PK;
				AssertEquals("TestNum", InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(invoice, testJob));
			}
		}

		public void TestConvertInvoiceNumbersSuffixesToInts()
		{
			var baseInvoiceNumber = "S0001";
			AssertContainsExactElementsInAnyOrder(new int[] { 0, 1 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001", "S0001/A" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 0, 1, 2 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001", "S0001/A", "S0001/B" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 0, 1, 4 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001", "S0001/A", "S0001/D" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 0, 3 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001", "S0001/C" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 0, 3 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001", "S0001/C" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "T0001" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 0 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "T0001", "S0001" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 1 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "T0001", "S0001/A" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(Array.Empty<int>(), InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001*A" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 0 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001*A", "S0001" }, baseInvoiceNumber, false));
			AssertContainsExactElementsInAnyOrder(new int[] { 1 }, InvoiceLiteralNumberGenerator.ConvertInvoiceNumbersSuffixesToInts_ForTestOnly(new ZString[] { "S0001*A", "S0001/A" }, baseInvoiceNumber, false));
		}

		public void TestGetNextConsolInvoiceNumber()
		{
			var consolInvoiceNumber = "C123";
			var transcationHeader = Factory.NewWithValidTestData<APInvoice>();
			AssertEquals("C123", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/ZZ" }, consolInvoiceNumber));
			Assert("Has the business context for MaximumJobInvoiceNumberError", Factory.HasContext(BusinessContext.MaximumJobInvoiceNumberError));
			Factory.RemoveContext(BusinessContext.MaximumJobInvoiceNumberError);

			AssertEquals("C123/DD", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/DC", "C123", "C123/AA" }, consolInvoiceNumber));
			AssertEquals("C123/A", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123" }, consolInvoiceNumber));
			AssertEquals("C123/B", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/A" }, consolInvoiceNumber));
			AssertEquals("C123/AA", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/Z" }, consolInvoiceNumber));
			AssertEquals("C123/BA", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/AZ" }, consolInvoiceNumber));
			AssertEquals("C123/ZB", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/ZA" }, consolInvoiceNumber));
			AssertEquals("C123/ZZ", InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, new ZString[] { "C123/ZY" }, consolInvoiceNumber));

			ZString[] existingNumbers = new ZString[] { "C123", "123/A" };
			string expectedMessage = "Existing number '123/A' doesn't start with expected 'C123'.";
			AssertGetNextConsolInvoiceNumber(existingNumbers, expectedMessage);

			existingNumbers = new ZString[] { "C123", "C123C123/A" };
			expectedMessage = "Number to add suffix for 'C123'. Existing number 'C123C123/A'. Suffix: 'C123/A'. Error: Suffix must start with: '/'.";
			AssertGetNextConsolInvoiceNumber(existingNumbers, expectedMessage);

			existingNumbers = new ZString[] { "C123//C6/D", "C123" };
			expectedMessage = "Number to add suffix for 'C123'. Existing number 'C123//C6/D'. Suffix: '//C6/D'. Error: Suffix conatins invalid characters: '/6/'.";
			AssertGetNextConsolInvoiceNumber(existingNumbers, expectedMessage);

			existingNumbers = new ZString[] { "C123/AAA" };
			expectedMessage = "Number to add suffix for 'C123'. Existing number 'C123/AAA'. Suffix: '/AAA'. Error: Maximum length of suffixes is exceeded.";
			AssertGetNextConsolInvoiceNumber(existingNumbers, expectedMessage);

			existingNumbers = new ZString[] { "C123/" };
			expectedMessage = "Number to add suffix for 'C123'. Existing number 'C123/'. Suffix: '/'. Error: Invalid suffix.";
			AssertGetNextConsolInvoiceNumber(existingNumbers, expectedMessage);

			ErrorReporter.Clear();
		}

		public void TestGetNextConsolInvoiceNumberGreaterThanMaxinumNumber()
		{
			using (AccountingConfigurationRegistry.Instance.MaximumNumberOfInvoicesAllowedOnJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 400))
			{
				var transcationHeader = Factory.NewWithValidTestData<APInvoice>();
				ZString[] existingNumbers = new ZString[] { "C123/ZY" };
				InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, existingNumbers, "C123");

				Assert("Has the business context for MaximumJobInvoiceNumberError", Factory.HasContext(BusinessContext.MaximumJobInvoiceNumberError));
				Factory.RemoveContext(BusinessContext.MaximumJobInvoiceNumberError);
			}
		}

		void AssertGetNextConsolInvoiceNumber(ZString[] existingConsolNumbers, string expectedMessage)
		{
			ErrorReporter.Clear();
			InvoiceLiteralNumberGenerator.GetNextConsolInvoiceNumber_ForTestOnly(Factory, existingConsolNumbers, "C123");
			AssertEquals("GetNextConsolInvoiceNumber", ErrorReporter.LastKeyReported);
			AssertEquals(expectedMessage, ErrorReporter.LastMessageReported);
		}

		public void TestGetNextConsolARInvoiceNumber()
		{
			GlbBranch branch1 = Factory.New<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbBranch branch2 = Factory.New<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			GlbCompany anotherCompany = Factory.New<GlbCompany>();
			GlbBranch wrongBranch = Factory.New<GlbBranch>();
			wrongBranch.GB_GC = anotherCompany.PK;

			string consolNumber = "C12345";
			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_GB = branch1.PK;
			invoice1.AH_ConsolidatedInvoiceRef = consolNumber + "/B";

			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_GB = branch2.PK;
			invoice2.AH_ConsolidatedInvoiceRef = consolNumber + "/A";

			ARInvoice invoice3 = Factory.New<ARInvoice>();
			invoice3.AH_GB = wrongBranch.PK;
			invoice3.AH_ConsolidatedInvoiceRef = consolNumber + "/X";

			ARCreditNote creditNote1 = Factory.New<ARCreditNote>();
			creditNote1.AH_GB = branch1.PK;
			creditNote1.AH_ConsolidatedInvoiceRef = consolNumber + "/C";

			string consolNumber2 = "C12345GW";
			ARInvoice invoice4 = Factory.New<ARInvoice>();
			invoice4.AH_GB = branch1.PK;
			invoice4.AH_ConsolidatedInvoiceRef = consolNumber2;

			ARInvoice invoice5 = Factory.New<ARInvoice>();
			invoice4.AH_GB = branch2.PK;
			invoice4.AH_ConsolidatedInvoiceRef = consolNumber2 + "/A";

			ARCreditNote creditNoteToSet = Factory.New<ARCreditNote>();
			creditNoteToSet.AH_GB = branch2.PK;

			AssertEquals("C12345/D", InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(Factory, consolNumber, creditNoteToSet.PK));
		}

		public void TestNoDeveloperExceptionIsThrownWhenThereIsNoSlashInTheSuffixOfExistingCASSInvoiceNumber()
		{
			AssertNoExceptionThrown(() =>
			{
				var shipment = TestObjectCreator.CreateShipment("S00001", true);
				var job = TestObjectCreator.CreateJob(TestObjectCreator.LocalClient, 1.0M, TestObjectCreator.Agent, 1.0M);
				var invoice1 = CreateTransaction("CASSAUD190404");
				var invoice2 = CreateTransaction("CASSAUD190404A");
				Factory.Save();

				var nextInvoiceNumber = InvoiceLiteralNumberGenerator.GetNextLiteralAPInvoiceNumberForCASS(Factory, invoice2, "CASSAUD190404");
				AssertEquals("CASSAUD190404/A", nextInvoiceNumber);

				APInvoice CreateTransaction(ZString invoiceNumber)
				{
					var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>(invoiceNumber, TestObjectCreator.AUD, 1, -100, -10, 0, -100, -10, 0);
					invoice.AH_OH = TestObjectCreator.AALSHI.PK;
					var charge = TestObjectCreator.CreateCharge(invoice.Lines[0]
						, chargeCode: TestObjectCreator.CC1
						, job: job
						, apInvoiceNumber: invoice.AH_TransactionNum
						, costAccount: invoice.Header);
					return invoice;
				}
			});
		}

		#region Implementation

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		string RunAllocationOfTransactionNumberScenario(bool createCreditNote)
		{
			InvoicingBase invoiceOrCreditNote;

			if (createCreditNote)
			{
				invoiceOrCreditNote = TestObjectCreator.CreateAPCreditNote("", TestObjectCreator.AALSHI, TestObjectCreator.AUD, 1, "");
			}
			else
			{
				invoiceOrCreditNote = TestObjectCreator.CreateAPInvoice<APInvoice>("", TestObjectCreator.AUD, 1, 100, 10, 0, 100, 10, 0);
				invoiceOrCreditNote.AH_OH = TestObjectCreator.AALSHI.PK;
			}

			return InvoiceLiteralNumberGenerator.GetNextLiteralAPInvoiceNumberForCASS(Factory, invoiceOrCreditNote, "CASSAUD121101");
		}

		#endregion
	}
}
