using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class ScheduleB3MessageSupporterTest : TestCaseWithFactory
	{
		public void TestCreateStmProcessQueueProcessor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supporter = new ScheduleB3MessageSupporter(declaration);
			var autoSendingMessageSupporter = supporter as Integration.Customs.IBaseAutoSendingMessageSupporter;
			AssertNotNull(autoSendingMessageSupporter);
			AssertEquals(declaration.Branch.PK, autoSendingMessageSupporter.RegistryBranchPK);

			var processor = autoSendingMessageSupporter.CreateStmProcessQueueProcessor(null, WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message)
				as Customs.Business.BatchProcessor.CustomsStmProcessQueueCreatorProcessor;
			AssertNotNull(processor);
			AssertEquals(declaration.PK, processor.BizObj.PK);
			AssertEquals(CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, processor.ApplicationCode);
			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, processor.TriggerActionCode);
		}

		public void TestScheduleB3MessageSupporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var supporter = new ScheduleB3MessageSupporter(declaration);
			Assert("SupportScheduleB3Message", supporter.SupportScheduleB3Message);
			AssertEquals("NotSupportScheduleB3MessageReason", "", supporter.NotSupportScheduleB3MessageReason);
			AssertType<ScheduleCADMessageProcessor>("CreateScheduleB3MessageProcessor", supporter.CreateScheduleB3MessageProcessor());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			supporter = new ScheduleB3MessageSupporter(declaration);
			Assert("SupportScheduleB3Message", !supporter.SupportScheduleB3Message);
			AssertEquals("NotSupportScheduleB3MessageReason", "Trigger action Schedule CAD Message is valid only for IMP or LVS declaration.", supporter.NotSupportScheduleB3MessageReason);
		}
	}
}
