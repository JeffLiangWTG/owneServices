using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice
{
	[TestedType(typeof(ExportDeclarationInvoiceServiceTask))]
	public class ExportDeclarationInvoiceServiceTaskTest : ServiceTaskTestCase<ExportDeclarationInvoiceServiceTask>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRunTask()
		{
			var lastRunTime = WowDataRegistry.Instance.DeclarationInvoiceExportLastRun;
			RunTaskSchedule(ServiceTask);
			AssertNotEquals("Registry-LastRun changed", lastRunTime, WowDataRegistry.Instance.DeclarationInvoiceExportLastRun);
			AssertEquals("Export Directory exists", true, Directory.Exists(BaseSourcePath));
			AssertEquals("Export Directory Not Empty", true, (Directory.GetFiles(BaseSourcePath).Length > 0));
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
		#region Implementation
		ExportDeclarationInvoiceServiceTask ServiceTask = new ExportDeclarationInvoiceServiceTask();
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Baseline")]
		protected override void SetUpCore()
		{
			base.SetUpCore();
			WowDataRegistry.Instance.DeclarationInvoiceExportDirectory = BaseSourcePath;
			ServiceTask = new ExportDeclarationInvoiceServiceTask();
			ServiceTask.ServiceLogger = new TestServiceLogger();
			InitialiseTaskSchedule(ServiceTask);
		}
		#endregion
	}
}
