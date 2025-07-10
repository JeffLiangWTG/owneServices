using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave
{
	public class TriggerActionWithOptionalFactorySaveProcessor : IProcessor
	{
		public TriggerActionWithOptionalFactorySaveProcessor(ProcessTaskNotification triggerAction, BusinessObject parent, IQueuedLog queuedLog, GlbStaff userContext = null)
		{
			TriggerAction = Argument.NotNull(triggerAction, nameof(triggerAction));
			QueuedLog = Argument.NotNull(queuedLog, nameof(queuedLog));
			Parent = Argument.NotNull(parent, nameof(parent));
			UserContext = userContext;
		}

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			var provider = QueuedLogReferenceProviderFactory.GetReferenceProvider(TriggerAction.PQ_TriggerType);
			var referenceMap = provider.CreateReferenceMap(TriggerAction, QueuedLog, Parent, UserContext);

			var reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(ZString.Empty, referenceMap);
			var staffCode = UserContext?.GS_Code ?? GlbStaff.CurrentUser.GS_Code;
			var branchCode = UserContext?.HomeBranch?.GB_Code ?? GlbBranch.CurrentBranch.GB_Code;
			var departmentCode = UserContext?.HomeDepartment?.GE_Code ?? GlbDepartment.CurrentDepartment.GE_Code;
			NewsTransmitter.CreateStmJobQueue(QueuedLog.Factory, QueuedLog, new TriggerActionWithOptionalFactorySaveLogSubscriber(), Events.MiscellaneousEventCode, staffCode, branchCode, departmentCode, reference); //We want to create JobQueue exactly for current user context in with trigger will be processed. This will allow to use exactly the same user context to process Job Queue record in dedicated subscriber and so keep the same environment for moved trigger action.
		}

		ProcessTaskNotification TriggerAction { get; }
		IQueuedLog QueuedLog { get; }
		BusinessObject Parent { get; }
		GlbStaff UserContext { get; }
	}
}
