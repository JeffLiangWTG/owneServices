using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CH.ServiceTasks.Testing;

[TestedType(typeof(MessageTokenRefresherService))]
sealed class MessageTokenRefresherServiceTest : ServiceTaskTestCase<MessageTokenRefresherService>
{
	protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

	public void TestHostedServiceAttribute()
	{
		var hostedServiceAttributes = GetHostedServiceAttributes();
		AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
		var hostedServiceAttribute = hostedServiceAttributes.Single();

		CombineAssertions(() =>
		{
			AssertEquals("Code", "CHT", hostedServiceAttribute.Code);
			AssertEquals("Description", "Swiss Customs Tokens Refresher", hostedServiceAttribute.Description);
			AssertEquals("Category", "CHC", hostedServiceAttribute.Category);
			AssertEquals("RequiresCompanyInCountry", Enterprise.Core.Constants.CountryCodes.Switzerland, hostedServiceAttribute.RequiresCompanyInCountry);
			AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
			AssertEquals("MinimumPeriod", "1minute", hostedServiceAttribute.MinimumPeriod);
			AssertEquals("DefaultScheduleRunEvery", "30seconds", hostedServiceAttribute.DefaultScheduleRunEvery);
		});
	}

	public void TestInitialiseSchedule()
	{
		var serviceTask = new MessageTokenRefresherService();
		InitialiseTaskSchedule(serviceTask, out StmServiceTask taskSchedule);

		CombineAssertions(() =>
		{
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.SecondsRange);
			AssertEquals("TaskPeriodCount", 30, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		});
	}

	public void TestRunTask()
	{
		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();
		var logs = InitialiseAndRunTaskSchedule(new MessageTokenRefresherService());
		AssertEquals($"Information|\t1 Token Refresh message(s) for company {GlbCompany.CurrentCompany.GC_Code} has been processed.\r\n", logs.ToString());
	}

	public void TestIsRequired()
	{
		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("There is no Token Credential configured in Switzerland.", MessageTokenRefresherService.IsRequired());

		CredentialsTestHelper.CreateCurrentCompanyTokenCredential();

		CertificateRequirementChecker.ResetForTesting();
		AssertEquals("IsRequred when a swiss company has relevant Token Credentials", "", MessageTokenRefresherService.IsRequired());
	}
}
