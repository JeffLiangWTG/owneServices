using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CancelH7SendMessageWrapper : H7CommonSendMessageWrapper, ICancelH7MessageDataProvider
	{
		public CancelH7SendMessageWrapper(AsycudaBill bill, ICertificateProvider certificate) : base(bill, certificate)
		{
		}

		public ZString DeclarationMRN => Bill.MovementReferenceNumber;
	}
}
