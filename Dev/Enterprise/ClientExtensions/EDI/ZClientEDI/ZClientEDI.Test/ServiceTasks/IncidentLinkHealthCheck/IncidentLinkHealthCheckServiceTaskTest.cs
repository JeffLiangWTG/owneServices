using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(IncidentLinkHealthCheckServiceTask))]
	class IncidentLinkHealthCheckServiceTaskTest : ServiceTaskTestCase<IncidentLinkHealthCheckServiceTask>
	{
		[ExpectNoExceptions]
		public void TestServiceTask()
		{
			AssertEquals("IHC", IncidentLinkHealthCheckServiceTask.Code);
		}

		public void TestDefinition()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		[ExpectNoExceptions]
		public void IncidentLinkHealthCheckServiceTaskEndToEnd()
		{
			using (var snapshot = SnapshotCreator.CreateSnapshot(Db.NewAdminConnection(), () => Db.Connection.CloseConnection()))
			{
				// Arrange
				var task = new IncidentLinkHealthCheckServiceTask();
				//Act
				// Assert
				task.RunTask(new CancellationToken());
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
