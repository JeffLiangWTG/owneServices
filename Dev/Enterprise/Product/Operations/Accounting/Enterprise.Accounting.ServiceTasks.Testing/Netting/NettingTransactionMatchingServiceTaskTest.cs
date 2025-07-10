using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.Netting.Testing
{
	[TestedType(typeof(NettingTransactionMatchingServiceTask))]
	public class NettingTransactionMatchingServiceTaskTest : ServiceTaskTestCase<NettingTransactionMatchingServiceTask>
	{
		[TestDate(2015, 2, 1)]
		public void TestNettingTransactionMatchingServiceTask_SuccessfulRun()
		{
			PrepareTestData();

			var serviceTask = new NettingTransactionMatchingServiceTask();
			var loger = InitialiseAndRunTaskSchedule(serviceTask);
			var log = loger.ToString();

			AssertContains("Information|Netting matching process starting.", log);
			AssertContains("Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '022015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.", log);
			Assert(!log.Contains("Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '022017'."));
			AssertContains("Information|Netting matching process completed.", log);

			using (Env.Instance.TemporaryServiceTaskContext(NettingTransactionMatchingServiceTask.Code, canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}
		}

		[TestDate(2015, 2, 1)]
		public void TestNettingTransactionMatchingServiceTask_SuccessfulRun_CompanyWithoutActiveBranch()
		{
			PrepareTestData();

			GlbCompany.CurrentCompany.Branches.ForEach(x => x.GB_IsActive = false);
			GlbCompany.CurrentCompany.Factory.Save();

			var serviceTask = new NettingTransactionMatchingServiceTask();
			var logger = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals(@"Information|Netting matching process starting.
Debug|Netting matching process starting for company: 'EDI', Netting Cycle: '022015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.
Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '022015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.
Information|Netting matching process completed.", logger.ToString().Trim());
		}

		void PrepareTestData()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var period1 = Factory.New<NettingSystemPeriod>();
			period1.NSP_NS_NettingSystem = NettingSystem.PK;
			period1.NSP_EarliestInvoiceDateUtc = ZDateTime.UtcToday;
			period1.NSP_LatestInvoiceDateUtc = ZDateTime.UtcToday.AddDays(28);
			period1.NSP_Period = "022015";
			period1.NSP_LatestApprovalDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_LatestFXOfferDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_LatestUploadDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_NettingExecutionDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_ValueDate = ZDate.Today.AddDays(14);
			period1.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(14);

			var period2 = Factory.New<NettingSystemPeriod>();
			period2.NSP_NS_NettingSystem = NettingSystem.PK;
			period2.NSP_EarliestInvoiceDateUtc = ZDateTime.UtcToday;
			period2.NSP_LatestInvoiceDateUtc = ZDateTime.UtcToday.AddDays(56);
			period2.NSP_Period = "022016";
			period2.NSP_LatestApprovalDateUtc = ZDateTime.UtcToday.AddDays(28);
			period2.NSP_LatestFXOfferDateUtc = ZDateTime.UtcToday.AddDays(28);
			period2.NSP_LatestUploadDateUtc = ZDateTime.UtcToday.AddDays(28);
			period2.NSP_NettingExecutionDateUtc = ZDateTime.UtcToday.AddDays(28);
			period2.NSP_ValueDate = ZDate.Today.AddDays(28);
			period2.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(28);

			var period3 = Factory.New<NettingSystemPeriod>();
			period3.NSP_NS_NettingSystem = NettingSystem.PK;
			period3.NSP_EarliestInvoiceDateUtc = ZDateTime.UtcToday;
			period3.NSP_LatestInvoiceDateUtc = ZDateTime.UtcToday.AddDays(74);
			period3.NSP_Period = "022017";
			period3.NSP_LatestApprovalDateUtc = ZDateTime.UtcToday.AddDays(32);
			period3.NSP_LatestFXOfferDateUtc = ZDateTime.UtcToday.AddDays(32);
			period3.NSP_LatestUploadDateUtc = ZDateTime.UtcToday.AddDays(32);
			period3.NSP_NettingExecutionDateUtc = ZDateTime.UtcToday.AddDays(32);
			period3.NSP_ValueDate = ZDate.Today.AddDays(32);
			period3.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(32);

			Factory.Save();

			CreateTransaction(NettingSystem, period1);
			Factory.Save();
		}

		[TestDate(2015, 2, 1)]
		public void TestNettingTransactionMatchingServiceTask_OnlyOnePeriodPresent()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var period1 = Factory.New<NettingSystemPeriod>();
			period1.NSP_NS_NettingSystem = NettingSystem.PK;
			period1.NSP_EarliestInvoiceDateUtc = ZDateTime.UtcToday;
			period1.NSP_LatestInvoiceDateUtc = ZDateTime.UtcToday.AddDays(28);
			period1.NSP_Period = "022015";
			period1.NSP_LatestApprovalDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_LatestFXOfferDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_LatestUploadDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_NettingExecutionDateUtc = ZDateTime.UtcToday.AddDays(14);
			period1.NSP_ValueDate = ZDate.Today.AddDays(14);
			period1.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(14);

			Factory.Save();

			var transaction = CreateTransaction(NettingSystem, period1);

			Factory.Save();

			var serviceTask = new NettingTransactionMatchingServiceTask();
			var loger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = loger.ToString();

			AssertContains("Information|Netting matching process starting.", log);
			AssertContains("Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '022015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.", log);
			AssertContains("Debug|No Netting Cycle found after current open Cycle: '022015'.", log);
			AssertContains("Information|Netting matching process completed.", log);
		}

		[TestDate(2015, 2, 25, 16, 1, 0)]
		public void TestNettingTransactionMatchingServiceTask_SuccessfulRun_UnsuccessfulTransactionsMove()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var period1 = Factory.New<NettingSystemPeriod>();
			period1.NSP_NS_NettingSystem = NettingSystem.PK;
			period1.NSP_EarliestInvoiceDateUtc = new ZDateTime(2015, 2, 1);
			period1.NSP_LatestApprovalDateUtc = new ZDateTime(2015, 2, 25, 16, 0, 0);
			period1.NSP_NettingExecutionDateUtc = new ZDateTime(2015, 2, 26);
			period1.NSP_LatestInvoiceDateUtc = new ZDateTime(2015, 2, 28);
			period1.NSP_LatestFXOfferDateUtc = new ZDateTime(2015, 2, 28);
			period1.NSP_LatestUploadDateUtc = new ZDateTime(2015, 2, 28);
			period1.NSP_ValueDate = new ZDate(2015, 2, 28);
			period1.NSP_OfferPrepaymentDate = new ZDate(2015, 2, 28);
			period1.NSP_Period = "022015";

			Factory.Save();

			Assert("Precondition: Latest approval date has elapsed", ZDateTime.UtcNow > period1.NSP_LatestApprovalDateUtc);

			var serviceTask = new NettingTransactionMatchingServiceTask();
			var loger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = loger.ToString();

			var expectedErrorMessage = string.Format("Netting matching process finished with the following error:{0}{1}", System.Environment.NewLine, "No Netting Cycle found after current open Cycle: 022015. The next Netting Cycle is required for moving unmatched transactions from current Cycle since Latest Approval Date has elapsed for the current cycle.");
			AssertContains("Information|Netting matching process starting.", log);
			AssertContains("Error|" + expectedErrorMessage, log);
			Assert(!log.Contains("No Netting Cycle found after current open Cycle: '022015'."));
			AssertContains("Information|Netting matching process completed.", log);
		}

		[TestDate(2016, 2, 25, 16, 1, 0)]
		public void TestNettingTransactionMatchingServiceTask_SuccessfulRun_SuccessfulTransactionsMove()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var period1 = Factory.New<NettingSystemPeriod>();
			period1.NSP_NS_NettingSystem = NettingSystem.PK;
			period1.NSP_EarliestInvoiceDateUtc = new ZDateTime(2015, 2, 1);
			period1.NSP_LatestApprovalDateUtc = new ZDateTime(2015, 2, 25, 16, 0, 0);
			period1.NSP_NettingExecutionDateUtc = new ZDateTime(2015, 2, 26);
			period1.NSP_LatestInvoiceDateUtc = new ZDateTime(2015, 2, 28);
			period1.NSP_LatestFXOfferDateUtc = new ZDateTime(2015, 2, 28);
			period1.NSP_LatestUploadDateUtc = new ZDateTime(2015, 2, 28);
			period1.NSP_ValueDate = new ZDate(2015, 2, 28);
			period1.NSP_OfferPrepaymentDate = new ZDate(2015, 2, 28);
			period1.NSP_Period = "022015";

			var period2 = Factory.New<NettingSystemPeriod>();
			period2.NSP_NS_NettingSystem = NettingSystem.PK;
			period2.NSP_EarliestInvoiceDateUtc = new ZDateTime(2015, 3, 1);
			period2.NSP_LatestApprovalDateUtc = new ZDateTime(2015, 3, 25, 16, 0, 0);
			period2.NSP_NettingExecutionDateUtc = new ZDateTime(2015, 3, 26);
			period2.NSP_LatestInvoiceDateUtc = new ZDateTime(2015, 3, 31);
			period2.NSP_LatestFXOfferDateUtc = new ZDateTime(2015, 3, 28);
			period2.NSP_LatestUploadDateUtc = new ZDateTime(2015, 3, 28);
			period2.NSP_ValueDate = new ZDate(2015, 3, 28);
			period2.NSP_OfferPrepaymentDate = new ZDate(2015, 3, 28);
			period2.NSP_Period = "032015";

			Factory.Save();

			Assert("Precondition: Latest approval date has elapsed", ZDateTime.UtcNow > period1.NSP_LatestApprovalDateUtc);
			Assert("Precondition: move unmatched transaction timestamp is not set", AccountingConfigurationRegistry.Instance.NettingLastMoveUnmatchedTransactionTimeStamp.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty) == DateTime.MinValue);

			var transaction = CreateTransaction(NettingSystem, period1);

			Factory.Save();

			var serviceTask = new NettingTransactionMatchingServiceTask();
			var loger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = loger.ToString();

			AssertContains("Information|Netting matching process starting.", log);
			AssertContains("Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '022015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.", log);
			AssertContains("Debug|Transactions that could not be matched in current Netting Cycle moved to next Cycle as Latest Approval Date has elapsed for cycle '022015'.", log);
			AssertContains("Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '032015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.", log);
			Assert(!log.Contains("Debug|Transactions that could not be matched in current Netting Cycle moved to next Cycle as Latest Approval Date has elapsed for cycle '032015'."));
			AssertContains("Information|Netting matching process completed.", log);

			var newFactory = new BusinessObjectFactory();
			var transactionInNewFactory = newFactory.Load<NettingReceivableTransaction>(transaction.PK);

			AssertEquals(transactionInNewFactory.NRT_NSP_Period, period2.PK);

			Assert("Move unmatched transaction timestamp is now set", AccountingConfigurationRegistry.Instance.NettingLastMoveUnmatchedTransactionTimeStamp.GetValueWithoutFallback(company.PK.ToGuid(), Guid.Empty, Guid.Empty) != DateTime.MinValue);

			//run service task again, the target this time is to show that moving unmatched transaction did not run this time, as it has already run once previouly
			serviceTask = new NettingTransactionMatchingServiceTask();
			loger = InitialiseAndRunTaskSchedule(serviceTask);

			log = loger.ToString();
			AssertContains("Information|Netting matching process starting.", log);
			AssertContains("Debug|Netting matching process ran successfully for company: 'EDI', Netting Cycle: '032015', Issuer: 'EDIHYEDAU', Recipient: 'EDIHYEDUS'.", log);
			AssertContains("Information|Netting matching process completed.", log);
		}

		[TestDate(2015, 2, 1)]
		public void TestNettingTransactionMatchingServiceTask_NoPeriodSetup()
		{
			AccountingConfigurationRegistry.Instance.IsNettingSystem.SetValue(Company.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var serviceTask = new NettingTransactionMatchingServiceTask();
			var loger = InitialiseAndRunTaskSchedule(serviceTask);

			var log = loger.ToString();

			AssertContains("Information|Netting matching process starting.", log);
			AssertContains("Debug|No open Netting Period found.", log);
			AssertContains("Information|Netting matching process completed.", log);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		NettingReceivableTransaction CreateTransaction(NettingSystem nettingSystem, NettingSystemPeriod period1)
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ZZ1ORG1";
			var issuer = Factory.New<NettingOrganisation>();
			issuer.NSO_NettingType = "FUL";
			issuer.NSO_OH_Organisation = org1.PK;
			issuer.NSO_NS_NettingSystem = nettingSystem.PK;
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "EDIHYEDAU");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "ZZ1ORG2";
			var recipient = Factory.New<NettingOrganisation>();
			recipient.NSO_NettingType = "FUL";
			recipient.NSO_OH_Organisation = org2.PK;
			recipient.NSO_NS_NettingSystem = nettingSystem.PK;
			org2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, "EDIHYEDUS");

			return (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period1, issuer, recipient, "INV343", "AUD", 300M);
		}

		NettingSystem NettingSystem
		{
			get
			{
				if (nettingSystem == null)
				{
					nettingSystem = Factory.New<NettingSystem>();
					nettingSystem.NS_Code = "NS1";
					nettingSystem.NS_GC = Company.PK;
					nettingSystem.NS_Description = "bla bla";
				}

				return nettingSystem;
			}
		}
		NettingSystem nettingSystem;

		GlbCompany Company
		{
			get
			{
				return company ?? (company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "EDI")));
			}
		}
		GlbCompany company;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			testObjectCreator = new NettingObjectCreator(Factory);
		}

		NettingObjectCreator testObjectCreator;
	}
}
