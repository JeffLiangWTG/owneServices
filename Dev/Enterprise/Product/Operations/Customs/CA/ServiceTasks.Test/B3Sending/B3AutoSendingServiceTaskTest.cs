using System;
using System.Collections.Generic;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.CA.ServiceTasks.Testing
{
	[TestedType(typeof(B3AutoSendingServiceTask))]
	sealed class B3AutoSendingServiceTaskTest : ServiceTaskTestCase<B3AutoSendingServiceTask>
	{
		public void TestRunTask()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var caCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany1.GC_Code = "CA1";
			caCompany1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			caCompany1.GC_OH_OrgProxy = org1.PK;
			var branch1 = caCompany1.Branches.AddNew();
			branch1.GB_Code = "AAA";

			var caCompany2 = Factory.NewWithValidTestData<GlbCompany>();
			caCompany2.GC_Code = "CA2";
			caCompany2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "SEA";
			caCompany2.GC_OH_OrgProxy = org2.PK;
			var branch2 = caCompany2.Branches.AddNew();
			branch2.GB_Code = "BBB";

			var caInactiveCompany = Factory.NewWithValidTestData<GlbCompany>();
			caInactiveCompany.GC_Code = "CA3";
			caInactiveCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org3 = Factory.New<OrgHeader>();
			org3.OH_Code = "INA";
			caInactiveCompany.GC_OH_OrgProxy = org2.PK;
			var branch3 = caInactiveCompany.Branches.AddNew();
			branch3.GB_Code = "III";
			branch3.GB_IsActive = false;

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.GC_Code = "USA";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var org4 = Factory.New<OrgHeader>();
			org4.OH_Code = "CHI";
			usCompany.GC_OH_OrgProxy = org1.PK;
			var branch4 = usCompany.Branches.AddNew();
			branch4.GB_Code = "CHI";
			Factory.Save();

			var logger = new TestServiceLogger();
			B3AutoSendingServiceTaskForTesting task = new B3AutoSendingServiceTaskForTesting();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				using (CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertExceptionThrown(typeof(ArgumentException), "Canadian Customs CAD Auto-Sending aborted, see log for details.", () => RunTaskSchedule(task));
				}
			}
			NUnit.Framework.Assert.That(logger[0], NUnit.Framework.Is.EqualTo("Error|Canadian Customs CAD Auto-Sending Service Task not allowed, registry Customs->Canada->Testing and Development->Activate Auto Entry Sending is off."));
			NUnit.Framework.Assert.That(logger[1], NUnit.Framework.Is.EqualTo("Error|Processing suspended due to previous error. Exception will be thrown to terminate service task."));

			logger.ClearLog();
			task = new B3AutoSendingServiceTaskForTesting();
			task.ServiceLogger = logger;
			InitialiseTaskSchedule(task);

			using (new User.IsBatchProcessorOverride(Env.CurrentUser))
			{
				using (CACustomsDataRegistry.Instance.ActivateAutoB3Sending.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					RunTaskSchedule(task);
				}
			}
			string[] expectedLogs = new string[] {
				"Information|Entry Auto Sender executing for Branch: AAA.",
				"Information|Entry Auto Sender executing for Branch: BBB."
			};
			NUnit.Framework.Assert.That(logger.Count, NUnit.Framework.Is.EqualTo(2), "Entry Auto Sender executing for 2 Branchs");
			NUnit.Framework.Assert.That(expectedLogs, NUnit.Framework.Has.Some.EqualTo(logger[0]), "Entry Auto Sender executing for 2 Branchs");
			NUnit.Framework.Assert.That(expectedLogs, NUnit.Framework.Has.Some.EqualTo(logger[1]), "Entry Auto Sender executing for 2 Branchs");
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		sealed class B3AutoSendingServiceTaskForTesting : B3AutoSendingServiceTask
		{
			protected override IB3AutoSender GetB3AutoSender() => new B3AutoSenderForTesting(ServiceLogger);
		}

		sealed class B3AutoSenderForTesting : IB3AutoSender
		{
			public B3AutoSenderForTesting(ILogger serviceLogger)
			{
				this.serviceLogger = serviceLogger;
			}

			public void Process()
			{
				serviceLogger.Log(LogType.Information, string.Format("Entry Auto Sender executing for Branch: {0}.", GlbBranch.CurrentBranch.GB_Code));
			}

			readonly ILogger serviceLogger;
		}
	}
}
