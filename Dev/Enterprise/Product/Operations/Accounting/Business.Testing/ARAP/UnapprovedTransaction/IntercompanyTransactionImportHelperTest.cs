using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class IntercompanyTransactionImportHelperTest : TestCaseWithFactory
	{
		public void TestSetBranchAndDepartmentOnUXMLImport()
		{
			//The idea is to repeat all non Auto import cases from TestSetBranchAndDepartmentOnAPInvoice in this test and see what works and what not for UXML case. 
			//I think it is better just to copy that test here and modify. Otherwise it will be mess as setup is very different, auto import should be excluded and some cases may not be applicable.
			//Check if failed cases fail intentionally (then fix conditions) or not (then fix production code)

			var importedTransaction = TestObjectCreator.CreateTransactionPendingAllocation("0000001", TestObjectCreator.Creditor1, 100);
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;

			var sourceWrapper = new IntercompanyTransactionImportHelper.IntercompanyBranchDepartmentDeciderSourceWrapper
			{
				AH_GE = TestObjectCreator.MiscDepartment.PK,
				IsJobRelated = true,
				JobBranch = branch
			};
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Branch should be transaction default value as debtor is not set in sourceWrapper.", TestObjectCreator.NonCurrentBranch.PK, importedTransaction.AH_GB);
			AssertEquals("calculatedDepartment should be transaction default value as JobDepartment is not set.", TestObjectCreator.MiscDepartment.PK, importedTransaction.AH_GE);
			var expectedBranchWarning = "Transaction Branch is set to the message branch because Transaction Branch cannot be set with reference to the invoice debtor organization proxy.";
			AssertHasRowWarning(importedTransaction, expectedBranchWarning);
			importedTransaction.ClearRowNotifications();

			sourceWrapper.AH_OH = TestObjectCreator.Agent.PK;
			sourceWrapper.JobBranch = null;
			sourceWrapper.JobDepartmentPK = TestObjectCreator.FEADepartment.PK;
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Branch should be transaction default value as not in current company for manual import.", TestObjectCreator.NonCurrentBranch.PK, importedTransaction.AH_GB);
			AssertEquals("calculatedDepartment should be from JobDepartment.", TestObjectCreator.FEADepartment.PK, importedTransaction.AH_GE);
			AssertHasRowWarning(importedTransaction, expectedBranchWarning);
			importedTransaction.ClearRowNotifications();

			TestObjectCreator.CreateBranch("TS3", company, TestObjectCreator.Agent);
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Branch should be transaction default value as job branch is not valid.", TestObjectCreator.NonCurrentBranch.PK, importedTransaction.AH_GB);
			AssertEquals("calculatedDepartment should be from JobDepartment.", TestObjectCreator.FEADepartment.PK, importedTransaction.AH_GE);
			AssertHasRowWarning(importedTransaction, expectedBranchWarning);
			importedTransaction.ClearRowNotifications();

			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("CB2", GlbCompany.CurrentCompany, TestObjectCreator.LocalClient);
			var branch3 = TestObjectCreator.CreateBranch("CB3", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);
			var branch4 = TestObjectCreator.CreateBranch("CB4", GlbCompany.CurrentCompany, TestObjectCreator.Debtor);

			sourceWrapper.JobBranch = branch1;
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Branch should be from job branch.", branch1.PK, importedTransaction.AH_GB);
			AssertEquals("calculatedDepartment should be from JobDepartment.", TestObjectCreator.FEADepartment.PK, importedTransaction.AH_GE);
			AssertNoRowWarnings(importedTransaction);

			sourceWrapper.AH_OH = TestObjectCreator.Debtor.PK;
			sourceWrapper.IsConsolInvoice = true;
			sourceWrapper.Consol = consol;
			consol.JK_OA_SendingForwarderAddress = TestObjectCreator.LocalClient.MainAddress.PK;
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Branch should be calculated from consol", branch2.PK, importedTransaction.AH_GB);
			AssertEquals("Department should be calculated from consol", TestObjectCreator.FESDepartment.PK, importedTransaction.AH_GE);
			AssertNoRowWarnings(importedTransaction);

			consol.JK_TransportMode = "";
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Branch should be calculated from consol", branch2.PK, importedTransaction.AH_GB);
			AssertEquals("Consol department can't be calculated and so it is not changed.", TestObjectCreator.NonCurrentDepartment.PK, importedTransaction.AH_GE);
			var expectedDepartmentWarning = "Intercompany Transaction Department is used because Department cannot be set with reference to the Consolidation 'C0000001'.";
			AssertNoRowWarningContaining(importedTransaction, expectedBranchWarning);
			AssertHasRowWarning(importedTransaction, expectedDepartmentWarning);
			importedTransaction.ClearRowNotifications();

			sourceWrapper.AH_GE = TestObjectCreator.FIADepartment.PK;
			sourceWrapper.IsConsolInvoice = false;
			sourceWrapper.IsJobRelated = false;
			importedTransaction.AH_GB = TestObjectCreator.NonCurrentBranch.PK;
			importedTransaction.AH_GE = TestObjectCreator.NonCurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnUXMLImport(importedTransaction, sourceWrapper);
			AssertEquals("Debtor is orgproxy to more then one branch and so default transaction branch is not changed.", TestObjectCreator.NonCurrentBranch.PK, importedTransaction.AH_GB);
			AssertEquals("Non-job invoice's department should go from AR invoice", TestObjectCreator.FIADepartment.PK, importedTransaction.AH_GE);
			AssertHasRowWarning(importedTransaction, expectedBranchWarning);
			AssertNoRowWarningContaining(importedTransaction, expectedDepartmentWarning);
			importedTransaction.ClearRowNotifications();
		}

		public void TestSetBranchAndDepartmentOnAPInvoice()
		{
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var jobRelatedTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("0000001", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			jobRelatedTransaction.AH_JH = job2.PK;
			CreateARInvoiceLine(jobRelatedTransaction, job2);

			var jobRelatedApinvoice = Factory.NewWithValidTestData<APInvoice>();
			CreateAPInvoiceLine(jobRelatedApinvoice, job2, ZGuid.Empty, ZGuid.Empty);
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Branch should be from org proxy.", branch.PK, jobRelatedApinvoice.AH_GB);
			AssertEquals("calculatedDepartment should be empty as job2 has no default department in a company of AP invoice.", ZGuid.Empty, jobRelatedApinvoice.AH_GE);
			Assert(jobRelatedApinvoice.AH_GEInfo.HasError("Please enter a Department."));
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as some Transaction line jobs have invalid department.");
			jobRelatedApinvoice.ClearRowNotifications();

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Branch should be empty as not in current company for manual import.", ZGuid.Empty, jobRelatedApinvoice.AH_GB);
			AssertEquals("calculatedDepartment should be empty as job2 has no default department in a company of AP invoice.", ZGuid.Empty, jobRelatedApinvoice.AH_GE);
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as some Transaction line jobs have invalid department.");
			jobRelatedApinvoice.ClearRowNotifications();

			jobRelatedApinvoice.Lines[0].Job.JH_GE = TestObjectCreator.FEADepartment.PK;
			jobRelatedApinvoice.Lines[0].Job.JH_GB = ZGuid.Empty;
			jobRelatedApinvoice.Lines[0].AL_JH = ZGuid.Empty;
			jobRelatedApinvoice.Lines[0].AL_JH = job2.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Branch should be from org proxy.", branch.PK, jobRelatedApinvoice.AH_GB);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Branch should be empty as not in current company for manual import.", ZGuid.Empty, jobRelatedApinvoice.AH_GB);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			TestObjectCreator.CreateBranch("TS3", company, TestObjectCreator.Agent);
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Branch should be empty as job branch is not valid.", ZGuid.Empty, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy.");
			jobRelatedApinvoice.ClearRowNotifications();

			var branch1 = TestObjectCreator.CreateBranch("CB1", GlbCompany.CurrentCompany);
			var branch2 = TestObjectCreator.CreateBranch("CB2", GlbCompany.CurrentCompany);

			jobRelatedApinvoice.Lines[0].Job.JH_GB = branch1.PK;
			jobRelatedApinvoice.Lines[0].AL_JH = ZGuid.Empty;
			jobRelatedApinvoice.Lines[0].AL_JH = job2.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Branch should be from job branch.", branch1.PK, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Branch should be from job branch.", branch1.PK, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			CreateAPInvoiceLine(jobRelatedApinvoice, job1, ZGuid.Empty, ZGuid.Empty);
			jobRelatedApinvoice.Lines[1].Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Periodic invoice: Branch should be empty as one job branch is not valid.", ZGuid.Empty, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy.");
			jobRelatedApinvoice.ClearRowNotifications();

			jobRelatedApinvoice.Lines[1].Job.JH_GB = branch2.PK;
			jobRelatedApinvoice.Lines[1].AL_JH = ZGuid.Empty;
			jobRelatedApinvoice.Lines[1].AL_JH = job1.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Periodic invoice: Branch should be empty as job branches are different.", ZGuid.Empty, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy.");
			jobRelatedApinvoice.ClearRowNotifications();

			jobRelatedApinvoice.Lines[0].Job.JH_GB = jobRelatedApinvoice.Lines[1].Job.JH_GB;
			jobRelatedApinvoice.Lines[0].AL_JH = ZGuid.Empty;
			jobRelatedApinvoice.Lines[0].AL_JH = job2.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("Periodic invoice: Branch should be from job branch.", branch2.PK, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Periodic invoice: Branch should be from job branch.", branch2.PK, jobRelatedApinvoice.AH_GB);
			AssertNoRowErrors(jobRelatedApinvoice);

			var consolRelatedTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("0000002", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			consolRelatedTransaction.AH_ConsolidatedInvoiceRef = "C0000001";
			CreateARInvoiceLine(consolRelatedTransaction, job1);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(consolRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Department should be calculated from consol", TestObjectCreator.FESDepartment.PK, jobRelatedApinvoice.AH_GE);

			consol.JK_TransportMode = "";
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(consolRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Consol department can't be calculated.", ZGuid.Empty, jobRelatedApinvoice.AH_GE);
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as Department cannot be set with reference to the Consolidation 'C0000001'.");
			jobRelatedApinvoice.ClearRowNotifications();

			var nonJobARTransaction = Factory.NewWithValidTestData<ARInvoice>();
			nonJobARTransaction.AH_GE = TestObjectCreator.FIADepartment.PK;
			var nonJobAPInvoice = Factory.NewWithValidTestData<APInvoice>();
			var nonJobAPLine = (InvoicingLineBase)nonJobAPInvoice.Lines.AddNew();
			nonJobAPLine.AL_GE = TestObjectCreator.FISDepartment.PK;
			nonJobAPLine.AL_GB = branch.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(nonJobARTransaction, nonJobAPInvoice, true);
			AssertEquals("Non-job invoice's department should go from AR invoice", TestObjectCreator.FIADepartment.PK, nonJobAPInvoice.AH_GE);
			AssertEquals("Non-job lines's branch should equal parent's", nonJobAPInvoice.AH_GB, nonJobAPLine.AL_GB);
			AssertEquals("Non-job lines's department should equal parent's", TestObjectCreator.FISDepartment.PK, nonJobAPLine.AL_GE);
		}

		public void TestSetBranchAndDepartmentOnAPInvoice_AmendingCreditNote()
		{
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0000001", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			arInvoice.AH_JH = job2.PK;
			CreateARInvoiceLine(arInvoice, job2);
			var arCreditNote = TestObjectCreator.CreateARCreditNoteReverseTransaction(arInvoice);
			arCreditNote.AH_OH = branch.GB_OH_OrgProxy;

			var apInvoice = Factory.NewWithValidTestData<APInvoice>();
			CreateAPInvoiceLine(apInvoice, job2, branch.PK, ZGuid.Empty);

			var apInvoiceAmending = (IAmending)apInvoice;
			var apCreditNote = (InvoicingBase)apInvoiceAmending.GenerateAmendingTransaction(TransactionTypes.CreditNote);
			AssertEquals("AP Credit Note branch should be equal to the original transaction.", apInvoice.AH_GB, apCreditNote.AH_GB);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(arCreditNote, apCreditNote, false);
			AssertEquals("AP Credit Note branch should be equal to the original transaction.", apInvoice.AH_GB, apCreditNote.AH_GB);
			AssertNotEquals("AP Credit Note branch should NOT be from org proxy.", branch.PK, apCreditNote.AH_GB);
		}

		public void TestSetBranchAndDepartmentOnAPInvoiceWithStampDutyChargeLine()
		{
			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var jobRelatedTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("0000001", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			jobRelatedTransaction.AH_JH = job2.PK;
			CreateARInvoiceLine(jobRelatedTransaction, job2);

			var jobRelatedApinvoice = Factory.NewWithValidTestData<APInvoice>();
			CreateAPInvoiceLine(jobRelatedApinvoice, job2, ZGuid.Empty, TestObjectCreator.FEADepartment.PK, null);
			job2.JH_GE = TestObjectCreator.FEADepartment.PK;
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			var stampDutyChargeCode = TestObjectCreator.CreateChargeCode("BOLLO", "Stamp Duty", "MRG", 1m, TestObjectCreator.SVAT2, TestObjectCreator.WHTFREE1);
			AccountingConfigurationRegistry.Instance.StampDutyChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, stampDutyChargeCode.PK.ToGuid());
			CreateAPInvoiceLine(jobRelatedApinvoice, null, ZGuid.Empty, ZGuid.Empty, stampDutyChargeCode.PK);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			jobRelatedApinvoice.Lines[0].AL_AC = ZGuid.Empty;

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);

			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, false);
			AssertEquals("calculatedDepartment should be from job2.", TestObjectCreator.FEADepartment.PK, jobRelatedApinvoice.AH_GE);
			AssertNoRowErrors(jobRelatedApinvoice);
		}

		public void TestGetAPBranchForAutoImport()
		{
			var jobRelatedTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("0000001", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			CreateARInvoiceLine(jobRelatedTransaction, job2);
			var calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => true);
			Assert("One branch found: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertEquals("One branch found: BranchForIntercompanyImport", "TS1", calculatedBranch.BranchForIntercompanyImport.GB_Code);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => false);
			Assert("One branch found, but invoice is not valid to import: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("One branch found, but invoice is not valid to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			jobRelatedTransaction.AH_OH = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => true);
			Assert("Branches in different companies: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Branches in different companies: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => false);
			Assert("Branches in different companies, but invoice is not valid to import: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Branches in different companies, but invoice is not valid to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			jobRelatedTransaction.AH_OH = TestObjectCreator.LocalClient.PK;
			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => true);
			Assert("No branches found: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("No branches found: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			jobRelatedTransaction.AH_OH = TestObjectCreator.Agent.PK;
			TestObjectCreator.CreateBranch("TS", company, TestObjectCreator.Agent);
			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => true);
			Assert("Branches in the same company: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertEquals("Branches in the same company: BranchForIntercompanyImport", "TS", calculatedBranch.BranchForIntercompanyImport.GB_Code);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(jobRelatedTransaction, x => false);
			Assert("Branches in the same company, but invoice is not valid to import: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Branches in the same company, but invoice is not valid to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			var consolRelatedTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("0000002", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			consolRelatedTransaction.AH_ConsolidatedInvoiceRef = "C0000001";
			CreateARInvoiceLine(consolRelatedTransaction, job1);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => true);
			Assert("Consol invoice with many branches in the same company: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertEquals("Consol invoice with many branches in the same company: BranchForIntercompanyImport", "TS2", calculatedBranch.BranchForIntercompanyImport.GB_Code);

			var differentCompanyBranch = TestObjectCreator.CreateBranch("BR3", differentCompany, consolRelatedTransaction.Header);
			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => true);
			Assert("Consol invoice with many branches in the different companies should not look at company level: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Consol invoice with many branches in the different companies should not look at company level: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			consolRelatedTransaction.AH_OH = TestObjectCreator.LocalClient2.PK;
			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => true);
			Assert("Consol invoice with company org proxy: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertEquals("Consol invoice with company org proxy: BranchForIntercompanyImport", "TS2", calculatedBranch.BranchForIntercompanyImport.GB_Code);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => false);
			Assert("Consol invoice with company org proxy, but not valid invoice to import: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Consol invoice with company org proxy, but not valid invoice to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			var company2 = TestObjectCreator.CreateNewCompany("TCC", orgProxy: TestObjectCreator.LocalClient2);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => x.PK == company.PK);
			Assert("Consol invoice with company org proxy in different companies and with one company valid to import: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Consol invoice with company org proxy in different companies and with one company valid to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => x.PK == differentCompany.PK);
			Assert("Consol invoice with company org proxy in different companies and with no company valid to import: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Consol invoice with company org proxy in different companies and with no company valid to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			var company2Branch = TestObjectCreator.CreateBranch("TC1", company2, TestObjectCreator.Agent2);
			consolRelatedTransaction.AH_OH = TestObjectCreator.Agent2.PK;
			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => true);
			Assert("Consol invoice with branch and another company org proxy: IsIntercompanyInvoice", calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Consol invoice with branch and another company org proxy: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);

			calculatedBranch = IntercompanyTransactionImportHelper.GetAPBranchForAutoImport(consolRelatedTransaction, x => false);
			Assert("Consol invoice with branch and another company org proxy, but not valid invoice to import: IsIntercompanyInvoice", !calculatedBranch.IsIntercompanyInvoice);
			AssertNull("Consol invoice with branch and another company org proxy, but not valid invoice to import: BranchForIntercompanyImport", calculatedBranch.BranchForIntercompanyImport);
		}

		public void TestGetDepartmentToLogIntoAPCompany()
		{
			var allowDepartment = Factory.New<AccAllowedBranchDepartmentCombo>();
			allowDepartment.AAB_GB_Branch = branch.PK;
			allowDepartment.AAB_GE_Department = TestObjectCreator.FIADepartment.PK;
			var department = IntercompanyTransactionImportHelper.GetDepartmentToLogIntoAPCompany(branch, TestObjectCreator.FESDepartment);
			AssertEquals("Calculated department should equal to branch's allow department.", TestObjectCreator.FIADepartment.PK, department.PK);

			var anotherAllowDepartment = Factory.New<AccAllowedBranchDepartmentCombo>();
			anotherAllowDepartment.AAB_GB_Branch = branch.PK;
			anotherAllowDepartment.AAB_GE_Department = TestObjectCreator.FESDepartment.PK;

			department = IntercompanyTransactionImportHelper.GetDepartmentToLogIntoAPCompany(branch, TestObjectCreator.FESDepartment);
			AssertEquals("Calculated department should equal to branch's allow department.", TestObjectCreator.FESDepartment.PK, department.PK);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		void CreateARInvoiceLine(InvoicingBase transaction, Job job)
		{
			var jobARLine = (ARInvoiceLine)transaction.Lines.AddNew();
			jobARLine.AL_AC = TestObjectCreator.CC1.PK;
			jobARLine.AL_GB = transaction.AH_GB;
			jobARLine.AL_JH = job.PK;
			jobARLine.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateJobCharge(jobARLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
		}

		void CreateAPInvoiceLine(InvoicingBase apinvoice, Job job, ZGuid jobBranchPK, ZGuid jobDepartmentPK)
		{
			var jobAPLine = (APInvoiceLine)apinvoice.Lines.AddNew();
			jobAPLine.AL_AC = TestObjectCreator.CC1.PK;
			jobAPLine.AL_GB = apinvoice.AH_GB;
			jobAPLine.AL_JH = job.PK;
			jobAPLine.AL_AT = TestObjectCreator.GST1.PK;
			TestObjectCreator.CreateJobCharge(jobAPLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			jobAPLine.AL_GE = apinvoice.AH_GE;
			jobAPLine.Job.JH_GE = jobBranchPK;
			jobAPLine.Job.JH_GB = jobDepartmentPK;
		}

		void CreateAPInvoiceLine(InvoicingBase apinvoice, Job job, ZGuid jobBranchPK, ZGuid jobDepartmentPK, ZGuid? chargePK)
		{
			var jobAPLine = (APInvoiceLine)apinvoice.Lines.AddNew();
			jobAPLine.AL_AC = chargePK ?? TestObjectCreator.CC1.PK;
			jobAPLine.AL_GB = apinvoice.AH_GB;
			jobAPLine.AL_GE = apinvoice.AH_GE;
			jobAPLine.AL_AT = TestObjectCreator.GST1.PK;
			if (job != null)
			{
				TestObjectCreator.CreateJobCharge(jobAPLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				job.JH_GE = jobDepartmentPK;
				job.JH_GB = jobBranchPK;
			}
			jobAPLine.AL_JH = job?.PK ?? ZGuid.Empty;
		}

		GlbBranch branch;
		GlbCompany differentCompany;
		GlbCompany company;
		ForwardingConsol consol;
		ForwardingShipment shipment1;
		ForwardingShipment shipment2;
		Job job1;
		Job job2;

		protected override void SetUp()
		{
			base.SetUp();

			company = TestObjectCreator.CreateNewCompany("TSC", orgProxy: TestObjectCreator.LocalClient2);
			branch = TestObjectCreator.CreateBranch("TS1", company, TestObjectCreator.Agent);
			TestObjectCreator.CreateBranch("TS2", company, GlbCompany.CurrentCompany.OrgProxy);

			differentCompany = TestObjectCreator.CreateNewCompany("BRC", orgProxy: TestObjectCreator.Agent2);

			TestObjectCreator.CreateBranch("BR1", differentCompany, GlbCompany.CurrentCompany.OrgProxy);

			consol = TestObjectCreator.CreateConsol("AUC", "DEF", "C0000001");
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			shipment1 = TestObjectCreator.CreateShipment("S0000001", consol);
			shipment2 = TestObjectCreator.CreateShipment("S0000002");

			job1 = TestObjectCreator.CreateJob(shipment1, false);
			job1.JH_GB = branch.PK;
			job1.JH_GE = TestObjectCreator.FESDepartment.PK;

			job2 = TestObjectCreator.CreateJob(shipment2, false);
			job2.JH_GB = branch.PK;
			job2.JH_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();
		}
	}
}
