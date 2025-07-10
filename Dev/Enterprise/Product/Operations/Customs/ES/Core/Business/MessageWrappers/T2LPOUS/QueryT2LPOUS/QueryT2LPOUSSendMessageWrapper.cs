using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business
{
	public class QueryT2LPOUSSendMessageWrapper : EntryHeaderCommonSendMessageWrapper, IQueryT2LMessageDataProvider
	{
		public QueryT2LPOUSSendMessageWrapper(CusEntryHeader entryHeader, ICertificateProvider certificate) : base(entryHeader, certificate)
		{
		}

		public ZString MRN => entryHeader.MovementReferenceNumber;
	}
}
