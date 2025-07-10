using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DVDQuerySendMessageWrapper : DVDCommonSendMessageWrapper, IQueryDVDMessageDataProvider
	{
		public DVDQuerySendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			RequestATCData = false;
		}

		public ZBool RequestATCData { get; }
	}
}
