
using CargoWise.Types;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDeclarationQueueValidationHelper : UPECustomsQueueValidationHelper
	{
		public UPEDeclarationQueueValidationHelper(UPEDeclarationQueue queue)
			: base(queue)
		{
		}

		public UPEDeclarationQueueValidationHelper(NonPersistentDeclarationQueue queue)
			: base(queue)
		{
		}

		protected override bool RequiresReasonCode
		{
			get
			{
				ZString queueName = Queue.QueueName;
				return
					queueName == DeclarationQueueCodeDescriptionPairList.Codes.BCA ||
										queueName == DeclarationQueueCodeDescriptionPairList.Codes.BCO ||
					queueName == DeclarationQueueCodeDescriptionPairList.Codes.EIR ||
					queueName == DeclarationQueueCodeDescriptionPairList.Codes.Classification ||
					queueName == DeclarationQueueCodeDescriptionPairList.Codes.Lodgement ||
					queueName == DeclarationQueueCodeDescriptionPairList.Codes.Submitted;
			}
		}
	}
}
