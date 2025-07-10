
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPEProcessQueueValidationHelper : UPEProcessQueueHelperBase
	{
		public UPEProcessQueueValidationHelper(UPEProcessQueue queue, ProcessQueueType.Enum queueType)
			: base(queue, queueType)
		{
		}

		public UPEProcessQueueValidationHelper(NonPersistentProcessQueue queue)
			: base(queue)
		{
		}

		virtual
 public void ValidateQueueName()
		{
			ValidateQueueNameCore();
		}

		protected virtual void ValidateQueueNameCore()
		{
			if (!AllowEmptyQueueNameAndStatuses)
			{
				MandatoryValidation.CheckEntered(Queue.QueueNameInfo);
			}
			ListValidation.ErrorIfInvalidCode(Queue.QueueNameInfo, Queue.Lookups.QueueList);

			ValidateQueueMovement();
		}

		void ValidateQueueMovement()
		{
			ZString proposedQueueName = Queue.QueueName;
			ZString originalQueueName = (ZString)Queue.QueueNameInfo.OriginalValue;

			if (originalQueueName != proposedQueueName &&
				!GlbStaff.CurrentUser.GS_IsController &&
				!CanMoveToQueue(originalQueueName, proposedQueueName))
			{
				Queue.QueueNameInfo.AddError("Only an administrator can manually move a shipment from queue '" + originalQueueName + "' to '" + proposedQueueName + "'");
			}
		}

		protected virtual bool CanMoveToQueue(ZString originalQueueName, ZString proposedQueueName)
		{
			return true;
		}

		virtual
 public void ValidateReasonCode()
		{
			ValidateReasonCodeCore();
		}

		protected virtual void ValidateReasonCodeCore()
		{
			if (RequiresReasonCode)
			{
				MandatoryValidation.CheckEntered(Queue.StatusInfo);
			}
			ListValidation.ErrorIfInvalidCode(Queue.StatusInfo, Queue.Lookups.StatusList);
		}

		virtual
 public void ValidateStatusCode()
		{
			if (RequiresStatusCode)
			{
				MandatoryValidation.CheckEntered(Queue.SubStatusInfo);
			}
			ListValidation.ErrorIfInvalidCode(Queue.SubStatusInfo, Queue.Lookups.SubStatusList);
		}

		virtual
 public void ValidateTaskAssignedTo()
		{
			ListValidation.ErrorIfInvalidCode(Queue.AssignedToInfo, Queue.Lookups.TaskAssignedToList);
		}

		public virtual void ValidateRemarks()
		{
		}

		protected virtual bool AllowEmptyQueueNameAndStatuses
		{
			get { return false; }
		}

		protected abstract bool RequiresReasonCode { get; }
		protected bool RequiresStatusCode
		{
			get { return Queue.HasSubStatuses; }
		}

		public void ValidateP4_CustomAttrib8()
		{
			ValidateP4_CustomAttrib8Core();
		}

		protected virtual void ValidateP4_CustomAttrib8Core()
		{
		}
	}
}
