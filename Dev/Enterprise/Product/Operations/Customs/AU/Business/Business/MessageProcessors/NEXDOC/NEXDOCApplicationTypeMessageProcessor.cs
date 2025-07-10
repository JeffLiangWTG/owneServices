using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor
	{
		public NEXDOCApplicationTypeMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.NEXDOCS;

		protected override string MessageFriendlyNameCore => string.Empty;

		protected override void ProcessMessageCore(EDIMessage message)
		{
			GetMessageProcessor(message)?.ProcessMessage(message);
		}

		protected CustomsMessageProcessor GetMessageProcessor(EDIMessage message)
		{
			var errorMessage = ZString.Empty;
			CustomsMessageProcessor processor = null;

			try
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(message.EM_MessageText);
				ZString documentType = xmlDoc.DocumentElement?.LocalName;
				if (!documentType.IsEmpty)
				{
					processor = Processors.FirstOrDefault(p => p.HandlesDocument(documentType));
					if (processor == null)
					{
						errorMessage = ZString.Format("Unknown message type '{0}'.", documentType);
					}
				}
				else
				{
					errorMessage = "Expected message to be XML.";
				}
			}
			catch (XmlException ex)
			{
				errorMessage = ex.Message;
			}

			if (!errorMessage.IsEmpty)
			{
				Logger.LogError(ZString.Format("Could not process NEXDOC message #{0}. {1}", message.EM_MessageNum, errorMessage));
				message.EM_Status = EDIMessage.Status.Error;
				processor = null;
			}

			return processor;
		}

		IEnumerable<NEXDOCMessageProcessor> Processors
		{
			get
			{
				yield return new ReadRexResponseProcessorRC5(Logger);
				yield return new RexAcknowledgeOwnershipResponseProcessor(Logger);
				yield return new RexForwardOwnershipResponseProcessor(Logger);
				yield return new RexTransferOwnershipResponseProcessor(Logger);
			}
		}
	}
}
