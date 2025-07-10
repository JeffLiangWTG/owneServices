using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.VersionReport;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Licensing.ServiceTasks.Testing
{
	[TestedType(typeof(SystemLicenceService))]
	sealed class SystemLicenceServiceTest : ServiceTaskTestCase<SystemLicenceService>
	{
		public void TestRunTask()
		{
			SystemDataRegistry.Instance.OnDemandLicenceUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(SystemDataRegistry.InitialLicenceUsageReportYear, 1, 1));
			var task = CreateAndRunTestService();

			AssertEquals("SendCurrentCalls", 1, task.TestSender.SendCurrentCalls);
			AssertEquals("BaseSender", typeof(VersionReportBuilderFactory), task.BaseSender.GetType());
			AssertEquals(1, task.UsageProcess.ExecuteCallCount);
		}

		[TestDate(2014, 5, 27, 23, 30, 0)]
		public void TestRunTask_OncePerDay()
		{
			var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			productRegistrationKey.CurrentBillingTimeZoneUtcOffsetForTest = 0.0d;
			productRegistrationKey.NextBillingTimeZoneUtcOffsetForTest = 11.0d;
			productRegistrationKey.NextUtcOffsetEffectiveTimeUtcForTest = new DateTime(2016, 1, 31, 13, 0, 0);

			SystemDataRegistry.Instance.OnDemandLicenceUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(SystemDataRegistry.InitialLicenceUsageReportYear, 1, 1));

			var task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);
			AssertEquals(TestDateAttribute.Date.AddHours(-48), task.LastDateFromUtcInclusive);
			AssertEquals(TestDateAttribute.Date.Date, task.LastDateToUtcExclusive);

			task = CreateAndRunTestService();
			AssertEquals("not sent - day hasn't changed", 0, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2014, 5, 28, 0, 0, 1);
			task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);
			AssertEquals(new DateTime(2014, 5, 27), task.LastDateFromUtcInclusive);
			AssertEquals(TestDateAttribute.Date.Date, task.LastDateToUtcExclusive);

			TestDateAttribute.Date = new DateTime(2014, 5, 28, 0, 0, 5);
			task = CreateAndRunTestService();
			AssertEquals("not sent - day hasn't changed", 0, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2014, 5, 28, 23, 59, 59);
			task = CreateAndRunTestService();
			AssertEquals("not sent - day hasn't changed", 0, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2014, 6, 1, 9, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);
			AssertEquals(new DateTime(2014, 5, 28), task.LastDateFromUtcInclusive);
			AssertEquals(TestDateAttribute.Date.Date, task.LastDateToUtcExclusive);

			TestDateAttribute.Date = new DateTime(2016, 1, 31, 0, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);
			AssertEquals(new DateTime(2016, 1, 31, 0, 0, 0), task.LastDateToUtcExclusive);

			TestDateAttribute.Date = new DateTime(2016, 1, 31, 12, 59, 59);
			task = CreateAndRunTestService();
			AssertEquals("not sent - day hasn't changed", 0, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2016, 1, 31, 13, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("sent - billing time zone offset applies", 1, task.CallsToSendCurrentVersionReport);
			AssertEquals(new DateTime(2016, 1, 31, 0, 0, 0), task.LastDateFromUtcInclusive);
			AssertEquals(new DateTime(2016, 1, 31, 13, 0, 0), task.LastDateToUtcExclusive);

			TestDateAttribute.Date = new DateTime(2016, 2, 1, 0, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("not sent - day in billing time zone hasn't changed", 0, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2016, 2, 1, 13, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);

			productRegistrationKey.CurrentBillingTimeZoneUtcOffsetForTest = 11.0d;
			productRegistrationKey.NextBillingTimeZoneUtcOffsetForTest = 10.0d;
			productRegistrationKey.NextUtcOffsetEffectiveTimeUtcForTest = new DateTime(2016, 4, 2, 16, 0, 0);

			TestDateAttribute.Date = new DateTime(2016, 4, 2, 13, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2016, 4, 3, 13, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("not sent - new offset applies and day in billing time zone hasn't changed", 0, task.CallsToSendCurrentVersionReport);

			TestDateAttribute.Date = new DateTime(2016, 4, 3, 14, 0, 0);
			task = CreateAndRunTestService();
			AssertEquals("sent", 1, task.CallsToSendCurrentVersionReport);
		}

		public void TestSendCurrentVersionReportWithNoCompanyActive()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			var activeCompanies = GlbCompany.GetActiveCompanies();
			foreach (var company in activeCompanies)
			{
				var companyBizO = Factory.Load<GlbCompany>(company.PK);
				companyBizO.GC_IsActive = false;
			}
			if (activeCompanies.Length > 0)
			{
				Factory.Save();
			}

			var interchangeType = ObjectFactory.GetType<IEDIInterchange>();
			var interchangeMessageCount = Factory.GetDatabaseCount(interchangeType);

			var task = new SystemLicenceServiceForSendCurrentVersionReportTesting();
			task.ServiceLogger = new TestServiceLogger();
			task.RunTask();

			// assert the interchange message is generated in the DB
			AssertEquals("New hearbeat interchange message should be created",
				interchangeMessageCount + 1,
				Factory.GetDatabaseCount(interchangeType));

			// assert the newly generated message with the correct company code
			var query = new ZQuery();
			query.OrderBy = EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc + OrderByClause.Descending;
			var interchangeSenderCode = Factory.LoadTop1<IEDIInterchange>(query).EI_From;
			var companyCode = interchangeSenderCode.ToString().Substring(3, 3);
			AssertEquals(companyCode, currentCompany.GC_Code);
		}

		public void TestSendCurrentVersionReportWithCurrentCompanyActive()
		{
			var currentCompany = GlbCompany.GetCurrentCompany(Factory);
			if (!currentCompany.GC_IsActive)
			{
				var companyBizO = Factory.Load<GlbCompany>(currentCompany.PK);
				companyBizO.GC_IsActive = true;
				Factory.Save();
			}

			var interchangeType = ObjectFactory.GetType<IEDIInterchange>();
			var interchangeMessageCount = Factory.GetDatabaseCount(interchangeType);

			var task = new SystemLicenceServiceForSendCurrentVersionReportTesting();
			task.ServiceLogger = new TestServiceLogger();
			task.RunTask();

			// assert the interchange message is generated in the DB
			AssertEquals("New hearbeat interchange message should be created",
				interchangeMessageCount + 1,
				Factory.GetDatabaseCount(interchangeType));

			// assert the newly generated message with the correct company code
			var query = new ZQuery();
			query.OrderBy = EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc + OrderByClause.Descending;
			var interchangeSenderCode = Factory.LoadTop1<IEDIInterchange>(query).EI_From;
			var companyCode = interchangeSenderCode.ToString().Substring(3, 3);
			AssertEquals(companyCode, currentCompany.GC_Code);
		}

		public void TestSendCurrentVersionReportWithCurrentCompanyInactive()
		{
			// create company 1 inactive with active branch, company 2 active without branch
			// company 3 active with branch, company 4 active with branch
			var company1Inactive = Factory.NewWithValidTestData<GlbCompany>();
			company1Inactive.GC_IsActive = false;
			var branchForCompany1Inactive = company1Inactive.Branches.AddNew();
			branchForCompany1Inactive.GB_IsActive = true;
			branchForCompany1Inactive.GB_Code = "B1";
			Factory.Save();

			var company2WithoutBranch = Factory.NewWithValidTestData<GlbCompany>();
			company2WithoutBranch.GC_IsActive = true;
			Factory.Save();

			var company3WithBranch = Factory.NewWithValidTestData<GlbCompany>();
			company3WithBranch.GC_IsActive = true;
			var activeBranch = company3WithBranch.Branches.AddNew();
			activeBranch.GB_IsActive = true;
			activeBranch.GB_Code = "B3";
			Factory.Save();

			var company4WithBranch = Factory.NewWithValidTestData<GlbCompany>();
			company4WithBranch.GC_IsActive = true;
			activeBranch = company4WithBranch.Branches.AddNew();
			activeBranch.GB_IsActive = true;
			activeBranch.GB_Code = "B2";
			Factory.Save();

			var interchangeType = ObjectFactory.GetType<IEDIInterchange>();
			var interchangeMessageCount = Factory.GetDatabaseCount(interchangeType);

			using (Env.SetTemporaryUserContext(
				GlbStaff.CurrentUser.PK.ToGuid(),
				branchForCompany1Inactive.PK.ToGuid(),
				GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var task = new SystemLicenceServiceForSendCurrentVersionReportTesting();
				task.ServiceLogger = new TestServiceLogger();
				task.RunTask();
			}

			// assert the interchange message is generated in the DB
			AssertEquals("New heartbeat interchange message should be created",
				interchangeMessageCount + 1,
				Factory.GetDatabaseCount(interchangeType)
			);

			// assert the newly generated message with the correct company code
			var query = new ZQuery();
			query.OrderBy = EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc + OrderByClause.Descending;
			var interchangeSenderCode = Factory.LoadTop1<IEDIInterchange>(query).EI_From;

			var companyCode = interchangeSenderCode.ToString().Substring(3, 3);
			AssertEquals(companyCode, DisposableEnvironment.GetActiveCompanies().OrderBy(x => x).First());
		}

		public void TestIsTimeForThisDatabaseSoNotAllRunAtSameTime()
		{
			var rego = ObjectFactory.Get<IProductRegistration>();

			rego.KeyForTest.DatabaseNumberForTest = 0;
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 0)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 59)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 1, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 59, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 23, 59, 0)));

			rego.KeyForTest.DatabaseNumberForTest = 1;
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 0)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 59)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 1, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 59, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 23, 59, 0)));

			rego.KeyForTest.DatabaseNumberForTest = 2;
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 0)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 59)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 1, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 59, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 23, 59, 0)));

			rego.KeyForTest.DatabaseNumberForTest = 58;
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 0)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 59)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 1, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 59, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 23, 59, 0)));

			rego.KeyForTest.DatabaseNumberForTest = 59;
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 0)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 59)));
			AssertEquals(false, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 1, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 59, 00)));
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 23, 59, 0)));

			rego.KeyForTest.DatabaseNumberForTest = 60;
			AssertEquals(true, SystemLicenceService.IsTimeForThisDatabaseSoNotAllRunAtSameTime(new ZDateTime(2017, 1, 1, 0, 0, 0)));
		}

		public void TestSqlException_Timeout()
		{
			SystemDataRegistry.Instance.OnDemandLicenceUsageReportDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new DateTime(SystemDataRegistry.InitialLicenceUsageReportYear, 1, 1));
			var task = new SystemLicenceServiceForTesting();
			var logger = new TestServiceLogger();
			task.ServiceLogger = logger;

			var timeoutException = SqlExceptionBuilder.CreateSqlException(
				SqlExceptionBuilder.CreateSqlErrorCollection(
					SqlExceptionBuilder.CreateSqlError(-2, 0, 11, Db.ServerName, "Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0)));

			task.ExceptionToThrow = timeoutException;
			task.RunTask();
			AssertContains("- retry next run", logger[0]);
			AssertStartsWith("Severity", "Warning", logger[0]);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		SystemLicenceServiceForTesting CreateAndRunTestService()
		{
			var task = new SystemLicenceServiceForTesting();
			task.ServiceLogger = new TestServiceLogger();
			using (Env.Instance.TemporaryServiceTaskContext(SystemLicenceService.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
			return task;
		}
	}
}
