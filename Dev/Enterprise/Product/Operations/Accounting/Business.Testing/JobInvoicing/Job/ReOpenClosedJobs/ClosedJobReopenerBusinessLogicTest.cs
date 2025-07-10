using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	// Additional security-related test cases are encompassed within the ClosedJobReopenerGUILogicTest located in Enterprise.Accounting.GUI.Testing.JobInvoicing
	public class ClosedJobReopenerBusinessLogicTest : TestCaseWithFactory
	{
		public void TestConstructorValidation()
		{
			var mockReOpenClosedJobDataProvider = new Mock<IReOpenClosedJobDataProvider>();
			var mockReopenClosedJobSecurityOverrideProvider = new Mock<IReopenClosedJobSecurityOverrideProvider>();

			AssertExceptionThrown<ArgumentNullException>("IReOpenClosedJobDataProvider null", () => new ClosedJobReopener(null, mockReopenClosedJobSecurityOverrideProvider.Object));
			AssertExceptionThrown<ArgumentNullException>("IReopenClosedJobSecurityOverrideProvider null", () => new ClosedJobReopener(mockReOpenClosedJobDataProvider.Object, null));
		}

		public void TestWarningMessageForNullJob()
		{
			var closedJobReopener = GetClosedJobReopener();

			var dummyPropertyInfo = GetDummyPropertyInfo();
			Env.Security.ReopenJob.IsAllowed = false;

			AssertNoWarnings(dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, null);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

			dummyPropertyInfo.ClearAllNotifications();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();
			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
		}

		public void TestWarningMessageForNonClosedJob()
		{
			var closedJobReopener = GetClosedJobReopener();
			var dummyPropertyInfo = GetDummyPropertyInfo();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Working.ToString();
			Env.Security.ReopenJob.IsAllowed = false;

			AssertNoWarnings(dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

			dummyPropertyInfo.ClearAllNotifications();
			job.JH_Status = JobHeaderStatus.Closed.ToString();
			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
		}

		public void TestWarningMessageForClosedJob_WhenErrorPresentOnProperty()
		{
			var testErrorMessage = "test error message";
			var closedJobReopener = GetClosedJobReopener();
			var dummyPropertyInfo = GetDummyPropertyInfo();

			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();
			Env.Security.ReopenJob.IsAllowed = false;

			dummyPropertyInfo.AddError(testErrorMessage);
			AssertHasErrors("Precondition: Property has error", dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

			dummyPropertyInfo.ClearAllNotifications();

			AssertNoWarnings(dummyPropertyInfo);
			AssertNoErrors(dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
		}

		public void TestWarningMessageForClosedJob_HasContextAllowReopenJobWhenImporting()
		{
			var closedJobReopener = GetClosedJobReopener();
			var dummyPropertyInfo = GetDummyPropertyInfo();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();
			Env.Security.ReopenJob.IsAllowed = false;

			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));
			AssertNoWarnings(dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

			dummyPropertyInfo.ClearAllNotifications();
			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobWarningMessage);
		}

		public void TestWarningMessageForClosedJob_HasContextCASS()
		{
			var closedJobReopener = GetClosedJobReopener();
			var dummyPropertyInfo = GetDummyPropertyInfo();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();
			Env.Security.ReopenJob.IsAllowed = false;

			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));
			AssertNoWarnings(dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

			dummyPropertyInfo.ClearAllNotifications();
			job.Factory.SetContext(BusinessContext.CASS);
			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobWarningMessage);
		}

		[TestDate(2023, 11, 16)]
		public void TestWarningMessageForClosedJob_IsPastAllowedRestrictionDate_True()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			using (var job = Job.CreateWithMutex(Factory, shipment))
			{
				var closedJobReopener = GetClosedJobReopener();
				var dummyPropertyInfo = GetDummyPropertyInfo();
				job.JH_Status = JobHeaderStatus.Closed.ToString();
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				job.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				Assert(job.IsPastAllowedRestrictionDate());
				Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
				Assert(!Factory.HasContext(BusinessContext.CASS));
				AssertNoWarnings(dummyPropertyInfo);

				closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

				AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
				AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

				dummyPropertyInfo.ClearAllNotifications();
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
				closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

				AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
				AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
				AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobWarningMessage);
			}
		}

		public void TestWarningMessageForClosedJob_IsPastAllowedRestrictionDate_False()
		{
			var closedJobReopener = GetClosedJobReopener();
			var dummyPropertyInfo = GetDummyPropertyInfo();
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();
			Env.Security.ReopenJob.IsAllowed = false;

			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));
			AssertNoWarnings(dummyPropertyInfo);

			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);

			dummyPropertyInfo.ClearAllNotifications();
			Env.Security.ReopenJob.IsAllowed = true;
			closedJobReopener.ValidateClosedJob(dummyPropertyInfo, job);

			AssertNoError(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertNoWarning(dummyPropertyInfo, ExpectedReopenClosedJobSecurityMessage);
			AssertHasWarning(dummyPropertyInfo, ExpectedReopenClosedJobWarningMessage);
		}

		public void TestReopenClosedJobs_HasNullJobs_ReturnsTrue()
		{
			var closedJobReopener = GetClosedJobReopener();
			Assert(closedJobReopener.ReopenClosedJobs());
		}

		public void TestReopenClosedJobs_HasNoClosedJobs_ReturnsTrue()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Working.ToString();

			var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
			Assert(closedJobReopener.ReopenClosedJobs());
		}

		public void TestReopenClosedJobs_UserDoesNotRights_HasContextAllowReopenJobWhenImporting()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();

			Env.Security.ReopenJob.IsAllowed = false;
			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));

			var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
			AssertEquals(false, closedJobReopener.ReopenClosedJobs());
			Assert(job.IsClosed);

			job.Factory.SetContext(BusinessContext.AllowReopenJobWhenImporting);
			AssertEquals(true, closedJobReopener.ReopenClosedJobs());
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
		}

		public void TestReopenClosedJobs_UserDoesNotRights_HasContextCASS()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();

			Env.Security.ReopenJob.IsAllowed = false;
			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));

			var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
			AssertEquals(false, closedJobReopener.ReopenClosedJobs());
			Assert(job.IsClosed);

			job.Factory.SetContext(BusinessContext.CASS);
			AssertEquals(true, closedJobReopener.ReopenClosedJobs());
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
		}

		[TestDate(2023, 11, 16)]
		public void TestReopenClosedJobs_UserHasReopenJobPastAllowedReOpenPeriodSecurity_IsPastAllowedRestrictionDate_True()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			using (var job = Job.CreateWithMutex(Factory, shipment))
			{
				var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
				job.JH_Status = JobHeaderStatus.Closed.ToString();
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				job.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				Assert(job.IsPastAllowedRestrictionDate());
				Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
				Assert(!Factory.HasContext(BusinessContext.CASS));

				AssertEquals(false, closedJobReopener.ReopenClosedJobs());
				Assert(job.IsClosed);

				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;
				AssertEquals(true, closedJobReopener.ReopenClosedJobs());
				AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
			}
		}

		public void TestReopenClosedJobs_UserHasReopenJobSecurity_IsPastAllowedRestrictionDate_False()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.ToString();

			var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
			Env.Security.ReopenJob.IsAllowed = false;

			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));

			AssertEquals(false, closedJobReopener.ReopenClosedJobs());
			Assert(job.IsClosed);

			Env.Security.ReopenJob.IsAllowed = true;
			AssertEquals(true, closedJobReopener.ReopenClosedJobs());
			AssertEquals(JobHeaderStatus.Working.Code, job.JH_Status);
		}

		public void TestReopenClosedJobs_JobReopenLogText()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.ToString();

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Working.ToString();

			Factory.Save();

			Env.Security.ReopenJob.IsAllowed = true;
			Assert(!Factory.HasContext(BusinessContext.AllowReopenJobWhenImporting));
			Assert(!Factory.HasContext(BusinessContext.CASS));

			var closedJobReopener = GetClosedJobReopener(new List<Job> { job1, job2 });
			AssertEquals(true, closedJobReopener.ReopenClosedJobs());
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);

			Factory.Save();

			AssertEquals("ClosedJobReopener must call Job.ReOpenByImport() method to reopen jobs. If this UT failes, a possible reason is Job.Reopen() was called instead, which is incorrect.", "Job status changed from CLS to WRK - Test Log for Job Reopening.", job1.GetLogs().AutoCreatedLog.DisplayEventReference);
			AssertEquals("ClosedJobReopener should not call Job.ReOpenByImport() method to reopen job as job already had working status", string.Empty, job2.GetLogs().AutoCreatedLog.DisplayEventReference);
		}

		IClosedJobReopener GetClosedJobReopener(List<Job> jobs = null)
		{
			var mockReOpenClosedJobDataProvider = new Mock<IReOpenClosedJobDataProvider>();
			var mockReopenClosedJobSecurityOverrideProvider = new Mock<IReopenClosedJobSecurityOverrideProvider>();

			ISecurityOverrideProvider testProvider = new DefaultAccessSecurityProvider();
			mockReopenClosedJobSecurityOverrideProvider.Setup(x => x.SecurityCertificates).Returns(testProvider.SecurityCertificates);

			mockReOpenClosedJobDataProvider.Setup(x => x.Factory).Returns(Factory);
			mockReOpenClosedJobDataProvider.Setup(x => x.GetAllJobs()).Returns(jobs);
			mockReOpenClosedJobDataProvider.Setup(x => x.JobReopenLogText()).Returns(" - Test Log for Job Reopening");

			return new ClosedJobReopener(mockReOpenClosedJobDataProvider.Object, mockReopenClosedJobSecurityOverrideProvider.Object);
		}

		ZPropertyInfo GetDummyPropertyInfo()
		{
			var bizO = Factory.NewWithValidTestData<DummyBusinessObjectForWarningCheck>();
			bizO.TemporaryPropertyForJobNumber = "ValueForTemporaryProperty";
			Factory.Save();

			return bizO.TemporaryPropertyForJobNumberInfo;
		}

		void SetJobClosureConfigurationSetupRegistry()
		{
			var header = new JobClosureConfigurationHeader();
			var configLine = header.ConfigurationCollection.AddNew();
			configLine.JobType = "SHP";
			configLine.DirectionCode = FreightShipmentDirection.Code.Import;
			configLine.Mode = TransportModes.Sea;
			configLine.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			configLine.Offset = 5;
			configLine.ReopenRestrictionOffset = 7;
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, header);
		}

		string ExpectedReopenClosedJobSecurityMessage
		{
			get
			{
				return "This job is currently closed. As you do not have the access right to re-open the Job, you will require authorization to proceed upon saving.";
			}
		}

		string ExpectedReopenClosedJobWarningMessage
		{
			get
			{
				return "This job is currently closed. The job will be reopened after saving.";
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		class DummyBusinessObjectForWarningCheck : DummyBusinessObject, IObsoleteValidation
		{
			public DummyBusinessObjectForWarningCheck(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString TemporaryPropertyForJobNumber
			{
				get
				{
					return temporaryPropertyForJobNumber;
				}
				set
				{
					temporaryPropertyForJobNumber = value;
				}
			}
			ZString temporaryPropertyForJobNumber;

			public ZPropertyInfo TemporaryPropertyForJobNumberInfo
			{
				get { return GetZPropertyInfo(nameof(TemporaryPropertyForJobNumber)); }
			}
		}
	}
}
