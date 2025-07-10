using System.Xml.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Integration.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using static Enterprise.Customs.IL.Business.Constants;

[assembly: UniversalCustomsInterchangeUnpacker(EDIInterchange.ApplicationCodes.ILCustoms, typeof(Enterprise.Customs.IL.Business.ILInterchangeUnpacker))]

namespace Enterprise.Customs.IL.Business
{
	public sealed class ILInterchangeUnpacker : IUniversalCustomsInterchangeUnpacker
	{
		public IUniversalCustomsInterchangeUnpackerResult Unpack(EDIInterchange interchange, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, LoggingInformation logger)
			=> (string)interchange.EI_InterchangeType switch
			{
				ILMessageTypeList.Codes.XER => UnpackUniversalEventInterchange((ILEDIInterchange)interchange, interchange.EI_BodyText, ZString.Empty, outgoingInterchange, outgoingMessage, logger),
				_ => UnpackILCustomsInterchange(interchange, interchange.EI_BodyText, ZString.Empty, outgoingInterchange, outgoingMessage, logger),
			};

		static IUniversalCustomsInterchangeUnpackerResult UnpackUniversalEventInterchange(EDIInterchange interchange, ZString messageBodyText, ZString correlationId, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, ILoggingInformation logger)
			=> new FeedbackMessageUnpacker<ILXERResponseMessage>().Unpack(interchange, messageBodyText, string.Empty, correlationId, outgoingInterchange, outgoingMessage, logger);

		static IUniversalCustomsInterchangeUnpackerResult UnpackILCustomsInterchange(EDIInterchange interchange, ZString messageBodyText, ZString correlationId, EDIInterchange outgoingInterchange, EDIMessage outgoingMessage, ILoggingInformation logger)
		{
			var doc = Extensions.TryParseXML(messageBodyText);
			if (doc == null)
			{
				return new EDIInterchangeUnpackerResult(Unpacker.TheInterchangeBodyTextIsNotValidXML);
			}

			var ns = XNamespace.Get("http://www.w3.org/2003/05/soap-envelope");
			var headerElement = doc.Root.Element(ns + Unpacker.ResponseHeader);

			var ns0 = XNamespace.Get("http://MalamTeam.Inf.ESB.Schemas.ResponseHeader");
			var responseHeaderElement = headerElement?.Element(ns0 + Unpacker.ResponseHeaderInner);
			var responseHeaderContent = (ZString)responseHeaderElement?.ToString();

			if (correlationId.IsEmpty && responseHeaderElement != null)
			{
				var correlationIdElement = responseHeaderElement.Element(ns0 + Unpacker.CorrelationId);
				correlationId = correlationIdElement?.Value;
			}

			var bodyElement = doc.Root.Element(ns + Unpacker.ResponseBody);
			var bodyContent = (ZString)bodyElement?.LastNode?.ToString();

			if (bodyContent.IsEmpty)
			{
				return new EDIInterchangeUnpackerResult(Unpacker.TheInterchangeBodyTextIsNotValidXML);
			}

			var customsFeedbackMessageName = Extensions.GetCustomsFeedbackMessageTagName(bodyContent);
			var feedbackMessageUnpacker = FeedbackMessageUnpackerProvider.GetFeedbackMessageUnpacker(customsFeedbackMessageName);

			return feedbackMessageUnpacker == null
				? new EDIInterchangeUnpackerResult($"{Unpacker.TheInterchangeBodyTextDoesNotContainValidFeedbackMessageName} {customsFeedbackMessageName}")
				: feedbackMessageUnpacker.Unpack(interchange, bodyContent, responseHeaderContent, correlationId, outgoingInterchange, outgoingMessage, logger);
		}
	}
}
