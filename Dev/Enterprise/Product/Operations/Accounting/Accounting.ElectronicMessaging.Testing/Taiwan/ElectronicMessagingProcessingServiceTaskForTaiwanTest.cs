using System;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Testing.Taiwan;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Taiwan.Testing
{
	[TestedType(typeof(ElectronicMessagingProcessingServiceTaskForTaiwan))]
	sealed class ElectronicMessagingProcessingServiceTaskForTaiwanTest : GEIElectronicMessagingProcessingServiceTaskTest<ElectronicMessagingProcessingServiceTaskForTaiwan>
	{
		protected override ZString CountryCode => CountryCodes.Taiwan;

		protected override ElectronicMessagingProcessingServiceTaskForTaiwan GetCountrySpecificServiceTask()
		{
			return new ElectronicMessagingProcessingServiceTaskForTaiwan_ForTest();
		}

		[UseSnapshotProtection]
		[TestDate(2020, 7, 30, 8, 0, 0)]
		public override void TestSuccessfulCreationOfEDIInterchange()
		{
			var company1 = Helper.CreateCompanyAndBranch("TW1", "BR2", CountryCode, true);

			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.FirstActiveBranch);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1 }, logger);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 7, 30, 8, 0, 0)]
		public override void TestServiceTaskProcessesEInvoiceOnlyForThoseCompaniesWhereFunctionalityIsEnabled()
		{
			var company1 = Helper.CreateCompanyAndBranch("TW1", "BR1", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch("TW2", "BR2", CountryCode, true);
			Factory.Save();

			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.FirstActiveBranch);

			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var serviceTask = GetCountrySpecificServiceTask();
				var logger = InitialiseAndRunTaskSchedule(serviceTask);

				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1  }, logger);
				AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, Array.Empty<int>(), logger);
			}
		}

		[UseSnapshotProtection]
		[TestDate(2020, 7, 30, 8, 0, 0)]
		public override void TestSuccessfulCreationOfEDIInterchange_OneCompany()
		{
			var company = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);

			//Create first set of compliance documents and run the service task.
			Helper.CreateARComplianceDocumentWithQueuedStatus(company.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company.FirstActiveBranch);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 2);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 2 }, logger);

			//Create another set of transactions and run the service task.
			Helper.CreateARComplianceDocumentWithQueuedStatus(company.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company.FirstActiveBranch);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company, 0, 2);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company, new int[] { 2, 2 }, logger);
		}

		[UseSnapshotProtection]
		[TestDate(2020, 7, 30, 8, 0, 0)]
		public override void TestSuccessfulCreationOfEDIInterchange_MoreThanOneCompany()
		{
			var company1 = Helper.CreateCompanyAndBranch(CountryCode + "1", "BRN", CountryCode, true);
			var company2 = Helper.CreateCompanyAndBranch(CountryCode + "2", "BR2", CountryCode, true);

			//Create first set of compliance documents for 2 comapnies and run the service task.
			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.FirstActiveBranch);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			var serviceTask = GetCountrySpecificServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 2 }, logger);

			//Create another set of compliance documents for 2 comapnies and run the service task again.
			Helper.CreateARComplianceDocumentWithQueuedStatus(company1.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.FirstActiveBranch);
			Helper.CreateARComplianceDocumentWithQueuedStatus(company2.FirstActiveBranch);

			AssertBatchesAndPivotsForCompany_BeforeProcess(company1, 0, 1);
			AssertBatchesAndPivotsForCompany_BeforeProcess(company2, 0, 2);

			serviceTask = GetCountrySpecificServiceTask();
			logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company1, new int[] { 1, 1 }, logger);
			AssertBatchesAndEDIInterchangesForCompany_AfterProcess(company2, new int[] { 2, 2 }, logger);
		}

		[UseSnapshotProtection]
		[TestDate(2019, 9, 4, 13, 35, 40, 111)]
		public override void TestSuccessfulCreatedEDIInterchangeBodyText()
		{
			TestObjectCreator.Debtor.OH_RL_NKClosestPort = "TWTPE";
			var company1 = Helper.CreateCompanyAndBranch("TW1", "BR2", CountryCode, true);

			var complianceDocumentHeader = Helper.CreateARComplianceDocumentWithQueuedStatus(company1.FirstActiveBranch, documentNumber: "AA00000011");

			var serviceTask = new ElectronicMessagingProcessingServiceTaskForTaiwan();
			InitialiseAndRunTaskSchedule(serviceTask);

			var ediInterchanges = Factory.Load<IXmlEDIInterchange>(new ZQuery());
			AssertEquals("expect 1 interchange created", 1, ediInterchanges.Length);
			AssertEquals(TaiwanEInvoiceTestHelper.GetEmbeddedResourceAsString("GEIInterchangeBody_Taiwan.xml").Trim(), ediInterchanges[0].EI_BodyText.Trim());
		}

		sealed class ElectronicMessagingProcessingServiceTaskForTaiwan_ForTest : ElectronicMessagingProcessingServiceTaskForTaiwan
		{
			protected override EDIInterchangeCreatorForEInvoicingBatchBase GetInterchangeCreator(GlbCompany company)
			{
				return new MockEDIInterchangeCreatorForTaiwanEInvoicingBatch(company, () => new MockAccEInvoiceBatchToGEIConverter());
			}
		}
	}
}
