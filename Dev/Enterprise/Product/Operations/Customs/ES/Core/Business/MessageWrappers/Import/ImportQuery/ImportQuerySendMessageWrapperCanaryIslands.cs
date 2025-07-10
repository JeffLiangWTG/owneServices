using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class ImportQuerySendMessageWrapperCanaryIslands : ImportCommonSendMessageWrapper, IDUAImportQueryMessageDataProvider
	{
		public ImportQuerySendMessageWrapperCanaryIslands(CusEntryHeader cusEntryHeader, ICertificateProvider certificateData) : base(cusEntryHeader, certificateData)
		{
			RequestATCData = RequestATCString;
		}
		const string RequestATCString = "S";

		public ZString RequestATCData { get; }
	}
}
