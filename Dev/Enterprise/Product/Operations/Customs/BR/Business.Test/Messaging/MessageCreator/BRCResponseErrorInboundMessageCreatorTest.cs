using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCResponseErrorInboundMessageCreatorTest : BRCInboundMessageCreatorAbstractTest
	{
		protected override ZString InterchangeType => MessageTypeList.Codes.XER;

		protected override ZString TransportType => EDIInterchange.TransportType.eAdaptor;

		public void TestGenerateMessageFromInterchange()
		{
			var responseMessage = UniversalEventTestDataHelper.CreateUniversalInterchangeXml(Events.InterchangeRejectedCode, MessageTypeList.Codes.XER, responseType: "Unauthorized", reason: "ErrorMessage");
			var expectedMessage = UniversalEventTestDataHelper.CreateUniversalEventXml(Events.InterchangeRejectedCode, MessageTypeList.Codes.XER, responseType: "Unauthorized", reason: "ErrorMessage");
			AssertCreateMessageFromInterchange(responseMessage, expectedMessage, MessageTypeList.Codes.XER, "XXX");
		}
	}
}
