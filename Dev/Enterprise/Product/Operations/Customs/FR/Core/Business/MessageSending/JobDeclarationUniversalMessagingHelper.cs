using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class JobDeclarationUniversalMessagingHelper : DataTransfer.Universal.JobDeclarationUniversalMessagingHelper
	{
		public JobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent messageSendingObject)
			: base(messageSendingObject)
		{
		}

		protected override ZString UniversalCustomsMessagingRecipientID => "FRCustoms";
	}
}
