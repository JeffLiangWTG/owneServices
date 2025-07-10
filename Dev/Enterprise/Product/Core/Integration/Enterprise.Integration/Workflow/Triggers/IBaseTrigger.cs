using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface IBaseTrigger : IWorkflowItem, ITriggerConditions, ITriggerUserContextConditions
	{
		IActiveBusinessObjectCollection TriggerActions { get; }

		ITriggerConditions TriggerConditions_ForBinding { get; }

		ZDateTime ActualDate { get; }

		ZBool ShouldTriggerOnEstimateEvents { get; }

		ZBool SuppressDuplicates { get; }

		ZInt DelayDurationSeconds { get; }

		bool AreTriggerConditionsMet(IStmALog @event, IBusiness job);

		void SetEventTime(IStmALog @event, IBusiness job, ZDateTimeOffset eventTime);

		void SetEstimateTime(IStmALog @event, ZDateTimeOffset estimateTime);

		void SetEventTimeWithoutFiringWorkflow(ZDateTimeOffset eventTime);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
		void Fire(IBusiness workflowParent, IStmALog @event);

		void Withdraw(IBusiness workflowParent, IStmALog @event);

		#region For Tests
#if DEBUG

		void SetShouldTriggerOnEstimateEvents_ForTests(bool value);

#endif
		#endregion
	}
}
