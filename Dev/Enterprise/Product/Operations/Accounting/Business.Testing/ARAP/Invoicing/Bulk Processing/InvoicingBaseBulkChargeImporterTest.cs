using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WiseRates.Tools;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkChargeImporter))]
	public class InvoicingBaseBulkChargeImporterTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			APInvoice invoice = Factory.New<APInvoice>();
			return new InvoicingBaseBulkChargeImporter(invoice);
		}

		public void TestLoadJobsCollection()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			ForwardingConsol consol = creator.CreateConsol("ABC", "DEF", "C0001");
			consol.JK_MasterBillNum = "12345678901";
			ForwardingShipment shipment = creator.CreateShipment("S0001", consol);
			shipment.JS_HouseBill = "10987654321";
			var job = creator.CreateJob(shipment);
			creator.CreateCharge(job);

			Factory.Save();

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();

			AssertEquals("Consol #", "C0001", importer.Jobs[0].JH_ConsolNo);
			AssertEquals("Master Bill #", "12345678901", importer.Jobs[0].JH_MasterBillNo);
			AssertEquals("House Bill #", "10987654321", importer.Jobs[0].JH_HouseBillNo);
		}

		public void TestLoadJobsCollection_isLoadedInBatches()
		{
			var creator = new TestObjectCreator(Factory);

			var parentJobs = new List<Job>();
			var childJobs = new List<Job>();
			for (int i = 0; i < 6; i++)
			{
				var consol = creator.CreateConsol("ABC", "DEF", "C000" + i);
				var shipment = creator.CreateShipment("S000" + i, consol);
				var job = creator.CreateJob(shipment);
				creator.CreateCharge(job);
				parentJobs.Add(job);

				var shipment2 = creator.CreateShipment("SC00" + i);
				var childJob = creator.CreateJob(shipment2);
				creator.CreateCharge(childJob);
				childJob.JH_JH_ParentJob = job.PK;
				childJobs.Add(childJob);
			}

			Factory.Save();

			AssertEquals("There should be 0 fetchhints for JobHeader", 0, Factory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));

			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var importer = new InvoicingBaseBulkChargeImporter(invoice);

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (Db.Connection.TrackExecutedCommands())
			{
				importer.LoadJobsCollection();
				var executedCommands = Db.Connection.ExecutedCommands.Where(c => c.Contains("SELECT JH_PK, JH_JH_ParentJob FROM dbo.JobHeader"));
				AssertEquals("Command should be executed 3 times because batch size in test is 5", 3, executedCommands.Count());
			}

			AssertEquals("There should be 3 fetchhints for JobHeader from LoadJobsCollection()", 3, Factory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));

			AssertEquals(12, importer.Jobs.Count);
			AssertEquals(12, importer.Charges.Length);

			var importedParentJobs = importer.Jobs.Where(x => parentJobs.Select(y => y.PK).Contains(x.PK)).Cast<InvoicingBaseBulkChargeImporterDependentJob>();
			AssertEquals("There are 6 parent jobs", 6, importedParentJobs.Count());
			Assert("All imported parent jobs have 2 charges, one from the parent job and another from the child job",
				importedParentJobs.All(y => y.Charges.Count == 2));

			var importedChildJobs = importer.Jobs.Where(x => childJobs.Select(y => y.PK).Contains(x.PK)).Cast<InvoicingBaseBulkChargeImporterDependentJob>();
			AssertEquals("There are 6 child jobs", 6, importedChildJobs.Count());
			Assert("All imported child jobs have 1 charge, one from the child job",
				importedChildJobs.All(y => y.Charges.Count == 1));

			AssertImport("Since child jobs' charges are contained in parent jobs, the passing in charges should equal to parent jobs' charges"
				, importer
				, invoice
				, parentJobs.SelectMany(x => x.Charges.Cast<Charge>()).ToArray());
		}

		public void TestLoadJobsCollectionWithSecuritySetting()
		{
			var creator = new TestObjectCreator(Factory);
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			var testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = creator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var charge1 = creator.CreateCharge(creator.Job1, creator.CC1, null, creator.AUD, 100m, creator.ABIGAS, null, creator.AUD, 0m, null);
				var charge2 = creator.CreateCharge(creator.Job1, creator.CC2, null, creator.AUD, 200m, creator.ABIGAS, null, creator.AUD, 0m, null);
				Factory.Save();

				var securityFactory = new BusinessObjectFactory();
				var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				var security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				var loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				var loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				var invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = Env.Security.PayablesViewingFinancialOutsideLoginPermission.Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				var invoice = creator.CreateInvoice(typeof(APInvoice), creator.AUD, 1m);
				var importer = new InvoicingBaseBulkChargeImporter(invoice);
				importer.LoadJobsCollection();

				AssertEquals("Precondition", 1, importer.Jobs.Count);
				AssertEquals("should show all charges", importer.Charges.Length, importer.ChargesFilteredByViewingPermission.Length);
				AssertEquals("should show all charges", 2, importer.Jobs[0].Charges.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				Env.Security.PayablesViewingFinancialOutsideLoginPermission.IsAllowed = false;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				importer.LoadJobsCollection();
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				AssertEquals("Precondition", 1, importer.Jobs.Count);
				AssertEquals("should show all charges", importer.Charges.Length, importer.ChargesFilteredByViewingPermission.Length);
				AssertEquals("should show all charges", 2, importer.Jobs[0].Charges.Count);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var charge2InNewFactory = newFactory.Load<Charge>(charge2.PK);
				charge2InNewFactory.JR_GB = testBranch.PK;
				newFactory.Save();

				importer.LoadJobsCollection();
				AssertEquals("Precondition", 1, importer.Jobs.Count);
				AssertEquals("should show all charges", 2, importer.Charges.Length);
				AssertEquals("should show the charge1", 1, importer.ChargesFilteredByViewingPermission.Length);
				AssertEquals("should show the charge1", 1, importer.Jobs[0].Charges.Count);
			}
		}

		public void TestLoadJobsCollectionASecondTime()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateCharge(testObjectCreator.Job1, testObjectCreator.CC1, null, testObjectCreator.AUD, 100m, testObjectCreator.ABIGAS, null, testObjectCreator.AUD, 0m, null);
			testObjectCreator.CreateCharge(testObjectCreator.Job1, testObjectCreator.CC2, null, testObjectCreator.AUD, 200m, testObjectCreator.ABIGAS, null, testObjectCreator.AUD, 0m, null);
			Factory.Save();

			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m);
			var importer = new InvoicingBaseBulkChargeImporter(invoice);

			var chargeCodeFilter = (ModuleGuidFilter)importer.Filters[InvoiceBulkOperationFilterHelper.ChargeCodeFilterName];
			chargeCodeFilter.IsActive = true;
			chargeCodeFilter.Property = testObjectCreator.CC1.PK;

			importer.LoadJobsCollection();

			AssertEquals("Precondition: There is 1 charge in the job's charges collection", 1, importer.Jobs[0].Charges.Count);

			importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();

			AssertEquals("Should be 2 charges in the job's charges collection. Job should not be caching the results from the first filter.", 2, importer.Jobs[0].Charges.Count);
		}

		public void TestJobReloader2Job()
		{
			AssertJobReloaderDbHits(2, 1);
		}

		public void TestJobReloader5Jobs()
		{
			AssertJobReloaderDbHits(5, 1);
		}

		public void TestJobReloader6Jobs()
		{
			AssertJobReloaderDbHits(6, 2);
		}

		public void TestJobReloader10Jobs()
		{
			AssertJobReloaderDbHits(10, 2);
		}

		public void TestJobReloader11Jobs()
		{
			AssertJobReloaderDbHits(11, 3);
		}

		void AssertJobReloaderDbHits(int numberOfJobs, int expectedDbHits)
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			for (int i = 0; i < numberOfJobs; i++)
			{
				var job = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 0m, null, 0m);
				testObjectCreator.CreateCharge(job, testObjectCreator.CC1, null, testObjectCreator.AUD, 100m, testObjectCreator.LocalClient, null, testObjectCreator.AUD, 0m, null);
				Factory.Save();
			}

			int hitsBefore = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			int fetchHintsHeaderBefore = Factory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName);
			int fetchHintsExRateBefore = Factory.ActiveFetchHintsForTable(JobExRateSchema.Constants.TableName);

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m);
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);

			importer.UpdateSelectedLocalTotalExecutionTimes = 0;
			importer.LoadJobsCollection();

			int hitsAfter = Factory.GetTableHitCount(JobChargeSchema.Constants.TableName);
			AssertEquals(string.Format("Expected {0} hit(s) with test batch size 5", expectedDbHits), expectedDbHits, hitsAfter - hitsBefore);

			int fetchHintsHeaderAfter = Factory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName);
			int fetchHintsExRateAfter = Factory.ActiveFetchHintsForTable(JobExRateSchema.Constants.TableName);
			Assert(numberOfJobs > fetchHintsHeaderAfter - fetchHintsHeaderBefore);
			Assert(numberOfJobs >= fetchHintsExRateAfter - fetchHintsExRateBefore);
		}

		public void TestLoadJobsCollectionCallsUpdateSelectedLocalTotalOnlyOnce()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			for (int i = 0; i < 10; i++)
			{
				var job = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 0m, null, 0m);
				testObjectCreator.CreateCharge(job, testObjectCreator.CC1, null, testObjectCreator.AUD, 100m, testObjectCreator.LocalClient, null, testObjectCreator.AUD, 0m, null);
			}
			Factory.Save();

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m);
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);

			importer.UpdateSelectedLocalTotalExecutionTimes = 0;
			importer.LoadJobsCollection();

			AssertEquals("Precondition: There are 10 jobs", 10, importer.Jobs.Count);
			AssertEquals("UpdateSelectedLocalTotal should have only executed once", 1, importer.UpdateSelectedLocalTotalExecutionTimes);

			importer.UpdateSelectedLocalTotalExecutionTimes = 0;
			importer.LoadJobsCollection();
			importer.LoadJobsCollection();

			AssertEquals("Precondition: There are 10 jobs", 10, importer.Jobs.Count);
			AssertEquals("Successive calls to LoadCollection should only execute UpdateSelectedLocalTotal once per call", 2, importer.UpdateSelectedLocalTotalExecutionTimes);
		}

		public void TestSelectingAJobCallsUpdateSelectedLocalTotalOnlyOnce()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 0m, null, 0m);
			for (int i = 0; i < 10; i++)
			{
				testObjectCreator.CreateCharge(job, testObjectCreator.CC1, null, testObjectCreator.AUD, 100m, testObjectCreator.LocalClient, null, testObjectCreator.AUD, 0m, null);
			}

			Factory.Save();

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m);
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();

			AssertEquals("Precondition: There is 1 job", 1, importer.Jobs.Count);

			importer.UpdateSelectedLocalTotalExecutionTimes = 0;
			importer.Jobs[0].IsSelectedForImport = true;

			AssertEquals("UpdateSelectedLocalTotal should have only executed once", 1, importer.UpdateSelectedLocalTotalExecutionTimes);
		}

		public void TestLoadJobsCollectionDoesNotFireIsSelectedChangedEventHandler()
		{
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			var job = testObjectCreator.CreateJob(testObjectCreator.LocalClient, 0m, null, 0m);
			testObjectCreator.CreateCharge(job, testObjectCreator.CC1, null, testObjectCreator.AUD, 100m, testObjectCreator.LocalClient, null, testObjectCreator.AUD, 0m, null);
			Factory.Save();

			InvoicingBase invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m);
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.Jobs.IsSelectedChangedTotalExecutionTimes = 0;
			importer.LoadJobsCollection();

			AssertEquals("Precondition: There is 1 job", 1, importer.Jobs.Count);
			AssertEquals("IsSelectedChanged event handler should not execute when loading jobs collection", 0, importer.Jobs.IsSelectedChangedTotalExecutionTimes);

			importer.LoadJobsCollection();
			importer.LoadJobsCollection();

			AssertEquals("IsSelectedChanged event handler should not execute when loading jobs collection successively", 0, importer.Jobs.IsSelectedChangedTotalExecutionTimes);
		}

		public void TestLoadJobsCollection_ShowWarningMessageWhenHasSqlException8623()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var invoice = testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, 1m);
			var importer = new InvoicingBaseBulkChargeImporter(invoice);
			var filterStrips = new FilterStripCollection(importer.Filters.ModuleFilters);

			for (var i = 0; i < 50; i++)
			{
				var containerNumberFilter = filterStrips.AddNew(InvoiceBulkOperationFilterHelper.ContainerNumberFilterName);
				((ModuleTextFilter)containerNumberFilter.CurrentModuleFilter).ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;
				((ModuleTextFilter)containerNumberFilter.CurrentModuleFilter).Property = i.ToString();
			}

			importer.Jobs.IsSelectedChangedTotalExecutionTimes = 0;
			ErrorReporter.Instance.Clear();
			AssertNoExceptionThrown(() => importer.LoadJobsCollection());
			AssertEquals("Your query is too complicated, please simplify your search conditions.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPortTransportChargeIncludedOnlyOnce()
		{
			var rnd = new Random();
			var testObjectCreator = new TestObjectCreator(Factory);

			var consol = testObjectCreator.CreateConsol();
			consol.JK_MasterBillNum = $"MB{rnd.Next()}";
			var shipment = testObjectCreator.CreateShipment($"FS{rnd.Next()}", consol);
			shipment.JS_HouseBill = $"HB{rnd.Next()}";
			var mainJob = testObjectCreator.CreateJob(shipment);
			testObjectCreator.CreateCharge(mainJob, testObjectCreator.CC1, null, testObjectCreator.AUD, 100m, testObjectCreator.Creditor1, testObjectCreator.AUD, 200m, null);

			var cartage = testObjectCreator.CreateCartage();
			var cartageJob = testObjectCreator.CreateJob(cartage);
			var cartageCharge = testObjectCreator.CreateCharge(cartageJob, testObjectCreator.CC1, null, testObjectCreator.AUD, 300m, testObjectCreator.Creditor1, testObjectCreator.AUD, 400m, null);
			cartage.Job.JH_JH_ParentJob = mainJob.PK;
			Factory.Save();

			APInvoice apInvoice = (APInvoice)testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, null, null, null);
			var importer = new InvoicingBaseBulkChargeImporter(apInvoice);
			ModuleGuidFilter creditorFilter = (ModuleGuidFilter)importer.Filters[InvoiceBulkOperationFilterHelper.CreditorFilterName];
			creditorFilter.IsActive = true;
			creditorFilter.Property = testObjectCreator.Creditor1.PK;

			importer.LoadJobsCollection();
			AssertEquals("There should be 2 Jobs", 2, importer.Charges.Length);
			// Make sure that only Cartage's job is selected for import
			var jobs = importer.Jobs.Cast<InvoicingBaseBulkChargeImporterDependentJob>();
			jobs.Single(j => j.PK == mainJob.PK).IsSelectedForImport = false;
			jobs.Single(j => j.PK == cartageJob.PK).IsSelectedForImport = true;

			AssertImport("We should only pass cartageCharge to Importer.", importer, apInvoice, cartageCharge);
		}

		public void TestNoAuditInformationInModuleFilters()
		{
			AssertNoExceptionThrown(() =>
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				var job = testObjectCreator.CreateJob("J0001", testObjectCreator.LocalClient, 1.0M, testObjectCreator.Agent, 1.0M);
				Factory.Save();

				APInvoice apInvoice = (APInvoice)testObjectCreator.CreateInvoice(typeof(APInvoice), testObjectCreator.AUD, null, null, null);
				var importer = new InvoicingBaseBulkChargeImporter(apInvoice);
				var filters = importer.Filters;

				AssertNotNull($"Audit Information: {FilterDescriptions.CreatedTime}", filters.ModuleFilters[FilterDescriptions.CreatedTime]);
				AssertNotNull($"Audit Information: {FilterDescriptions.CreatingUser}", filters.ModuleFilters[FilterDescriptions.CreatingUser]);
				AssertNotNull($"Audit Information: {FilterDescriptions.LastEditTime}", filters.ModuleFilters[FilterDescriptions.LastEditTime]);
				AssertNotNull($"Audit Information: {FilterDescriptions.LastEditUser}", filters.ModuleFilters[FilterDescriptions.LastEditUser]);

				filters[FilterDescriptions.CreatingUser].IsActive = true;
				(filters[FilterDescriptions.CreatingUser] as ModuleNkFilter).Property = job.JH_SystemCreateUser;
				importer.LoadJobsCollection();
			});
		}

		void AssertImport(string comment, InvoicingBaseBulkChargeImporter importer, InvoicingBase passingInInvoice, params Charge[] expectedPassingInCharges)
		{
			var mockIInvoicingBaseLineImporter = CreateMockInvoicingBaseLineImporter(passingInInvoice, expectedPassingInCharges);
			using (ObjectFactory.Substitute<IInvoicingBaseLineImporter>(mockIInvoicingBaseLineImporter.Object))
			{
				AssertNoExceptionThrown(comment, () => importer.Import());
			}
			mockIInvoicingBaseLineImporter.Verify(
				x => x.ImportLinesFromChargeCollection(It.IsAny<InvoicingBase>(), It.IsAny<IEnumerable<Charge>>())
				, Times.Exactly(1)
			);
		}

		Mock<IInvoicingBaseLineImporter> CreateMockInvoicingBaseLineImporter(InvoicingBase expectedPassingInInvoice, params Charge[] expectedPassingInCharges)
		{
			var mockIInvoicingBaseLineImporter = new Mock<IInvoicingBaseLineImporter>();
			mockIInvoicingBaseLineImporter.Setup(
				x => x.ImportLinesFromChargeCollection(
					expectedPassingInInvoice
					, It.Is<Charge[]>(charges => charges.SequenceEqualIgnoringOrder(expectedPassingInCharges, null, true))
				));
			return mockIInvoicingBaseLineImporter;
		}
	}
}
