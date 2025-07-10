using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DVDQuerySendMessageWrapperCanaryIslands : DVDCommonSendMessageWrapper, IQueryDVDMessageDataProvider
	{
		public DVDQuerySendMessageWrapperCanaryIslands(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			RequestATCData = true;
		}

		public ZBool RequestATCData { get; }
	}
}
