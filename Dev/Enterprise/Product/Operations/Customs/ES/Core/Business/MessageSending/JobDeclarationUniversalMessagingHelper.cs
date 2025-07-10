using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Business
{
	public class JobDeclarationUniversalMessagingHelper : DataTransfer.Universal.JobDeclarationUniversalMessagingHelper
	{
		public JobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent messageSendingObject)
			: base(messageSendingObject)
		{
		}

		protected override ZString UniversalCustomsMessagingRecipientID => "ESCustoms";
	}
}
