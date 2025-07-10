using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.ServiceTasks.DataTransfer.Testing
{
	[TestedType(typeof(AutomaticGLTakeUpServiceTask))]
	public class AutomaticGLTakeUpServiceTaskTest : ServiceTaskTestCase<AutomaticGLTakeUpServiceTask>
	{
		[TestDate(2014, 06, 06, 13, 0, 0)]
		[ExpectNoExceptions]
		public void TestAutomaticGLTakeUpServiceRun_NoActiveBranch()
		{
			AutomaticProcessRegistryBusinessObject bizO = new AutomaticProcessRegistryBusinessObject(Factory);
			bizO.NextRunDateTime = ZDateTime.Now;
			bizO.IntervalType = "DAYS";
			bizO.Interval = 1;

			AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bizO);

			foreach (var branch in GlbCompany.CurrentCompany.Branches)
			{
				var loadedBranch = Factory.Load<GlbBranch>(branch.PK);
				loadedBranch.GB_IsActive = false;
			}
			Factory.Save();

			AutomaticGLTakeUpServiceTask task = new AutomaticGLTakeUpServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertEquals(3, serviceLog.Count);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task started.", serviceLog[0]);
			AssertEquals("Debug|No active branch found for Company: 'EDI'.", serviceLog[1]);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task completed.", serviceLog[2]);

			using (Env.Instance.TemporaryServiceTaskContext(AutomaticGLTakeUpServiceTask.Code, canRunInAnyBranch: true))
			{
				task.RunTask();
			}
		}

		[TestDate(2014, 06, 06, 13, 0, 0)]
		public void TestAutomaticGLTakeUpServiceRun_CurrentCompany_FailCase()
		{
			AutomaticProcessRegistryBusinessObject bizO = new AutomaticProcessRegistryBusinessObject(Factory);
			bizO.NextRunDateTime = ZDateTime.Now;
			bizO.IntervalType = "MINUTES";
			bizO.Interval = 1;

			AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bizO);
			AutomaticGLTakeUpServiceTask task = new AutomaticGLTakeUpServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertEquals(4, serviceLog.Count);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task started.", serviceLog[0]);
			AssertEquals("Debug|Automatic Sub Ledger Takeup running for Company: 'EDI'.", serviceLog[1]);
			AssertStartsWith("Expected error message", @"Error|Automatic Sub Ledger Takeup for 'EDI' failed with the following error.
Please set up the following Control Accounts in the registry (Accounting > General Ledger Defaults > Control Account): Revenue Suspense Control Account, Cost Suspense Control Account
Please retry aggregation once you have corrected this error.
GL accounts were last taken up on ", serviceLog[2]);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task completed.", serviceLog[3]);

			AssertEquals(ZDateTime.Now.AddMinutes(1),
				AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).NextRunDateTime);
		}

		[TestDate(2014, 06, 06, 13, 0, 0)]
		public void TestAutomaticGLTakeUpServiceRun_CurrentCompany_SuccessCase()
		{
			AutomaticProcessRegistryBusinessObject bizO = new AutomaticProcessRegistryBusinessObject(Factory);
			bizO.NextRunDateTime = ZDateTime.Now;
			bizO.IntervalType = "DAYS";
			bizO.Interval = 1;

			AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bizO);

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			AutomaticGLTakeUpServiceTask task = new AutomaticGLTakeUpServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);
			AssertEquals(4, serviceLog.Count);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task started.", serviceLog[0]);
			AssertEquals("Debug|Automatic Sub Ledger Takeup running for Company: 'EDI'.", serviceLog[1]);
			AssertEquals("Debug|Automatic Sub Ledger Takeup successfully ran for Company: 'EDI'.", serviceLog[2]);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task completed.", serviceLog[3]);

			AssertEquals(ZDateTime.Now.AddDays(1),
				AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).NextRunDateTime);
		}

		[TestDate(2014, 06, 06, 13, 0, 0)]
		public void TestAutomaticGLTakeUpServiceRun_NonCurrentCompany()
		{
			AutomaticProcessRegistryBusinessObject bizO = new AutomaticProcessRegistryBusinessObject(Factory);
			bizO.NextRunDateTime = ZDateTime.Now.AddHours(-12);
			bizO.IntervalType = "HOURS";
			bizO.Interval = 2;

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			TestHelper.SetUpPeriods(TestObjectCreator.NonCurrentNonDemoCompany.PK);

			AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(TestObjectCreator.NonCurrentNonDemoCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bizO);
			AutomaticGLTakeUpServiceTask task = new AutomaticGLTakeUpServiceTask();
			var serviceLog = InitialiseAndRunTaskSchedule(task);

			AssertEquals(4, serviceLog.Count);

			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task started.", serviceLog[0]);
			AssertEquals("Debug|Automatic Sub Ledger Takeup running for Company: 'SIN'.", serviceLog[1]);
			AssertEquals("Debug|Automatic Sub Ledger Takeup successfully ran for Company: 'SIN'.", serviceLog[2]);
			AssertEquals("Information|Automatic Sub Ledger Takeup Service Task completed.", serviceLog[3]);
		}

		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ATU", hostedServiceAttribute.Code);
				AssertEquals("Description", "Accounting Automatic Sub Ledger Takeup Service Task", hostedServiceAttribute.Description);
				AssertEquals("Category", "ACC", hostedServiceAttribute.Category);
				AssertEquals("Type", typeof(AutomaticGLTakeUpServiceTask), hostedServiceAttribute.Type);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "15minutes", hostedServiceAttribute.MinimumPeriod);
			});
		}

		// No nudging: no queue table.
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHasNoQueueProvider()
		{
			var queueProvider = GetHostedServiceQueueProviderInstanceByServiceTaskCode("ATU");
			AssertNull("No queue table for this service task.", queueProvider);
		}

		[TestDate(2014, 06, 06, 13, 0, 0)]
		public void TestAutomaticGLTakeUpServiceRun_ShouldNotAccessEnvCurrentBranchDirectly()
		{
			var bizO = new AutomaticProcessRegistryBusinessObject(Factory);
			bizO.NextRunDateTime = ZDateTime.Now;
			bizO.IntervalType = "DAYS";
			bizO.Interval = 1;

			AccountingConfigurationRegistry.Instance.AllowAutomaticSubLedgerTakeup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, bizO);
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var task = new AutomaticGLTakeUpServiceTask();
			task.ServiceLogger = (new Mock<ILogger>()).Object;

			try
			{
				using (EnvProxy.Instance.TemporaryServiceTaskContext(AutomaticGLTakeUpServiceTask.Code, true))
				{
					task.RunTask();
				}
			}
			finally
			{
				AssertEquals("No Errors", false, ErrorReporter.LastMessageReported.Contains("Direct access to Env.CurrentBranch is not allowed from service tasks"));
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();

			TestHelper.InsertBanks();

			TestHelper.UpdateChargeAccounts(); // TODO This part is slow. Need to Improve...

			TestHelper.SetUpPeriods();

			TestHelper.SetControlAccounts();

			TestHelper.Branch1 = GlbBranch.CurrentBranch.PK.ToGuid();
			TestHelper.Department1 = GlbDepartment.CurrentDepartment.PK.ToGuid();
			TestHelper.PostDate1 = ZDateTime.Now;
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		BatchTestHelper TestHelper
		{
			get { return testHelper ?? (testHelper = new BatchTestHelper(Factory)); }
		}
		BatchTestHelper testHelper;
	}
}
