using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageSending
{
	public class FrNctsTransmissionMessageGenerator : NctsTransmissionMessageGenerator
	{
		public FrNctsTransmissionMessageGenerator(NctsMessageFunctionSet messageFunction) : base(messageFunction)
		{
		}

		protected override IMessageNumberStrategy GetMessageNumberStrategy(BuilderResult builderResult)
		{
			return new FRMessageNumberStrategy(builderResult.Message.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
		}

		protected override ZString GetApplicationCode()
		{
			return EDIMessage.ApplicationCodes.FRCustomsMessage;
		}

		protected override (ZString MessageText, ZString MessageType, ZString MessageSubType) GetMessageTextAndMessageType(EU.NCTS.Business.NctsHeader header, NctsMessageFunctionSet how, ErrorCollector errorCollector)
		{
			var result = base.GetMessageTextAndMessageType(header, how, errorCollector);
			result.MessageType = new ZString(how.Code).Right(3);  // e.g. 015
			result.MessageSubType = MessageSubTypeList.Codes.DT;
			return result;
		}

		protected override ZString GetMessageOwner(EU.NCTS.Business.NctsHeader header, ErrorCollector errorCollector)
		{
			return header.Branch.Company.GC_CustomsRegistrationNo.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);
		}

		protected override ZString GetMessageCodeFromMessage(EDIMessage message) => message.EM_MessageType;

		protected override EU.NCTS.Business.MessageGeneration.NctsMessageFunctionSetsProvider GetMessageFunctionSetsProvider()
		{
			return new NctsMessageFunctionSetsProvider();
		}
	}
}
