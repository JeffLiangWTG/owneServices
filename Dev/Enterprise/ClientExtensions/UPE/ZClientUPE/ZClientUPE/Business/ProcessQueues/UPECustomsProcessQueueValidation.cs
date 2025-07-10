using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public abstract class UPECustomsProcessQueueValidation : ProcessQueueValidation
	{
		public UPECustomsProcessQueueValidation(UPEProcessQueue processQueue)
			: base(processQueue)
		{
		}

		protected override void CheckP4_CustomsQueue()
		{
			base.CheckP4_CustomsQueue();
			CustomsQueueValidationHelper.ValidateQueueName();
		}

		protected override void CheckP4_CustomsStatus()
		{
			base.CheckP4_CustomsStatus();
			CustomsQueueValidationHelper.ValidateReasonCode();
		}

		protected override void CheckP4_CustomsSubStatus()
		{
			base.CheckP4_CustomsSubStatus();
			CustomsQueueValidationHelper.ValidateStatusCode();
		}

		protected override void CheckP4_GS_NKCustomsTaskAssignedTo()
		{
			base.CheckP4_GS_NKCustomsTaskAssignedTo();
			CustomsQueueValidationHelper.ValidateTaskAssignedTo();
		}

		internal UPECustomsQueueValidationHelper CustomsQueueValidationHelper
		{
			get
			{
				if (fCustomsQueueValidationHelper == null)
				{
					fCustomsQueueValidationHelper = GetNewCustomsQueueValidationHelper();
				}
				return fCustomsQueueValidationHelper;
			}
		}

		protected abstract UPECustomsQueueValidationHelper GetNewCustomsQueueValidationHelper();
		UPECustomsQueueValidationHelper fCustomsQueueValidationHelper;
	}
}
