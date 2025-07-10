using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportWorkflowDescriptor : WorkflowDescriptor
	{
		public override string Code => WorkflowDescriptors.CusExitReportWorkflowDescriptorCode;

		public override IMultilingualString Description => ResString.GetMultilingualString("72EE6AB0-1C78-4C94-9766-5BB8A211CC83", "Exit Report Line Trigger");

		public override Type WorkflowProviderType => typeof(CusExitReport);

		public override ControllerID ControllerID => null;

		public override BusinessContext[] DocumentBusinessContext => new[] { BusinessContext.INVALID };

		public override MessageRecipientPartyType SupportedMessageRecipientParties(IBaseTrigger trigger, IBusiness business) => MessageRecipientPartyType.Email;

		public override bool SupportsWorkflowTemplates => false;

		public override bool SupportsEventTracking => true;

		public override bool SupportsBufferManagement => false;

		public override bool AreTasksCompanySpecific => false;

		public override bool SupportsUniversalTemplates => false;

		public override bool RequiresBranch => false;

		public override bool RequiresDepartment => false;

		public override bool RequiresClient => false;

		protected override IProcessor GetWorkflowTriggerActionCore(WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			var result = base.GetWorkflowTriggerActionCore(source, queuedLog);
			if (result != null)
			{
				return result;
			}

			var action = source.Action;
			var triggerType = action.PQ_TriggerType;
			if (triggerType == WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage)
			{
				if (source.Job is CusExitReport exitReport && exitReport is EUExitControl.ISupportAutoSendExitReportTransferMessage supporter)
				{
					return supporter.CreateStmProcessQueueProcessor(exitReport, triggerType);
				}
			}
			return null;
		}
	}
}
