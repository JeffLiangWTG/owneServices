using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using CargoWise.Customs.ES.MessageDefinitions;
using CargoWise.Customs.ES.MessageDefinitions.Xhub.Products.Customs;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.EDIMessages;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business
{
	public class ESCInboundInterchangeProcessor : BranchInboundInterchangeProcessor
	{
		public ESCInboundInterchangeProcessor(IEnumerable<string> applicationCodes) : base(applicationCodes)
		{
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange) =>
			(interchange.EI_TransportType == EDIInterchange.TransportType.xT)
				? new DirectxTInboundMessageCreator(Logger)
				: new EHubInboundMessageCreator(Logger);

		#region EHubInboundMessageCreator Class
		class EHubInboundMessageCreator : CommonInboundMessageCreator
		{
			public EHubInboundMessageCreator(LoggingInformation logger) : base(logger)
			{
			}

			const string YesString = "Y";

			protected override void CreateMessagesForInterchangeCore(EDIInterchange interchange)
			{
				var headers = GetHeaders(interchange);
				if (headers != null)
				{
					var newEDIMessage = CreateMessageWithCommonData(interchange);
					newEDIMessage.EM_ApplicationReference = headers.EntryReferenceNumber;
					newEDIMessage.EM_IsTestMessage = headers.TestMessage.Equals(YesString, StringComparison.OrdinalIgnoreCase);
				}
			}

			Headers GetHeaders(EDIInterchange interchange)
			{
				Headers headers = null;
				try
				{
					using (var textReader = interchange.GetEI_HeaderTextReader())
					{
						headers = CargoWise.Customs.Shared.MessageContracts.XmlObjectSerializer.DeserializeWithXSDValidation<Headers>(HeadersXsdSchemaEmbeddedResourceName, textReader);
					}
				}
				catch (Exception ex) when (ex is XmlSchemaValidationException || ex is InvalidOperationException)
				{
					interchange.EI_Status = EDIInterchange.Status.Error;
					logger.Log(string.Format(CultureInfo.CurrentCulture, "Error reading xml {0}", ex.ToString()), Integration.LogType.Error);
				}
				return headers;
			}

			const string HeadersXsdSchemaEmbeddedResourceName = "CargoWise.Customs.ES.MessageDefinitions.Interchange.Headers.xsd";
		}
		#endregion

		#region DirectxTInboundMessageCreator Class
		class DirectxTInboundMessageCreator : CommonInboundMessageCreator
		{
			public DirectxTInboundMessageCreator(LoggingInformation logger) : base(logger)
			{
			}

			protected override void CreateMessagesForInterchangeCore(EDIInterchange interchange)
			{
				var newEDIMessage = CreateMessageWithCommonData(interchange);
			}

			protected override string ParseTextString(string textString, EDIMessage ediMessage, string interchangeFrom, string interchangeHeaderText)
			{
				MessageProcessingResult parsingResult = null;
				if (interchangeFrom == SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms)
				{
					var parser = new EdifactResponseParser(textString);
					parsingResult = parser.Parse();
				}
				else if (SoapResponseParserInterchangeFromCodes.Contains(interchangeFrom)
						|| (DocResponseParserInterchangeFromCodes.Contains(interchangeFrom) && ediMessage.EM_MessageType != DeclarationMessageTypeList.Codes.EsDocumentRequest))
				{
					var parser = new SoapResponseParser(textString);
					parsingResult = parser.Parse();
				}
				else if (DocResponseParserInterchangeFromCodes.Contains(interchangeFrom) && !textString.Contains(UniversalEventTag))
				{
					var parser = new DocumentResponseParser(textString, interchangeHeaderText, logger);
					return parser.Parse();
				}

				if (parsingResult != null)
				{
					if (parsingResult.IsSuccessful)
					{
						return Encoding.UTF8.GetString(parsingResult.ResponseMessage.ToArray());
					}
					else
					{
						if (textString.Contains(UniversalEventTag))
						{
							ediMessage.EM_MessageType = DeclarationMessageTypeList.Codes.CustomsServiceErrorUniversalEvent;

							return base.ParseTextString(textString, ediMessage, interchangeFrom, interchangeHeaderText);
						}
						else
						{
							ediMessage.EM_MessageType = DeclarationMessageTypeList.Codes.CustomsServiceError;

							return XmlUtils.SerializeToString(new CommonCustomsServiceError
							{
								ErrorType = parsingResult.ResponseStatus.ToString("D"),
								ErrorDescription = parsingResult.ErrorMessage
							});
						}
					}
				}
				else
				{
					return base.ParseTextString(textString, ediMessage, interchangeFrom, interchangeHeaderText);
				}
			}

			const string UniversalEventTag = "UniversalEvent";

			ImmutableList<ZString> SoapResponseParserInterchangeFromCodes => soapResponseParserInterchangeFromCodes
																				??= new List<ZString>()
																						{
																							SpanishCustomsTypeCodeList.Codes.SoapProSpanishCustomsForDirectXt,
																							SpanishCustomsTypeCodeList.Codes.SoapTestSpanishCustomsForDirectXt,
																							SpanishCustomsTypeCodeList.Codes.AsynchronousProSpanishCustomsForDirectXt,
																							SpanishCustomsTypeCodeList.Codes.AsynchronousTestSpanishCustomsForDirectXt
																						}.ToImmutableList();
			ImmutableList<ZString> soapResponseParserInterchangeFromCodes;

			ImmutableList<ZString> DocResponseParserInterchangeFromCodes => docResponseParserInterchangeFromCodes
																				??= new List<ZString>()
																						{
																							SpanishCustomsTypeCodeList.Codes.DocumentSpanishCustoms,
																							SpanishCustomsTypeCodeList.Codes.DocumentTestSpanishCustoms
																						}.ToImmutableList();
			ImmutableList<ZString> docResponseParserInterchangeFromCodes;
		}
		#endregion

		#region CommonInboundMessageCreator Class
		abstract class CommonInboundMessageCreator : IInboundMessageCreator
		{
			public CommonInboundMessageCreator(LoggingInformation logger)
			{
				this.logger = logger;
			}
			protected readonly LoggingInformation logger;

			public void CreateMessagesForInterchange(EDIInterchange interchange) => CreateMessagesForInterchangeCore(interchange);

			protected abstract void CreateMessagesForInterchangeCore(EDIInterchange interchange);

			protected EDIMessage CreateMessageWithCommonData(EDIInterchange interchange)
			{
				var newEDIMessage = interchange.ContainedMessages.AddNew(typeof(ESEDIMessage));
				newEDIMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
				newEDIMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
				newEDIMessage.EM_EI = interchange.PK;
				newEDIMessage.EM_GB = interchange.EI_GB;
				newEDIMessage.EM_MessageType = interchange.EI_InterchangeType;
				newEDIMessage.EM_IsActive = true;
				newEDIMessage.EM_MessageNum = interchange.EI_InterchangeNum.Left(newEDIMessage.EM_MessageNumInfo.MaxLength);
				newEDIMessage.SetEM_MessageTextSource(new TextReaderSource(LargeMessageHelper.CopyAndDispose(interchange.GetEI_BodyTextReader())));
				ProcessMessageText(newEDIMessage, interchange.EI_From, interchange.EI_HeaderText);
				return newEDIMessage;
			}

			void ProcessMessageText(EDIMessage ediMessage, string interchangeFrom, string interchangeHeaderText)
			{
				using (var textReader = ediMessage.GetEM_MessageTextReader())
				{
					var textString = textReader.ReadToEnd();

					textString = ParseTextString(textString, ediMessage, interchangeFrom, interchangeHeaderText);

					var isEdifact = interchangeFrom == SpanishCustomsTypeCodeList.Codes.EdifactSpanishCustoms;

					if (isEdifact && textString.Contains((NoResString)"<Respuesta>")) // Only for EDIFACT messages
					{
						var messageText = GetStringBetween(textString, (NoResString)"<Respuesta>", (NoResString)"</Respuesta>");
						var unbSegment = GetUNBString(messageText);
						messageText = RemoveString(messageText, unbSegment);

						ediMessage.EM_MessageText = messageText;
					}
					else
					{
						ediMessage.EM_MessageText = CleanXML(textString);
					}
				}
			}

			protected virtual string ParseTextString(string textString, EDIMessage ediMessage, string interchangeFrom, string interchangeHeaderText) => textString;

			string GetStringBetween(string fullText, string startString, string endString)
			{
				int startIndex = fullText.IndexOf(startString, StringComparison.OrdinalIgnoreCase) + startString.Length;
				int endIndex = fullText.IndexOf(endString, startIndex, StringComparison.OrdinalIgnoreCase);
				return fullText.Substring(startIndex, endIndex - startIndex);
			}

			string GetUNBString(string fullText)
			{
				var unbString = "UNB";
				if (fullText.Contains(unbString))
				{
					var endString = "'";
					int startIndex = fullText.IndexOf(unbString, StringComparison.OrdinalIgnoreCase);
					int endIndex = fullText.IndexOf(endString, StringComparison.OrdinalIgnoreCase) + endString.Length;
					return fullText.Substring(startIndex, endIndex);
				}
				else
				{
					return string.Empty;
				}
			}

			string RemoveString(string fullText, string stringToRemove)
			{
				int index = fullText.IndexOf(stringToRemove, StringComparison.OrdinalIgnoreCase);
				return fullText.Remove(index, stringToRemove.Length);
			}

			string CleanXML(string fullText)
			{
				var mandatoryTagsPattern = FormattableString.Invariant($"(?!{string.Join("|", MandatoryTags)})");

				var regex = new Regex(string.Format(@"\s*<\s*{0}\w+\s*/\s*>|\s*<\s*{0}\w+\s*>\s*<\s*/\s*{0}\w+\s*>", mandatoryTagsPattern));
				var result = TrimTags(regex.Replace(fullText, ""));

				return result;
			}

			string TrimTags(string xmlText)
			{
				var existingTagsToTrim = GetApplicableTagsToTrim(xmlText);

				if (existingTagsToTrim.Any())
				{
					var xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(xmlText);

					foreach (var tag in existingTagsToTrim)
					{
						var elements = xmlDocument.GetElementsByTagName(tag.Name);

						foreach (XmlNode element in elements)
						{
							ZString textToTrim = element.InnerText;
							element.InnerText = textToTrim.Left(tag.MaxLength);
						}
					}

					return xmlDocument.OuterXml;
				}

				return xmlText;
			}

			List<(string Name, int MaxLength)> GetApplicableTagsToTrim(string xmlText)
			{
				var result = new List<(string Tag, int MaxLength)>();

				foreach (var tag in TagsToTrim)
				{
					if (xmlText.Contains(tag.Key))
					{
						result.Add((tag.Key, tag.Value));
					}
				}

				return result;
			}

			IEnumerable<string> MandatoryTags => new List<string> { "C47TributoIndicadorMaxMinNor", "C47TributoUnidadFiscal" };
			ImmutableDictionary<string, int> TagsToTrim => new Dictionary<string, int>()
			{
				{ "C31DescripcionDeLaMercancia", 250 },
			}.ToImmutableDictionary();
		}
		#endregion
	}
}
