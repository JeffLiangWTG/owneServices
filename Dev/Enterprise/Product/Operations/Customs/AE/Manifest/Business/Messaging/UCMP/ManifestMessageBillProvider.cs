using Enterprise.Customs.AE.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class ManifestMessageBillProvider : IMessageAttacheeProvider<ICUSRESDataProvider>
{
	public IMessageAttachee GetAttachee(EDIMessage message, ICUSRESDataProvider dataProvider)
	{
		return message.Factory.GetMessageAttachee<AsycudaBill>(AsycudaBillSchema.ABL_SenderReference, dataProvider.OutgoingAccessReference);
	}
}
