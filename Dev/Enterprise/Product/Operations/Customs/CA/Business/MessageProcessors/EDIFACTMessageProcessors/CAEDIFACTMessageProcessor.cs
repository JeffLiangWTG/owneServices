using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Edifact.Auto;
using Enterprise.Messaging.MessageProcessors;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.Edifact;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public abstract class EDIFACTMessageProcessor : Customs.Business.MessageProcessors.EDIFACTMessageProcessor
	{
		protected EDIFACTMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override CustomsMessageProcessor GetMessageProcessor(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			CustomsMessageProcessor processor = null;
			try
			{
				var edifactMessage = ediMessage.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
				processor = GetMessageProcessorCore(ediMessage, edifactMessage);
			}
			catch (InvalidFormatException ex)
			{
				processor = new SyntaxEDIFACTMessageHandler(Logger);
				MessageProcessorErrorReporter.ProcessException(new MessageProcessorException(ex.Message, ediMessage, this), true);
			}
			return processor;
		}

#if DEBUG
		internal
#endif
		protected CustomsMessageProcessor GetMessageProcessorCore(Enterprise.Messaging.Business.EDIMessage ediMessage, SegmentGroup edifactMessage)
		{
			CustomsMessageProcessor result;
			string errorMessage = null;
			var cusresMessageD00A = edifactMessage as Enterprise.Edifact.D00A.Messages.CUSRES.CUSRESMessage;
			var cusresMessageD96A = edifactMessage as Enterprise.Edifact.D96A.Messages.CUSRES.CUSRESMessage;
			var cusresMessageD11B = edifactMessage as Enterprise.Edifact.D11B.Messages.GOVCBR.GOVCBRMessage;

			if (cusresMessageD00A != null)
			{
				string serviceOption = D00AMessageUtilities.GetDocumentName(cusresMessageD00A.BGM);
				result = string.IsNullOrEmpty(serviceOption) ?
					new GenericSyntaxResponseMessageProcessor(Logger) :
					new MessageProcessorProvider().GetProcessor(serviceOption, Logger);
				if (result == null)
				{
					errorMessage = Res.GetString("655B49ED-4A7C-4BF4-B427-E6D84C5DDFB3", "Could not find a supporting Processor Class. Service option: {0}", serviceOption);
				}
			}
			else if (cusresMessageD96A != null)
			{
				var docMessageName = cusresMessageD96A.BGM[0].DocumentMessageName.DocumentMessageName;
				result = new MessageProcessorProvider().GetProcessor(docMessageName, Logger);
				if (result == null)
				{
					errorMessage = Res.GetString("A9256868-FC62-4AAC-9B03-F6B91594276C", "Could not find a supporting Processor Class. Document message name: {0}", docMessageName);
				}
			}
			else if (edifactMessage is Enterprise.Edifact.D99B.Messages.CUSRES.CUSRESMessage
					|| edifactMessage is Enterprise.Edifact.D99B.Messages.CUSDEC.CUSDECMessage
					|| (edifactMessage == null && ediMessage.EM_MessageType == MessageTypeList.Codes.Query))
			{
				result = new MessageProcessorProvider().GetProcessor((EDIMessage)ediMessage, Logger);
				if (result == null)
				{
					errorMessage = Res.GetString("4928AA91-80EF-46A3-BA20-AD12F6AC2AAD", "Could not find a supporting Processor Class. Message type: {0}", ediMessage.GetType());
				}
			}
			else if (cusresMessageD11B != null)
			{
				var docNameCode = cusresMessageD11B.BGM[0].DocumentMessageName.DocumentNameCode;
				result = new MessageProcessorProvider().GetProcessor(docNameCode, Logger);
				if (result == null)
				{
					errorMessage = Res.GetString("3F6CF8B9-8E4F-4FB8-B457-A1EB1813D89A", "Could not find a supporting Processor Class. Document name code: {0}", docNameCode);
				}
			}
			else
			{
				throw new CriticalMessageProcessorException("Incorrect message version", "Inbound message is incorrect version", ediMessage, this);
			}

			if (errorMessage != null)
			{
				throw new CriticalMessageProcessorException("Supporting Processor Class", errorMessage, ediMessage, this);
			}

			return result;
		}
	}
}
