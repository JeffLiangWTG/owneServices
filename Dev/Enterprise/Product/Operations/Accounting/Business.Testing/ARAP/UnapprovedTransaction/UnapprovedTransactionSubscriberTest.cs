using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Registry.Business.CostVarianceApprovalAuthorisationRequirement;

namespace Enterprise.BatchProcessor.Accounting.Testing
{
	[TestedType(typeof(UnapprovedTransactionSubscriber))]
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	class UnapprovedTransactionSubscriberTest : LogSubscriberTest<UnapprovedTransactionSubscriber>
	{
		public void TestTransactionHeaderMustHaveTransactionNumber_WhenEnableValidationWhenAutoImportIntercompanyInvoices()
		{
			using (AccountingConfigurationRegistry.Instance.EnableValidationWhenAutoImportIntercompanyInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTransactionHeaderMustHaveTransactionNumber();
				AssertEquals("Precondition: No exception should be thrown", 0, ExceptionReporterTestListener.Instance.Count);

				RunLogWalkerCycleForTest();

				AssertStartsWith("Should trigger Accounting Critical Validation Error: TransactionHeaderMustHaveTransactionNumber",
					"Developer Details (Critical Validation Failure): \r\n\r\nThis transaction must have transaction number.", ExceptionReporterTestListener.Instance[0].InnerException.Message);
				AssertEquals(true, NotifiedEventList.Contains(@"[UnapprovedTransactionsConverter] failed to process logs. Affected records will be processed again one-by-one.
An error has occurred. Your unsaved work must be re-entered.

Please close the form in which you were working and re-enter the data.

Error Message: This transaction must have transaction number."));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestTransactionHeaderMustHaveTransactionNumber_WhenDisableValidationWhenAutoImportIntercompanyInvoices()
		{
			using (AccountingConfigurationRegistry.Instance.EnableValidationWhenAutoImportIntercompanyInvoices.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTransactionHeaderMustHaveTransactionNumber();
				RunLogWalkerCycleForTest();

				AssertEquals("Should not trigger Accounting Critical Validation Error: TransactionHeaderMustHaveTransactionNumber", 0, ExceptionReporterTestListener.Instance.Count);
				AssertEquals(true, NotifiedEventList.Contains(@"[UnapprovedTransactionsConverter] An attempt to post an Intercompany Imported AP CRD  for AUD 11.00 has failed because of the following validation errors:

Transaction Num.: Please enter a Transaction Num..
Please try to do manual import.
"));
			}
		}

		void AssertTransactionHeaderMustHaveTransactionNumber()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			TestObjectCreator.GLHeader1.AG_Description = "Test";
			differentBranch1.OrgProxy.CompanyData.OB_APCostsSelfBilled = true;
			var queryForBranch = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, CurrentCompanyBranch.OrgProxy.PK);
			Factory.Load<GlbBranch>(queryForBranch).Where(x => x.PK != CurrentCompanyBranch.PK).ForEach(x => x.GB_OH_OrgProxy = ZGuid.Empty);
			Factory.Save();

			var arInvoice = CreateInvoice<ARInvoice>(differentBranch1, CurrentCompanyBranch.OrgProxy, false, "INV001", ZGuid.Empty, ZString.Empty, 10M, (x) =>
			{
				var line1 = x.Lines[0];
				line1.AL_OSExTaxAmount = 10M;
				line1.AL_AT = TestObjectCreator.GST1.PK;
			});

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			AssertEquals(GlbCompany.CurrentCompany.PK, resultInvoice.AH_GC);
			AssertEquals("INV001", resultInvoice.AH_TransactionNum);

			var arCreditNote = CreateInvoice<ARCreditNote>(differentBranch1, CurrentCompanyBranch.OrgProxy, false, "INV002", arInvoice.PK, ZString.Empty, 10M, (x) =>
			{
				var line1 = x.Lines[0];
				line1.AL_OSExTaxAmount = 10M;
				line1.AL_AT = TestObjectCreator.GST1.PK;
			});
		}

		[TestDate(2015, 1, 15)]
		public void TestRelativeCompanyHasNoActiveBranch()
		{
			differentCompany.Branches.DeleteAll();
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year, differentCompany.PK);
			periodTestHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year, GlbCompany.CurrentCompany.PK);

			TestObjectCreator.CreateChargeCode("CC3", "Charge Code 3", Core.Constants.ChargeType.Margin, 0, TestObjectCreator.GSTFREE1, TestObjectCreator.WHT1, differentCompany);

			var companyDataForDifferentCompany = GetOrCreateOrgCompanyData(GlbCompany.CurrentCompany.OrgProxy, differentCompany);
			companyDataForDifferentCompany.OB_IsCreditor = true;

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = TestObjectCreator.CreateJob(shipment1);
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = false;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			Factory.Save();

			ARInvoice aRInvoiceSource1 = CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, differentCompany.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, -10);
			aRInvoiceSource1.AH_JH = job.PK;
			aRInvoiceSource1.Department.GE_Misc = false;
			ARInvoiceLine aRLine1 = (ARInvoiceLine)aRInvoiceSource1.Lines[0];
			aRLine1.AL_Desc = "Desc";
			aRLine1.AL_OSExTaxAmount = -10m;
			aRLine1.AL_AC = TestObjectCreator.CC3.PK;
			aRInvoiceSource1.AH_ConsolidatedInvoiceRef = shipment1.JS_UniqueConsignRef;
			aRLine1.ChargeList.Load();
			aRLine1.GenericCharge = TestObjectCreator.CC3.PK;
			aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
			aRLine1.AL_JH = job.PK;
			aRLine1.AL_GB = aRInvoiceSource1.AH_GB;

			var chargeLine1 = TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC3, TestObjectCreator.AUD);
			chargeLine1.JR_OH_SellAccount = differentCompany.GC_OH_OrgProxy;

			Factory.Save();

			AssertNoExceptionThrown("Should not throw exception if there are no branchs active for transaction's company", () => RunLogWalkerCycleForTest());

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable).AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, differentCompany.PK));
			AssertNull("New AP invoice should not created as no active branchs for transaction's company.", resultInvoice);

			var mutexErrorList = (from log in NotifiedEventList
								  where log.Contains($@"Company {differentCompany.GC_Code} does not have any active branch. Transactions from the company will not be processed.")
								  select log).ToList();
			AssertEquals("Error message was added to the log", 1, mutexErrorList.Count);
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestErrorHandleWhenImportMoreThanTwoChargeWithBlankBranchDefault()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var companyDataForDifferentCompany = GetOrCreateOrgCompanyData(GlbCompany.CurrentCompany.OrgProxy, differentCompany);
			companyDataForDifferentCompany.OB_IsCreditor = true;
			var companyDataForIssuingCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			companyDataForIssuingCompany.OB_IsDebtor = true;

			var shipment = TestObjectCreator.CreateShipment("S0000555");
			var job = TestObjectCreator.CreateJob(shipment);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestObjectCreator.CreateJob(shipment);
				Factory.Save();
			}

			UnapprovedTransactionSubscriber.UpdateConvertedInvoice_ForTestOnly = invoice =>
			{
				var foundedJob = invoice.Factory.Load<JobHeader>(invoice.Lines[0].AL_JH);
				foundedJob.JH_GB = ZGuid.Empty;
			};

			Factory.Save();

			Action<ARInvoice> extraActions = (x) =>
			{
				x.Department.GE_Misc = false;
				var aRLine1 = x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);

				var aRLine2 = (InvoicingLineBase)x.Lines.AddNew();
				aRLine2.AL_OSExTaxAmount = 10M;
				aRLine2.AL_GB = x.Company.Branches[0].PK;
				aRLine2.AL_Desc = "Desc";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = 10M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine2.ChargeList.Load();
				aRLine2.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine2.AL_JH = job.PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine2, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};

			CreateInvoice(GlbBranch.CurrentBranch, differentBranch1.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 20M, extraActions);

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1")).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertNull("New AP invoice should not be created as empty branch error", resultInvoice);

			Assert(NotifiedEventList.Contains("[UnapprovedTransactionsConverter] Unable to post Intercompany Transaction. Job S0000555's Branch cannot be empty. Please check branch defaulting configuration.\r\n"));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestFaillingUnitTestForBlockingIssues_NoEventSpecified()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var companyDataForDifferentCompany = GetOrCreateOrgCompanyData(GlbCompany.CurrentCompany.OrgProxy, differentCompany);
			companyDataForDifferentCompany.OB_IsCreditor = true;
			var companyDataForIssuingCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			companyDataForIssuingCompany.OB_IsDebtor = true;

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = true;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, config);

			var shipment = TestObjectCreator.CreateShipment("S0000555");
			var job = TestObjectCreator.CreateJob(shipment);
			var disposableJob = (JobHeader)TestObjectCreator.CreateJob(shipment);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestObjectCreator.CreateJob(shipment);
				Factory.Save();
			}

			UnapprovedTransactionSubscriber.UpdateConvertedInvoice_ForTestOnly = invoice =>
			{
				var foundedJob = invoice.Factory.Load<JobHeader>(invoice.Lines[0].AL_JH);
				disposableJob = foundedJob;
			};

			Factory.Save();

			Action<ARInvoice> extraActions = (x) =>
			{
				x.Department.GE_Misc = false;
				var aRLine1 = x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};

			CreateInvoice(GlbBranch.CurrentBranch, differentBranch1.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActions);

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1")).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertNull("Not created, no matching event code", resultInvoice);

			AssertEquals("If job is not created, the mutex should be released", true, disposableJob.IsDisposed);

			Assert(NotifiedEventList.Contains("[UnapprovedTransactionsConverter] Intercompany transaction testAR1 cannot be imported because a required event as specified in the Auto Import Intercompany Event Configuration registry is not recorded on the job.\r\nThis transaction will either need to be manually imported into the job using the Job Invoicing > Import AP Invoices Issued by Other Group Companies option, or imported via the Intercompany Transaction Approval module."));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestFaillingUnitTestForBlockingIssues_WhenInvoiceAuthorisationLevelGreaterThanCompanyAuthorisationLevel()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var companyDataForDifferentCompany = GetOrCreateOrgCompanyData(GlbCompany.CurrentCompany.OrgProxy, differentCompany);
			companyDataForDifferentCompany.OB_IsCreditor = true;
			var companyDataForIssuingCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			companyDataForIssuingCompany.OB_IsDebtor = true;

			var valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = Core.Constants.CostVarianceComparisonOption.Job;
			var upTo1 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo1.VarianceSign = VarianceSigns.Plus;
			upTo1.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo1.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo1.Amount = 1M;

			var upTo2 = valuesForTest.AuthorisationRequirements.AddNew();
			upTo2.VarianceSign = VarianceSigns.Plus;
			upTo2.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			upTo2.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upTo2.Amount = 2M;

			var above = valuesForTest.AuthorisationRequirements.AddNew();
			above.VarianceSign = VarianceSigns.Plus;
			above.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			above.Amount = upTo2.Amount;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);

			var postingConfigCollection = new IntercompanyPostingConfigurationCollection();
			var postingConfig = postingConfigCollection.AddNew();
			postingConfig.Company = GlbCompany.CurrentCompany.GC_Code;
			postingConfig.MaxCostVarianceApprovalLevel = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, postingConfigCollection);

			var shipment = TestObjectCreator.CreateShipment("S0000555");
			var job = TestObjectCreator.CreateJob(shipment);
			var disposableJob = (JobHeader)TestObjectCreator.CreateJob(shipment);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestObjectCreator.CreateJob(shipment);
				Factory.Save();
			}

			UnapprovedTransactionSubscriber.UpdateConvertedInvoice_ForTestOnly = invoice =>
			{
				var foundedJob = invoice.Factory.Load<JobHeader>(invoice.Lines[0].AL_JH);
				disposableJob = foundedJob;
			};

			Factory.Save();

			Action<ARInvoice> extraActions = (x) =>
			{
				x.Department.GE_Misc = false;
				var aRLine1 = x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};

			CreateInvoice(GlbBranch.CurrentBranch, differentBranch1.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActions);

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1")).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertNull("Not created, no matching event code", resultInvoice);

			AssertEquals("If job is not created, the mutex should be released", true, disposableJob.IsDisposed);

			Assert(NotifiedEventList.Contains("[UnapprovedTransactionsConverter] Intercompany transaction testAR1 cannot be imported because the difference between the accrual and the transaction to be imported exceeds the cost variance approval level as defined in the Intercompany Posting Configuration registry.\r\nThis transaction will either need to be manually imported into the job using the Job Invoicing > Import AP Invoices Issued by Other Group Companies option, or imported via the Intercompany Transaction Approval module where appropriate approval can be sought."));
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestErrorHandleWhenImportMoreThanTwoChargeAndEachChargeHaveDifferentJobWithBlankBranchDefault()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var companyDataForDifferentCompany = GetOrCreateOrgCompanyData(GlbCompany.CurrentCompany.OrgProxy, differentCompany);
			companyDataForDifferentCompany.OB_IsCreditor = true;
			var companyDataForIssuingCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			companyDataForIssuingCompany.OB_IsDebtor = true;

			var shipment1 = TestObjectCreator.CreateShipment("S0000555");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var shipment2 = TestObjectCreator.CreateShipment("S0000556");
			var job2 = TestObjectCreator.CreateJob(shipment2);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				TestObjectCreator.CreateJob(shipment1);
				TestObjectCreator.CreateJob(shipment2);
				Factory.Save();
			}

			UnapprovedTransactionSubscriber.UpdateConvertedInvoice_ForTestOnly = invoice =>
			{
				var foundedJob1 = invoice.Factory.Load<JobHeader>(invoice.Lines[0].AL_JH);
				foundedJob1.JH_GB = ZGuid.Empty;
				invoice.Lines[1].AL_JH = job2.PK;
				var foundedJob2 = invoice.Factory.Load<JobHeader>(invoice.Lines[1].AL_JH);
				foundedJob2.JH_GB = ZGuid.Empty;
			};

			Factory.Save();

			Action<ARInvoice> extraActions = (x) =>
			{
				x.Department.GE_Misc = false;
				var aRLine1 = x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job1.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

				var aRLine2 = (InvoicingLineBase)x.Lines.AddNew();
				aRLine2.AL_OSExTaxAmount = 10M;
				aRLine2.AL_GB = x.Company.Branches[0].PK;
				aRLine2.AL_Desc = "Desc";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = 10M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine2.ChargeList.Load();
				aRLine2.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine2.AL_JH = job2.PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};

			CreateInvoice(GlbBranch.CurrentBranch, differentBranch1.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 20M, extraActions);

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1")).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));

			AssertNull("New AP invoice should not be created as empty branch error", resultInvoice);

			Assert(NotifiedEventList.Contains("[UnapprovedTransactionsConverter] Unable to post Intercompany Transaction. Job S0000555's Branch cannot be empty. Please check branch defaulting configuration.\r\nUnable to post Intercompany Transaction. Job S0000556's Branch cannot be empty. Please check branch defaulting configuration.\r\n"));
		}

		[TestDate(2015, 1, 15)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestLoginToInvoiceRecipientBranchWhenJobBranchDefaultIsLoginBranch()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year, differentCompany.PK);
			periodTestHelper.PostPeriodsForEntireYear(ZDateTime.Now.Year, GlbCompany.CurrentCompany.PK);

			TestObjectCreator.CreateChargeCode("CC3", "Charge Code 3", Core.Constants.ChargeType.Margin, 0, TestObjectCreator.GSTFREE1, TestObjectCreator.WHT1, differentCompany);

			JobBranchDefaultOrderRule rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 0;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToLoginUserDefault = 1;
			AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, rule);
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowForwardDatingofAPInvoiceDate.SetValue(differentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			GlbBranch differentBranch3 = Factory.NewWithValidTestData<GlbBranch>();
			differentBranch3.GB_Code = "ZZZ";
			differentBranch3.GB_GC = differentCompany.PK;
			OrgHeader differentBranch3Proxy = Factory.NewWithValidTestData<OrgHeader>();
			differentBranch3.GB_OH_OrgProxy = differentBranch3Proxy.PK;

			var companyDataForDifferentCompany = GetOrCreateOrgCompanyData(GlbCompany.CurrentCompany.OrgProxy, differentCompany);
			companyDataForDifferentCompany.OB_IsCreditor = true;
			var companyDataForIssuingCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			companyDataForIssuingCompany.OB_IsDebtor = true;

			ForwardingShipment shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			AssertEquals("", differentCompany.GC_RN_NKCountryCode, differentBranch1.GB_RL_NKHomePort.Left(2));
			shipment1.JS_RL_NKDestination = differentBranch1.GB_RL_NKHomePort; // This means 'import' shipment
			Job job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;

			ForwardingShipment shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_RL_NKDestination = differentBranch1.GB_RL_NKHomePort; // This means 'import' shipment
			Job job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				Job differentCompanyjob1 = TestObjectCreator.CreateJob(shipment1);
				Job differentCompanyjob2 = TestObjectCreator.CreateJob(shipment2);
				Factory.Save();
			}

			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			Factory.Save();

			ARInvoice aRInvoiceSource1 = CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, differentBranch3.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M);
			aRInvoiceSource1.AH_JH = job1.PK;
			aRInvoiceSource1.Department.GE_Misc = false;
			ARInvoiceLine aRLine = (ARInvoiceLine)aRInvoiceSource1.Lines[0];
			aRLine.AL_Desc = "Desc";
			aRLine.AL_OSExTaxAmount = 10M;
			aRLine.AL_AC = TestObjectCreator.CC3.PK;
			aRInvoiceSource1.AH_ConsolidatedInvoiceRef = shipment1.JS_UniqueConsignRef;
			aRLine.ChargeList.Load();
			aRLine.GenericCharge = TestObjectCreator.CC3.PK;
			aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
			aRLine.AL_JH = job1.PK;
			aRLine.AL_GB = aRInvoiceSource1.AH_GB;

			var chargeLine = TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC3, TestObjectCreator.AUD);
			chargeLine.JR_OH_SellAccount = differentBranch3Proxy.PK;

			ARInvoice aRInvoiceSource2 = CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, differentBranch1.OrgProxy, false, "testAR2", ZGuid.Empty, ZString.Empty, 10M);
			aRInvoiceSource2.AH_JH = job2.PK;
			aRInvoiceSource2.Department.GE_Misc = false;
			ARInvoiceLine aRLine2 = (ARInvoiceLine)aRInvoiceSource2.Lines[0];
			aRLine2.AL_Desc = "Desc";
			aRLine2.AL_OSExTaxAmount = 10M;
			aRLine2.AL_AC = TestObjectCreator.CC3.PK;
			aRInvoiceSource2.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;
			aRLine2.ChargeList.Load();
			aRLine2.GenericCharge = TestObjectCreator.CC3.PK;
			aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
			aRLine2.AL_JH = job2.PK;
			aRLine2.AL_GB = aRInvoiceSource2.AH_GB;

			var chargeLine2 = TestObjectCreator.CreateJobCharge(aRLine2, job2, TestObjectCreator.CC3, TestObjectCreator.AUD);
			chargeLine2.JR_OH_SellAccount = differentBranch1.GB_OH_OrgProxy;

			Factory.Save();

			RunLogWalkerCycleForTest();

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, differentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);

			AssertEquals("Job Branch should be branch of org proxy that invoice was issued to", differentBranch1.GB_Code, resultInvoice.Lines[0].Job.Branch.GB_Code);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR2").AddToFilter(AccTransactionHeaderSchema.AH_GC, differentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);

			AssertEquals("Job Branch should be branch of org proxy that invoice was issued to", differentBranch1.GB_Code, resultInvoice.Lines[0].Job.Branch.GB_Code);
		}

		public void TestConvertingConsolAPInvoicesFromSisterCompaniesReleaseMutex()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = false;

			var originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			var consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0000001");
			var jobShipment1 = TestObjectCreator.CreateShipment("S0000001", consol);
			Factory.Save();

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();
				var job1 = new Job.Loader(jobShipment1).TryCreateWithoutMutexForTestOnly();
				var consolInvoice = CreateInvoice<ARInvoice>(differentBranch1, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testConsolAR1", ZGuid.Empty, ZString.Empty, -10);
				var consolInvoiceline = consolInvoice.Lines[0];
				consolInvoice.AH_JH = ZGuid.Empty;
				consolInvoice.AH_ConsolidatedInvoiceRef = "C0000001";
				var chargeCode1 = TestObjectCreator.CreateChargeCode("TEST");
				chargeCode1.AC_GC = differentCompany.PK;
				consolInvoiceline.AL_AC = chargeCode1.PK;
				consolInvoiceline.AL_OSExTaxAmount = -10m;
				consolInvoiceline.AL_LineAmount = -10m;
				consolInvoiceline.AL_JH = job1.PK;
				consolInvoiceline.AL_GB = consolInvoice.Company.Branches[0].PK;
				TestObjectCreator.CreateJobCharge(consolInvoiceline, job1, chargeCode1, TestObjectCreator.AUD);
				Factory.Save();
			}

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testConsolAR1")).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertNull("New AP invoice should not be created as chargecode does not match", resultInvoice);

			var newFactory = new BusinessObjectFactory();
			var job = new Job.Loader(newFactory, jobShipment1).Load();
			AssertNull("Precondition : job should not be created yet", job);
			job = new Job.Loader(newFactory, jobShipment1).TryCreateWithMutex();
			AssertNotNull("Job should be created now", job);
			job.Dispose();
		}

		public void TestDefaultShowErrorHandler()
		{
			using (new DisposableAction(() => InvoicingBase.ShouldClearAttachedFileContent_ForTestOnly = true, () => InvoicingBase.ShouldClearAttachedFileContent_ForTestOnly = false))
			{
				var subscriber = new UnapprovedTransactionSubscriber();
				var testQueueLog = new QueuedLogForTesting(Factory);
				var dummyLogger = new DummyLogger();

				var arInvoice = CreateInvoice<ARInvoice>(differentBranch1, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "arInvoice", ZGuid.Empty, ZString.Empty, -10);
				AccountingConfigurationRegistry.Instance.PayableAllowUserToStoreARDoc.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				var type = typeof(UnapprovedTransactionSubscriber);

				var process = type.GetMethod("Process", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

				subscriber.SetDefaultLogger(dummyLogger);

				process.Invoke(subscriber, new Object[] { testQueueLog, arInvoice });

				var resultString = @"[UnapprovedTransactionsConverter] Caption : File not found, Message: Could not generate AR Invoice to attach to eDocs.

Please check if the registry: 'Accounting -> Receivable Defaults -> Form Configurations -> Invoice -> AR Invoice Menu Item Name' is overridden.
* If it is overridden, please check if the registry has a valid value and if the menu path is valid.
* If it is not overridden or the registry value is valid, please contact support.";

				AssertEquals(1, dummyLogger.Logs.Count(x => x.Type == LogType.Warning && x.Message == resultString));
			}
		}

		public void TestIssueIsNotRaisedIfMutexExceptionOccursWhilePostingSisterCompanyInvoice()
		{
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;

			foreach (GlbCompany company in Factory.Load<GlbCompany>(new ZQuery()))
			{
				GlbBranch firstBranch = company.Branches.Count > 0 ? company.Branches[0] : null;
				if (firstBranch != null)
				{
					using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, firstBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
					{
						AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					}
				}
			}

			TestObjectCreator.CreateBranch("PRX", GlbCompany.CurrentCompany, TestObjectCreator.Agent);

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0000555");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				Job job = TestObjectCreator.CreateJob(shipment);

				OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				ARInvoice aRInvoiceSource1 = CreateInvoice<ARInvoice>(differentBranch1, TestObjectCreator.Agent, false, "testAR1", ZGuid.Empty, ZString.Empty, -10);
				aRInvoiceSource1.Department.GE_Misc = false;
				aRInvoiceSource1.AH_ConsolidatedInvoiceRef = "C001001";
				ARInvoiceLine aRLine1 = (ARInvoiceLine)aRInvoiceSource1.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10m;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC3.PK;
				aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
				aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job.PK;

				consol.Shipments.Add(shipment);

				Factory.Save();

				var jobWithMutex = TestObjectCreator.CreateJob(shipment);
				try
				{
					RunLogWalkerCycleForTest();
				}
				finally
				{
					jobWithMutex.Dispose();
				}
				var mutexErrorList = (from log in NotifiedEventList
									  where log.Contains("User (undefined) is in the process of creating the Job S0000555. You cannot work on the job until he/she saves it or cancels the changes.")
									  select log).ToList();
				Assert("Mutex error entry added to the log", mutexErrorList.Count > 0);

				AssertNotNull("Since it crashes the LWK every time you try and process this log, there will be an exception being reported for this log. (Just previously it wasn't in the error reporter)", ErrorReporter.LastExceptionReported);
			}
			ErrorReporter.Clear();
		}

		[TestDate(2023, 1, 3, 5, 26, 32, 253)]
		public void TestPrepareToRertyIfMutexExceptionOccursWhilePostingSisterCompanyInvoice()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			var originalDepartment = GlbDepartment.CurrentDepartment;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);
			var retryInterval = 6;
			AccountingConfigurationRegistry.Instance.JobLockedRetryIntervalInMinutes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, retryInterval);

			TestObjectCreator.CreateBranch("PRX", GlbCompany.CurrentCompany, TestObjectCreator.Agent);
			var shipment = TestObjectCreator.CreateShipment("S0000555");
			ZGuid aRInvoiceSourcePK;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch1.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var job = TestObjectCreator.CreateJob(shipment);

				var orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				var aRInvoiceSource1 = CreateInvoice<ARInvoice>(differentBranch1, TestObjectCreator.Agent, false, "testAR1", ZGuid.Empty, ZString.Empty, -10);
				aRInvoiceSourcePK = aRInvoiceSource1.PK;
				var aRLine1 = (ARInvoiceLine)aRInvoiceSource1.Lines[0];
				TestObjectCreator.CreateJobCharge(aRLine1, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10m;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC3.PK;
				aRLine1.AL_GB = aRInvoiceSource1.AH_GB;
				aRLine1.AL_AT = TestObjectCreator.GSTFREE1.PK;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job.PK;

				Factory.Save();
			}

			var jobWithMutex = TestObjectCreator.CreateJob(shipment);
			var isDelayFiredSqlText = $"SELECT TOP 1 SJ_IsDelayFired FROM dbo.StmJobQueue WHERE SJ_FilterName ='UnapprovedTransactionsConverter' AND SJ_ParentTableCode = 'AH' AND SJ_ParentID = '{aRInvoiceSourcePK}'";
			var eventTimeSqlText = $"SELECT TOP 1 SJ_EventTime FROM dbo.StmJobQueue WHERE SJ_FilterName ='UnapprovedTransactionsConverter' AND SJ_ParentTableCode = 'AH' AND SJ_ParentID = '{aRInvoiceSourcePK}'";

			try
			{
				Globals.IsUserInteractive = false;
				RunLogWalkerCycleForTest();

				var isDelayFired = Db.Connection.ExecuteScalar<bool>(isDelayFiredSqlText);
				var eventTime = Db.Connection.ExecuteScalar<DateTime>(eventTimeSqlText);

				Assert(isDelayFired);
				AssertEquals(new DateTime(2023, 1, 3, 5, 26, 32, 253).AddMinutes(retryInterval), eventTime);
			}
			finally
			{
				jobWithMutex.Dispose();
			}
			var mutexErrorList = (from log in NotifiedEventList
								  where log.Contains("User (undefined) is in the process of creating the Job S0000555. You cannot work on the job until he/she saves it or cancels the changes.")
								  select log).ToList();
			Assert("Mutex error entry added to the log", mutexErrorList.Count > 0);

			Assert(NotifiedEventList.Contains("[UnapprovedTransactionsConverter] [Times:1] Retrying an attempt to post an Intercompany Imported since the job is locked by another user."));

			ErrorReporter.Clear();
		}

		[TestDate(2015, 1, 15)]
		public void TestReopenClosedJobWhilePostingIntercompanyInvoice_ShipmentLevel()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0000555");
			Job job = TestObjectCreator.CreateJob(shipment);

			OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			Action<ARInvoice> extraActionReopenClosedJob = (x) =>
			{
				x.AH_JH = job.PK;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = 10M;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
				aRLine.ChargeList.Load();
				aRLine.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine.AL_JH = job.PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;

				TestObjectCreator.CreateJobCharge(aRLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
				job.JH_Status = JobHeaderStatus.Closed.Code;
			};
			ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch1, CurrentCompanyBranch.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionReopenClosedJob);

			RunLogWalkerCycleForTest();

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			AssertEquals("header's branch should equal job's branch", job.JH_GB, resultInvoice.AH_GB);
			AssertEquals("header's department should equal job's department", job.JH_GE, resultInvoice.AH_GE);
			AssertEquals("line's branch should equal job's branch", job.JH_GB, resultInvoice.Lines[0].AL_GB);
			AssertEquals("line's department should equal job's department", job.JH_GB, resultInvoice.Lines[0].AL_GB);

			AssertEquals("[ReopenJob Allowed] Job Status set to WRK", JobHeaderStatus.Working.Code, job.JH_Status);
		}

		[TestDate(2015, 1, 15)]
		public void TestReopenClosedJobWhilePostingIntercompanyInvoice_ConsolLevel()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);
			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = false;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, config);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");

			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S0000555");
			Job job1 = TestObjectCreator.CreateJob(shipment1);

			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S0000556");
			Job job2 = TestObjectCreator.CreateJob(shipment2);

			OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(differentBranch1.OrgProxy, GlbCompany.CurrentCompany);
			Action<ARInvoice> extraActionReopenClosedJob = (x) =>
			{
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine1 = (ARInvoiceLine)x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job1.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

				InvoicingLineBase aRLine2 = (InvoicingLineBase)x.Lines.AddNew();
				aRLine2.AL_OSExTaxAmount = 10M;
				aRLine2.AL_GB = x.Company.Branches[0].PK;

				aRLine2.AL_Desc = "Desc";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = 10M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine2.ChargeList.Load();
				aRLine2.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine2.AL_JH = job2.PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST1.PK;

				TestObjectCreator.CreateJobCharge(aRLine2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

				job1.JH_Status = JobHeaderStatus.Closed.Code;
				job2.JH_Status = JobHeaderStatus.Closed.Code;

				consol.Shipments.Add(shipment1);
				consol.Shipments.Add(shipment2);
			};
			ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch1, CurrentCompanyBranch.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionReopenClosedJob);

			RunLogWalkerCycleForTest();

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);

			AssertEquals("[ReopenJob Allowed] Job Status set to WRK", JobHeaderStatus.Working.Code, job1.JH_Status);
			AssertEquals("[ReopenJob Allowed] Job Status set to WRK", JobHeaderStatus.Working.Code, job2.JH_Status);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoImportIntercompanyInvoiceAfterStartDate_Consol()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = true;
			var settings1 = config.IntercompanyEventSettingCollection.AddNew();
			settings1.StmEventCode = Events.CustomisableEvent00.Code;
			settings1.StartDate = ZDate.Today.AddDays(5);

			var settings2 = config.IntercompanyEventSettingCollection.AddNew();
			settings2.StmEventCode = Events.CustomisableEvent01.Code;
			settings2.StartDate = ZDate.Today.AddDays(-5);

			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, config);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001002");
			var logEntry2 = consol2.Logs.AddNew();
			using (logEntry2.LockForUpdatingKeyFieldsForTesting())
			{
				logEntry2.SL_SE_NKEvent = Events.CustomisableEvent00.Code;
				logEntry2.SL_EventTime = ZDateTime.Now;
			}

			ForwardingConsol consol3 = TestObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001003");
			var logEntry3 = consol3.Logs.AddNew();
			using (logEntry3.LockForUpdatingKeyFieldsForTesting())
			{
				logEntry3.SL_SE_NKEvent = Events.CustomisableEvent01.Code;
				logEntry3.SL_EventTime = ZDateTime.Now;
			}

			ForwardingShipment shipment1 = TestObjectCreator.CreateShipment("S0000555");
			Job job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;

			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S0000556");
			Job job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;

			OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			Action<ARInvoice> extraAction = (x) =>
			{
				x.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine1 = (ARInvoiceLine)x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job1.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

				InvoicingLineBase aRLine2 = (InvoicingLineBase)x.Lines.AddNew();
				aRLine2.AL_OSExTaxAmount = 10M;
				aRLine2.AL_GB = x.Company.Branches[0].PK;

				aRLine2.AL_Desc = "Desc";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = 10M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine2.ChargeList.Load();
				aRLine2.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine2.AL_JH = job2.PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

				consol.Shipments.Add(shipment1);
				consol.Shipments.Add(shipment2);
			};
			ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraAction);

			shipment1 = TestObjectCreator.CreateShipment("S0000557");
			job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;

			shipment2 = TestObjectCreator.CreateShipment("S0000558");
			job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;

			extraAction = (x) =>
			{
				x.AH_ConsolidatedInvoiceRef = consol2.JK_UniqueConsignRef;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine1 = (ARInvoiceLine)x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job1.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

				InvoicingLineBase aRLine2 = (InvoicingLineBase)x.Lines.AddNew();
				aRLine2.AL_OSExTaxAmount = 10M;
				aRLine2.AL_GB = x.Company.Branches[0].PK;

				aRLine2.AL_Desc = "Desc";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = 10M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine2.ChargeList.Load();
				aRLine2.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine2.AL_JH = job2.PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

				consol2.Shipments.Add(shipment1);
				consol2.Shipments.Add(shipment2);
			};
			ARInvoice aRInvoiceSource2 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, 10M, extraAction);

			shipment1 = TestObjectCreator.CreateShipment("S0000559");
			job1 = TestObjectCreator.CreateJob(shipment1);
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;

			shipment2 = TestObjectCreator.CreateShipment("S0000560");
			job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;

			extraAction = (x) =>
			{
				x.AH_ConsolidatedInvoiceRef = consol3.JK_UniqueConsignRef;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine1 = (ARInvoiceLine)x.Lines[0];
				aRLine1.AL_Desc = "Desc";
				aRLine1.AL_OSExTaxAmount = 10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine1.ChargeList.Load();
				aRLine1.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine1.AL_JH = job1.PK;
				aRLine1.AL_GB = x.Company.Branches[0].PK;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

				InvoicingLineBase aRLine2 = (InvoicingLineBase)x.Lines.AddNew();
				aRLine2.AL_OSExTaxAmount = 10M;
				aRLine2.AL_GB = x.Company.Branches[0].PK;

				aRLine2.AL_Desc = "Desc";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = 10M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine2.ChargeList.Load();
				aRLine2.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine2.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine2.AL_JH = job2.PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine2, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);

				consol3.Shipments.Add(shipment1);
				consol3.Shipments.Add(shipment2);
			};
			ARInvoice aRInvoiceSource3 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, 10M, extraAction);

			RunLogWalkerCycleForTest();

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("Not created, no matching event code", resultInvoice);
			APInvoice resultInvoice2 = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR2").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("Not created, no matching event code", resultInvoice2);
			APInvoice resultInvoice3 = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR3").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("Not created, no matching event code", resultInvoice3);
		}

		[TestDate(2015, 1, 15)]
		public void TestAutoImportIntercompanyInvoice_TracingExchangeErrorStack()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var shipment = TestObjectCreator.CreateShipment("S0000555");
			var job = TestObjectCreator.CreateJob(shipment);
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			differentBranch1.Company.GC_RX_NKLocalCurrency = "USD";
			differentBranch1.Company.GC_IsReciprocal = true;

			GlbCompany.CurrentCompany.GC_IsReciprocal = true;
			var orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			orgCompany.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

			Factory.Save();

			UnapprovedTransactionSubscriber.UpdateConvertedInvoice_ForTestOnly = invoice =>
			{
				invoice.UseJobExchangeRate = true;
				invoice.SuspendValidation();
				invoice.AH_ExchangeRate = 0m;
			};

			CreateInvoice<ARInvoice>(
				differentBranch1,
				Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy),
				false,
				"Test_AR0000001",
				ZGuid.Empty,
				ZString.Empty,
				-40m,
				(aRInvoice) =>
				{
					aRInvoice.AH_JH = job.PK;
					aRInvoice.Department.GE_Misc = false;
					aRInvoice.UseJobExchangeRate = true;
					aRInvoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
					aRInvoice.AH_ExchangeRate = 1m;

					var aRLine = (ARInvoiceLine)aRInvoice.Lines[0];
					aRLine.AL_JH = job.PK;
					aRLine.AL_Desc = "Desc";
					aRLine.AL_RX_NKTransactionCurrency = "AUD";
					aRLine.AL_ExchangeRate = 1m;
					aRLine.AL_OSExTaxAmount = 40m;
					aRLine.AL_LocalExTaxAmount = 40m;
					aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
					aRLine.ChargeList.Load();
					aRLine.GenericCharge = TestObjectCreator.CC1.PK;
					aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
					aRLine.AL_GB = aRInvoice.AH_GB;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;

					var charge = TestObjectCreator.CreateCharge(aRLine, job, TestObjectCreator.CC1, TestObjectCreator.USD);
				});

			RunLogWalkerCycleForTest();

			var exceptionMessage1 = ErrorReporter.ExceptionsThrown.FirstOrDefault(msg => msg.Contains("This transaction exchange rate is less than or equal to 0"));
			AssertNotNull(exceptionMessage1);
			AssertContains("Actual type:APInvoice", exceptionMessage1);
			AssertContains("stack trace is not collected in first time",
				"EvaluateTransactionHeaderWithZeroExchangeRate: There is no data collected for this PK. It might be because data is collected only after first error report. Please look at reports with Seq# >= 1.",
				exceptionMessage1);
			ErrorReporter.ExceptionsThrown.Clear();

			CreateInvoice<ARInvoice>(
				differentBranch1,
				Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy),
				false,
				"Test_AR0000002",
				ZGuid.Empty,
				ZString.Empty,
				-40m,
				(aRInvoice) =>
				{
					aRInvoice.AH_JH = job.PK;
					aRInvoice.Department.GE_Misc = false;
					aRInvoice.UseJobExchangeRate = true;
					aRInvoice.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
					aRInvoice.AH_ExchangeRate = 1m;

					var aRLine = (ARInvoiceLine)aRInvoice.Lines[0];
					aRLine.AL_JH = job.PK;
					aRLine.AL_Desc = "Desc";
					aRLine.AL_RX_NKTransactionCurrency = "AUD";
					aRLine.AL_ExchangeRate = 1m;
					aRLine.AL_OSExTaxAmount = 40m;
					aRLine.AL_LocalExTaxAmount = 40m;
					aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
					aRLine.ChargeList.Load();
					aRLine.GenericCharge = TestObjectCreator.CC1.PK;
					aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
					aRLine.AL_GB = aRInvoice.AH_GB;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;

					var charge = TestObjectCreator.CreateCharge(aRLine, job, TestObjectCreator.CC1, TestObjectCreator.USD);
				});

			RunLogWalkerCycleForTest();

			AssertEquals("PreCondition", 1, ErrorReporter.ExceptionsThrown.Count(msg => msg.Contains("This transaction exchange rate is less than or equal to 0")));
			var exceptionMessage2 = ErrorReporter.ExceptionsThrown.FirstOrDefault(msg => msg.Contains("This transaction exchange rate is less than or equal to 0"));
			AssertNotNull(exceptionMessage2);
			AssertContains("Actual type:APInvoice", exceptionMessage2);
			AssertContains("Exchange rate was changed after property info validation:", exceptionMessage2);
			AssertContains("stack trace must get this method to find where edit exchange rate", nameof(TestAutoImportIntercompanyInvoice_TracingExchangeErrorStack), exceptionMessage2);

			UnapprovedTransactionSubscriber.UpdateConvertedInvoice_ForTestOnly = null;
			ErrorReporter.Instance.Clear();
		}

		public void TestAutoImportIntercompanyInvoiceAfterStartDate_Shipment()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = true;
			var settings1 = config.IntercompanyEventSettingCollection.AddNew();
			settings1.StmEventCode = Events.CustomisableEvent00.Code;
			settings1.StartDate = ZDate.Today.AddDays(5);

			var settings2 = config.IntercompanyEventSettingCollection.AddNew();
			settings2.StmEventCode = Events.CustomisableEvent01.Code;
			settings2.StartDate = ZDate.Today;

			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, config);

			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ForwardingShipment shipment = TestObjectCreator.CreateShipment("S0000555");
			Job job = TestObjectCreator.CreateJob(shipment);
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;

			ForwardingShipment shipment2 = TestObjectCreator.CreateShipment("S0000556");
			Job job2 = TestObjectCreator.CreateJob(shipment2);
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;

			var logEntry2 = job2.Logs.AddNew();
			using (logEntry2.LockForUpdatingKeyFieldsForTesting())
			{
				logEntry2.SL_SE_NKEvent = Events.CustomisableEvent00.Code;
				logEntry2.SL_EventTime = ZDateTime.Now;
			}

			ForwardingShipment shipment3 = TestObjectCreator.CreateShipment("S0000557");
			Job job3 = TestObjectCreator.CreateJob(shipment3);
			job3.JH_GC = GlbCompany.CurrentCompany.PK;
			job3.JH_GB = GlbBranch.CurrentBranch.PK;
			var logEntry3 = shipment3.Logs.AddNew();
			using (logEntry3.LockForUpdatingKeyFieldsForTesting())
			{
				logEntry3.SL_SE_NKEvent = Events.CustomisableEvent01.Code;
				logEntry3.SL_EventTime = ZDate.Today;
			}

			TestObjectCreator.Factory.Save();

			OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

			Action<ARInvoice> extraAction = (x) =>
			{
				x.AH_JH = job.PK;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = 10M;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = shipment.JS_UniqueConsignRef;
				aRLine.ChargeList.Load();
				aRLine.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine.AL_JH = job.PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine, job, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};
			ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraAction);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

			Action<ARInvoice> extraAction2 = (x) =>
			{
				x.AH_JH = job2.PK;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = 10M;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;
				aRLine.ChargeList.Load();
				aRLine.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine.AL_JH = job2.PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};
			ARInvoice aRInvoiceSource2 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, 10M, extraAction2);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

			Action<ARInvoice> extraAction3 = (x) =>
			{
				x.AH_JH = job3.PK;
				x.Department.GE_Misc = false;
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = 10M;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = shipment3.JS_UniqueConsignRef;
				aRLine.ChargeList.Load();
				aRLine.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine.AL_JH = job3.PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.CreateJobCharge(aRLine, job3, TestObjectCreator.CC1, TestObjectCreator.AUD);
			};
			ARInvoice aRInvoiceSource3 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, 10M, extraAction3);

			RunLogWalkerCycleForTest();

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created, job has no event code entry", resultInvoice);

			APInvoice resultInvoice2 = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR2").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created, job has event code entry in the future", resultInvoice2);

			APInvoice resultInvoice3 = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR3").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created, job has event code entry in the past", resultInvoice3);
		}

		public void TestAutoImportIntercompanyInvoiceAfterStartDate_WithoutJob()
		{
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = true;
			var settings1 = config.IntercompanyEventSettingCollection.AddNew();
			settings1.StmEventCode = Events.CustomisableEvent00.Code;
			settings1.StartDate = ZDate.Today.AddDays(5);

			var settings2 = config.IntercompanyEventSettingCollection.AddNew();
			settings2.StmEventCode = Events.CustomisableEvent01.Code;
			settings2.StartDate = ZDate.Today;

			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, config);

			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			TestObjectCreator.Factory.Save();

			var orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

			Action<ARInvoice> extraAction = (x) =>
			{
				x.Department.GE_Misc = false;
				var aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = 10M;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				aRLine.ChargeList.Load();
				aRLine.GenericCharge = TestObjectCreator.CC1.PK;
				aRLine.AL_GE = TestObjectCreator.FIADepartment.PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
			};
			var aRInvoice = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraAction);

			var branchFilter = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, aRInvoice.AH_OH);
			branchFilter.AddToFilter(new ZQuery(GlbBranchSchema.GB_IsActive, true));
			var foundBranches = Factory.Load<GlbBranch>(branchFilter);
			foundBranches.ForEach(x => x.GB_IsActive = false);
			CurrentCompanyBranch.GB_IsActive = true;
			Factory.Save();

			AssertEquals("There has only one company which GC_OH_OrgProxy is equal to debtor of AR invoice.", 1, Factory.Load<GlbBranch>(branchFilter).Length);

			var companyFilter = new ZQuery(GlbCompanySchema.GC_OH_OrgProxy, aRInvoice.AH_OH);
			companyFilter.AddToFilter(new ZQuery(GlbCompanySchema.GC_IsActive, true));
			AssertEquals("There has only one branch which GB_OH_OrgProxy is equal to debtor of AR invoice.", 1, Factory.Load<GlbCompany>(companyFilter).Length);
			AssertEquals("Invoice doesn't have job", Guid.Empty, aRInvoice.AH_JH);
			AssertNoExceptionThrown(() => RunLogWalkerCycleForTest());
		}

		[TestDate(2015, 1, 15)]
		public void TestConvertUnapprovedPayableTransactionsLedgerTypeAR()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);
			TestObjectCreator.CreateBranch("PRX", GlbCompany.CurrentCompany, TestObjectCreator.Agent);

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = false;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, config);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_ParentID = jobShipment.PK;

			OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			Action<ARInvoice> extraActionAR = (x) =>
			{
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = 10M;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
			};
			ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch1, TestObjectCreator.Agent, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			extraActionAR = (x) =>
			{
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = 10M;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				x.AH_JH = job.PK;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine.AL_GB = x.AH_GB;
			};
			ARInvoice aRInvoiceSource2 = CreateInvoice(originalBranch, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			extraActionAR = (x) =>
			{
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = 10M;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				x.AH_JH = job.PK;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine.AL_GB = x.AH_GB;
			};
			ARInvoice aRInvoiceSource3 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			extraActionAR = (x) =>
			{
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = 10M;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				x.AH_JH = job.PK;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine.AL_GB = x.AH_GB;
			};
			ARInvoice aRInvoiceSource4 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), true, "testAR4", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			extraActionAR = (x) =>
			{
				((IMatching)x).CurrentMatchGroup.AddNew().AP_AH = x.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(x);
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = 10M;
				x.AH_JH = job.PK;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine.AL_GB = x.AH_GB;
				x.AH_IsCancelled = ZBool.True;
			};
			ARInvoice aRInvoiceSource5 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), true, "testAR5", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			ARInvoice reversedInvoice = CreateInvoice<ARInvoice>(differentBranch1, Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), true, "testARRev", ZGuid.Empty, ZString.Empty, -10M);
			((IMatching)reversedInvoice).CurrentMatchGroup.AddNew().AP_AH = reversedInvoice.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(reversedInvoice);
			reversedInvoice.IsCancelled = ZBool.True;
			Factory.Save();

			extraActionAR = (x) =>
			{
				((IMatching)x).CurrentMatchGroup.AddNew().AP_AH = x.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(x);
				x.AH_TransactionBelongsToGroup = reversedInvoice.PK;
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = 10M;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				x.AH_IsCancelled = ZBool.True;
			};
			ARInvoice aRInvoiceSource6 = CreateInvoice(differentBranch1, TestObjectCreator.Agent, false, "testAR6", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			RunLogWalkerCycleForTest();

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR2").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should't be created as AR Invoice branch belong to current company", resultInvoice);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR3").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should't be created as AR Invoice organization proxy doesn't belong to current company", resultInvoice);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR4").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should't be created as AR Invoice is already posted", resultInvoice);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR5").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should't be created as AR Invoice is cancelled", resultInvoice);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR6").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created as AR Invoice is cancelled but belongs to group which is already posted", resultInvoice);
		}

		public void TestUnapprovedTransactionCandidatesWithError()
		{
			var differentBranch3 = TestObjectCreator.CreateNewBranch(differentCompany, "AB3");
			differentBranch3.GB_OH_OrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYBN", true, true).PK;
			Factory.Save();

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			new AccountingPeriodTestHelper().SetupPeriods();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, differentBranch3.OrgProxy, false, "testAR1", ZGuid.Empty, ZString.Empty, 10);

			var differentCompany2 = TestObjectCreator.CreateNewCompany("CBA");
			var differentCompany2Branch = TestObjectCreator.CreateNewBranch(differentCompany2, "BA1");
			differentCompany2Branch.GB_OH_OrgProxy = differentBranch3.GB_OH_OrgProxy;
			Factory.Save();
			RunLogWalkerCycleForTest();

			Func<List<string>, IEnumerable<string>> errorList = (eventList) => eventList.Where(log => log.Contains("Intercompany Transaction AR INV testAR2 debtor ZORGPROXYBN company EDI for AUD 10.00 cannot be auto-imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy. Please try to use manual import."));
			AssertEquals("Cannot find branch error entry should not be added to the log because it is not intercompany transaction and we should just ignore it.", 0, errorList(NotifiedEventList).Count());

			AssertNotNull("Precondition: CurrentBranch.OrgProxy", GlbBranch.CurrentBranch.OrgProxy);
			GetOrCreateOrgCompanyData(GlbBranch.CurrentBranch.OrgProxy, differentCompany).OB_IsCreditor = true;
			CreateInvoice<ARInvoice>(GlbBranch.CurrentBranch, differentBranch3.OrgProxy, false, "testAR2", ZGuid.Empty, ZString.Empty, 10);

			Factory.Save();
			RunLogWalkerCycleForTest();

			AssertEquals("Cannot find branch error entry should be added to the log because it is intercompany transactions and just org proxy setup is not correct.", 1, errorList(NotifiedEventList).Count());
		}

		[ExpectNoExceptions]
		public void TestProcessLogQueueItemsUsesCorrectCasting()
		{
			var gLJournalSource = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var gLLine1 = TestObjectCreator.CreateGLJournalLine(gLJournalSource, 10m, DebitCredit.CR, TestObjectCreator.GetGLAccountFromDB().PK);
			gLJournalSource.Lines.Add(gLLine1);
			var gLLine2 = TestObjectCreator.CreateGLJournalLine(gLJournalSource, 10m, DebitCredit.DR, TestObjectCreator.GetGLAccountFromDB().PK);
			gLJournalSource.Lines.Add(gLLine2);
			Factory.Save();
			RunLogWalkerCycleForTest();
		}

		[ExpectNoExceptions]
		public void TestConvertUnapprovedPayableTransactionsLedgerTypeARWhenUserHasntRights()
		{
			Env.Security.APUnapprovedInvoices.IsAllowed = false;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_ParentID = jobShipment.PK;

			OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			Action<ARInvoice> extraActionARWhenUserHasntRights = (x) =>
			{
				ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
				aRLine.AL_Desc = "Desc";
				aRLine.AL_OSExTaxAmount = aRLine.AL_LineAmount = -10m;
				aRLine.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine.ChargeList;
				chargeList.Load();
				aRLine.GenericCharge = chargeList[0].PK;
				aRLine.AL_GB = x.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
			};
			ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch1, Factory.Load<OrgHeader>(CurrentCompanyBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, -10, extraActionARWhenUserHasntRights);

			RunLogWalkerCycleForTest();
		}

		[TestDate(2019, 10, 15)]
		public void TestAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage()
		{
			AssertAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage(true);
		}

		[TestDate(2019, 10, 15)]
		public void TestAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentLinesWarningMessage()
		{
			AssertAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage(false);
		}

		[TestDate(2019, 10, 15)]
		public void TestConvertSameUnapprovedTransactionMultipleTimes()
		{
			Env.Security.APUnapprovedInvoices.IsAllowed = false;
			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);
			new AccountingPeriodTestHelper().SetupPeriods();

			RunLogWalkerCycleForTest();
			NotifiedEventList.Clear();

			var time = ZDateTime.Now;
			var transactionHeader = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, time, time);
			Factory.Save();

			RunLogWalkerCycleForTest();

			var errorMsg1 = "[UnapprovedTransactionsConverter] failed to process logs. Affected records will be processed again one-by-one.\r\nObject reference not set to an instance of an object.";
			var errorMsg2 = "[UnapprovedTransactionsConverter] failed to process logs. Affected records will be processed again one-by-one.\r\nAn item with the same key has already been added.";

			AssertEquals("No null object ref exception", false, NotifiedEventList.Any(x => x.Contains(errorMsg1)));
			AssertEquals("No dictionary has duplicate key exception", false, NotifiedEventList.Any(x => x.Contains(errorMsg2)));
			AssertEquals("No other errors", false, NotifiedEventList.Any(x => x.Contains("failed to process logs")));
		}

		void AssertAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage(bool flag)
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment.CurrentDepartment);
			TestObjectCreator.CreateBranch("PRX", GlbCompany.CurrentCompany, TestObjectCreator.Agent);

			var config = new IntercompanyEventConfiguration();
			config.EnableEventConfiguration = false;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyEventConfiguration.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, config);

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_ParentID = jobShipment.PK;

			var orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch1.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			Action<ARInvoice> extraActionAR = (x) =>
			{
				var aRLine1 = (ARInvoiceLine)x.Lines[0];
				aRLine1.AL_Desc = "Desc1";
				aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10M;
				aRLine1.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				var chargeList = aRLine1.ChargeList;
				chargeList.Load();
				aRLine1.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine1.AL_GB = x.AH_GB;
				aRLine1.AL_AT = TestObjectCreator.GST1.PK;
				TestObjectCreator.GST1.AT_PostingGroupId = 1;

				var aRLine2 = (ARInvoiceLine)x.Lines.AddNew();
				aRLine2.AL_Desc = "Desc2";
				aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = aRLine2.AL_OSAmount = 20M;
				aRLine2.AL_AG = TestObjectCreator.CC1.AC_AG_RevenueAccount;
				x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
				chargeList = aRLine2.ChargeList;
				chargeList.Load();
				aRLine2.GenericCharge = chargeList.Find(new ZQuery(ViewGenericChargeSchema.VC_Code, "4710.00.00"))[0].PK;
				aRLine2.AL_GB = x.AH_GB;
				aRLine2.AL_AT = TestObjectCreator.GST2.PK;
				TestObjectCreator.GST2.AT_PostingGroupId = 2;
			};
			var aRInvoiceSource = CreateInvoice(differentBranch1, TestObjectCreator.Agent, false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, flag);
			orgCompany.OB_IsCreditor = true;
			orgCompany.OB_APCreateVATComplianceDocumentOnPosting = Enterprise.Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			Factory.Save();

			RunLogWalkerCycleForTest();

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);

			List<string> negativeComplianceWarningList;
			if (flag)
			{
				negativeComplianceWarningList = (from log in NotifiedEventList
												 where log.Contains("Transaction AP INV testAR1 is posted successfully. However, Compliance Document records could not be created as negative compliance documents are not allowed.")
												 select log).ToList();
			}
			else
			{
				negativeComplianceWarningList = (from log in NotifiedEventList
												 where log.Contains("Transaction AP INV testAR1 is posted successfully. However, Compliance Document records could not be created as negative compliance document lines are not allowed.")
												 select log).ToList();
			}

			Assert("negative compliance document is not created", negativeComplianceWarningList.Count > 0);
		}

		void SetAutoImportIntercompanyInvoicesRegistry(GlbDepartment department)
			{
			foreach (GlbCompany company in Factory.Load<GlbCompany>(new ZQuery()))
			{
				GlbBranch firstBranch = company.Branches.Count > 0 ? company.Branches[0] : null;
				if (firstBranch != null)
				{
					using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, firstBranch.PK.ToGuid(), department.PK.ToGuid()))
					{
						AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
					}
				}
			}
		}

		OrgCompanyData GetOrCreateOrgCompanyData(OrgHeader org, GlbCompany company)
			{
			OrgCompanyData orgCompanyData;
			org.CompanyDataCollection.Load(new ZQuery(OrgCompanyDataSchema.OB_GC, company.PK));
			if (org.CompanyDataCollection.Count == 0)
			{
				orgCompanyData = Factory.New<OrgCompanyData>();
				orgCompanyData.OB_GC = company.PK;
				orgCompanyData.OB_OH = org.PK;
			}
			else
			{
				orgCompanyData = org.CompanyDataCollection[0];
			}
			return orgCompanyData;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		T CreateInvoice<T>(GlbBranch branch, OrgHeader org, ZBool isPostedInternal, ZString invoiceNum, ZGuid transactionGroup, ZString transactionReference, ZDecimal amount, Action<T> extraSetup = null) where T : InvoicingBase
			{
			T result = Factory.New<T>();
			result.AH_GB = branch.PK;
			result.AH_Ledger = typeof(T).Name.Substring(0, 2);
			result.AH_PostedInternal = isPostedInternal;
			result.AH_OH = org.PK;
			result.AH_TransactionBelongsToGroup = transactionGroup;
			result.AH_TransactionReference = transactionReference;
			result.AH_PostDate = ZDateTime.Now;

			InvoicingLineBase line = (InvoicingLineBase)result.Lines.AddNew();
			line.AL_LineAmount = line.AL_OSAmount = amount;
			line.AL_GB = result.Company.Branches[0].PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			if (extraSetup != null)
			{
				extraSetup(result);
			}

			result.AH_TransactionNum = invoiceNum;
			result.IsManuallySetTransactionNumber_ForTestOnly = true;

			Factory.Save();

			return result;
		}

		#region Implementation

		GlbCompany differentCompany;
		GlbBranch differentBranch1;
		GlbBranch differentBranch2;
		OrgHeader differentCompanyOrgProxy;
		OrgHeader differentBranchOrgProxy;
		GlbBranch originalBranch;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

			originalBranch = GlbBranch.CurrentBranch;

			differentCompany = TestObjectCreator.CreateNewCompany("ABC");
			differentBranch1 = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			differentBranch2 = TestObjectCreator.CreateNewBranch(differentCompany, "AB2");
			differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			var closestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))?.RL_Code;
			differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true, closestPort);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;
			differentBranch1.GB_OH_OrgProxy = differentBranchOrgProxy.PK;
			Factory.Save();
		}

		protected GlbBranch CurrentCompanyBranch
		{
			get
			{
				return Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			}
		}

		protected override void TearDown()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = null;
			base.TearDown();
		}

		class DummyLogger : ILogger
		{
			public readonly List<(LogType Type, string Message)> Logs = new List<(LogType, string)>();
			public void Log(LogType type, string message) => Log(type, message, null);

			public void Log(LogType type, string message, Exception ex) => Logs.Add((type, message));
		}

		#endregion
	}
}
