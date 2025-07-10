using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.AccountingDependency;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(TransactionWithOverriddenBranchAndDepartment))]
	public class TransactionWithOverriddenBranchAndDepartmentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var invoice = Factory.New<ARInvoice>();
			var bizo = new TransactionWithOverriddenBranchAndDepartment(invoice);
			return bizo;
		}

		public void TestReadOnlyProperties()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);

			var wrappedBizO = new TransactionWithOverriddenBranchAndDepartment(invoice);
			AssertEquals("AH_OH", invoice.AH_OH, wrappedBizO.AH_OH);
			AssertEquals("HeaderFullName", invoice.HeaderFullName, wrappedBizO.HeaderFullName);
			AssertEquals("AH_TransactionType", invoice.AH_TransactionType, wrappedBizO.AH_TransactionType);
			AssertEquals("JR_GB", invoice.AH_TransactionCategory, wrappedBizO.AH_TransactionCategory);
			AssertEquals("JR_GE", invoice.AH_TransactionNum, wrappedBizO.AH_TransactionNum);
			AssertEquals("AH_JH", invoice.AH_JH, wrappedBizO.AH_JH);
			AssertEquals("AH_RX_NKTransactionCurrency", invoice.AH_RX_NKTransactionCurrency, wrappedBizO.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_OSTotalAmount", invoice.AH_OSTotalAmount, wrappedBizO.AH_OSTotalAmount);
			AssertEquals("AH_Desc", invoice.AH_Desc, wrappedBizO.AH_Desc);
			AssertEquals("AH_OSTaxAmount", invoice.AH_OSTaxAmount, wrappedBizO.AH_OSTaxAmount);
			AssertEquals("AH_OSExTaxAmount", invoice.AH_OSExTaxAmount, wrappedBizO.AH_OSExTaxAmount);
			AssertEquals("AH_GB", invoice.AH_GB, wrappedBizO.AH_GB);
			AssertEquals("AH_GE", invoice.AH_GE, wrappedBizO.AH_GE);
		}

		public void TestBothChargeAndWrappedChargeObjectsHaveSameLookupList()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			var wrappedBizo = new TransactionWithOverriddenBranchAndDepartment(invoice);
			AssertEquals("Must have the same Lookup", invoice.Lookups, wrappedBizo.Lookups);
		}

		public void TestEmptyBranch()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);

			overrideTest.AH_GB = Guid.Empty;

			var expectedError = "Please enter a Branch.";
			Assert(overrideTest.AH_GBInfo.HasError(expectedError));

			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			branch1.GB_GC = Env.CurrentCompany.PK;
			branch1.GB_Code = "SGB";
			branch1.GB_BranchName = "SG Branch";
			branch1.GB_City = "Singapore";

			overrideTest.AH_GB = branch1.PK;
			Assert(!overrideTest.AH_GBInfo.HasError(expectedError));
		}

		public void TestEmptyDepartment()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);

			overrideTest.AH_GE = Guid.Empty;
			var expectedError = "Please enter a Department.";

			Assert(overrideTest.AH_GEInfo.HasError(expectedError));

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>(TestBusinessObjectKind.MinimumRequiredToSave);
			overrideTest.AH_GE = department1.PK;
			Assert(!overrideTest.AH_GEInfo.HasError(expectedError));
		}

		public void TestInvalidBranchBelongingToDifferentCompany()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);

			var company1 = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			company1.GC_Code = "CPY";
			company1.GC_City = "Singapore";
			company1.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Singapore;
			company1.GC_Name = "Singapore Company";

			var branch1 = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			branch1.GB_GC = company1.PK;
			branch1.GB_Code = "SGB";
			branch1.GB_BranchName = "SG Branch";
			branch1.GB_City = "Singapore";

			var branch2 = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			branch2.GB_GC = Env.CurrentCompany.PK;
			branch2.GB_Code = "RGB";
			branch2.GB_BranchName = "RG Branch";
			branch2.GB_City = "RGBBB";

			var expectedError = "Enter a valid Branch.";

			overrideTest.AH_GB = branch2.PK;
			Assert(!overrideTest.AH_GBInfo.HasError(expectedError));

			overrideTest.AH_GB = branch1.PK;
			Assert(overrideTest.AH_GBInfo.HasError(expectedError));
		}

		public void TestInvalidInactiveBranch()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);

			var branch1 = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_Code = "SGB";
			branch1.GB_BranchName = "SG Branch";
			branch1.GB_City = "Singapore";
			branch1.GB_IsActive = false;

			var expectedError = "Enter a valid Branch.";

			overrideTest.AH_GB = branch1.PK;
			Assert(overrideTest.AH_GBInfo.HasError(expectedError));
		}

		public void TestInvalidDepartment()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);

			Factory.Save();

			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, invoice,
					() => { overrideTest.ValidateAH_GE_ForTestOnly(); }, overrideTest.AH_GEInfo);
		}

		public void TestEnforcePostingAtBranchLevelEnabledOverrideValidation()
		{
			var objectCreator = new TestObjectCreator(Factory);

			var invoice = objectCreator.CreateARInvoice<ARInvoice>("TRN0001", objectCreator.AUD, 1.0M, objectCreator.Debtor);
			invoice.SetContext(BusinessContext.OverrideTransactionBranchAndDepartment);

			var line = (InvoicingLineBase)invoice.Lines.AddNew();

			line.AL_GB = objectCreator.NonCurrentBranch.PK;

			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var expectedError = @"Branch is invalid as the branch set in the charges do not belong to the same Posting Group as that of the header.";

			overrideTest.AH_GB = GlbBranch.CurrentBranch.PK;
			Assert(overrideTest.AH_GBInfo.HasError(expectedError));

			overrideTest.AH_GB = objectCreator.NonCurrentBranch.PK;
			Assert(!overrideTest.AH_GBInfo.HasError(expectedError));

			invoice.AH_GB = objectCreator.NonCurrentBranch.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;

			Assert(line.AL_GBInfo.HasError(@"Please review the charge lines entered and overridden header and ensure all branches entered belong to the same Posting Group. Header and all charges posted in the one transaction must be in the same Posting Group.
Posting is prevented because header and charges have been entered using a mix of Posting Groups."));
		}

		public void TestOriginalAH_GB_GetTransactionHeaderBranchForOverrideBranchAndDepartment()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var branch1 = testObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var branch2 = testObjectCreator.CreateBranch("BBB", GlbCompany.CurrentCompany);

			var invoice = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "2322", testObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M);
			invoice.AH_GB = branch1.PK;

			Factory.Save();

			invoice.AH_GB = branch2.PK;

			var mockIAccountingDependencyFactory = new Mock<IAccountingDependencyFactory>();
			var branchLevelPostingHelperMock = new Mock<IBranchLevelPostingHelper>();

			ObjectFactory.Substitute(mockIAccountingDependencyFactory.Object);
			mockIAccountingDependencyFactory.Setup(x => x.GetBranchLevelPostingHelper()).Returns(branchLevelPostingHelperMock.Object);

			var expectedBranchPKFromMock = ZGuid.NewZGuid();
			branchLevelPostingHelperMock.Setup(x => x.GetTransactionHeaderBranchForOverrideBranchAndDepartment(invoice)).Returns(expectedBranchPKFromMock);

			var overrideInvoiceBranchAndDept = new TransactionWithOverriddenBranchAndDepartment(invoice);
			AssertEquals("If NOT in context, GetTransactionHeaderBranchForOverrideBranchAndDepartment is called", expectedBranchPKFromMock, overrideInvoiceBranchAndDept.AH_GB);
			branchLevelPostingHelperMock.Verify(x => x.GetTransactionHeaderBranchForOverrideBranchAndDepartment(invoice), Times.Once);

			invoice.SetContext(BusinessContext.OverrideTransactionBranchAndDepartment);
			overrideInvoiceBranchAndDept = new TransactionWithOverriddenBranchAndDepartment(invoice);
			AssertEquals("If in context, GetTransactionHeaderBranchForOverrideBranchAndDepartment is NOT called", branch1.PK, overrideInvoiceBranchAndDept.AH_GB);
		}

		public void TestApplyChange()
		{
			var invoice = Factory.New<APInvoice>();
			var expectedBranchPK = ZGuid.NewZGuid();
			var expectedDepartmentPK = ZGuid.NewZGuid();
			var overrideInvoiceBranchAndDept = new TransactionWithOverriddenBranchAndDepartment(invoice);
			overrideInvoiceBranchAndDept.AH_GB = expectedBranchPK;
			overrideInvoiceBranchAndDept.AH_GE = expectedDepartmentPK;

			overrideInvoiceBranchAndDept.AppendChange();
			AssertEquals(expectedBranchPK, invoice.AH_GB);
			AssertEquals(expectedDepartmentPK, invoice.AH_GE);
		}

		public void TestApprovingInvoiceOverrideBranch()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var branch1 = testObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var originalBranch = GlbBranch.CurrentBranch;

			var invoice = testObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(testObjectCreator.AALSHI, 100);
			var approvalRequestInOtherFactory = testObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(invoice);

			var invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.ReferenceFreeText.Equals("Transaction Branch Edited: From '" + originalBranch.GB_Code + "' to 'AAA'"));
			AssertNull("have not overridden yet", invoiceApprovedLog);

			AssertEquals("Precondition: default branch", invoice.AH_GB, originalBranch.PK);

			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);
			invoice.SetContext(BusinessContext.OverrideTransactionBranchAndDepartment);
			overrideTest.AH_GB = branch1.PK;
			overrideTest.AppendChange(); // apply the override
			overrideTest.CreateLogAfterOverridingBranchAndDepartment_ForTestOnly();

			invoiceApprovedLog = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.ReferenceFreeText.Equals("Transaction Branch Edited: From '" + originalBranch.GB_Code + "' to 'AAA'"));
			AssertNotNull("overridden", invoiceApprovedLog);

			approvalRequestInOtherFactory.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalRequestInOtherFactory.Factory.Save();

			var newFactory = new BusinessObjectFactory();

			invoice.RemoveContext(BusinessContext.OverrideTransactionBranchAndDepartment);
			invoice.MoveFromIncompleteToPayableLedger();
			newFactory.Save();

			AssertEquals("If override is applied, it should still be overriden when approved", invoice.AH_GB, branch1.PK);
		}

		public void TestCreateLogAfterOverridingBranchAndDepartment_WhenBranchIsDeleted()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var branch = testObjectCreator.CreateBranch("AAA", GlbCompany.CurrentCompany);
			var invoice = testObjectCreator.CreateAPInvoiceForApprovalRequest<APInvoice>(testObjectCreator.AALSHI, 100);

			var overrideTest = new TransactionWithOverriddenBranchAndDepartment(invoice);
			invoice.SetContext(BusinessContext.OverrideTransactionBranchAndDepartment);
			overrideTest.AH_GB = branch.PK;
			overrideTest.AppendChange();

			var newFactory = new BusinessObjectFactory();
			newFactory.Load<GlbBranch>(branch.PK).Delete();
			newFactory.Save();

			var expectedReferenceFreeText = "Transaction Branch Edited: From '" + GlbBranch.CurrentBranch.GB_Code + "' to ''";
			var log = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.ReferenceFreeText.Equals(expectedReferenceFreeText));
			AssertNull("Preconditon", log);

			AssertNoExceptionThrown(() => overrideTest.CreateLogAfterOverridingBranchAndDepartment_ForTestOnly());

			log = invoice.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(x => x.ReferenceFreeText.Equals(expectedReferenceFreeText));
			AssertNotNull(log);
		}

		public void TestInterCompanyInvalidDepartmentRowErrorIsRemovedDuringPreSaveVaidation()
		{
			var company = TestObjectCreator.CreateNewCompany("TSC", orgProxy: TestObjectCreator.LocalClient2);
			var branch = TestObjectCreator.CreateBranch("TS1", company, TestObjectCreator.Agent);
			var shipment = TestObjectCreator.CreateShipment("S0000002");
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GB = branch.PK;
			job.JH_GE = TestObjectCreator.FESDepartment.PK;

			Factory.Save();

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			var jobRelatedTransaction = TestObjectCreator.CreateARInvoice<ARInvoice>("0000001", TestObjectCreator.LocalCurrency, 1m, TestObjectCreator.Agent);
			jobRelatedTransaction.AH_JH = job.PK;
			CreateARInvoiceLine(jobRelatedTransaction, job);

			var jobRelatedApinvoice = Factory.NewWithValidTestData<APInvoice>();
			CreateAPInvoiceLine(jobRelatedApinvoice, job, ZGuid.Empty, ZGuid.Empty);
			IntercompanyTransactionImportHelper.SetBranchAndDepartmentOnAPInvoice(jobRelatedTransaction, jobRelatedApinvoice, true);
			AssertEquals("Branch should be from org proxy.", branch.PK, jobRelatedApinvoice.AH_GB);
			AssertEquals("calculatedDepartment should be empty as job has no default department in a company of AP invoice.", ZGuid.Empty, jobRelatedApinvoice.AH_GE);
			Assert(jobRelatedApinvoice.AH_GEInfo.HasError("Please enter a Department."));
			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as some Transaction line jobs have invalid department.");

			var overrideInvoiceBranchAndDept = new TransactionWithOverriddenBranchAndDepartment(jobRelatedApinvoice);
			overrideInvoiceBranchAndDept.AH_GE = TestObjectCreator.FIADepartment.PK;

			overrideInvoiceBranchAndDept.AppendChange();
			AssertEquals(branch.PK, jobRelatedApinvoice.AH_GB);
			AssertEquals(TestObjectCreator.FIADepartment.PK, jobRelatedApinvoice.AH_GE);

			AssertHasRowError(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as some Transaction line jobs have invalid department.");
			overrideInvoiceBranchAndDept.RunPreSaveValidation();
			AssertNoRowErrorContaining(jobRelatedApinvoice, "Intercompany Transaction cannot be imported as some Transaction line jobs have invalid department.");
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
	}
}
