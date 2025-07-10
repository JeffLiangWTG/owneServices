using System;
using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageBuilders;
using static Enterprise.Customs.IL.Business.Constants;

namespace Enterprise.Customs.IL.Business
{
	public class InfMsgGenericUnpacker : IFeedbackMessageUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, ZString bodyText, ZString responseHeaderText, ZString correlationId, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, Integration.BatchProcessor.ILoggingInformation logger)
		{
			if (interchange.EI_InterchangeType != ILMessageTypeList.Codes.GEN)
			{
				return new EDIInterchangeUnpackerResult(Unpacker.TheInterchangeTypeIncorrect);
			}

			var ns0 = XNamespace.Get("http://MalamTeam.Inf.ESB.Schemas.ResponseHeader");
			var responseHeader = XDocument.Parse(responseHeaderText);
			var statusElement = responseHeader.Root.Element(ns0 + Unpacker.Status);

			if (statusElement.Value != Unpacker.StatusSuccess)
			{
				return new EDIInterchangeUnpackerResult(Unpacker.TheStatusIsNotSuccess);
			}

			IMessageBuilder messageBuilder = new ILGEN910MessageBuilder(interchange.Company, new LoggerWrapper(logger));
			messageBuilder.PopulateMessages();
			interchange.Factory.Save();

			return new EDIInterchangeUnpackerResult(Array.Empty<EDIMessage>());
		}
	}
}
