using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class TagChannelReassignmentStrategy : TicketReassignmentStrategy
	{
		internal TagChannelReassignmentStrategy(TicketReassignmentParameters parameters)
			: base(parameters)
		{
		}

		protected override bool CanReassign()
		{
			return false;
		}

		protected override string GetReassignmentMessage()
		{
			throw new NotSupportedException("Tag channels do not support drag/drop");
		}

		protected override void PerformReassignment(ProcessTask task)
		{
			throw new NotSupportedException("Tag channels do not support drag/drop");
		}

		protected override bool ShouldReturnTicketToOriginalLocation()
		{
			return true;
		}

		protected override CrossChannelTaskAssignments GetAllowedReassignmentOptionFlags()
		{
			return CrossChannelTaskAssignments.None;
		}
	}
}
