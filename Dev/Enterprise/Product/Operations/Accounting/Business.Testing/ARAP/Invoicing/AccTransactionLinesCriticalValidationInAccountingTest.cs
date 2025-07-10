using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class AccTransactionLinesCriticalValidationInAccountingTest : AccTransactionLinesCriticalValidationTest
	{
		public void TestCostTransactionLineWithoutJobChargeWithHeaderNotInDB()
		{
			var creator = new TestObjectCreator(Factory);

			var job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_JobNum = job1.PK.ToString().Substring(1, JobHeaderSchema.JH_JobNum.MaxLength).ToUpper();
			Factory.Save();

			var invoice = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV001", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, creator.AALSHI, creator.GLHeader2.PK, "FIN");

			AssertEquals("Precondition", 1, invoice.Lines.Count);

			var line = invoice.Lines[0];
			line.AL_JH = job1.PK;

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobTransactionLineWithoutJobChargeWithDetailedInfo", true,
				CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5,
				"CST transaction line that does not have a related job charge",
				"Query:WHERE JR_JH = ",
				"Header: PK = " + line.AL_AH.ToString(),
				"Line: PK = " + line.PK.ToString(),
				"Job: Job Number = " + job1.JH_JobNum,
				@"Header Object constructor call stack: 
APInvoiceConstructor: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.");

			AssertEquals("Precondition", LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals("Precondition", TransactionTypes.Invoice, invoice.AH_TransactionType);
			Assert("Precondition", !AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value);
			Assert("Precondition", !invoice.IsInDatabase);
			AssertOnSavingCheck(line, testCase);

			var invoice2 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV002", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, creator.AALSHI, creator.GLHeader2.PK, "FIN");

			AssertEquals("Precondition", 1, invoice2.Lines.Count);

			var line2 = invoice2.Lines[0];
			line2.AL_JH = job1.PK;

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobTransactionLineWithoutJobChargeWithDetailedInfo", true,
				CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5,
				"CST transaction line that does not have a related job charge",
				"Query:WHERE JR_JH = ",
				"Header: PK = " + line2.AL_AH.ToString(),
				"Line: PK = " + line2.PK.ToString(),
				"Job: Job Number = " + job1.JH_JobNum,
				@"Header Object constructor call stack: 
APInvoiceConstructor:
   at");

			AssertEquals("Precondition", LedgerTypes.AccountsPayable, invoice2.AH_Ledger);
			AssertEquals("Precondition", TransactionTypes.Invoice, invoice2.AH_TransactionType);
			Assert("Precondition", !AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value);
			Assert("Precondition", !invoice2.IsInDatabase);
			AssertOnSavingCheck(line2, testCase);

			using (AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var invoice3 = creator.CreateInvoiceWithLine(typeof(APInvoice), "INV003", creator.AUD, 1M, 1000M, 100M, 1000M, 100M, creator.AALSHI, creator.GLHeader2.PK, "FIN");

				AssertEquals("Precondition", 1, invoice3.Lines.Count);

				var line3 = invoice3.Lines[0];
				line3.AL_JH = job1.PK;

				testCase = new TestCaseDefinition_ForSeparateTestsMethods("JobTransactionLineWithoutJobChargeWithDetailedInfo", true,
					CriticalValidationErrorType.CostTransactionLineWithoutJobCharge_5,
					"CST transaction line that does not have a related job charge",
					"Query:WHERE JR_JH = ",
					"Header: PK = " + line3.AL_AH.ToString(),
					"Line: PK = " + line3.PK.ToString(),
					"Job: Job Number = " + job1.JH_JobNum,
					"Header Object constructor call stack:    at ");

				AssertEquals("Precondition", LedgerTypes.AccountsPayable, invoice3.AH_Ledger);
				AssertEquals("Precondition", TransactionTypes.Invoice, invoice3.AH_TransactionType);
				Assert("Precondition", AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value);
				Assert("Precondition", !invoice3.IsInDatabase);
				AssertOnSavingCheck(line3, testCase);
			}

			var newFactory = new BusinessObjectFactory();
			var transaction = newFactory.NewWithValidTestData<APInvoice>();
			transaction.AH_Ledger = LedgerTypes.IncompleteTransactions;
			transaction.AH_TransactionType = TransactionTypes.IncompleteInvoice;
			newFactory.Save();

			transaction.AH_Ledger = LedgerTypes.AccountsPayable;
			transaction.AH_TransactionType = TransactionTypes.Invoice;

			var line4 = newFactory.NewWithValidTestData<APInvoiceLine>();
			line4.AL_AH = transaction.PK;
			line4.AL_LineType = TransactionLineTypes.Cost;
			line4.AL_AC = Env.Registry.FreightChargeCode;
			line4.AL_JH = job1.PK;

			Assert("Precondition", line4.TransactionHeader.IsInDatabase);

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => newFactory.Save());
			AssertContains("Error message is reported for CST line", "CST transaction line that does not have a related job charge.", ex.Message);
			AssertNotContains("Error message is reported for CST line", "Header Object constructor call stack:", ex.Message);
			ErrorReporter.Clear();
		}

		[TestDate(2009, 11, 20)]
		public void TestGetTransactionLineInfoWithSubAccounts()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var line = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "T001", testObjectCreator.AUD, 1m, 190m, 0m, 190m, 0m).Lines[0];
			line.SubAccounts.Add(testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, testObjectCreator.AALSHI.PK));
			line.SubAccounts.Add(testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.StaffAndResources, testObjectCreator.Staff.PK));

			var expectedInfo = $"Line: PK = {line.PK}, Charge Code = , GL Account = {line.GLHeader.AccountNum}, Type = CST, OS Amount = -190, Local Amount = -190, GST = 0, Tax Rate = , Tax Class = , Exchange Rate = 1, Currency = AUD, Post Date = , Reverse Date = , Post To GL = N, Reverse To GL = N, Header PK = {line.AL_AH}, Job PK = 00000000-0000-0000-0000-000000000000, Organization = , Revenue Recognition Type = IMM, Is In DB = No, Is Final = No, Sub Accounts = ORG:{testObjectCreator.AALSHI.PK} STR:{testObjectCreator.Staff.PK}, Has Changes = Yes.";

			AssertEquals("Info should be as expected.", expectedInfo, line.GetTransactionLineInfo());
		}

		[TestDate(2009, 11, 20)]
		public void TestSubAccountDetailsCannotBeSetForJobRelatedLines_LineNotInDB()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			Factory.Save();

			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(10), testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var line = testObjectCreator.CreateARInvoiceLine(invoice, job, testObjectCreator.CC1, testObjectCreator.AUD, 1m, "text", 500m);
			var charge = testObjectCreator.CreateJobCharge(line, job, testObjectCreator.CC1);
			line.SubAccounts.Add(testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, testObjectCreator.AALSHI.PK));
			line.SubAccounts.Add(testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.StaffAndResources, testObjectCreator.Staff.PK));

			Assert("Precondition: Line is not in db", !line.IsInDatabase);
			Assert("AL_JH is set", !line.AL_JH.IsEmpty);
			Assert("Sub Account info is set", line.SubAccounts.Count > 0);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"SubAccountDetailsCannotBeSetForJobRelatedLines", true,
				CriticalValidationErrorType.SubAccountDetailsCannotBeSetForJobRelatedLines_1,
				"Transaction Line which is related to Job and has Sub Account Details set.");

			AssertOnSavingCheck(line, testCase);
		}

		[TestDate(2009, 11, 20)]
		public void TestSubAccountDetailsCannotBeSetForJobRelatedLines_LineInDB()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.InsertJobHeader(Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
			job.JH_JobNum = TestObjectCreator.GetRandomString(10);
			Factory.Save();

			var invoice = testObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(10), testObjectCreator.AUD, 1m, testObjectCreator.Debtor);
			var line = testObjectCreator.CreateARInvoiceLine(invoice, null, testObjectCreator.CC1, testObjectCreator.AUD, 1m, "text", 500m);
			Factory.Save();

			line.AL_JH = job.PK;
			var charge = testObjectCreator.CreateJobCharge(line, job, testObjectCreator.CC1);
			line.SubAccounts.Add(testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.Organization, testObjectCreator.AALSHI.PK));
			line.SubAccounts.Add(testObjectCreator.CreateTransactionLineSubAccount<TransactionLineSubAccount>(line.PK, Core.Constants.SubAccountType.StaffAndResources, testObjectCreator.Staff.PK));

			Assert("Precondition: Line is in db", line.IsInDatabase);
			Assert("AL_JH has change", line.AL_JHInfo.HasChanges);
			Assert("Sub Account info is set", line.SubAccounts.Count > 0);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"SubAccountDetailsCannotBeSetForJobRelatedLines", true,
				CriticalValidationErrorType.SubAccountDetailsCannotBeSetForJobRelatedLines_1,
				"Transaction Line which is related to Job and has Sub Account Details set.");

			AssertOnSavingCheck(line, testCase);
		}

		#region Tax Branch

		public void TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_LineTaxBranchIsEmpty()
		{
			var line = CreateLineAndCharge(TransactionLineTypes.Cost, true);
			line.TransactionHeader.AH_GB_TaxBranch = ZGuid.Empty;
			line.AL_GB_TaxBranch = ZGuid.Empty;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertBehavior();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertBehavior();

			void AssertBehavior()
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					"TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_LineTaxBranchIsEmpty", true,
					CriticalValidationErrorType.TransactionLineTaxBranchDifferentFromChargeTaxBranch,
					"Transaction line tax branch '<empty>' is different from line charge tax branch 'SYD'.");

				AssertOnSavingCheck(line, testCase);
			}
		}

		public void TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_CostLine()
		{
			var line = CreateLineAndCharge(TransactionLineTypes.Cost, true);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertBehavior();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertBehavior();

			void AssertBehavior()
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
					"CheckLineTaxBranchAreSyncWithLineChargeTaxBranch_CostLine", true,
					CriticalValidationErrorType.TransactionLineTaxBranchDifferentFromChargeTaxBranch,
					"Transaction line tax branch 'BNE' is different from line charge tax branch 'SYD'.");

				AssertOnSavingCheck(line, testCase);
			}
		}

		public void TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_RevenueLine()
		{
			var line = CreateLineAndCharge(TransactionLineTypes.Revenue, false);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertBehavior();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertBehavior();

			void AssertBehavior()
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_RevenueLine", true,
				CriticalValidationErrorType.TransactionLineTaxBranchDifferentFromChargeTaxBranch,
				"Transaction line tax branch 'BNE' is different from line charge tax branch 'SYD'.");

				AssertOnSavingCheck(line, testCase);
			}
		}

		public void TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_UALine()
		{
			var line = CreateLineAndCharge(TransactionLineTypes.UnapprovedCost, true);

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertBehavior();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertBehavior();

			void AssertBehavior()
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods(
				"TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_UALine", true,
				CriticalValidationErrorType.TransactionLineTaxBranchDifferentFromChargeTaxBranch,
				"Transaction line tax branch 'BNE' is different from line charge tax branch 'SYD'.");

				AssertOnSavingCheck(line, testCase);
			}
		}

		public void TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_Valid()
		{
			var line = CreateLineAndCharge(TransactionLineTypes.Cost, true);
			line.TransactionHeader.AH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			line.AL_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			Factory.Save();

			line.TransactionHeader.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			line.AL_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			var charge = line.LoadRelatedJobCharge();
			AssertNotEquals("Precondition", line.AL_GB_TaxBranch, charge.JR_GB_CostTaxBranch);
			AssertEquals(true, line.IsInDatabase);

			var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_Valid_LinePersisted");
			AssertOnSavingCheck(line, testCase);

			line = CreateLineAndCharge(TransactionLineTypes.Cost, true, hasCharge: false);

			testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_Valid_GenericCharge");
			AssertOnSavingCheck(line, testCase);
		}

		public void TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_ReverseLine()
		{
			var line = CreateLineAndCharge(TransactionLineTypes.Cost, true, hasCharge: false);
			line.AL_ReverseDate = ZDateTime.Now;

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertBehavior();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertBehavior();

			void AssertBehavior()
			{
				var testCase = new TestCaseDefinition_ForSeparateTestsMethods("TestCheckLineTaxBranchAreSyncWithLineChargeTaxBranch_Valid_ReverseLine");
				AssertOnSavingCheck(line, testCase);
			}
		}

		public void TestNoCriticalValidationErrorForAutoJobRevenueJournal()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "DESC", TestObjectCreator.AUD, 10m, null, TestObjectCreator.AUD, 10m, GlbBranch.CurrentBranch.OrgProxy);

			charge.JR_GE = TestObjectCreator.FISDepartment.PK;
			charge.JR_GB_InternalBranch = GlbBranch.CurrentBranch.PK;
			charge.JR_GE_InternalDept = TestObjectCreator.FEADepartment.PK;
			charge.JR_JH_InternalJob = charge.JR_JH;

			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.JobRevenueJournal);
			AssertEquals("Precondition: no JobRevenueJournal", false, Factory.Exists(typeof(AccTransactionHeader), query));
			AssertEquals("Precondition: can create JobRevenueJournal", true, charge.ShouldCreateSellJRJ);

			charge.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;
			Factory.Save();
			AssertEquals("Saved without Critical Validation Error.", true, Factory.Exists(typeof(AccTransactionHeader), query));
		}

		AccTransactionLines CreateLineAndCharge(string transactionLineType, bool isCost, bool hasCharge = true)
		{
			var line = GetLine(Factory, true, transactionLineType, hasCharge: hasCharge, hasHeader: true);  // Adding a type cast because VS2022 runtime checker cannot recognize static protected methods.

			var schemaColumn = isCost ? JobChargeSchema.JR_AL_APLine : JobChargeSchema.JR_AL_ARLine;
			var charge = Factory.LoadTop1<JobCharge>(new ZQuery(schemaColumn, line.PK));

			line.TransactionHeader.AH_GB_TaxBranch = GlbBranch.CurrentBranch.PK;
			line.AL_GB_TaxBranch = GlbBranch.CurrentBranch.PK;

			if (hasCharge)
			{
				if (isCost)
				{
					charge.JR_GB_CostTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				}
				else
				{
					charge.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;
				}
			}
			else
			{
				line.AL_AC = ZGuid.Empty;
				line.AL_JH = ZGuid.Empty;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
			}

			AssertEquals("Pre-condition", "BNE", GlbBranch.CurrentBranch.GB_Code);
			AssertEquals("SYD", TestObjectCreator.NonCurrentBranch.GB_Code);

			return line;
		}

		#endregion

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
