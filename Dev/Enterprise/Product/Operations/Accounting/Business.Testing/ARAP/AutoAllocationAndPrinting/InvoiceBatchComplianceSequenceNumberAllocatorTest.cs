using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Business.ARAP.AutoAllocationAndPrinting.Testing
{
	[TestedType(typeof(InvoiceBatchComplianceSequenceNumberAllocator))]
	public class InvoiceBatchComplianceSequenceNumberAllocatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldAskUseCurrentBranchBooksOption()
		{
			AssertNull(invoice5.ComplianceSequenceFromSubType);
			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(Factory);
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			invoice5.AH_GB = creator.NonCurrentBranch.PK;
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			invoice5.AH_ComplianceSubType = "TXC";
			AssertNotNull(invoice5.ComplianceSequenceFromSubType);
			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newFactory.Save();
			AssertEquals("Should ask user books option", true, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			newFactory.Save();
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			newFactory.Save();
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			sequenceTXC.XD_LockBy = GlbStaff.CurrentUser.PK;
			newFactory.Save();
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			invoice5.AH_TransactionReference = "ABC000000001";
			AssertNotNull(invoice5.ComplianceSequence);
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			{
				AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));
			}

			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchBooksOption_ForTestOnly(invoice5));
		}

		public void TestShouldAskUseCurrentBranchDepartmentBooksOption()
		{
			AssertNull(invoice5.ComplianceSequenceFromSubType);
			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(Factory);
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));

			invoice5.AH_GE = creator.NonCurrentDepartment.PK;
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));
			invoice5.AH_ComplianceSubType = "TXC";
			AssertNotNull(invoice5.ComplianceSequenceFromSubType);
			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newFactory.Save();
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			newFactory.Save();
			AssertEquals("Should ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			newFactory.Save();
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			sequenceTXC.XD_LockBy = GlbStaff.CurrentUser.PK;
			newFactory.Save();
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));

			invoice5.AH_TransactionReference = "ABC000000001";
			AssertNotNull(invoice5.ComplianceSequence);
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			AssertEquals("Should ask use books option", true, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocationRuleReceivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceDocumentNumberAllocationRuleTypes.HBD.Code))
			{
				AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));
			}

			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			AssertEquals("Should ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			AssertEquals("Should not ask use books option", false, allocator.ShouldAskUseCurrentBranchDepartmentBooksOption_ForTestOnly(invoice5));
		}

		public void TestAllocationCodeToBeCalledOnSaving()
		{
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice2, invoice3, invoice4 };
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
			BusinessObjectFactory.SaveTogether(factories);

			AssertEquals("Factories array should contain 2 elements", 2, factories.Length);
			AssertEquals("First element is main Factory", TestObjectCreator.Factory, factories[0]);
			AssertEquals("Second element is SaveInTransactionWithRollBackAction ", typeof(SaveInTransactionWithRollBackAction), factories[1].GetType());
		}

		public void TestAllocationDoesNothingWhenSomeTransactionsHavePreprintedConfiguration()
		{
			sequenceTCR.XD_SO_ComplianceTemplate = ZGuid.Empty;
			Assert("Rollup Configuration should now be empty", sequenceTCR.XD_RollupBehaviourWhenMaxExceeded.IsEmpty);
			sequenceTCR.Factory.Save();

			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice2, invoice3, invoice4 };
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
			bool didAllocationFailAsExpected = false;
			try
			{
				BusinessObjectFactory.SaveTogether(factories);
			}
			catch (CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException)
			{
				didAllocationFailAsExpected = true;
			}

			Assert("Process should have failed", didAllocationFailAsExpected);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			invoice1 = newFactory.Load<ARInvoice>(invoice1.PK);
			invoice2 = newFactory.Load<ARInvoice>(invoice2.PK);
			invoice3 = newFactory.Load<ARInvoice>(invoice3.PK);
			invoice4 = newFactory.Load<ARInvoice>(invoice4.PK);

			AssertEquals("Transaction 1 Reference should still be empty (because sequence has preprinted config)", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Transaction 2 Reference should still be empty (because sequence has preprinted config)", ZString.Empty, invoice2.AH_TransactionReference);
			AssertEquals("Transaction 3 Reference should still be empty (because some transactions in the batch failed)", ZString.Empty, invoice3.AH_TransactionReference);
			AssertEquals("Transaction 4 Reference should still be empty (because some transactions in the batch failed)", ZString.Empty, invoice4.AH_TransactionReference);
		}

		public void TestAllocateToNonPreprintedTransactionsOnly()
		{
			sequenceTCR.XD_SO_ComplianceTemplate = ZGuid.Empty;
			Assert("Rollup Configuration should now be empty", sequenceTCR.XD_RollupBehaviourWhenMaxExceeded.IsEmpty);
			sequenceTCR.Factory.Save();

			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice3, invoice4 };
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
			bool didAllocationFail = false;
			try
			{
				BusinessObjectFactory.SaveTogether(factories);
			}
			catch (CannotAllocateSequenceNumberToPrePrintedSequenceWhenNotPrintingException)
			{
				didAllocationFail = true;
			}

			Assert("Process should not have failed", !didAllocationFail);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			invoice1 = newFactory.Load<ARInvoice>(invoice1.PK);
			invoice2 = newFactory.Load<ARInvoice>(invoice2.PK);
			invoice3 = newFactory.Load<ARInvoice>(invoice3.PK);
			invoice4 = newFactory.Load<ARInvoice>(invoice4.PK);

			AssertEquals("Transaction 1 Reference should still be empty (because sequence has preprinted config)", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Transaction 2 Reference should still be empty (because sequence has preprinted config)", ZString.Empty, invoice2.AH_TransactionReference);
			AssertEquals("Transaction 3 Reference should be populated (because it was selected to be allocated)", "XYZ000001000", invoice3.AH_TransactionReference);
			AssertEquals("Transaction 4 Reference should be populated (because it was selected to be allocated)", "XYZ000001001", invoice4.AH_TransactionReference);
		}

		public void TestComplianceSequenceNumbersGetAllocatedInCollectionSequence()
		{
			var newFactory = new BusinessObjectFactory();
			invoice1 = newFactory.Load<ARInvoice>(invoice1.PK);
			invoice2 = newFactory.Load<ARInvoice>(invoice2.PK);
			invoice3 = newFactory.Load<ARInvoice>(invoice3.PK);
			invoice4 = newFactory.Load<ARInvoice>(invoice4.PK);

			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Precondition - transaction 2 referencs is empty", ZString.Empty, invoice2.AH_TransactionReference);
			AssertEquals("Precondition - transaction 3 referencs is empty", ZString.Empty, invoice3.AH_TransactionReference);
			AssertEquals("Precondition - transaction 4 referencs is empty", ZString.Empty, invoice4.AH_TransactionReference);

			var transactionsReloaded = new TransactionHeader[] { invoice1, invoice2, invoice3, invoice4 };
			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, newFactory);
			SetupUserQuestions(allocator);
			var factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, newFactory);
			BusinessObjectFactory.SaveTogether(factories);

			var query = new ZQuery();
			var printJobs = newFactory.Load<StmPrintJob>(query);

			AssertEquals("Should be 4 print jobs", 4, printJobs.Length);

			AssertEquals("Postcondition - transaction 1 is printed", true, invoice1.AH_InvoicePrinted);
			AssertEquals("Postcondition - transaction 2 is printed", true, invoice2.AH_InvoicePrinted);
			AssertEquals("Postcondition - transaction 3 is printed", true, invoice3.AH_InvoicePrinted);
			AssertEquals("Postcondition - transaction 4 is printed", true, invoice4.AH_InvoicePrinted);

			AssertEquals("Postcondition - transaction 1 referencs is generated", "ABC000000001", invoice1.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 2 referencs is generated", "ABC000000002", invoice2.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 3 referencs is generated", "XYZ000001000", invoice3.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 4 referencs is generated", "XYZ000001001", invoice4.AH_TransactionReference);
		}

		[TestDate(2025, 01, 20, 00, 00, 00)]
		public void TestComplianceSequenceNumbersOrder_PST()
		{
			AssertComplianceSequenceNumbersOrder(ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		[TestDate(2025, 01, 20, 00, 00, 00)]
		public void TestComplianceSequenceNumbersOrder_INV()
		{
			AssertComplianceSequenceNumbersOrder(ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		[TestDate(2025, 01, 20, 00, 00, 00)]
		public void TestComplianceSequenceNumbersOrder_NOT()
		{
			AssertComplianceSequenceNumbersOrder(ComplianceNumberAllocationDateOptions.NoControl.Code);
		}

		public void AssertComplianceSequenceNumbersOrder(string dateOption)
		{
			const string complianceSubType = "TXI";
			const string invalidComplianceSubType = "XXX";
			Factory.Load<AccTransactionHeader>(new ZQuery(AccTransactionHeaderSchema.AH_ComplianceSubType, complianceSubType)).ForEach(x => x.AH_ComplianceSubType = invalidComplianceSubType);
			Factory.Save();

			invoice1 = TestObjectCreator.SetupComplianceInvoice(complianceSubType, false);
			invoice2 = TestObjectCreator.SetupComplianceInvoice(complianceSubType, false);
			invoice3 = TestObjectCreator.SetupComplianceInvoice(complianceSubType, false);

			var now = ZDateTime.Now;
			invoice1.AH_PostDate = now;
			invoice1.AH_InvoiceDate = now.AddHours(12);
			invoice2.AH_PostDate = now.AddHours(2);
			invoice2.AH_InvoiceDate = now;
			invoice3.AH_PostDate = now.AddHours(1);
			invoice3.AH_InvoiceDate = now.AddHours(-1);
			TestObjectCreator.Factory.Save();

			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
			{
				var transactionsReloaded = new TransactionHeader[] { invoice1, invoice2, invoice3 };
				var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
				SetupUserQuestions(allocator);
				ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
				BusinessObjectFactory.SaveTogether(factories);

				if (dateOption == ComplianceNumberAllocationDateOptions.PostDate.Code
					|| dateOption == ComplianceNumberAllocationDateOptions.NoControl.Code)
				{
					AssertEquals("ABC000000001", invoice1.AH_TransactionReference);
					AssertEquals("ABC000000002", invoice3.AH_TransactionReference);
					AssertEquals("ABC000000003", invoice2.AH_TransactionReference);
				}
				else
				{
					AssertEquals("ABC000000001", invoice3.AH_TransactionReference);
					AssertEquals("ABC000000002", invoice2.AH_TransactionReference);
					AssertEquals("ABC000000003", invoice1.AH_TransactionReference);
				}
			}
		}

		public void TestComplianceSequenceNumbersAssignedWithLastPostDateUsedSavingException()
		{
			AssertComplianceSequenceNumbersAssignedWithLastInvoiceDateUsedSavingException(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code, "Post Date");
		}

		public void TestComplianceSequenceNumbersAssignedWithLastInvoiceDateUsedSavingException()
		{
			AssertComplianceSequenceNumbersAssignedWithLastInvoiceDateUsedSavingException(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code, "Invoice Date");
		}

		void AssertComplianceSequenceNumbersAssignedWithLastInvoiceDateUsedSavingException(string dateOption, string dateLabel)
		{
			var invoice6 = Factory.NewWithValidTestData<ARInvoice>();
			invoice6.AH_PostDate = new ZDate(2020, 4, 26);
			invoice6.AH_InvoiceDate = new ZDate(2020, 4, 26);
			invoice6.AH_XD_ComplianceBook = sequenceARI.PK;

			Factory.Save();

			invoice1 = TestObjectCreator.SetupComplianceInvoice("ARI", false);

			if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice1.AH_InvoiceDate = new ZDateTime(2020, 4, 25);
			}
			else if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code)
			{
				invoice1.AH_PostDate = new ZDateTime(2020, 4, 25);
			}

			TestObjectCreator.Factory.Save();

			var transactionsReloaded = new TransactionHeader[] { invoice1 };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
			{
				try
				{
					var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
					SetupUserQuestions(allocator);
					ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
					BusinessObjectFactory.SaveTogether(factories);
					Fail("should never reach this line");
				}
				catch (UnableToAllocateNumberDueToPostDateEarlierThanLastDateUsedException ex)
				{
					AssertEquals($@"Compliance Numbers cannot be allocated.
 Last posted transaction with the same Compliance Sub Type ARI has {dateLabel} = 26-Apr-20, that is greater than the current one(s).", ex.UserFriendlyMessage);
				}
			}
		}

		public void TestComplianceSequenceNumbersAssignedWithLastSparseBookSavingException_PST()
		{
			AssertComplianceSequenceNumbersAssignedWithLastSparseBookSavingException(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code);
		}

		public void TestComplianceSequenceNumbersAssignedWithLastSparseBookSavingException_INV()
		{
			AssertComplianceSequenceNumbersAssignedWithLastSparseBookSavingException(AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code);
		}

		public void AssertComplianceSequenceNumbersAssignedWithLastSparseBookSavingException(string dateOption)
		{
			invoice1 = TestObjectCreator.SetupComplianceInvoice("ARI", false);
			invoice2 = TestObjectCreator.SetupComplianceInvoice("ARI", false);
			invoice3 = TestObjectCreator.SetupComplianceInvoice("ARI", false);

			var dateLabel = "Post Date";
			if (dateOption == AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.InvoiceDate.Code)
			{
				invoice1.AH_InvoiceDate = new ZDateTime(2020, 4, 26);
				invoice2.AH_InvoiceDate = new ZDateTime(2020, 4, 26);
				invoice3.AH_InvoiceDate = new ZDateTime(2020, 4, 27);
				dateLabel = "Invoice Date";
			}
			else
			{
				invoice1.AH_PostDate = new ZDateTime(2020, 4, 26);
				invoice2.AH_PostDate = new ZDateTime(2020, 4, 26);
				invoice3.AH_PostDate = new ZDateTime(2020, 4, 27);
			}

			invoice1.AH_TransactionReference = "00001";

			TestObjectCreator.Factory.Save();

			var transactionsReloaded = new TransactionHeader[] { invoice3 };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, dateOption))
			{
				try
				{
					var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
					SetupUserQuestions(allocator);
					ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
					BusinessObjectFactory.SaveTogether(factories);
					Fail("should never reach this line");
				}
				catch (UnableToAllocateNumberDueToSparseComplianceBookException ex)
				{
					AssertEquals($@"Compliance Numbers cannot be allocated.
 There is some transaction with the same Compliance Sub Type ARI in earlier {dateLabel} and Compliance Number empty.
 Please allocate Compliance Number to all transactions with {dateLabel} < 27-Apr-20.", ex.UserFriendlyMessage);
				}
			}
		}

		[TestDate(2021, 8, 3, 10, 0, 0)]
		public void TestComplianceSequenceNumbersAssignedWithInvoiceDateLessThanPreviousException()
		{
			var ptCompany = TestObjectCreator.CreateCompanyAndBranch(Constants.CountryCodes.Portugal);
			TestObjectCreator.Factory.Save();
			using (TestObjectCreator.SwitchEnvToCompany(ptCompany))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				SetupComplianceSequenceBookTXI(menuText: "Govt Compliance Inv PT");
				TestObjectCreator.Factory.Save();

				invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI", false, ZDateTime.Now.AddDays(1));
				TestObjectCreator.Factory.Save();

				AssertEquals("TXI ABC/000000001", invoice1.AH_TransactionReference);

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				invoice2 = TestObjectCreator.SetupComplianceInvoice("TXI", false, ZDateTime.Now);
				TestObjectCreator.Factory.Save();

				Assert(invoice2.AH_TransactionReference.IsEmpty);

				var transactionsReloaded = new TransactionHeader[] { invoice2 };
				var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
				SetupUserQuestions(allocator);
				var factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

				var ex = AssertExceptionThrown<InvoiceDateLessThanPreviousException>(() => BusinessObjectFactory.SaveTogether(factories));
				AssertEquals(@"Invoice date must be equal or higher than previous document.", ex.UserFriendlyMessage);
			}
		}

		public void TestComplianceSequenceNumbersAssignedWithPostDateLessThanPreviousException()
		{
			var ptCompany = TestObjectCreator.CreateCompanyAndBranch(Constants.CountryCodes.Portugal);
			TestObjectCreator.Factory.Save();
			using (TestObjectCreator.SwitchEnvToCompany(ptCompany))
			using (AccountingMasterFilesRegistry.Instance.ComplianceNumberAllocationDate_AR.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.NoControl.Code))
			{
				SetupComplianceSequenceBookTXI(menuText: "Govt Compliance Inv PT");
				TestObjectCreator.Factory.Save();

				invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI", false);
				invoice1.AH_PostDate = ZDateTime.Now.AddDays(1);
				TestObjectCreator.Factory.Save();

				AssertEquals("TXI ABC/000000001", invoice1.AH_TransactionReference);

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				invoice2 = TestObjectCreator.SetupComplianceInvoice("TXI", false);
				invoice2.AH_PostDate = ZDateTime.Now;
				TestObjectCreator.Factory.Save();

				Assert(invoice2.AH_TransactionReference.IsEmpty);

				var transactionsReloaded = new TransactionHeader[] { invoice2 };
				var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
				SetupUserQuestions(allocator);

				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				var factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);

				var ex = AssertExceptionThrown<PostDateLessThanPreviousException>(() => BusinessObjectFactory.SaveTogether(factories));
				AssertEquals(@"Post date must be equal or higher than previous document.", ex.UserFriendlyMessage);
			}
		}

		public void TestComplianceSequenceNumbersAssignedWithInvoiceDateGreaterThanPostDateException()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				try
				{
					AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);

					invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI", false);
					invoice1.AH_InvoiceDate = ZDateTime.Now.AddDays(1);
					invoice1.AH_PostDate = ZDateTime.Now;
					TestObjectCreator.Factory.Save();

					Assert(invoice1.AH_TransactionReference.IsEmpty);

					var transactionsReloaded = new TransactionHeader[] { invoice1 };
					var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
					SetupUserQuestions(allocator);
					var factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
					BusinessObjectFactory.SaveTogether(factories);

					Fail("Should never reach this line");
				}
				catch (InvoiceDateGreaterThanPostDateException ex)
				{
					AssertEquals(@"Invoice date must be equal or lower than post date.", ex.UserFriendlyMessage);
				}
			}
		}

		#region ResetSequenceNumberIfRequiredByUserWhenAllowed

		public void TestResetSequenceNumberIfRequiredByUser_WhenAllowedForEInvoicing_UserAnswerNo()
		{
			var updateActionPermissionsMock = SetupUpdateActionPermissionsMock();

			var expectedTransactionReference = "ABC000000001";
			var allocator = SetupAllocatorAndComplianceSequence(expectedTransactionReference);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			updateActionPermissionsMock.Setup(x => x.CheckIsComplianceNumberResetAllowed(It.IsAny<IComplianceNumberResetStatusInputData>())).Returns(true);

			RunResetSequanceNumberFunctionality(allocator);

			AssertEquals("LastMessage to user", InvoiceBatchComplianceSequenceNumberAllocator.QuestionAssignNewSequenceNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Postcondition: AH_TransactionReference", expectedTransactionReference, invoice1.AH_TransactionReference);
			updateActionPermissionsMock.Verify(x => x.CheckIsComplianceNumberResetAllowed(invoice1), Times.Once);
		}

		public void TestResetSequenceNumberIfRequiredByUser_WhenAllowedForEInvoicing_UserAnswerYes()
		{
			var updateActionPermissionsMock = SetupUpdateActionPermissionsMock();

			var initialTransactionReference = "ABC000000001";
			var allocator = SetupAllocatorAndComplianceSequence(initialTransactionReference);
			var expectedTransactionReference = "ABC000000005";
			AssertNotEquals("Precondition: initialTransactionReference", expectedTransactionReference, initialTransactionReference);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			updateActionPermissionsMock.Setup(x => x.CheckIsComplianceNumberResetAllowed(It.IsAny<IComplianceNumberResetStatusInputData>())).Returns(true);

			RunResetSequanceNumberFunctionality(allocator);

			AssertEquals("LastMessage to user", InvoiceBatchComplianceSequenceNumberAllocator.QuestionAssignNewSequenceNumber, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Postcondition: AH_TransactionReference", expectedTransactionReference, invoice1.AH_TransactionReference);
			updateActionPermissionsMock.Verify(x => x.CheckIsComplianceNumberResetAllowed(invoice1), Times.Once);
		}

		public void TestResetSequenceNumberIfRequiredByUser_WhenNotAllowedForEInvoicing()
		{
			var updateActionPermissionsMock = SetupUpdateActionPermissionsMock();

			var expectedTransactionReference = "ABC000000001";
			var allocator = SetupAllocatorAndComplianceSequence(expectedTransactionReference);

			updateActionPermissionsMock.Setup(x => x.CheckIsComplianceNumberResetAllowed(It.IsAny<IComplianceNumberResetStatusInputData>())).Returns(false);

			RunResetSequanceNumberFunctionality(allocator);

			AssertNull("LastMessage to user", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Postcondition: AH_TransactionReference", expectedTransactionReference, invoice1.AH_TransactionReference);
			updateActionPermissionsMock.Verify(x => x.CheckIsComplianceNumberResetAllowed(invoice1), Times.Once);
		}

		public void TestResetSequenceNumberIfRequiredByUser_WhenNotAllowedBecausePortugal()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Portugal))
			{
				var updateActionPermissionsMock = SetupUpdateActionPermissionsMock();

				var expectedTransactionReference = "ABC000000001";
				var allocator = SetupAllocatorAndComplianceSequence(expectedTransactionReference);

				updateActionPermissionsMock.Setup(x => x.CheckIsComplianceNumberResetAllowed(It.IsAny<IComplianceNumberResetStatusInputData>())).Returns(true);

				RunResetSequanceNumberFunctionality(allocator);

				AssertNull("Even when CheckIsComplianceNumberResetAllowed() returns true, Portugal never allows changing the compliance sequence", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Postcondition: AH_TransactionReference", expectedTransactionReference, invoice1.AH_TransactionReference);
				updateActionPermissionsMock.Verify(x => x.CheckIsComplianceNumberResetAllowed(invoice1), Times.Once);
			}
		}

		static Mock<IElectronicInvoicingUpdateActionPermissions> SetupUpdateActionPermissionsMock()
		{
			var eInvoicingFactory = new Mock<IElectronicInvoicingAccountingObjectFactory>();
			ObjectFactory.Substitute(eInvoicingFactory.Object);
			var updateActionPermissionsMock = new Mock<IElectronicInvoicingUpdateActionPermissions>();
			eInvoicingFactory.Setup(x => x.GetElectronicInvoicingUpdateActionPermissions()).Returns(updateActionPermissionsMock.Object);
			return updateActionPermissionsMock;
		}

		#endregion

		public void TestCanContinueWithAllocationHandlesMultipleInvoices()
		{
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice2 };

			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Precondition - transaction 2 referencs is empty", ZString.Empty, invoice2.AH_TransactionReference);
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			Assert("CanContinueWithAllocation_ForTestOnly should be true", allocator.CanContinueWithAllocation_ForTestOnly);

			invoice1.AH_ComplianceSubType = "";
			try
			{
				allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
				SetupUserQuestions(allocator);
				bool result = allocator.CanContinueWithAllocation_ForTestOnly;
				Fail("should never reach this line");
			}
			catch (UnableToAllocateNumberDueToSubtypeMissingException ex)
			{
				AssertEquals(@"Compliance Numbers cannot be allocated. 
 This transaction does not have a recognized Compliance Sub Type", ex.UserFriendlyMessage);
			}

			invoice1.AH_ComplianceSubType = "TXI";
			invoice1.AH_InvoicePrinted = true;
			try
			{
				allocator = new InvoiceBatchComplianceSequenceNumberAllocator("TAX", transactionsReloaded, TestObjectCreator.Factory);
				SetupUserQuestions(allocator);
				bool result = allocator.CanContinueWithAllocation_ForTestOnly;
				Fail("should never reach this line");
			}
			catch (UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyException ex)
			{
				AssertEquals(@"Please change your selection.
 Compliance Numbers cannot be allocated because at least one selected transaction has already been printed. Please amend your selection", ex.UserFriendlyMessage);
			}

			invoice1.AH_InvoicePrinted = false;
			invoice1.AH_GB = TestObjectCreator.Factory.NewWithValidTestData<GlbBranch>().PK;
			allocator = new InvoiceBatchComplianceSequenceNumberAllocator("TAX", transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("Can't continue because user choose to NOT use sequence from current branch", false, allocator.CanContinueWithAllocation_ForTestOnly);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("Can continue because user choose to use sequence from current branch", true, allocator.CanContinueWithAllocation_ForTestOnly);
		}

		public void TestShouldNotAddLogToOriginalSequenceWhenTransactionReferenceLengthLessThanPrefix()
		{
			invoice1.AH_ComplianceSubType = "TXI";
			sequenceTXI.XD_NextNumber = 2;
			var transactionsReloaded = new TransactionHeader[] { invoice1 };

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			Assert("CanContinueWithAllocation_ForTestOnly should be true", allocator.CanContinueWithAllocation_ForTestOnly);

			invoice1.AH_InvoicePrinted = true;
			allocator = new InvoiceBatchComplianceSequenceNumberAllocator("TAX", transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);

			AssertEquals(0, invoice1.ComplianceSequence.Logs.GetAllLogs().Count);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AssertNoExceptionThrown(() => AssertEquals("Can continue because user choose to reprint and regenerating the sequence number", true, allocator.CanContinueWithAllocation_ForTestOnly));
			AssertEquals("Can continue because user choose to reprint and regenerating the sequence number", true, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is reset to blank", "", invoice1.AH_TransactionReference);
			AssertEquals(0, invoice1.ComplianceSequence.Logs.GetAllLogs().Count);
		}

		public void TestCanContinueWithAllocationHandlesSinglePrintedInvoice()
		{
			invoice1.AH_TransactionReference = "ABC000000001";
			sequenceTXI.XD_NextNumber = 2;
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1 };

			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			Assert("CanContinueWithAllocation_ForTestOnly should be true", allocator.CanContinueWithAllocation_ForTestOnly);

			invoice1.AH_InvoicePrinted = true;
			allocator = new InvoiceBatchComplianceSequenceNumberAllocator("TAX", transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("Can't continue because user choose to NOT reprint", false, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is NOT regenerated", "ABC000000001", invoice1.AH_TransactionReference);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("Can continue because user choose to reprint without regenerating the sequence number", true, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is NOT generated", "ABC000000001", invoice1.AH_TransactionReference);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("Can continue because user choose to reprint and regenerating the sequence number", true, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is reset to blank", "", invoice1.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 1 Printed flag is reset to false", false, invoice1.AH_InvoicePrinted);

			invoice1.AH_TransactionReference = "UVW000099999";
			invoice1.AH_InvoicePrinted = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			try
			{
				var result = allocator.CanContinueWithAllocation_ForTestOnly;
				Fail("should never reach this line");
			}
			catch (FailedToFindComplianceSequenceFromSequenceNumberException ex)
			{
				AssertEquals(@$"Could not identify the original Compliance Invoice Book.
 {BrandingFactory.Instance.ProductName} could not identify the Compliance Invoice Book that was used to assign this transaction a Compliance Number.", ex.UserFriendlyMessage);
			}
		}

		public void TestCanContinueWithAllocationHandlesSingleUnPrintedInvoice()
		{
			invoice5 = TestObjectCreator.SetupComplianceInvoice("TXC");

			invoice5.AH_TransactionReference = "ABC000000001";
			sequenceTXC.XD_NextNumber = 2;
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice5 };

			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			Assert("CanContinueWithAllocation_ForTestOnly should be true", allocator.CanContinueWithAllocation_ForTestOnly);

			invoice5.AH_InvoicePrinted = false;
			allocator = new InvoiceBatchComplianceSequenceNumberAllocator("TAX", transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			AssertEquals("Can continue because user choose to NOT reassign number but number already exists", true, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is NOT regenerated", "ABC000000001", invoice5.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 1 Printed flag is still to false", false, invoice5.AH_InvoicePrinted);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			AssertEquals("Can continue because user choose to reassign the sequence number", true, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is NOT reset to blank", "", invoice5.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 1 Printed flag is still to false", false, invoice5.AH_InvoicePrinted);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			invoice5.AH_TransactionReference = "";

			invoice5.AH_GE = creator.NonCurrentDepartment.PK;
			invoice5.ComplianceSequence.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			AssertEquals("Can continue when reference number is blank", true, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is still blank", "", invoice5.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 1 Printed flag is still false", false, invoice5.AH_InvoicePrinted);

			sequenceTXC.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			newFactory.Save();
			AssertEquals("Can continue when reference number is blank", false, allocator.CanContinueWithAllocation_ForTestOnly);
			AssertEquals("Postcondition - transaction 1 referencs is still blank", "", invoice5.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 1 Printed flag is still false", false, invoice5.AH_InvoicePrinted);
		}

		public void TestShowUserBranchBooksOptionForBDP()
		{
			bool isShowUserBranchBooksOption = false;
			bool isShowUserBranchDepartmentBooksOption = false;
			sequenceTXI.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			sequenceTXI.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			invoice1.AH_TransactionReference = "ABC000000002";
			invoice1.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice1.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice1.AH_ComplianceSubType = "TXI";

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new TransactionHeader[] { invoice1 }, Factory);
			allocator.AskReprintInvoiceOption += delegate() { return true; };
			allocator.AskReassignNumberOption += delegate() { return true; };
			allocator.AskUseCurrentBrancheBooksOption += delegate() { isShowUserBranchBooksOption = true; return true; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate() { isShowUserBranchDepartmentBooksOption = true; return true; };

			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
			AssertNotNull(invoice1.ComplianceSequence);

			var result = allocator.CanContinueWithAllocation_ForTestOnly;

			AssertEquals(true, result);
			AssertEquals(true, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
		}

		public void TestShowUserBranchBooksOptionForCTR()
		{
			bool isShowUserBranchBooksOption = false;
			bool isShowUserBranchDepartmentBooksOption = false;
			sequenceTXI.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			Factory.Save();

			invoice1.AH_TransactionReference = "ABC000000002";
			invoice1.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			invoice1.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice1.AH_ComplianceSubType = "TXI";

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new TransactionHeader[] { invoice1 }, Factory);
			allocator.AskReprintInvoiceOption += delegate() { return true; };
			allocator.AskReassignNumberOption += delegate() { return true; };
			allocator.AskUseCurrentBrancheBooksOption += delegate() { isShowUserBranchBooksOption = true; return true; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate()
			{ isShowUserBranchDepartmentBooksOption = true; return true; };

			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
			AssertNotNull(invoice1.ComplianceSequence);

			var result = allocator.CanContinueWithAllocation_ForTestOnly;

			AssertEquals(true, result);
			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
		}

		public void TestShowUserBranchDepartmentBooksOptionForCOM()
		{
			bool isShowUserBranchBooksOption = false;
			bool isShowUserBranchDepartmentBooksOption = false;
			sequenceTXI.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Company;
			Factory.Save();

			invoice1.AH_TransactionReference = "ABC000000002";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice1.AH_ComplianceSubType = "TXI";

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new TransactionHeader[] { invoice1 }, Factory);
			allocator.AskReprintInvoiceOption += delegate() { return true; };
			allocator.AskReassignNumberOption += delegate() { return true; };
			allocator.AskUseCurrentBrancheBooksOption += delegate() { isShowUserBranchBooksOption = true; return true; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate() { isShowUserBranchDepartmentBooksOption = true; return true; };

			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
			AssertNotNull(invoice1.ComplianceSequence);

			var result = allocator.CanContinueWithAllocation_ForTestOnly;

			AssertEquals(true, result);
			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
		}

		public void TestShowUserBranchDepartmentBooksOptionForBRN()
		{
			bool isShowUserBranchBooksOption = false;
			bool isShowUserBranchDepartmentBooksOption = false;
			sequenceTXI.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Branch;
			Factory.Save();

			invoice1.AH_TransactionReference = "ABC000000002";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice1.AH_ComplianceSubType = "TXI";

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new TransactionHeader[] { invoice1 }, Factory);
			allocator.AskReprintInvoiceOption += delegate() { return true; };
			allocator.AskReassignNumberOption += delegate() { return true; };
			allocator.AskUseCurrentBrancheBooksOption += delegate() { isShowUserBranchBooksOption = true; return true; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate() { isShowUserBranchDepartmentBooksOption = true; return true; };

			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
			AssertNotNull(invoice1.ComplianceSequence);

			var result = allocator.CanContinueWithAllocation_ForTestOnly;

			AssertEquals(true, result);
			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
		}

		public void TestShowUserBranchDepartmentBooksOptionForBDP()
		{
			bool isShowUserBranchBooksOption = false;
			bool isShowUserBranchDepartmentBooksOption = false;
			sequenceTXI.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.BranchDepartment;
			sequenceTXI.XD_GE_Department = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			invoice1.AH_TransactionReference = "ABC000000002";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice1.AH_ComplianceSubType = "TXI";

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new TransactionHeader[] { invoice1 }, Factory);
			allocator.AskReprintInvoiceOption += delegate() { return true; };
			allocator.AskReassignNumberOption += delegate() { return true; };
			allocator.AskUseCurrentBrancheBooksOption += delegate() { isShowUserBranchBooksOption = true; return true; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate() { isShowUserBranchDepartmentBooksOption = true; return true; };

			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
			AssertNotNull(invoice1.ComplianceSequence);

			var result = allocator.CanContinueWithAllocation_ForTestOnly;

			AssertEquals(true, result);
			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(true, isShowUserBranchDepartmentBooksOption);
		}

		public void TestShowUserBranchDepartmentBooksOptionForCTR()
		{
			bool isShowUserBranchBooksOption = false;
			bool isShowUserBranchDepartmentBooksOption = false;
			sequenceTXI.XD_AllocationLevel = Core.Constants.ComplianceBookAllocationLevel.Counter;
			Factory.Save();

			invoice1.AH_TransactionReference = "ABC000000002";
			invoice1.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice1.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			invoice1.AH_ComplianceSubType = "TXI";

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new TransactionHeader[] { invoice1 }, Factory);
			allocator.AskReprintInvoiceOption += delegate() { return true; };
			allocator.AskReassignNumberOption += delegate() { return true; };
			allocator.AskUseCurrentBrancheBooksOption += delegate() { isShowUserBranchBooksOption = true; return true; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate() { isShowUserBranchDepartmentBooksOption = true; return true; };

			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
			AssertNotNull(invoice1.ComplianceSequence);

			var result = allocator.CanContinueWithAllocation_ForTestOnly;

			AssertEquals(true, result);
			AssertEquals(false, isShowUserBranchBooksOption);
			AssertEquals(false, isShowUserBranchDepartmentBooksOption);
		}

		public void TestAutoAllocateComplianceSequenceNumbersWhenExceedsLengthLimit()
		{
			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);

			var configurationCollection = ComplianceNumberSequenceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.ComplianceNumberSequenceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			sequenceTXI.XD_NumberFormat = "CCC";
			sequenceTXI.XD_Prefix = "TEST";
			sequenceTXI.XD_MaximumNumberDigits = 9;
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1 };

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);

			var message = "System should throw 'ComplianceNumberExceedMaximumLengthException' exception when the generated compliance number exceeds the maximum length";
			var expectedMessage = "The length of generated compliance number('TXI     TEST000000001') has exceeded the total length of 20 characters.\r\n Please update the compliance invoice book with a number format that will not exceed the length limit.";
			var exception = AssertExceptionThrown<ComplianceNumberExceedMaximumLengthException>(message, () => {
				allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
				allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			});
			AssertEquals("The expected exception message", expectedMessage, exception.UserFriendlyMessage);
		}

		public void TestAutoAllocateComplianceSequenceNumbersWhenHasNonCMTChargeZeroAmountLine()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.VietNam);

			var message = "System should throw 'Compliance Number cannot be allocated to this transaction as it contains a Zero Amount Non-Comment Charge Line.";
			var expectedMessage = "Compliance Number cannot be allocated to this transaction as it contains a Zero Amount Non-Comment Charge Line.";

			TestObjectCreator.CreateARInvoiceLine(invoice1, null, TestObjectCreator.CC1, TestObjectCreator.VND, 1m, "ArInvoice Line", 0m);
			var transactionsReloaded1 = new TransactionHeader[] { invoice1 };

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var allocator1 = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded1, TestObjectCreator.Factory);
			SetupUserQuestions(allocator1);
			allocator1.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			var exception1 = AssertExceptionThrown<HasNonCMTChargeZeroAmountLineException>(message, () =>
			{
				allocator1.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			});
			AssertEquals("The expected exception message", expectedMessage, exception1.UserFriendlyMessage);

			var arCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			arCreditNote1.AH_OH = invoice2.AH_OH;
			arCreditNote1.OriginalTransactionReference = invoice2.PK;
			arCreditNote1.AH_ComplianceSubType = "TXI";
			TestObjectCreator.CreateARCreditNoteLine(arCreditNote1, null, TestObjectCreator.CC1, 0m, TestObjectCreator.VND, 1.0m, "Credit Line");
			var transactionsReloaded2 = new TransactionHeader[] { arCreditNote1 };

			var allocator2 = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded2, TestObjectCreator.Factory);
			SetupUserQuestions(allocator2);
			allocator2.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			var exception2 = AssertExceptionThrown<HasNonCMTChargeZeroAmountLineException>(message, () =>
			{
				allocator2.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			});
			AssertEquals("The expected exception message", expectedMessage, exception2.UserFriendlyMessage);

			var arCreditNote2 = Factory.NewWithValidTestData<ARCreditNote>();
			arCreditNote2.AH_OH = invoice2.AH_OH;
			arCreditNote2.OriginalTransactionReference = invoice2.PK;
			arCreditNote2.AH_ComplianceSubType = "TXI";
			TestObjectCreator.CreateARCreditNoteLine(arCreditNote2, null, TestObjectCreator.CC1, 12m, TestObjectCreator.VND, 1.0m, "Credit Line");
			var transactionsReloaded3 = new TransactionHeader[] { arCreditNote2 };

			var allocator3 = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded3, TestObjectCreator.Factory);
			SetupUserQuestions(allocator3);
			allocator3.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			AssertNoExceptionThrown(() =>
			{
				allocator3.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			});

			var arCreditNote3 = Factory.NewWithValidTestData<ARCreditNote>();
			arCreditNote3.AH_OH = invoice2.AH_OH;
			arCreditNote3.OriginalTransactionReference = invoice2.PK;
			arCreditNote3.AH_ComplianceSubType = "TXI";
			TestObjectCreator.CreateARCreditNoteLine(arCreditNote3, null, TestObjectCreator.CommentChargeCode, 0m, TestObjectCreator.VND, 1.0m, "Credit Line");
			var transactionsReloaded4 = new TransactionHeader[] { arCreditNote3 };

			var allocator4 = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded4, TestObjectCreator.Factory);
			SetupUserQuestions(allocator4);
			allocator4.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			AssertNoExceptionThrown(() =>
			{
				allocator4.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			});
		}

		public void TestAutoAllocateComplianceSequenceNumbersAllocateZeroInvoice()
		{
			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Precondition - transaction 2 referencs is empty", ZString.Empty, invoice2.AH_TransactionReference);

			sequenceTXI.XD_IsActive = false;
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice2 };

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			try
			{
				allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
				allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
				Fail("should never reach this line");
			}
			catch (NoComplianceInvoicesToPrintLBDException ex)
			{
				AssertContains(ComplianceSequenceNumberAllocationErrorMessages.NoComplianceInvoicesToPrintLBDMessage, ex.UserFriendlyMessage);
				Assert(allocator.AllocationProcessFailed_ForTestOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestAutoAllocateComplianceSequenceNumbersAllocatePartialInvoicesAndProceed()
		{
			AccountingMasterFilesRegistry.Instance.ComplianceAllowPartialSequenceNumberAllocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Precondition - transaction 3 referencs is empty", ZString.Empty, invoice3.AH_TransactionReference);

			sequenceTXI.XD_IsActive = true;
			sequenceTCR.XD_IsActive = false;
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice3 };

			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			AssertEquals("Precondition - PartialAllocationOccured is false", false, allocator.PartialAllocationOccured);
			allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();

			AssertEquals("PartialAllocationOccured should be true", true, allocator.PartialAllocationOccured);
			AssertEquals("Postcondition - transaction 1 referencs is generated", "ABC000000001", invoice1.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 3 referencs is generated", "", invoice3.AH_TransactionReference);
			Assert(!allocator.AllocationProcessFailed_ForTestOnly);
		}

		public void TestAutoAllocateComplianceSequenceNumbersAllocatePartialInvoicesAndDoNotProceed()
		{
			AccountingMasterFilesRegistry.Instance.ComplianceAllowPartialSequenceNumberAllocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);
			AssertEquals("Precondition - transaction 3 referencs is empty", ZString.Empty, invoice3.AH_TransactionReference);

			sequenceTXI.XD_IsActive = true;
			sequenceTCR.XD_IsActive = false;
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice3 };

			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			try
			{
				allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
				allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
				Fail("should never reach this line");
			}
			catch (AllocationComplianceSequenceFullException ex)
			{
				AssertContains(ComplianceSequenceNumberAllocationErrorMessages.ComplianceSequenceIsFullOrExpiredExceptionMessage, ex.UserFriendlyMessage);
			}

			AssertEquals("Postcondition - transaction 1 referencs is generated (will not be saved as exception is threw)", "ABC000000001", invoice1.AH_TransactionReference);
			AssertEquals("Postcondition - transaction 3 referencs is NOT generated", "", invoice3.AH_TransactionReference);
			Assert(allocator.AllocationProcessFailed_ForTestOnly);
		}

		public void TestAutoAllocateComplianceSequenceNumbersReassignNumberDependesOnUsersChoice()
		{
			invoice1.AH_TransactionReference = "ABC000000001";
			sequenceTXI.XD_IsActive = true;
			sequenceTXI.XD_NextNumber = 5;
			TestObjectCreator.Factory.Save();
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1 };

			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			AssertEquals("Postcondition - transaction 1 referencs is re-generated because user choose to reassign a new sequence number", "ABC000000005", invoice1.AH_TransactionReference);

			invoice2.AH_TransactionReference = "ABC000000002";
			sequenceTXI.XD_NextNumber = 6;
			transactionsReloaded = new TransactionHeader[] { invoice2 };
			allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
			AssertEquals("Postcondition - transaction 2 referencs is NOT re-generated because user choose to NOT reassign a new sequence number", "ABC000000002", invoice2.AH_TransactionReference);
		}

		public void TestAutoAllocateComplianceSequenceNumbersNotAllocateWhenDisableComplianceDocumentModule()
		{
			AssertEquals("Precondition - transaction 1 referencs is empty", ZString.Empty, invoice1.AH_TransactionReference);

			sequenceTXI.XD_IsActive = true;
			TestObjectCreator.Factory.Save();
			var transactionsReloaded = new TransactionHeader[] { invoice1 };

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(invoice1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
				AssertEquals("transaction 1 referencs will be empty when EnableComplianceDocumentModule is true.", ZString.Empty, invoice1.AH_TransactionReference);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(invoice1.Company.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
				AssertNotEquals("transaction 1 referencs will not be empty when EnableComplianceDocumentModule is false.", ZString.Empty, invoice1.AH_TransactionReference);
			}
		}

		public void TestWhenSubtypeUndefinedThrowsExecption()
		{
			invoice1.AH_ComplianceSubType = ZString.Empty;
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1 };
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			ITransactionParticipant[] participants = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
			try
			{
				BusinessObjectFactory.SaveTogether(participants);
			}
			catch (NoComplianceInvoicesToPrintLBDException ex)
			{
				AssertContains(ComplianceSequenceNumberAllocationErrorMessages.NoComplianceInvoicesToPrintLBDMessage, ex.UserFriendlyMessage);
			}
		}

		public void TestPrintedMultipleInvoicesWhichArePrintedThrowsExecption()
		{
			invoice1.AH_InvoicePrinted = true;
			TransactionHeader[] transactionsReloaded = new TransactionHeader[] { invoice1, invoice2 };
			InvoiceBatchComplianceSequenceNumberAllocator allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, transactionsReloaded, TestObjectCreator.Factory);
			SetupUserQuestions(allocator);
			ITransactionParticipant[] participants = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
			try
			{
				BusinessObjectFactory.SaveTogether(participants);
			}
			catch (UnableToPrintDueToAtLeastOneInvoicePrintedAlreadyException ex)
			{
				AssertEquals(@"Please change your selection.
 Compliance Numbers cannot be allocated because at least one selected transaction has already been printed. Please amend your selection", ex.UserFriendlyMessage);
			}
		}

		#region Tests for Turkey behaviors

		public void TestComplianceDocumentPrintingDoesNotAllocateComplianceNumberWithMANOptionSelected() =>
			AssertComplianceNumberAllocation(GovtTaxInvoicePrintTask.GovtTaxInvoice, isTestForAlreadyAllocatedComplianceNumber: false, "");

		public void TestAllocateComplianceDocumentAllocatesComplianceNumberWithMANOptionSelected() =>
			AssertComplianceNumberAllocation(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly, isTestForAlreadyAllocatedComplianceNumber: false, "ABC000000001");

		public void TestAllocateComplianceDocumentDoesNotChangeComplianceNumberIfItIsAlreadySetWhenTurkeyEInvoicingEnabled() =>
			AssertComplianceNumberAllocation(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly, isTestForAlreadyAllocatedComplianceNumber: true, "CBA000000001");

		public void TestComplianceDocumentPrintingDoesNotAllocateComplianceNumberWithMANOptionSelected_MultiTransactions() =>
			AssertComplianceNumberAllocation(GovtTaxInvoicePrintTask.GovtTaxInvoice, isTestForAlreadyAllocatedComplianceNumber: false, "", "");

		public void TestAllocateComplianceDocumentAllocatesComplianceNumberWithMANOptionSelected_MultiTransactions() =>
			AssertComplianceNumberAllocation(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly, isTestForAlreadyAllocatedComplianceNumber: false, "ABC000000001", "ABC000000002");

		public void TestAllocateComplianceDocumentDoesNotChangeComplianceNumberIfItIsAlreadySetWhenTurkeyEInvoicingEnabled_MultiTransactions() =>
			AssertComplianceNumberAllocation(GovtTaxInvoicePrintTask.AllocateSequenceNumberOnly, isTestForAlreadyAllocatedComplianceNumber: true, "CBA000000001", "CBA000000002");

		void AssertComplianceNumberAllocation(string govtTaxInvoicePrintTask, bool isTestForAlreadyAllocatedComplianceNumber, params string[] expectedComplianceNumbers)
		{
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.ComplianceDocumentNumberAllocation_Receivables.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Manual))
			{
				var sequenceEIN = TestObjectCreator.SetupComplianceSequence(menuPK1, "EIN", "ABC", 1, 100, 1);
				var transactionsReloaded = new List<TransactionHeader>();
				foreach (var expectedComplianceNumber in expectedComplianceNumbers)
				{
					var invoice = TestObjectCreator.SetupComplianceInvoice("EIN", false, complianceNumber: isTestForAlreadyAllocatedComplianceNumber ? expectedComplianceNumber : "");
					transactionsReloaded.Add(invoice);
				}
				TestObjectCreator.Factory.Save();

				var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(govtTaxInvoicePrintTask, transactionsReloaded.ToArray(), TestObjectCreator.Factory);
				SetupUserQuestions(allocator);

				ITransactionParticipant[] factories = InvoiceBatchComplianceSequenceNumberAllocator.GetFactoriesWithAllocationCodeToBeCalledOnSaving(allocator, TestObjectCreator.Factory);
				BusinessObjectFactory.SaveTogether(factories);
				AssertEquals(false, allocator.AllocationProcessFailed_ForTestOnly);
				AssertEquals("Count of processed transactions for allocation/printing should be the same", transactionsReloaded.Count, allocator.TransactionsGoingToBePrinted_ForTestOnly.Count);

				var param = 0;
				foreach (var transaction in transactionsReloaded)
				{
					AssertEquals(expectedComplianceNumbers[param], transaction.AH_TransactionReference);
					param++;
				}
			}
		}

		#endregion

		void SetupUserQuestions(InvoiceBatchComplianceSequenceNumberAllocator allocator)
		{
			allocator.AskReprintInvoiceOption += delegate()
			{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionReprintPrintedInvoice, "Reprint Invoice", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
			allocator.AskReassignNumberOption += delegate()
			{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionAssignNewSequenceNumber, "Reassign sequence number", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
			allocator.AskUseCurrentBrancheBooksOption += delegate()
			{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionUseCurrentBranchesBooks, "Allocate sequence number", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
			allocator.AskUseCurrentBrancheDepartmentBooksOption += delegate()
			{ return Globals.Message.Show(InvoiceBatchComplianceSequenceNumberAllocator.QuestionUseCurrentBrancheDepartmentsBooks, "Allocate sequence number", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes; };
		}

		ZGuid menuPK1, menuPK2;
		AccComplianceSequence sequenceTXI, sequenceTCR, sequenceTXC, sequenceARI;
		ARInvoice invoice1, invoice2, invoice3, invoice4, invoice5;
		BusinessObjectFactory newFactory;
		TestObjectCreator creator;

		protected override void SetUp()
		{
			base.SetUp();

			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Peru);

			(menuPK1, sequenceTXI) = SetupComplianceSequenceBookTXI(menuText: "Govt Compliance Inv");

			invoice1 = TestObjectCreator.SetupComplianceInvoice("TXI");
			invoice2 = TestObjectCreator.SetupComplianceInvoice("TXI");

			menuPK2 = TestObjectCreator.SetupComplianceMenuAndPivot("Govt Compliance Crd");
			sequenceTCR = TestObjectCreator.SetupComplianceSequence(menuPK2, "TCR", "XYZ", 1000, 2000, 1000);
			var printQueue2 = TestObjectCreator.CreatePrintQueue(Factory);
			sequenceTCR.XD_SQ_DocumentPrintQueue = printQueue2.PK;

			invoice3 = TestObjectCreator.SetupComplianceInvoice("TCR");
			invoice4 = TestObjectCreator.SetupComplianceInvoice("TCR");

			newFactory = new BusinessObjectFactory();
			creator = new TestObjectCreator(newFactory);
			sequenceTXC = creator.SetupComplianceSequence(menuPK1, "TXC", "ABC", 1, 100, 1);
			sequenceTXC.XD_RollupBehaviourWhenMaxExceeded = Constants.ComplianceRollupBehaviourType.SinglePageSummarize;

			sequenceARI = TestObjectCreator.SetupComplianceSequence(menuPK1, "ARI", "A", 1, 100, 1, Core.Constants.ComplianceBookAllocationLevel.Branch);

			invoice5 = newFactory.NewWithValidTestData<ARInvoice>();
		}
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceBatchComplianceSequenceNumberAllocator(Factory);
		}

		InvoiceBatchComplianceSequenceNumberAllocator SetupAllocatorAndComplianceSequence(ZString transactionReference)
		{
			invoice1.AH_TransactionReference = transactionReference;
			sequenceTXI.XD_IsActive = true;
			sequenceTXI.XD_NextNumber = 5;
			Factory.Save();

			var allocator = new InvoiceBatchComplianceSequenceNumberAllocator(GovtTaxInvoicePrintTask.GovtTaxInvoice, new[] { invoice1 }, Factory);
			SetupUserQuestions(allocator);

			return allocator;
		}

		static void RunResetSequanceNumberFunctionality(InvoiceBatchComplianceSequenceNumberAllocator allocator)
		{
			allocator.OnFactorySavingBeforeTransactionCore_ForTestOnly();
			allocator.AutoAllocateComplianceSequenceNumbers_ForTestOnly();
		}

		(ZGuid menuPk, AccComplianceSequence sequence) SetupComplianceSequenceBookTXI(string menuText = "Govt Compliance Inv")
		{
			var menuPK = TestObjectCreator.SetupComplianceMenuAndPivot(menuText);
			var sequence = TestObjectCreator.SetupComplianceSequence(menuPK, "TXI", "ABC", 1, 100, 1);
			sequence.XD_RollupBehaviourWhenMaxExceeded = Constants.ComplianceRollupBehaviourType.SinglePageSummarize;
			sequence.XD_PrintingAuthorizationNumber = "Test";
			sequence.XD_SQ_DocumentPrintQueue = TestObjectCreator.CreatePrintQueue(Factory).PK;
			return (menuPK, sequence);
		}

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;
	}
}
