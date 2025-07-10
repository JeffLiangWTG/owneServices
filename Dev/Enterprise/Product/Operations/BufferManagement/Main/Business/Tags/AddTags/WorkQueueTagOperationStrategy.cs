using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Business
{
	class WorkQueueTagOperationStrategy : ITagOperationStrategy
	{
		ITagOperationResult ITagOperationStrategy.AddTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog)
		{
			if (WorkQueueSecurity.CheckAddToQueueSecurity(magnitude, showSecurityDialog))
			{
				var queue = (WorkQueue)magnitude;
				var factory = queue.Factory;
				var processHeader = factory.Load<ProcessHeader>(tagable.PK);

				if (processHeader == null)
				{
					return QueueMemberOperationResult.Failed(TagOperationResult.ObjectNotInDatabase);
				}

				return queue.AddMember(processHeader);
			}
			else
			{
				return QueueMemberOperationResult.Failed(TagOperationResult.NoPermissionMessage);
			}
		}

		ITagOperationResult ITagOperationStrategy.RemoveTag(ITagable tagable, ITagMagnitude magnitude, bool showSecurityDialog)
		{
			if (WorkQueueSecurity.CheckRemoveFromQueueSecurity(magnitude, showSecurityDialog))
			{
				var queue = (WorkQueue)magnitude;
				var processHeader = (ProcessHeader)tagable;
				var factory = magnitude.Factory;
				var membershipLink = queue.GetMembershipLink(processHeader);
				if (membershipLink != null)
				{
					var membershipLinkInRightFactory = factory.Load<WorkQueueMembershipLink>(membershipLink.PK);
					if (membershipLinkInRightFactory == null)
					{
						return QueueMemberOperationResult.Failed(TagOperationResult.ObjectNotInDatabase);
					}
					membershipLinkInRightFactory.Delete();
					return QueueMemberOperationResult.Success(membershipLinkInRightFactory);
				}
				else
				{
					return QueueMemberOperationResult.NoOperation(Res.GetString("48205C6D-F3B7-4B53-8D92-0736C6137AF4", "Member was not in queue"));
				}
			}
			else
			{
				return QueueMemberOperationResult.Failed(TagOperationResult.NoPermissionMessage);
			}
		}
	}
}
