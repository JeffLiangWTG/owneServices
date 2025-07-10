
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECalloutQueueValidation : ProcessQueueValidation
	{
		public UPECalloutQueueValidation(UPECalloutQueue parent)
			: base(parent)
		{
		}

		protected override void CheckP4_QueueName()
		{
			base.CheckP4_QueueName();
			CommercialQueueValidationHelper.ValidateQueueName();
		}

		protected override void CheckP4_Status()
		{
			base.CheckP4_Status();
			CommercialQueueValidationHelper.ValidateReasonCode();
			ValidateP4_QueueName();
		}

		protected override void CheckP4_SubStatus()
		{
			base.CheckP4_SubStatus();
			CommercialQueueValidationHelper.ValidateStatusCode();
		}

		protected override void CheckP4_GS_NKTaskAssignedTo()
		{
			base.CheckP4_GS_NKTaskAssignedTo();
			CommercialQueueValidationHelper.ValidateTaskAssignedTo();
		}

		protected override void CheckP4_CustomAttrib8()
		{
			base.CheckP4_CustomAttrib8();
			CommercialQueueValidationHelper.ValidateP4_CustomAttrib8();
			ValidateP4_QueueName();
		}

		protected override void CheckP4_CustomAttrib6()
		{
			base.CheckP4_CustomAttrib6();
			MandatoryValidation.CheckEntered(Parent.P4_CustomAttrib6Info);
			ListValidation.ErrorIfInvalidCode(Parent.P4_CustomAttrib6Info, new DutyTypeCodeDescriptionPairList());
		}

		internal UPECommercialQueueValidationHelper CommercialQueueValidationHelper
		{
			get
			{
				if (fCommercialQueueValidationHelper == null)
				{
					fCommercialQueueValidationHelper = GetNewCommercialQueueValidationHelper();
				}
				return fCommercialQueueValidationHelper;
			}
		}

		protected virtual UPECommercialQueueValidationHelper GetNewCommercialQueueValidationHelper()
		{
			return new UPECommercialQueueValidationHelper(Parent);
		}

		protected new UPECalloutQueue Parent
		{
			get { return (UPECalloutQueue)base.Parent; }
		}

		UPECommercialQueueValidationHelper fCommercialQueueValidationHelper;
	}
}
