using CargoWise.Common;

namespace Enterprise.BufferManagement.Business
{
	public enum BufferReleaseOutcome
	{
		Unknown = 0,
		Releasable,
		ReleasableByCCPMSchedule,
		ManuallyReleased,
		NonReleaseGateTransfer,
		BlockedByResourceCapacity,
		BlockedByMissingResource,
		BlockedByResourceLeave,
		BlockedByWorkflowEmptiness,
	}

	public static class BufferReleaseOutcomeExtensions
	{
		public static bool IsReleasable(this BufferReleaseOutcome outcome)
		{
			return outcome.In(BufferReleaseOutcome.Releasable, BufferReleaseOutcome.ReleasableByCCPMSchedule);
		}
	}
}
