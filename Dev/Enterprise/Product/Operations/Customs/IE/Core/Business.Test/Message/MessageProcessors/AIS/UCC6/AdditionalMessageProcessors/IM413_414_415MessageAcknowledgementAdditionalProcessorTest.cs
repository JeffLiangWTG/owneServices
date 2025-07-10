using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	sealed class AISMessageAcknowledgementAdditionalProcessorTest_413 : AISMessageAcknowledgementAdditionalProcessorTest
	{
		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override ZString MessageType => AISOutgoingMessageTypeList.Codes.AmendmentRequest;
	}

	sealed class AISMessageAcknowledgementAdditionalProcessorTest_414 : AISMessageAcknowledgementAdditionalProcessorTest
	{
		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override ZString MessageType => AISOutgoingMessageTypeList.Codes.InvalidationRequest;
	}

	sealed class AISMessageAcknowledgementAdditionalProcessorTest_415 : AISMessageAcknowledgementAdditionalProcessorTest
	{
		protected override ZString ApplicationCode => EDIMessage.ApplicationCodes.IECustomsImport;

		protected override ZString MessageType => AISOutgoingMessageTypeList.Codes.CustomsDeclaration;
	}
}
