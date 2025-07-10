using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobReopenSecurityCheckHelperTest : TestCaseWithFactory
	{
		public void TestCheckDeletedJob()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;

			var apInv = Factory.NewWithValidTestData<APInvoice>();
			apInv.AddRelatedJobsForReversing_ForTestOnly(job1);
			apInv.AddRelatedJobsForReversing_ForTestOnly(job2);

			AssertCollectionContains(job1, apInv.RelatedJobsForReversing);

			job1.Delete();

			AssertCollectionNotContains(job1, apInv.RelatedJobsForReversing);

			Assert(job1.IsDeleted);
			Assert(!job2.IsDeleted);
			AssertEquals(DataRowState.Detached, ((INeedRow)job1).Row.RowState);
			AssertNotEquals(DataRowState.Detached, ((INeedRow)job2).Row.RowState);

			APLineRelatedJobOpener.ReopenClosedJobwithSuspendedValidation(apInv);

			AssertCollectionContains(job2, apInv.RelatedJobsForReversing);
			AssertEquals(JobHeaderStatus.Working.Code, apInv.RelatedJobsForReversing.First().JH_Status);
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		[TestDate(2020, 01, 01)]
		public void TestCanReopenJobInteractiveSecurity()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var requestLoginCalled = false;
			var testProvider = new SecurityOverrideProviderTest();
			testProvider.OnRequestLoginCredentialsCall += (o, e) => requestLoginCalled = true;
			((ISecurityOverrideProviderSource)job).Provider = testProvider;

			Env.Security.ReopenJob.IsAllowed = false;

			//BusinessContext.AllowReopenJobWhenImporting
			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.AllowReopenJobWhenImporting);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			//BusinessContext.CASS
			requestLoginCalled = false;
			job.Factory.SetContext(BusinessContext.CASS);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.CASS);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			// BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote
			requestLoginCalled = false;
			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			var config = CreateJobClosureConfiguration("ALL", "ALL", "ALL", "JOP", 5, 7);
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.Add(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			Env.Security.ReopenJob.IsAllowed = false;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJob.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", !requestLoginCalled);

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 20);
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			job.JH_A_JOP = new ZDateTime(2019, 12, 20);
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", !requestLoginCalled);

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));

			requestLoginCalled = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));

			Env.Security.ReopenJob.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, job));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);
		}

		public void TestCanReopenJobInteractiveSecurity_WithLoginFormCancelledStackTrace()
		{
			var consol = TestObjectCreator.CreateConsol();
			var shipment = TestObjectCreator.CreateShipment("S001", consol);
			var closedJob = TestObjectCreator.CreateJob(shipment);
			closedJob.JH_A_JOP = new ZDateTime(2019, 12, 30);
			closedJob.JH_Status = JobHeaderStatus.Closed.Code;

			//to set AutoJobClosureConfiguration for ReopenJobPastAllowedReOpenPeriod
			var config = CreateJobClosureConfiguration("ALL", "ALL", "ALL", "JOP", 5, 7);
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.Add(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			Factory.Save();

			var cost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC1, 20, TestObjectCreator.Creditor1, AllocationMethod.Shipment);
			cost.PrepareForPosting();

			var requestLoginCalled = false;
			var testProvider = new SecurityOverrideProviderTest();
			testProvider.OnRequestLoginCredentialsCall += (o, e) => requestLoginCalled = true;
			((ISecurityOverrideProviderSource)closedJob).Provider = testProvider;

			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(closedJob, closedJob));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			var ex = AssertExceptionThrown<OnSavingCriticalCheckException>(() => Factory.Save());

			CombineAssertions(() =>
			{
				var apportionedChargeOnClosedJob = closedJob.Charges[0];
				AssertNotNull("Pre-condition: charge should be apportioned", apportionedChargeOnClosedJob);

				AssertEquals(nameof(CriticalValidationErrorType.JobChargeLinkedToClosedJob), ex.ErrorType);
				AssertContains(CriticalValidationMessageTemplate.JobChargeLinkedToClosedJobErrorMessage, ex.DeveloperErrorMessage);
				AssertContains("Job Status HasChanges: False (CLS)", ex.DeveloperErrorMessage);
				AssertContains("\r\nUser can re-open jobs: False, False", ex.DeveloperErrorMessage);
				AssertContains("\r\nLast Job Opened Time: ", ex.DeveloperErrorMessage);
				AssertContains("\r\nLast Job Closed Time: ", ex.DeveloperErrorMessage);
				AssertContains("\r\nLast Job Closed User: E", ex.DeveloperErrorMessage);

				AssertContains("\r\n\r\nJobCharge:", ex.DeveloperErrorMessage);
				AssertContains("\r\n\r\nJobChargeCreatedOnClosedJobStackTrace:", ex.DeveloperErrorMessage);
				AssertContains("\r\n\r\nJobChargeConstructorStackTrace:", ex.DeveloperErrorMessage);
				AssertContains("\r\n\r\nLoginFormCancelledJobReopenJobChargeCreatedOnClosedJobStackTrace:", ex.DeveloperErrorMessage);
				AssertContains("\nReopenJobPastAllowedReOpenPeriod: False", ex.DeveloperErrorMessage);
				AssertContains("\nReopenJob: False", ex.DeveloperErrorMessage);
				AssertContains("\nStackTrace:", ex.DeveloperErrorMessage);
				AssertContains("\r\nConsolCost ConstructorStackTrace:", ex.DeveloperErrorMessage);
			});
			ExceptionReporterTestListener.Instance.Clear();
		}

		[TestDate(2020, 01, 01)]
		public void TestCanReopenJobInteractiveSecurityWithMultipleJobs()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var config = CreateJobClosureConfiguration("ALL", "ALL", "ALL", "JOP", 5, 7);
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.Add(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			// jobs without closure configuration
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();

			var requestLoginCalled = false;
			var testProvider = new SecurityOverrideProviderTest();
			testProvider.OnRequestLoginCredentialsCall += (o, e) => requestLoginCalled = true;
			((ISecurityOverrideProviderSource)job).Provider = testProvider;

			Env.Security.ReopenJob.IsAllowed = false;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new List<Job> { job, job1 }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJob.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new List<Job> { job, job1 }));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			var shipment2 = testObjectCreator.CreateShipment("S00002", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
			var shipment3 = testObjectCreator.CreateShipment("S00003", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
			var shipment4 = testObjectCreator.CreateShipment("S00004", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
			var shipment5 = testObjectCreator.CreateShipment("S00005", "CNSHA", "AUSYD", transportMode: TransportModes.Air);

			using (Job job2 = testObjectCreator.CreateJob(shipment2))
			using (Job job3 = testObjectCreator.CreateJob(shipment3))
			using (Job job4 = testObjectCreator.CreateJob(shipment4))
			using (Job job5 = testObjectCreator.CreateJob(shipment5))
			{
				// jobs with closure configuration but not past reopen allowed date
				job2.JH_A_JOP = new ZDateTime(2019, 12, 30);
				job3.JH_A_JOP = new ZDateTime(2019, 12, 30);
				((ISecurityOverrideProviderSource)job2).Provider = testProvider;

				Env.Security.ReopenJob.IsAllowed = false;
				Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job2, new List<Job> { job2, job3 }));
				Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

				requestLoginCalled = false;
				Env.Security.ReopenJob.IsAllowed = true;
				Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job2, new List<Job> { job2, job3 }));
				Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

				// jobs with closure configuration and past reopen allowed date
				job4.JH_A_JOP = new ZDateTime(2019, 12, 20);
				job5.JH_A_JOP = new ZDateTime(2019, 12, 20);
				((ISecurityOverrideProviderSource)job4).Provider = testProvider;

				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
				Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job4, new List<Job> { job4, job5 }));
				Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

				requestLoginCalled = false;
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
				Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job4, new List<Job> { job4, job5 }));
				Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

				// jobs mixed
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
				Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new List<Job> { job, job2, job4 }));
				Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

				requestLoginCalled = false;
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
				Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new List<Job> { job, job2, job4 }));
				Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestCanReopenJobNonInteractiveSecurity()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var requestLoginCalled = false;
			var testProvider = new SecurityOverrideProviderTest();
			testProvider.OnRequestLoginCredentialsCall += (o, e) => requestLoginCalled = true;
			((ISecurityOverrideProviderSource)job).Provider = testProvider;

			Env.Security.ReopenJob.IsAllowed = false;

			//BusinessContext.AllowReopenJobWhenImporting
			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.AllowReopenJobWhenImporting);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			//BusinessContext.CASS
			job.Factory.SetContext(BusinessContext.CASS);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.CASS);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			// BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote
			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.AllowReopenJobWhenAutoPostingARCreditNote);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			var anotherJob = new TestObjectCreator(Factory).CreateJob(null, 0, null, 0);
			((ISecurityOverrideProviderSource)anotherJob).Provider = testProvider;

			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, anotherJob));

			Env.Security.ReopenJob.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, anotherJob));

			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			var config = CreateJobClosureConfiguration("ALL", "ALL", "ALL", "JOP", 5, 7);
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.Add(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			job.JH_A_JOP = new ZDateTime(2019, 12, 20);
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 20);
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));

			Env.Security.ReopenJob.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(job.Factory, job));
		}

		[TestDate(2020, 01, 01)]
		public void TestCanReopenJobNonInteractiveSecurityWithMultipleJobs()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var config = CreateJobClosureConfiguration("ALL", "ALL", "ALL", "JOP", 5, 7);
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.Add(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			// jobs without closure configuration
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();

			Env.Security.ReopenJob.IsAllowed = false;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job, job1 }));

			Env.Security.ReopenJob.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job, job1 }));

			var shipment2 = testObjectCreator.CreateShipment("S00002", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
			var shipment3 = testObjectCreator.CreateShipment("S00003", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
			var shipment4 = testObjectCreator.CreateShipment("S00004", "CNSHA", "AUSYD", transportMode: TransportModes.Air);
			var shipment5 = testObjectCreator.CreateShipment("S00005", "CNSHA", "AUSYD", transportMode: TransportModes.Air);

			using (Job job2 = testObjectCreator.CreateJob(shipment2))
			using (Job job3 = testObjectCreator.CreateJob(shipment3))
			using (Job job4 = testObjectCreator.CreateJob(shipment4))
			using (Job job5 = testObjectCreator.CreateJob(shipment5))
			{
				// jobs with closure configuration but not past reopen allowed date
				job2.JH_A_JOP = new ZDateTime(2019, 12, 30);
				job3.JH_A_JOP = new ZDateTime(2019, 12, 30);

				Env.Security.ReopenJob.IsAllowed = false;
				Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job2, job3 }));

				Env.Security.ReopenJob.IsAllowed = true;
				Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job2, job3 }));

				// jobs with closure configuration and past reopen allowed date
				job4.JH_A_JOP = new ZDateTime(2019, 12, 20);
				job5.JH_A_JOP = new ZDateTime(2019, 12, 20);

				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
				Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job4, job5 }));

				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
				Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job4, job5 }));

				// jobs mixed
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
				Assert(!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job, job2, job4 }));

				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
				Assert(JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(Factory, new List<Job> { job, job2, job4 }));
			}
		}

		[TestDate(2020, 01, 01)]
		public void TestCanReopenJob_InteractiveSecurityCheck_Context()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var requestLoginCalled = false;
			var testProvider = new SecurityOverrideProviderTest();
			testProvider.OnRequestLoginCredentialsCall += (o, e) => requestLoginCalled = true;
			((ISecurityOverrideProviderSource)job).Provider = testProvider;

			Env.Security.ReopenJob.IsAllowed = false;

			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, Array.Empty<Job>()));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.AllowReopenJobWhenImporting);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, Array.Empty<Job>()));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			job.Factory.SetContext(BusinessContext.CASS);
			Assert("Canreopen should return true", JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, Array.Empty<Job>()));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			job.Factory.RemoveContext(BusinessContext.CASS);
			Assert("Canreopen should return false", !JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, Array.Empty<Job>()));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);
		}

		[TestDate(2020, 01, 01)]
		public void TestCanReopenJob_InteractiveSecurityCheck_Jobs()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();

			var requestLoginCalled = false;
			var testProvider = new SecurityOverrideProviderTest();
			testProvider.OnRequestLoginCredentialsCall += (o, e) => requestLoginCalled = true;
			((ISecurityOverrideProviderSource)job).Provider = testProvider;

			Env.Security.ReopenJob.IsAllowed = false;

			var config = CreateJobClosureConfiguration("ALL", "ALL", "ALL", "JOP", 5, 7);
			var regValue = new JobClosureConfigurationHeader();
			regValue.ConfigurationCollection.RemoveAll();
			regValue.ConfigurationCollection.Add(config);
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue);

			Env.Security.ReopenJob.IsAllowed = false;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJob.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			requestLoginCalled = false;
			List<Job> jobs = null;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, jobs));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 20);
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			Env.Security.ReopenJob.IsAllowed = false;
			requestLoginCalled = false;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			job.JH_A_JOP = new ZDateTime(2019, 12, 30);
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
			Assert(!JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should be called", requestLoginCalled);

			requestLoginCalled = false;
			Env.Security.ReopenJob.IsAllowed = true;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);

			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;
			Assert(JobReopenSecurityCheckHelper.CanReopenJob_InteractiveSecurityCheck(job, new Job[] { job }));
			Assert("RequestGrantedConfirmation should not be called", !requestLoginCalled);
		}

		JobClosureConfiguration CreateJobClosureConfiguration(string jobType, string direction, string mode, string dateOption, ZInt offset, ZInt restrictionOffset)
		{
			var configuration = new JobClosureConfiguration();
			configuration.JobType = jobType;
			configuration.DirectionCode = direction;
			configuration.Mode = mode;
			configuration.JobClosureDateOptionCode = dateOption;
			configuration.Offset = offset;
			configuration.ReopenRestrictionOffset = restrictionOffset;
			return configuration;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		internal class SecurityOverrideProviderTest : SecurityOverrideProvider
		{
			public event EventHandler OnRequestLoginCredentialsCall;

			protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
			{
				throw new NotImplementedException();
			}

			protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
			{
				OnRequestLoginCredentialsCall?.Invoke(this, new EventArgs());
				return null;
			}
		}
	}
}
