using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.DataTransfer.CreditControlledDocumentApproval
{
	[Serializable]
	public class CreditControlledDocumentApprovalLogSubscriber : LogSubscriber
	{
		public override string Name => "CreditApprovallogSubscriber";
		public override string[] EventTypes => new string[] { Events.CreditApprovalRequested.Code };
		public override string[] TableNames => new string[] { GenApprovalRequestSchema.Constants.TableName };

		protected override ILogBatcher GetLogBatcher() => new LogBatcher();

		class LogBatcher : LogBatcher<ZGuid?>
		{
			protected override void AddGroupingFetchHints(IEnumerable<IQueuedLog> enumberable) { }
			protected override ZGuid? GetGroupLogKey(IQueuedLog log) => GetApprovalRequestBranch(log);
			protected override LogsGroupContext SetContextForLogsGroup(ZGuid? groupKey, IEnumerable<IQueuedLog> queuedLogs)
			{
				var branchPk = GetApprovalRequestBranch(queuedLogs.FirstOrDefault());
				if (branchPk.HasValue)
				{
					return new LogsGroupContext(false, new TemporaryUserContext() { BranchPK = branchPk.Value.ToGuid(), DepartmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid() }.Set());
				}
				return new LogsGroupContext(false);
			}

			static ZGuid? GetApprovalRequestBranch(IQueuedLog log) => log.Factory.Load<CreditControlledDocumentsApproval>(log.SJ_ParentID)?.XP_GB_RequestingBranch;
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			if (AccountingMasterFilesRegistry.Instance.CreditControlApprovalMode.Value == AccountingMasterFilesConstants.CreditControlApprovalModes.ApproveLocallyInCW1.Code)
			{
				return;
			}

			foreach (var queuedLog in queuedLogs)
			{
				var approvalRequest = queuedLog.Factory.Load<CreditControlledDocumentsApproval>(queuedLog.SJ_ParentID);

				if (approvalRequest != null
					&& approvalRequest.XP_SubSystem == Core.Constants.GenApprovalRequestSubSystem.Accounting
						&& approvalRequest.XP_ApprovalType == Core.Constants.GenApprovalRequestApprovalType.ARCreditControlledDocuments
							&& (approvalRequest.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Requested || approvalRequest.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Cancelled))
				{
					var parentObject = approvalRequest.ParentBusinessObject;
					if (parentObject != null)
					{
						var workflowProvider = (IWorkflowProvider)parentObject;
						var workflowDescriptor = workflowProvider != null && workflowProvider.WorkflowType != string.Empty ? WorkflowDescriptors.Instance.TryGetValueSafe(workflowProvider.WorkflowType) : null;
						if (workflowDescriptor != null)
						{
							var notificationBuffer = new NotificationBuffer();
							var eventInfo = new CreditApprovalEventInfo(approvalRequest);
							var action = new ActionInfo(RecipientRoleType.CCA, approvalRequest);
							action.TriggerEventCode = Events.CreditApprovalRequested.Code;
							Func<IDataWritingManager, ITopLevelDataObjectWriter> dataWriterGetter = (outboundSessionTracker) => workflowDescriptor.GetUniversalShipmentDataObjectWriter(outboundSessionTracker);
							var processor = UniversalXmlWorkflowProcessorBuilder.New(
								action,
								new OrgProxyCommunicationModeProvider(EDICommunicationsMode.Modules.CreditControlledDocumentApproval, EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment),
								dataWriterGetter,
								parentObject,
								eventInfo,
								null,
								null);

							var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
							processor.Process(notificationBuffer, replaceThisTokenEventuallyQuestionMarkExclamationMark);
						}
					}
				}
			}
		}
	}
}
