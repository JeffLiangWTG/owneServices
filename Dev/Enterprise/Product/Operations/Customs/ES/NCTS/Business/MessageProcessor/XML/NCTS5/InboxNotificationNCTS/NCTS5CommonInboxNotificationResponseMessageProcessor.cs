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

namespace Enterprise.Customs.ES.NCTS.Business
{
	public abstract class NCTS5CommonInboxNotificationResponseMessageProcessor<TResponse, TPrettyMessage> : NCTS5CommonResponseMessageProcessor<TResponse, TPrettyMessage>
		where TResponse : class, ICommonServiceSegment, IResponseCode, IMRNField
		where TPrettyMessage : IMessagePrettyFormatter
	{
		protected NCTS5CommonInboxNotificationResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override ZBool IsOnlyAcceptedDeclaration => true;
		protected sealed override ZBool IsInboxDeclaration => true;

		protected sealed override NctsHeader FindRelevantBusinessObjectCore(EDIMessage message, BusinessObject[] sentBusinessObjects, bool isDirectxTMessage)
		{
			try
			{
				if (message.EM_MessageText.IsEmpty)
				{
					throw new InvalidOperationException(Res.GetString("614F1D9E-4CC1-414C-B1D0-636BD615C435", "Message Text is empty so can't continue with processing"));
				}
				ZString mrnCode;
				using (var textReader = message.GetEM_MessageTextReader())
				using (var bodyTextReader = XMLResponseMessageHelper.GetXmlBody(textReader))
				{
					var response = ESXmlObjectSerializer.DeserializeWithoutValidation<TResponse>(XsdSchemaEmbeddedResourceName, bodyTextReader, isAES: false, isNCTS: true);
					mrnCode = response.MRN;
				}

				return !mrnCode.IsEmpty ? sentBusinessObjects.Cast<NctsHeader>().FirstOrDefault(x => x.MovementReferenceNumber == mrnCode) : null;
			}
			catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
			{
				throw;
			}
		}
	}
}
