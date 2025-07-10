using System;
using System.Linq;
using System.Xml.Schema;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public abstract class EALAESCommonInboxNotificationResponseMessageProcessor<TResponse, TPrettyMessage> : EALCommonResponseMessageProcessor<TResponse, TPrettyMessage>
		where TResponse : class, ICommonServiceSegment, IResponseCode, IMRNField
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected EALAESCommonInboxNotificationResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override ZBool IsOnlyAcceptedDeclaration => true;
		protected sealed override ZBool IsInboxDeclaration => true;

		protected sealed override CusExitReport FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			try
			{
				if (message.EM_MessageText.IsEmpty)
				{
					throw new InvalidOperationException(Res.GetString("E84322A5-BBA6-4865-A1DD-64C26877740C", "Message Text is empty so can't continue with processing"));
				}
				ZString mrnCode;
				using (var textReader = message.GetEM_MessageTextReader())
				using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
				{
					var response = ESXmlObjectSerializer.DeserializeWithoutValidation<TResponse>(XsdSchemaEmbeddedResourceName, bodyTextReader, isAES: true, isNCTS: false);
					mrnCode = response.MRN;
				}

				return !mrnCode.IsEmpty ? sentBusinessObjects.Cast<CusExitReport>().FirstOrDefault(x => (x.Consignment?.CXC_MovementReference ?? ZString.Empty) == mrnCode) : null;
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				throw;
			}
		}
	}
}
