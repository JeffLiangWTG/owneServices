using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	class UnapprovedTransactionImporterTest : TestCaseWithFactory
	{
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJobRefNumForShipment()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("S0000555");
			JobHeader job1 = new JobHeader.Loader(jobShipment).TryCreateWithoutMutexForTestOnly();

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();

				Action<ARInvoice> extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoice1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);

				extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = ZGuid.Empty;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRConsolInvoice1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testConsolAR", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);
			}

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, jobShipment);
			testImporter.Process(notifications);

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testConsolAR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created", resultInvoice);
		}

		public void TestInvoiceBranchToLoginBranch_PostingToLoginBranchEnabled()
		{
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			InvoicingBase arInvoice;
			ForwardingShipment shipment;
			JobHeader job;

			SetupTestForInvoiceBranchTesting(out arInvoice, out shipment, out job);

			AssertEquals("Precondition: ar invoice has 2 lines", 2, arInvoice.Lines.Count);
			AssertNotEquals("Precondition: ar invoice lines has different branches", arInvoice.Lines[0].AL_GB, arInvoice.Lines[1].AL_GB);

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, shipment);
			testImporter.Process(notifications);

			APInvoice apInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", apInvoice);

			AssertEquals("Invoice line1 is set to job branch", job.JH_GB, apInvoice.Lines[0].AL_GB);
			AssertEquals("Invoice line2 is set to job branch", job.JH_GB, apInvoice.Lines[1].AL_GB);
			AssertNotEquals("Invoice Header and line has different branches", apInvoice.AH_GB, apInvoice.Lines[0].AL_GB);
			AssertEquals("Invoice Header is set to current login branch as per registry setting", GlbBranch.CurrentBranch.PK, apInvoice.AH_GB);

			job.Dispose();
		}

		public void TestInvoiceBranchToLoginBranch_BranchLevelPostingEnabled()
		{
			AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			InvoicingBase arInvoice;
			ForwardingShipment shipment;
			JobHeader job;

			SetupTestForInvoiceBranchTesting(out arInvoice, out shipment, out job);

			AssertEquals("Precondition: ar invoice has 2 lines", 2, arInvoice.Lines.Count);
			AssertNotEquals("Precondition: ar invoice lines has different branches", arInvoice.Lines[0].AL_GB, arInvoice.Lines[1].AL_GB);

			var branchLevelPostingConfiguration = new BranchLevelPostingConfiguration() { EnableBranchLevelPosting = true };
			AccountingConfigurationRegistry.Instance.PayableEnforceBranchLevelPosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, branchLevelPostingConfiguration);

			NotificationBuffer notifications = new NotificationBuffer();
			var testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, shipment);
			testImporter.Process(notifications);

			var apInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", apInvoice);

			AssertEquals("Invoice line1 is set to job branch", job.JH_GB, apInvoice.Lines[0].AL_GB);
			AssertEquals("Invoice line2 is set to job branch", job.JH_GB, apInvoice.Lines[1].AL_GB);
			AssertEquals("Invoice Header and Line will have the same branch", apInvoice.AH_GB, apInvoice.Lines[0].AL_GB);

			job.Dispose();
		}

		void SetupTestForInvoiceBranchTesting(out InvoicingBase arInvoice, out ForwardingShipment shipment, out JobHeader job)
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			shipment = TestObjectCreator.CreateShipment("S0000555");
			job = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = TestObjectCreator.NonCurrentBranch.PK;

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			arInvoice = null;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				var branch = TestObjectCreator.CreateBranch("BBB", differentCompany);
				Factory.Save();

				periodTestHelper.SetupPeriods();

				var jobInSisterCompany = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();
				jobInSisterCompany.JH_GB = differentBranch.PK;

				ZString uniqueConsignRef = shipment.JS_UniqueConsignRef;
				Action<ARInvoice> extraActionSetupLine = (x) =>
				{
					x.AH_JH = jobInSisterCompany.PK;
					x.AH_ConsolidatedInvoiceRef = uniqueConsignRef;

					var arLine = x.Lines[0];
					arLine.AL_AC = TestObjectCreator.CC1.PK;
					arLine.AL_JH = jobInSisterCompany.PK;
					arLine.AL_GB = x.Company.Branches[0].PK;
					arLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(arLine, jobInSisterCompany, TestObjectCreator.CC1, TestObjectCreator.AUD);

					var arLine1 = (InvoicingLineBase)x.Lines.AddNew();
					arLine1.AL_OSExTaxAmount = 20M;
					arLine1.AL_AC = TestObjectCreator.CC1.PK;
					arLine1.AL_JH = jobInSisterCompany.PK;
					arLine1.AL_GB = branch.PK;
					arLine1.AL_OSTaxAmount = 2M;
					TestObjectCreator.CreateJobCharge(arLine1, jobInSisterCompany, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				arInvoice = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR", ZGuid.Empty, ZString.Empty, 10M, extraActionSetupLine);

				jobInSisterCompany.Dispose();
			}
		}

		[TestDate(2011, 11, 29, 12, 00, 00)]
		public void TestImportingConsolAPInvoicesFromSisterCompaniesReleaseMutex()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = false;

			var originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0000001");
			var jobShipment1 = TestObjectCreator.CreateShipment("S0000001", consol);
			Factory.Save();

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();
				var job1 = new Job.Loader(jobShipment1).TryCreateWithoutMutexForTestOnly();
				var consolInvoice = CreateInvoice<ARInvoice>(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testConsolAR1", ZGuid.Empty, ZString.Empty, -10);
				var consolInvoiceline = consolInvoice.Lines[0];
				consolInvoice.AH_JH = ZGuid.Empty;
				consolInvoice.AH_ConsolidatedInvoiceRef = "C0000001";
				var chargeCode1 = TestObjectCreator.CreateChargeCode("TEST");
				chargeCode1.AC_GC = differentCompany.PK;
				consolInvoiceline.AL_AC = chargeCode1.PK;
				consolInvoiceline.AL_JH = job1.PK;
				consolInvoiceline.AL_GB = consolInvoice.Company.Branches[0].PK;
				TestObjectCreator.CreateJobCharge(consolInvoiceline, job1, chargeCode1, TestObjectCreator.AUD);
				Factory.Save();
			}

			NotificationBuffer notifications = new NotificationBuffer();
			var testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, consol);
			testImporter.Process(notifications);

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testConsolAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created as chargecode does not match", resultInvoice);
			AssertEquals("Should send error email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var newFactory = new BusinessObjectFactory();
			var job = new Job.Loader(newFactory, jobShipment1).Load();
			AssertNull("Precondition : job should not be created yet", job);
			job = new Job.Loader(newFactory, jobShipment1).TryCreateWithMutex();
			AssertNotNull("Job should be created now", job);
			job.Dispose();
		}

		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestJobRefNumForConsol()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingConsol consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0000001");
			ForwardingShipment jobShipment1 = TestObjectCreator.CreateShipment("S0000001", consol);
			JobHeader job1 = new Job.Loader(jobShipment1).TryCreateWithoutMutexForTestOnly();
			ForwardingShipment jobShipment2 = TestObjectCreator.CreateShipment("C0000002", consol);
			JobHeader job2 = new Job.Loader(jobShipment2).TryCreateWithoutMutexForTestOnly();

			ForwardingConsol consol2 = TestObjectCreator.CreateConsol("ABC", "DEF", "C0000002");
			ForwardingShipment jobShipment3 = TestObjectCreator.CreateShipment("C0000001", consol2);
			Job job3 = new Job.Loader(jobShipment3).TryCreateWithoutMutexForTestOnly();

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();

				Action<ARInvoice> extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = ZGuid.Empty;
					x.AH_ConsolidatedInvoiceRef = "C0000001";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRConsolInvoice1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testConsolAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);

				extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = "S0000001";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoice1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);

				extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = job2.PK;
					x.AH_ConsolidatedInvoiceRef = "C0000002";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job2.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoice2 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);

				extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = ZGuid.Empty;
					x.AH_ConsolidatedInvoiceRef = "C0000002";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job3.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job3, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRConsolInvoice2 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testConsolAR2", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);

				extraActionJobRefNum = (x) =>
				{
					var aRLine = x.Lines[0];
					x.AH_JH = job3.PK;
					x.AH_ConsolidatedInvoiceRef = "C0000001";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job3.PK;
					aRLine.AL_GB = x.Company.Branches[0].PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job3, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoice3 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, 10M, extraActionJobRefNum);
			}

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, consol);
			testImporter.Process(notifications);

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testConsolAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR2").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testConsolAR2").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created", resultInvoice);
			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR3").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created", resultInvoice);
		}

		public void TestConvertUnapprovedPayableTransactionsLedgerTypeAR()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C0000555");
			consol.JK_UniqueConsignRef = "C0000557";

			ForwardingShipment consolShipment = consol.Shipments.AddNew();
			consolShipment.JS_UniqueConsignRef = "S0000556";

			JobHeader orignaljob1 = new JobHeader.Loader(jobShipment).TryCreateWithoutMutexForTestOnly();
			JobHeader orignaljob2 = new JobHeader.Loader(consolShipment).TryCreateWithoutMutexForTestOnly();

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();

				var job1 = new JobHeader.Loader(jobShipment).TryCreateWithoutMutexForTestOnly();
				var job2 = new JobHeader.Loader(consolShipment).TryCreateWithoutMutexForTestOnly();
				Factory.Save();

				#region Setting up different source AR invoices for Job Shipment to check Candidates filter

				#region AR Invoice passing selection criterias

				OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

				Action<ARInvoice> extraActionAR = (x) =>
				{
					ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion

				#region AR Invoice with non matching Branch

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_Desc = "Desc";
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
					Factory.Save();
				};
				ARInvoice aRInvoiceSource2 = CreateInvoice(originalBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR2", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

				#endregion

				#region AR Invoice with non matching Org Proxy

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					x.AH_JH = job2.PK;
					aRLine.AL_JH = job2.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					TestObjectCreator.CreateJobCharge(aRLine, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoiceSource3 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(differentBranch.GB_OH_OrgProxy), false, "testAR3", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion

				#region AR Invoice internally posted

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					x.AH_JH = job2.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_JH = job2.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					TestObjectCreator.CreateJobCharge(aRLine, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRInvoiceSource4 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), true, "testAR4", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion

				#region AR Invoice is cancelled

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					((IMatching)x).CurrentMatchGroup.AddNew().AP_AH = x.PK;
					TestObjectCreator.SetupMatchLinkMatchDate(x);
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_Desc = "Desc";
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					x.AH_IsCancelled = ZBool.True;
					JobCharge charge = TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
					charge.SetARLineForcedForTest(ZGuid.Empty);
				};
				ARInvoice aRInvoiceSource5 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), true, "testAR5", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion

				#region AR Invoice is cancelled but belongs to group which is already posted

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				ARInvoice reversedInvoice = CreateInvoice<ARInvoice>(originalBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), true, "testARRev", ZGuid.Empty, ZString.Empty, -10M);
				((IMatching)reversedInvoice).CurrentMatchGroup.AddNew().AP_AH = reversedInvoice.PK;
				TestObjectCreator.SetupMatchLinkMatchDate(reversedInvoice);
				reversedInvoice.IsCancelled = ZBool.True;
				Factory.Save();

				extraActionAR = (x) =>
				{
					((IMatching)x).CurrentMatchGroup.AddNew().AP_AH = x.PK;
					TestObjectCreator.SetupMatchLinkMatchDate(x);
					x.AH_TransactionBelongsToGroup = reversedInvoice.PK;
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_Desc = "Desc";
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					x.AH_IsCancelled = ZBool.True;
					x.AH_JH = job1.PK;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
				};
				ARInvoice aRInvoiceSource6 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR6", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion

				#region AR Invoice with unrelevant AH_ConsolidatedInvoiceRef

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = "S0000333";
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					JobCharge charge = TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				ARInvoice aRUnrelatedInvoiceSource = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARUnrelated", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion

				#endregion

				#region Setting up Consol and its Shipment source AR Invoices

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					x.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					JobCharge charge = TestObjectCreator.CreateJobCharge(aRLine, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
					charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
					aRLine.AL_JH = job2.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					charge.SetAmountsFromLinkedLinesForTests();
				};
				ARInvoice aRConsolInvoiceSource = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARConsol", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);

				orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				extraActionAR = (x) =>
				{
					var aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					x.AH_JH = job2.PK;
					x.AH_ConsolidatedInvoiceRef = consolShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					JobCharge charge = TestObjectCreator.CreateJobCharge(aRLine, job2, TestObjectCreator.CC1, TestObjectCreator.AUD);
					charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
					x.AH_JH = job2.PK;
					aRLine.AL_JH = job2.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					charge.SetAmountsFromLinkedLinesForTests();
					Factory.Save();
				};
				ARInvoice aRConsolShipmentInvoiceSource = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testARConsolShipment", ZGuid.Empty, ZString.Empty, 10M, extraActionAR);
				#endregion
			}

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, jobShipment);

			testImporter.Process(notifications);
			AssertEquals("Shouldn't have sent error email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
			AssertEquals("job related ap invoice header's branch should equal job's branch", orignaljob1.JH_GB, resultInvoice.AH_GB);
			AssertEquals("job related ap invoice header's department should equal job's department", orignaljob1.JH_GE, resultInvoice.AH_GE);
			AssertEquals("job related ap invoice line's branch should equal job's branch", orignaljob1.JH_GB, resultInvoice.Lines[0].AL_GB);
			AssertEquals("job related ap invoice line's department should equal job's department", orignaljob1.JH_GE, resultInvoice.Lines[0].AL_GE);

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

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testARUnrelated").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should't be created as AR Invoice has an unrelated AH_ConsolidatedInvoiceRef", resultInvoice);

			testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, consol);

			testImporter.Process(notifications);
			AssertEquals("Shouldn't have sent error email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testARConsol").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created as AR Invoice is related to the Consol", resultInvoice);
			AssertEquals("consol related ap invoice header's branch should equal job's branch", orignaljob2.JH_GB, resultInvoice.AH_GB);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testARConsolShipment").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created as AR Invoice is related tho the Consol's Shipment", resultInvoice);
		}

		public void TestImportARTransactionWithError()
		{
			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0000001");
			var consolShipment = TestObjectCreator.CreateShipment("S0000001", consol);
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var originalBranchProxy = Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy);
			TestObjectCreator.CreateBranch("AAA", differentCompany, originalBranchProxy);

			ARInvoice aRConsolInvoiceSource = null;
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();

				var job1 = TestObjectCreator.CreateJob(consolShipment, false);
				OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
				aRConsolInvoiceSource = CreateInvoice<ARInvoice>(differentBranch, originalBranchProxy, false, "testARConsol", ZGuid.Empty, ZString.Empty, -10);
				var aRLine = (ARInvoiceLine)aRConsolInvoiceSource.Lines[0];
				aRLine.AL_AC = TestObjectCreator.CC1.PK;
				aRLine.AL_Desc = "Desc";
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				aRLine.AL_AG = TestObjectCreator.GLHeader1.PK;
				aRConsolInvoiceSource.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				aRLine.AL_GB = aRConsolInvoiceSource.AH_GB;
				aRLine.AL_AT = TestObjectCreator.GST1.PK;
				var charge = TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				aRLine.AL_JH = job1.PK;
				charge.SetAmountsFromLinkedLinesForTests();
				Factory.Save();
			}

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, consol);
			Env.OutgoingMailManager.EmailsCreated.Clear();
			testImporter.Process(notifications);
			var expectedSubject = string.Format("Failed to auto-import {0} {1} {2} ({3} {4}) from other Group Company",
														aRConsolInvoiceSource.AH_Ledger,
														aRConsolInvoiceSource.AH_TransactionType,
														aRConsolInvoiceSource.AH_TransactionNum,
														aRConsolInvoiceSource.AH_RX_NKTransactionCurrency,
														aRConsolInvoiceSource.AH_OSTotalAmount.ToString(aRConsolInvoiceSource.TransactionCurrency.Decimals));

			var expectedBody = string.Format(@"An attempt to import an invoice with organization {0} in company [{1}] branch [{2}] department [{3}] has failed because of the following error:
Intercompany Invoice cannot be auto-imported as Transaction Branch or Company cannot be set with reference to the invoice debtor organization proxy. Please try to use manual import.",
									aRConsolInvoiceSource.Factory.Load<OrgHeader>(aRConsolInvoiceSource.AH_OH).OH_Code,
									aRConsolInvoiceSource.Company.GC_Code,
									aRConsolInvoiceSource.Branch.GB_Code,
									aRConsolInvoiceSource.Department.GE_Code);

			AssertEquals("Should send email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(expectedSubject, Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals(expectedBody, Env.OutgoingMailManager.EmailsCreated[0].Body);

			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			NotificationBuffer newNotifications = new NotificationBuffer();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			testImporter.Process(newNotifications);
			AssertEquals("Should not send email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestUnapprovedTransactionCandidatesAreNotValidated()
		{
			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "HKHKG", "C0000555");
			consol.JK_UniqueConsignRef = "C0000557";

			ForwardingShipment consolShipment = consol.Shipments.AddNew();
			consolShipment.JS_UniqueConsignRef = "S0000556";

			JobHeader job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job1.JH_GC = GlbCompany.CurrentCompany.PK;
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_ParentID = jobShipment.PK;
			job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			JobHeader job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job2.JH_GC = GlbCompany.CurrentCompany.PK;
			job2.JH_GB = GlbBranch.CurrentBranch.PK;
			job2.JH_ParentID = consolShipment.PK;
			job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
			ARInvoice aRInvoiceSource1;

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();

				job1 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job1.JH_GC = GlbCompany.CurrentCompany.PK;
				job1.JH_GB = GlbBranch.CurrentBranch.PK;
				job1.JH_ParentID = jobShipment.PK;
				job1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
				job2.JH_GC = GlbCompany.CurrentCompany.PK;
				job2.JH_GB = GlbBranch.CurrentBranch.PK;
				job2.JH_ParentID = consolShipment.PK;
				job2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

				OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

				Action<ARInvoice> extraActionUA = (x) =>
				{
					ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_AT = TestObjectCreator.GST1.PK;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};
				aRInvoiceSource1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, -10, extraActionUA);
			}

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, jobShipment);

			testImporter.Process(notifications);

			Assert("UnapprovedTransactionCandidateCollection validation should be suspended", testImporter.UnapprovedTransactionConverterForTest.Candidates.IsValidationSuspended);
			AssertNotNull("ARInvoiceSource1 should be included", testImporter.UnapprovedTransactionConverterForTest.Candidates.FindByPK(aRInvoiceSource1.PK));
			AssertNoWarnings("ARInvoiceSource1 shouldn't have any warnings because validation should not run", testImporter.UnapprovedTransactionConverterForTest.Candidates.FindByPK(aRInvoiceSource1.PK));
		}

		[TestDate(2011, 12, 15, 15, 01, 00)]
		public void TestErrorNotificationEmail()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			JobHeader job1 = new JobHeader.Loader(jobShipment).TryLoadOrCreateWithoutMutexForTestOnly();

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			// Do not setup periods for the target Company - should cause a validation error on Inported Transaction 

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();
				job1 = new JobHeader.Loader(jobShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				#region AR Invoice passing selection criterias

				OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

				Action<ARInvoice> extraActionNotificationEmail = (x) =>
				{
					ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
				};

				ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionNotificationEmail);
				#endregion
			}

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, jobShipment);

			testImporter.Process(notifications);
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created", resultInvoice);

			// Now set up periods and the import should succeseed
			periodTestHelper.SetupPeriods();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testImporter.Process(notifications);
			AssertEquals("Should not send email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
		}

		[TestDate(2011, 12, 15, 15, 01, 00)]
		public void TestErrorNotificationEmailOnAuthorisationLevel()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			GlbDepartment originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			ForwardingShipment jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			JobHeader job1 = new JobHeader.Loader(jobShipment).TryLoadOrCreateWithoutMutexForTestOnly();

			var chargeCode = TestObjectCreator.CC1;
			Factory.Save();

			// Do not setup periods for the target Company - should cause a validation error on Inported Transaction 

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();
				job1 = new JobHeader.Loader(jobShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				#region AR Invoice passing selection criterias

				OrgCompanyData orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

				Action<ARInvoice> extraActionNotificationEmail = (x) =>
				{
					ARInvoiceLine aRLine = (ARInvoiceLine)x.Lines[0];
					aRLine.AL_Desc = "Desc";
					aRLine.AL_AC = TestObjectCreator.CC1.PK;
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine.AL_GB = x.AH_GB;
					aRLine.AL_JH = job1.PK;
					aRLine.AL_OSTaxAmount = 1M;
					TestObjectCreator.CreateJobCharge(aRLine, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
					Factory.Save();
				};
				ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10M, extraActionNotificationEmail);
				#endregion
			}

			#region Setting up Authorisation requirements and Intercompany import configuration

			CostVarianceApproval approval = new CostVarianceApproval();
			approval.VarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle.PercentageVariance;
			approval.VarianceComparisonOption = Enterprise.Core.Constants.CostVarianceComparisonOption.Job;
			CostVarianceApprovalAuthorisationRequirement upTo5 = approval.AuthorisationRequirements.AddNew();
			upTo5.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			upTo5.Amount = 5m;
			upTo5.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			CostVarianceApprovalAuthorisationRequirement above5 = approval.AuthorisationRequirements.AddNew();
			above5.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			above5.Amount = 5m;
			above5.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, approval);

			IntercompanyPostingConfigurationCollection postingConfigCollection = new IntercompanyPostingConfigurationCollection();
			IntercompanyPostingConfiguration postingConfig = postingConfigCollection.AddNew();
			postingConfig.Company = differentBranch.Company.GC_Code;
			postingConfig.MaxCostVarianceApprovalLevel = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingConfigCollection);

			#endregion

			NotificationBuffer notifications = new NotificationBuffer();
			UnapprovedTransactionImporter testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, jobShipment);

			testImporter.Process(notifications);
			AssertEquals("Should have sent email", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals(@"An attempt to post an Intercompany Imported AP INV testAR1 for AUD 11.00 has failed because of the following validation errors:

Error - Accounts Payable Invoice: The Authorization Level '2nd Level Only' required to post this transaction exceeds Maximum Authorization Level specified in the 'Accounting > Intercompany Posting Configuration' registry item
", Env.OutgoingMailManager.EmailsCreated[0].Body);

			APInvoice resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNull("New AP invoice should not be created", resultInvoice);

			// Set appropriate Approval level in the Intercompany Posting Configuration
			postingConfig.MaxCostVarianceApprovalLevel = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.IntercompanyPostingConfiguration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, postingConfigCollection);
			Env.OutgoingMailManager.EmailsCreated.Clear();

			testImporter.Process(notifications);
			AssertEquals("Should not send email", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);
		}

		[TestDate(2019, 10, 21, 15, 01, 00)]
		public void TestAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage()
		{
			AssertAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage(true);
		}

		[TestDate(2019, 10, 21, 15, 01, 00)]
		public void TestAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentLineWarningMessage()
		{
			AssertAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage(false);
		}

		void AssertAutoImportIntercompanyInvoiceWithNegativeComplianceDocumentWarningMessage(bool flag)
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = true;

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			var originalDepartment = GlbDepartment.CurrentDepartment;
			AccountingConfigurationRegistry.Instance.AutoImportIntercompanyInvoices.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var jobShipment = TestObjectCreator.CreateShipment("0099");
			jobShipment.JS_UniqueConsignRef = "S0000555";

			var job1 = new JobHeader.Loader(jobShipment).TryLoadOrCreateWithoutMutexForTestOnly();

			var chargeCode1 = TestObjectCreator.CC1;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, differentBranch.PK.ToGuid(), originalDepartment.PK.ToGuid()))
			{
				periodTestHelper.SetupPeriods();
				job1 = new JobHeader.Loader(jobShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				var orgCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);

				#region AR Invoice passing selection criterias

				Action<ARInvoice> extraAction = (x) =>
				{
					var aRLine1 = (ARInvoiceLine)x.Lines[0];
					aRLine1.AL_Desc = "Desc1";
					aRLine1.AL_OSExTaxAmount = aRLine1.AL_LineAmount = -10M;
					aRLine1.AL_AC = TestObjectCreator.CC1.PK;
					aRLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
					x.AH_JH = job1.PK;
					x.AH_ConsolidatedInvoiceRef = jobShipment.JS_UniqueConsignRef;
					aRLine1.AL_GB = x.AH_GB;
					aRLine1.AL_JH = job1.PK;
					aRLine1.AL_AT = TestObjectCreator.GST1.PK;
					TestObjectCreator.GST1.AT_PostingGroupId = 1;
					TestObjectCreator.CreateJobCharge(aRLine1, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);

					var aRLine2 = (ARInvoiceLine)x.Lines.AddNew();
					aRLine2.AL_Desc = "Desc2";
					aRLine2.AL_OSExTaxAmount = aRLine2.AL_LineAmount = aRLine2.AL_OSAmount = 20M;
					aRLine2.AL_AC = TestObjectCreator.CC1.PK;
					aRLine2.AL_AG = TestObjectCreator.GLHeader2.PK;
					aRLine2.AL_JH = job1.PK;
					aRLine2.AL_GB = x.AH_GB;
					aRLine2.AL_AT = TestObjectCreator.GST2.PK;
					TestObjectCreator.GST2.AT_PostingGroupId = 2;
					TestObjectCreator.CreateJobCharge(aRLine2, job1, TestObjectCreator.CC1, TestObjectCreator.AUD);
					Factory.Save();
				};
				ARInvoice aRInvoiceSource1 = CreateInvoice(differentBranch, Factory.Load<OrgHeader>(originalBranch.GB_OH_OrgProxy), false, "testAR1", ZGuid.Empty, ZString.Empty, 10m, extraAction);
				#endregion
			}

			AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.AllowNegativeComplianceDocumentLines.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, flag);

			var differentBranchCompany = GetOrCreateOrgCompanyData(Factory.Load<OrgHeader>(differentBranch.GB_OH_OrgProxy), GlbCompany.CurrentCompany);
			differentBranchCompany.OB_IsCreditor = true;
			differentBranchCompany.OB_APCreateVATComplianceDocumentOnPosting = Enterprise.Core.Constants.OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge;
			Factory.Save();

			var notifications = new NotificationBuffer();
			var testImporter = new UnapprovedTransactionImporter(GlbCompany.CurrentCompany.PK, jobShipment);

			testImporter.Process(notifications);

			var resultInvoice = Factory.LoadTop1<APInvoice>(new ZQuery(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable), JoinCondition.And, new ZQuery(AccTransactionHeaderSchema.AH_TransactionNum, "testAR1").AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK)));
			AssertNotNull("New AP invoice should be created", resultInvoice);

			Assert("notification has warning message", notifications.HasWarnings);
			if (flag)
			{
				AssertContains("negative compliance document warning message is added to notifications", "Transaction AP INV testAR1 is posted successfully. However, Compliance Document records could not be created as negative compliance documents are not allowed.", notifications.AsString);
			}
			else
			{
				AssertContains("negative compliance document warning message is added to notifications", "Transaction AP INV testAR1 is posted successfully. However, Compliance Document records could not be created as negative compliance document lines are not allowed.", notifications.AsString);
			}
		}

		#region Implementation

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
			line.AL_OSExTaxAmount = amount;
			line.AL_GB = result.Company.Branches[0].PK;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			if (result is ARInvoice)
			{
				result.IsManuallySetTransactionNumber_ForTestOnly = true;
			}

			result.AH_TransactionNum = invoiceNum;

			if (extraSetup != null)
			{
				extraSetup(result);
			}

			Factory.Save();

			return result;
		}

		protected ZGuid StaffGroupPK
		{
			get
			{
				GlbGroup group = Factory.New<GlbGroup>();
				group.Staff.Add(Factory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK)));
				return group.PK;
			}
		}

		protected ZString EmailAddress
		{
			get { return new ZString("blahblah@whatever.example"); }
		}

		protected void SetupStaffMemberEmailAddress()
		{
			BusinessObjectFactory staffMemberFactory = new BusinessObjectFactory();
			GlbStaff currentStaffMember = staffMemberFactory.Load<GlbStaff>(new ZGuid(GlbStaff.CurrentUser.PK));
			currentStaffMember.GS_EmailAddress = EmailAddress;
			staffMemberFactory.Save();
		}

		GlbCompany differentCompany;
		GlbBranch differentBranch;
		OrgHeader differentCompanyOrgProxy;
		OrgHeader differentBranchOrgProxy;
		GlbBranch originalBranch;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now);

			originalBranch = GlbBranch.CurrentBranch;

			differentCompany = TestObjectCreator.CreateNewCompany("ABC");
			differentBranch = TestObjectCreator.CreateNewBranch(differentCompany, "AB1");
			differentCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYC", true, true);
			var closestPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))?.RL_Code;
			differentBranchOrgProxy = TestObjectCreator.CreateOrgHeader("ORGPROXYB", true, true, closestPort);
			differentCompany.GC_OH_OrgProxy = differentCompanyOrgProxy.PK;
			differentBranch.GB_OH_OrgProxy = differentBranchOrgProxy.PK;

			SetupStaffMemberEmailAddress();
			Guid groupPk = StaffGroupPK.ToGuid();
			Factory.Load<GlbDepartment>(GlbDepartment.CurrentDepartment.PK).GE_Misc = false;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.IntercompanyTransactionsImportNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, groupPk);
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		protected override void TearDown()
		{
			InvoiceXmlValueObjectSerializer.IsCrossLedgerOverrideforTest = null;
			base.TearDown();
		}

		#endregion
	}
}
