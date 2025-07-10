using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.Ccsuk.ServiceTasks.Testing
{
	[TestedType(typeof(CcsukServiceTask))]
	class ServiceTaskTests : ServiceTaskTestCase<CcsukServiceTask>
	{
		public void TestUnsuccessfulLogonNotifiesGroup()
		{
			var anyUser = Factory.NewWithValidTestData<GlbStaff>();
			anyUser.GS_EmailAddress = "wtg@wisetechglobal.com";
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.Staff.Add(anyUser);
			Factory.Save();
			GBCustomsDataRegistry.Instance.NotificationCcsukErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			var task = new CcsukServiceTask();
			try
			{
				InitialiseAndRunTaskSchedule(task);
			}
			catch { }
			AssertEquals(1, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertEquals("wtg@wisetechglobal.com", Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.RecipientsAsDelimitedString());
			var body = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body;
			AssertContains("Could not connect to or could not log on to CCSUK network", body);
			AssertContains("Exception message:<BR/>", body);
		}

		public void TestUnsuccessfulLogonNotifiesWtgHosted()
		{
			EnvProxy.SetHostedLocationForTest("blah");
			GBCustomsDataRegistry.Instance.HostedCcsukAlertsEmailAddress.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "alertMe@wtg.com");
			var addresses = new CcsukIpAddressesSettingCollection();
			var a1 = new CcsukIpaddressesSetting();
			var a2 = new CcsukIpaddressesSetting();
			a1.LocalIpAddress = "127.1.1.1";
			a1.CcsukParticipantIpAddress = "172.1.1.1";
			a2.LocalIpAddress = "127.2.2.2";
			a2.CcsukParticipantIpAddress = "172.2.2.2";
			addresses.Add(a1);
			addresses.Add(a2);
			GBCustomsDataRegistry.Instance.CcsukIpAddresses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, addresses);
			var task = new CcsukServiceTask();
			try
			{
				InitialiseAndRunTaskSchedule(task);
			}
			catch { }
			var emails = Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("1 email for production system", 1, emails.Count);
			var subject = emails[0].Subject;
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var expectedSubject = " Ent=" + registrationKey.EnterpriseCode + ", LicDB=" + registrationKey.DatabaseName + " Srvr=" + registrationKey.ServerCode + ", Host=" + System.Environment.MachineName + ", DB=" + Db.Connection.ServerNameReportedByDatabase;
			AssertContains(expectedSubject, subject);
			AssertEquals("alertMe@wtg.com", emails[0].Recipients.RecipientsAsDelimitedString());
			AssertContains(@"Local=127.1.1.1	Participant=172.1.1.1
Local=127.2.2.2	Participant=172.2.2.2", emails[0].Body);
			AssertContains("No acceptable participant IPs were given in the registry, cannot even try to connect", emails[0].Body);

			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			try
			{
				InitialiseAndRunTaskSchedule(task);
			}
			catch { }
			emails = Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("No new emails expected for non production system - should still be 1 from the last run", 1, emails.Count);
		}

		public void TestCUKTaskCanRunOnAnyActiveUKBranch()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_HostName = "ServiceHostForTest";
			Factory.Save();
			var governorMock = new Mock<IServiceManagerGovernor>();

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "COM";
			company.GC_Name = "COMPANY TEST";
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.Italy;
			var activeBranch = company.Branches.AddNew();
			activeBranch.FillWithValidTestData();
			activeBranch.GB_Code = "STS";
			activeBranch.GB_IsActive = true;
			Factory.Save();

			using (ObjectFactory.Substitute(governorMock.Object))
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, activeBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				GlbBranch.Loader loader = new GlbBranch.Loader(Factory);
				var branches = loader.LoadAllBranchesInThisCountryActiveOnly(Core.Constants.CountryCodes.UnitedKingdom);
				branches.ForEach(x => x.GB_IsActive = false);
				Factory.Save();

				var logText = RunCcsukTaskAndGetLog();

				AssertContains("Task cannot run, there is no active GB branch. Task is deactivated", logText);
				governorMock.Verify(g => g.SetServiceTaskIsActive(CcsukServiceTask.Code, false), Times.Once);

				var anyUser = Factory.NewWithValidTestData<GlbStaff>();
				anyUser.GS_EmailAddress = "wtg@wisetechglobal.com";
				var group = Factory.NewWithValidTestData<GlbGroup>();
				group.Staff.Add(anyUser);

				company = Factory.New<GlbCompany>();
				company.GC_Code = "UKO";
				company.GC_Name = "COMPANY TEST";
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
				var branch = company.Branches.AddNew();
				branch.FillWithValidTestData();
				branch.GB_Code = "TST";
				branch.GB_IsActive = true;
				Factory.Save();
				GBCustomsDataRegistry.Instance.NotificationCcsukErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

				logText = RunCcsukTaskAndGetLog();
				AssertEquals(1, Environment.Env.OutgoingCustomsMailManager.EmailsCreated.Count);
				AssertEquals("wtg@wisetechglobal.com", Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Recipients.RecipientsAsDelimitedString());
				var body = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0].Body;
				AssertContains("Could not connect to or could not log on to CCSUK network", body);
				AssertContains("Exception message:<BR/>", body);
				AssertNotContains("Service Task: CUK accesses environment current branch without setting the environment first.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestCcsukGoLiveDate()
		{
			var log = SetupGoLiveDateRegistryAndRunTask(ZDateTime.MinSmallDateTimeValue);
			AssertEquals("Task is run if no registry value", true, HasCssukServiceTaskBeenRun(log));

			var goLiveDateInTheFuture = DateTime.UtcNow.AddDays(1);
			log = SetupGoLiveDateRegistryAndRunTask(goLiveDateInTheFuture);
			AssertEquals("Task is not run", false, HasCssukServiceTaskBeenRun(log));
			AssertContains($"CCSUK go live date is not yet reached, this task will not run.  It will run at {goLiveDateInTheFuture}.", log);

			log = SetupGoLiveDateRegistryAndRunTask(DateTime.UtcNow.AddDays(-1));
			AssertEquals("Task is run", true, HasCssukServiceTaskBeenRun(log));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		string SetupGoLiveDateRegistryAndRunTask(ZDateTime dateValue)
		{
			GBCustomsDataRegistry.Instance.CcsukGoLiveDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dateValue.ToDateTime());
			var logText = RunCcsukTaskAndGetLog();
			return logText;
		}

		string RunCcsukTaskAndGetLog()
		{
			var task = new CcsukServiceTask();
			try
			{
				InitialiseAndRunTaskSchedule(task);
			}
			catch { }
			return task.ServiceLogger.ToString();
		}

		bool HasCssukServiceTaskBeenRun(string log)
		{
			return log.Contains("TASK STARTING UP");
		}

		public void TestServiceTaskSchedule()
		{
			var task = new CcsukServiceTask();
			InitialiseTaskSchedule(task, out StmServiceTask taskSchedule);
			CombineAssertions(() =>
			{
				Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
				AssertEquals("TaskPeriodCount", 10, taskSchedule.Recurrence.Period);
			});
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Production);
		}

		protected override void TearDownCore()
		{
			LicenceTypeChanger.SetSystemLicence(DatabaseTypes.Codes.Training);
			base.TearDownCore();
		}
	}
}
