using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class NoOperationTicketReassignmentStrategy : TicketReassignmentStrategy
	{
		internal NoOperationTicketReassignmentStrategy(TicketReassignmentParameters parameters)
			: base(parameters)
		{
		}

		protected override bool CanReassign()
		{
			return false;
		}

		protected override CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags()
		{
			return CrossChannelTaskAssignments.None;
		}

		protected override string GetReassignmentMessage()
		{
			throw new NotSupportedException();
		}

		protected override void PerformReassignment(ProcessTask task)
		{
			throw new NotSupportedException();
		}

		protected override bool ShouldReturnTicketToOriginalLocation()
		{
			return true;
		}
	}
}
