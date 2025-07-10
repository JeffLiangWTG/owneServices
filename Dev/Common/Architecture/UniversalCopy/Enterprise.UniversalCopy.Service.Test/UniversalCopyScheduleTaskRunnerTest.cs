using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalCopy.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.UniversalCopy.Service.Testing
{
	[TestedType(typeof(UniversalCopyScheduleTaskRunner))]
	[UseSnapshotProtection(skipTransaction: true)]
	class UniversalCopyScheduleTaskRunnerTest : ServiceTaskTestCase<UniversalCopyScheduleTaskRunner>
	{
		public void TestHostedServiceAttribute()
		{
			var hostedServiceAttributes = GetHostedServiceAttributes();
			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "SUC", hostedServiceAttribute.Code);
				AssertEquals("Description", "Scheduled Universal Copy Task Runner", hostedServiceAttribute.Description);
				AssertEquals("Category", "SYS", hostedServiceAttribute.Category);
				AssertEquals("IsMandatory", true, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "5minutes", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
			});
		}

		public void TestUniversalCopyScheduleIsRun()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "Test";
			var task = Factory.New<StmUniversalCopyScheduleTask>();
			task.Parent.CopyObject = dummy;
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "DUM_UC";
			var glowInterface = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true);
			copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(glowInterface, typeof(DummyBusinessObject), BusinessObjectCopyManager.CopyTreeConfiguration), copyTemplate);
			var entityNode = copyTemplate.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			var descriptionNode = entityNode.Nodes.Find(node => node.Name == "Z0_Description") as PropertyCopyTemplateNode;
			descriptionNode.CopyMethod = CopyMethod.Copy;
			copyTemplate.PrepareForSave();
			task.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
			task.S5_TaskPeriod = "D";
			task.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromMinutes(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();

			using (EnvProxy.Instance.TemporaryServiceTaskContext("SUC", canRunInAnyBranch: true))
			{
				new UniversalCopyScheduleTaskRunner() { ServiceLogger = serviceLogger }.RunTask();
			}

			AssertEquals("Should not have any errors reported", 0, ExceptionReporterTestListener.Instance.Count);

			var allDummies = Factory.Load<DummyBusinessObject>(new ZQuery());
			AssertEquals("Expected a new dummy. Service log:\r\n" + serviceLogger.ToString(), 2, allDummies.Length);
			AssertNotEquals(allDummies[0].PK, allDummies[1].PK);
			AssertEquals(allDummies[0].Z0_Description, allDummies[1].Z0_Description);

			task.Reload();
			Assert("task.S5_NextScheduledPrintRunTimeUtc should be updated, it should be later than " + ZDateTime.UtcNow + " but is " + task.S5_NextScheduledPrintRunTimeUtc,
				task.S5_NextScheduledPrintRunTimeUtc > ZDateTime.UtcNow);
		}

		public void TestCopiedObjectWithValidationErrorsIsNotSaved()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "Bad";
			var task = Factory.New<StmUniversalCopyScheduleTask>();
			task.Parent.CopyObject = dummy;
			var copyTemplate = Factory.New<UniversalCopyTemplate>();
			copyTemplate.S9_ModuleID = "DUM_UC";
			var glowInterface = GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true);
			copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(glowInterface, typeof(DummyBusinessObject), BusinessObjectCopyManager.CopyTreeConfiguration), copyTemplate);
			var entityNode = copyTemplate.CopyTemplateTree.CopyTemplateNode.InnerNode as EntityCopyTemplateNode;
			var descriptionNode = entityNode.Nodes.Find(node => node.Name == "Z0_Description") as PropertyCopyTemplateNode;
			descriptionNode.CopyMethod = CopyMethod.Copy;
			copyTemplate.PrepareForSave();
			task.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
			task.S5_TaskPeriod = "D";
			task.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.Add(TimeSpan.FromMinutes(-1));
			Factory.Save();

			var serviceLogger = new TestServiceLogger();
			new UniversalCopyScheduleTaskRunner() { ServiceLogger = serviceLogger }.RunTask();

			var allDummies = Factory.Load<DummyBusinessObject>(new ZQuery());
			AssertEquals("Expected no new dummies. Service log:\r\n" + serviceLogger.ToString(), 1, allDummies.Length);
			AssertContains("Error - Z0_Description: Bad!", serviceLogger.ToString());
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestDescriptionForLogEnvironmentAccessNoError()
		{
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_Code = "YYY";
			newBranch.GB_GC = EnvProxy.Instance.CurrentCompany.PK;
			newBranch.GB_IsActive = true;

			Factory.Save();

			var dummy = Factory.New<DummyWithCurrentBranchAccess>();
			dummy.Z0_Description = "Test";

			var task = Factory.New<StmUniversalCopyScheduleTask>();
			task.Parent.CopyObject = dummy;

			task.S5_GB = newBranch.PK;

			using (EnvProxy.Instance.TemporaryServiceTaskContext("SUC", canRunInAnyBranch: true))
			{
				var taskDescription = task.DescriptionForLog;
				AssertContains("Dummy in Branch YYY", taskDescription);
			}

			AssertEquals("Should not have any errors reported", 0, ExceptionReporterTestListener.Instance.Count);
		}

		class DummyWithCurrentBranchAccess : DummyBusinessObject
		{
			public DummyWithCurrentBranchAccess(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ZString HumanReadableNameCore => "Dummy in Branch " + EnvProxy.Instance.CurrentBranch?.Code;
		}
	}
}
