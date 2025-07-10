using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWise.Customs.DE.MessageDefinitions.ZHub;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.Messaging
{
	public sealed class DECustomsDataProvider<T> : IDisposable
	{
		public DECustomsDataProvider(string customsXsdSchemaEmbeddedResourceName, TextReader textReader)
		{
			deCustomsData = XmlObjectSerializer.DeserializeWithXSDValidation<DECustomsData>(typeof(DECustomsData).GetEmbeddedResourcePath(), textReader);
			this.customsXsdSchemaEmbeddedResourceName = customsXsdSchemaEmbeddedResourceName;
		}
		readonly DECustomsData deCustomsData;
		readonly string customsXsdSchemaEmbeddedResourceName;

		public ZDateTimeOffset LogbookTime
		{
			get
			{
				if (!logbookTime.HasValue)
				{
					logbookTime = ZDateTimeOffset.Empty;
					if (ZDateTimeOffset.TryParse(deCustomsData.LogbookTime, out var messageReceiveDateTime, CultureInfo.InvariantCulture))
					{
						logbookTime = messageReceiveDateTime;
					}
				}
				return logbookTime.Value;
			}
		}
		ZDateTimeOffset? logbookTime;

		public T Message
		{
			get
			{
				if (message == null)
				{
					using (var messageNodeReader = new StringReader(((XmlNode[])deCustomsData.CustomsData)[0].OuterXml))
					{
						message = XmlObjectSerializer.DeserializeWithXSDValidation<T>(customsXsdSchemaEmbeddedResourceName, messageNodeReader);
					}
				}
				return message;
			}
		}
		T message;

		public List<AttachedDocument> AttachedDocuments
		{
			get
			{
				if (attachedDocuments == null)
				{
					attachedDocuments = new List<AttachedDocument>();
					if (deCustomsData.AttachedDocumentCollection != null)
					{
						foreach (var attachedDocument in deCustomsData.AttachedDocumentCollection)
						{
							attachedDocuments.Add(new AttachedDocument
							{
								FileName = attachedDocument.FileName,
								Type = new DocumentType
								{
									Code = attachedDocument.Type.Code,
									Description = attachedDocument.Type.Description,
								},
								ImageData = (SubStreamableStream)new MemoryStream(attachedDocument.ImageData, 0, attachedDocument.ImageData.Length)
							});
						}
					}
				}
				return attachedDocuments;
			}
		}
		List<AttachedDocument> attachedDocuments;

		public void Dispose()
		{
			if (attachedDocuments == null)
			{
				return;
			}

			foreach (var document in attachedDocuments)
			{
				document?.Dispose();
			}
		}
	}
}
