using System;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business;
using Enterprise.DataTransfer.Native.Business.Requests;
using Enterprise.DataTransfer.Native.Business.Update;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeDataMessageProcessor : IDataMessageProcessor
	{
		public NativeDataMessageProcessor(IXmlImportLogger logger)
		{
			this.logger = logger;
		}

		readonly IXmlImportLogger logger;

		public MessageStatus Process(IUniversalObjectFactory factory, IEDIMessage message, IXmlSessionTracker logger, ITopLevelDataObject topLevelDataObject = null, bool recordBillingInformation = true, ICodeMappingManager mapper = null)
		{
			// TODO: We may want to hook this factory into the processing at a later time
			return Process(message);
		}

		public MessageStatus Process(IEDIMessage message)
		{
			XElement requestElement = GetMessageAsXElement(message);
			if (requestElement == null)
			{
				return MessageStatus.Rejected;
			}

			var requestDataObject = GetMessageAsDataObject(message, requestElement);
			return ProcessMessageDataObject(message, requestDataObject);
		}

		XElement GetMessageAsXElement(IEDIMessage message)
		{
			try
			{
				using (var reader = message.GetEM_MessageTextReader())
				{
					return XElement.Load(reader);
				}
			}
			catch (XmlException exception)
			{
				logger.LogBoth(LogType.Error, exception.Message);
				return null;
			}
		}

		static Request GetMessageAsDataObject(IEDIMessage message, XElement messageElement)
		{
			var requestProcessor = RequestDeserializerBuilder.GetDeserializer(messageElement);
			var request = requestProcessor.Deserialize(messageElement);

			if (request.Settings != null)
			{
				request.Settings.TargetCompanyPK = message.Branch != null ? message.Branch.CompanyPK : Guid.Empty;
			}

			return request;
		}

		MessageStatus ProcessMessageDataObject(IEDIMessage requestMessage, Request messageDataObject)
		{
			using (NativeHandler.SetUserContext(requestMessage.Factory, messageDataObject.Settings?.DataContext, logger, requestMessage))
			{
				var handler = new UpdateHandler(new FactoryProvider());
				var dataImportResponse = handler.Execute(messageDataObject);
				return LogResultAndCalculateMessageStatus(dataImportResponse);
			}
		}

		MessageStatus LogResultAndCalculateMessageStatus(UpdateResponse responseData)
		{
			var logBuffer = responseData.LogBuffer;
			foreach (var log in logBuffer.Logs())
			{
				logger.Log(log.Type, log.Message);
			}

			if (!logBuffer.HasError())
			{
				var entityImported = responseData.EntityInfo;
				logger.Log(LogType.Information, "-".PadRight(80, '-'));
				logger.Log(LogType.Information, Res.GetString("8ceda1c9-797d-4794-ac6f-e8e560657380", "Imported: {0}", entityImported.Name));
				if (!string.IsNullOrEmpty(entityImported.LocalCode) || !string.IsNullOrEmpty(entityImported.ExternalCode))
				{
					logger.Log(LogType.Information, Res.GetString("2f6ffc84-d8fc-4818-8cf6-75ee56cc8826", "Local Code: {0}", entityImported.LocalCode));
					logger.Log(LogType.Information, Res.GetString("97a350a2-93ca-435c-a2c9-f4ecef77b78e", "External Code: {0}", entityImported.ExternalCode));
				}

				return MessageStatus.Processed;
			}

			return MessageStatus.Rejected;
		}

#if DEBUG
		public bool ThrowConcurrencyExceptionOnSave { get; set; }
#endif
	}
}
