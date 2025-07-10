using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public sealed class NctsEdiMessagePrettier : NctsEdiMessageXmlPrettier
	{
		public NctsEdiMessagePrettier(EDIMessage message) : base(message)
		{ }

		protected override void FillSharedFields(NCTSPrettierSharedFields sharedFields)
		{
			base.FillSharedFields(sharedFields);
			sharedFields.MessageRecipient = dataProvider.MessageRecipient;
		}
	}
}
