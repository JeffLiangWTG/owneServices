using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class WorkflowGunTest : TestCaseWithFactory
	{
		public void TestFireWorkflow_CheckAdditionaInformation()
		{
			var dummyLogParent = Factory.New<DummyWithLogs>();
			dummyLogParent.ProcessingLogs += (s, e) =>
			{
				ErrorReporter.ReportOnceWithAdditionalInfo("", "", StmALog.ErrorReportKeys.Category.WorkflowGun);
			};
			var log = dummyLogParent.Logs.AddNew(Events.CustomisableEvent00);
			new WorkflowGun(log, FireWorkflowMode.UpdateAll).TriggerWorkflow(dummyLogParent);
			AssertContains($"master.LogsParentTableName:\r\nDummyBizo\r\nmaster.LogsParentPK:\r\n{log.SL_Parent}\r\nmaster.TableName:\r\nDummyBizo", ErrorReporter.LastMessageReported);
			ErrorReporter.Instance.Clear();
		}
	}
}
