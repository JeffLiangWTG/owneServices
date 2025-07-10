
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class UPECargoReportQueueValidationHelper : UPECustomsQueueValidationHelper
	{
		public UPECargoReportQueueValidationHelper(UPECargoReportQueue queue)
			: base(queue)
		{
		}

		public UPECargoReportQueueValidationHelper(NonPersistentCargoReportQueue queue)
			: base(queue)
		{
		}

		protected override bool RequiresReasonCode
		{
			get
			{
				ZString queueName = Queue.QueueName;
				return queueName == CargoReportQueueCodeDescriptionPairList.Codes.Hold ||
						queueName == CargoReportQueueCodeDescriptionPairList.Codes.EIR ||
						queueName == CargoReportQueueCodeDescriptionPairList.Codes.AwaitingEvaluation ||
						queueName == CargoReportQueueCodeDescriptionPairList.Codes.Intervention ||
						queueName == CargoReportQueueCodeDescriptionPairList.Codes.Quarantine;
			}
		}
	}
}
