using CargoWise.Common;

namespace Enterprise.BufferManagement.Business
{
	public class QueueMemberOperationResult : TagOperationResult
	{
		internal static QueueMemberOperationResult Success(WorkQueueMembershipLink link)
		{
			Argument.NotNull(link, "link");

			return new QueueMemberOperationResult { WasSuccessful = true, Link = link };
		}

		internal static new QueueMemberOperationResult NoOperation(string message)
		{
			return new QueueMemberOperationResult { WasSuccessful = true, Message = message };
		}

		internal static new QueueMemberOperationResult Failed(string message)
		{
			return new QueueMemberOperationResult { WasSuccessful = false, Message = message };
		}

		QueueMemberOperationResult()
		{
		}

		public new WorkQueueMembershipLink Link
		{
			get { return (WorkQueueMembershipLink)base.Link; }
			set { base.Link = value; }
		}
	}
}
