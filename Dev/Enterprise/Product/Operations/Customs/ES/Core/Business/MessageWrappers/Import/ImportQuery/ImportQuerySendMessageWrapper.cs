using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportQuerySendMessageWrapper : ImportCommonSendMessageWrapper, IDUAImportQueryMessageDataProvider
	{
		public ImportQuerySendMessageWrapper(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			RequestATCData = ZString.Empty;
		}

		public ZString RequestATCData { get; }
	}
}
