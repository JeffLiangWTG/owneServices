
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPECommercialQueueValidationHelper : UPEProcessQueueValidationHelper
	{
		public UPECommercialQueueValidationHelper(UPECalloutQueue queue)
			: base(queue, ProcessQueueType.Enum.Commercial)
		{
			Callout = queue.Parent as Callout;
		}

		public UPECommercialQueueValidationHelper(NonPersistentCalloutQueue queue)
			: base(queue)
		{
		}

		protected override void ValidateQueueNameCore()
		{
			base.ValidateQueueNameCore();
			if (UPECalloutQueue != null)
			{
				if (Queue.QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill && Queue.Status == ReasonCodeDescriptionPairList.CRBL.Codes._R0_Rebill)
				{
					if (UPECalloutQueue.AccountNumberAccountClass == "7" || UPECalloutQueue.AccountNumberAccountClass == "8" || UPECalloutQueue.AccountNumberAccountClass == "9" || UPECalloutQueue.AccountNumberAccountClass == "13")
					{
						Queue.QueueNameInfo.AddError("Rebill Queue and Status Shipments with Debtor Group 7, 8, 9, 13 should be moved to A/R queue.");
					}
				}
			}
		}

		protected override void ValidateP4_CustomAttrib8Core()
		{
			base.ValidateP4_CustomAttrib8Core();
			if (!Queue.P4_CustomAttrib8.IsEmpty && !IsValidAccountNumber(Queue.P4_CustomAttrib8))
			{
				Queue.P4_CustomAttrib8Info.AddError("This account number is not valid");
			}
			if (Queue.QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill && Queue.Status == ReasonCodeDescriptionPairList.CRBL.Codes._R0_Rebill)
			{
				if (Queue.P4_CustomAttrib8.IsEmpty)
				{
					Queue.P4_CustomAttrib8Info.AddError("If a shipment is moved to Rebill - Account Number must be entered.'");
				}
			}
		}

		protected override bool RequiresReasonCode
		{
			get
			{
				ZString queueName = Queue.QueueName;
				return
					queueName == CommercialQueueCodeDescriptionPairList.Codes.Hold ||
					queueName == CommercialQueueCodeDescriptionPairList.Codes.EIR;
			}
		}

		protected override bool CanMoveToQueue(ZString originalQueueName, ZString proposedQueueName)
		{
			bool result = Callout == null || Callout.Payment.PaymentMethod != UPECargoPaymentMethod.None;
			if (!result)
			{
				switch (originalQueueName)
				{
					case CommercialQueueCodeDescriptionPairList.Codes.Hold:
						result =
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.EIR ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Finance ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.AR;
						break;
					case CommercialQueueCodeDescriptionPairList.Codes.EIR:
						result =
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Hold ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Finance ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill;
						break;
					case CommercialQueueCodeDescriptionPairList.Codes.Finance:
						result =
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Hold ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.EIR ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill ||
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.AR;
						break;
					case CommercialQueueCodeDescriptionPairList.Codes.AR:
						result =
							proposedQueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill;
						break;
				}
			}
			return result;
		}

		bool IsValidAccountNumber(ZString accountNumber)
		{
			OrgHeader account = OrgHeader.FindByOrgCusCode(Factory, UPEOrgCusCode.CodeTypes.UPSCustomerAccountNumber, Queue.P4_CustomAttrib8);
			return account != null;
		}

		UPECalloutQueue UPECalloutQueue
		{
			get
			{
				UPECalloutQueue result = null;
				UPEActiveProcessQueue uPEActiveProcessQueue = Queue as UPEActiveProcessQueue;
				if (uPEActiveProcessQueue != null)
				{
					result = uPEActiveProcessQueue.ProcessQueue as UPECalloutQueue;
				}
				return result;
			}
		}

		readonly Callout Callout;
	}
}
