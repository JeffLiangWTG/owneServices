using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
#if NETFRAMEWORK
using Enterprise.Freight.Business;
#endif

namespace Enterprise.Customs.ES.Business
{
	public class DocumentResponseParser
	{
		public DocumentResponseParser(string responseContent, string interchangeHeaderText, LoggingInformation logger)
		{
			this.responseContent = responseContent;
			headerDict = GetHeaderTextDictionary(interchangeHeaderText, logger);
		}

		readonly string responseContent;
		readonly Dictionary<string, string> headerDict;

		const string headerDictKeyForFileName = "custom.ES.DocumentFilename";
		const string headerDictKeyForFileType = "custom.ES.DocumentType";
		const string headerDictKeyForFileDescription = "custom.ES.DocumentFileDescription";

		public string Parse()
		{
			var newDocResponse = new AttachedDocument()
			{
				FileName = headerDict.GetValueOrDefault(headerDictKeyForFileName),
				Type = new AttachedDocumentType()
				{
					Code = headerDict.GetValueOrDefault(headerDictKeyForFileType),
					Description = headerDict.GetValueOrDefault(headerDictKeyForFileDescription)
				},
				ImageData = System.Convert.FromBase64String(responseContent),
				IsPublished = true
			};

			return XmlUtils.SerializeToString(newDocResponse);
		}

		Dictionary<string, string> GetHeaderTextDictionary(ZString headerText, LoggingInformation logger)
		{
			var result = new Dictionary<string, string>();

			try
			{
				result = MessageHelper.GetHeaderTextDictionary(headerText);
			}
			catch (JsonException)
			{
				logger.Log(string.Format(CultureInfo.CurrentCulture, "Error processing Header Text, should be a correct json"), Integration.LogType.Error);
			}

			return result;
		}
	}
}
