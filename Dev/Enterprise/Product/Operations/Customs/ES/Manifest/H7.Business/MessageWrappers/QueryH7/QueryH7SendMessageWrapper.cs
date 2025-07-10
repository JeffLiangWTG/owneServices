using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class QueryH7SendMessageWrapper : H7CommonSendMessageWrapper, IQueryH7MessageDataProvider
	{
		public QueryH7SendMessageWrapper(AsycudaBill bill, ICertificateProvider certificate)
			: base(bill, certificate)
		{
		}

		public ZString DeclarationMRN => Bill.H7MovementReferenceNumber;

		public ZString G3DeclarationMRN => ZString.Empty;

		public ZString NextH7DeclarationMRN => ZString.Empty;
	}
}
