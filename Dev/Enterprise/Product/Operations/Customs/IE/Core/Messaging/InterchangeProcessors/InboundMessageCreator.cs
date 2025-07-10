using System;
using System.IO;
using System.Linq;
using System.Xml;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Messaging
{
	public abstract class InboundMessageCreator<T> : IInboundMessageCreator
	{
		protected InboundMessageCreator(LoggingInformation logger)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
		}
		protected LoggingInformation logger;

		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			try
			{
				logger.Log(string.Format("Processing Interchange (Type:{0}, Number:{1}).", interchange.EI_InterchangeType, interchange.EI_InterchangeNum));
				var firstElementFromBody = GetFirstElementFromSOAPEnvelopeBody(interchange);
				var firstElementFromBodyXml = firstElementFromBody?.OuterXml;
				if (string.IsNullOrEmpty(firstElementFromBodyXml))
				{
					HandleDataWhenItIsNotASOAPEnvelope(interchange);
				}
				else
				{
					CreateMessagesForInterchange(interchange, firstElementFromBody.LocalName, firstElementFromBodyXml);
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				SetToErrorAndLog(interchange, GetErrorLog(e));
			}
		}

		protected virtual void SetToErrorAndLog(EDIInterchange interchange, string errorReportLog)
		{
			interchange.Logs.AddNew(Events.ErrorReport, errorReportLog);
			interchange.EI_Status = EDIInterchange.Status.Error;
			logger.LogError(string.Format((NoResString)"Interchange #{0}: Status set to '{1}' due to the following error: {2}", interchange.EI_InterchangeNum, EDIInterchange.Status.Error, errorReportLog));
		}

		static XmlElement GetFirstElementFromSOAPEnvelopeBody(EDIInterchange interchange)
		{
			XmlElement firstElementFromBody = null;
			try
			{
				using (var reader = interchange.GetEI_BodyTextReader())
				{
					if (interchange.EI_ApplicationCode.EqualsIgnoringCase(EDIInterchange.ApplicationCodes.IECustomsEMCS))
					{
						var envelope = IEXmlObjectSerializer.Deserialize<CargoWise.Customs.Shared.MessageDefinitions.SOAP.Version1_1.SCHEMA.Envelope>(reader);
						firstElementFromBody = envelope.Body.Any?.FirstOrDefault();
					}
					else
					{
						var envelope = IEXmlObjectSerializer.Deserialize<CargoWise.Customs.Shared.MessageDefinitions.SOAP.Version1_2.SCHEMA.Envelope>(reader);
						firstElementFromBody = envelope.Body.Any?.FirstOrDefault();
					}
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				// do nothing as interhange doesn't contain a valid SOAP Envelop
			}
			return firstElementFromBody;
		}

		protected virtual void HandleDataWhenItIsNotASOAPEnvelope(EDIInterchange interchange)
		{
			SetToErrorAndLog(interchange, $"{EDIInterchangeSchema.EI_BodyText.Name} does not contain a valid SOAP Envelope Body or the Body section is empty.");
		}

		protected virtual void CreateMessagesForInterchange(EDIInterchange interchange, string elementName, string xmlBody)
		{
			CreateMessagesForInterchange(interchange, IEXmlObjectSerializer.Deserialize<T>(new StringReader(xmlBody)));
		}

		protected abstract void CreateMessagesForInterchange(EDIInterchange interchange, T response);
		protected abstract string GetErrorLog(Exception e);
	}
}
