using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Xsl;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.JP.Common
{
	sealed class NACCSMessageInterpreter
	{
		public NACCSMessageInterpreter(EDIMessage message)
		{
			this.message = Argument.NotNull(message, nameof(message));
		}

		const string ResultCodeName = "Result Code";

		const char ProcedureCodePaddingChar = '_';

		readonly EDIMessage message;

		IJPInboundMessageParseResult parseResult;

		public string MessageText => JPMessageUtils.ConvertMessageToString(message.EM_MessageData);

		public ZString Interprete()
		{
			var result = new List<string>();
			try
			{
				if (message.EM_MessageData == null)
				{
					return $"Invalid message format: Empty Message";
				}

				ErrorCodesParser errorCodesParser = null;

				var parser = NACCSFactoryService.GetMessageFlatParser(message.Factory);
				IEnumerable<(FieldDefinition field, string data)> parsedBody;
				if (message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive)
				{
					(parseResult, parsedBody) = parser.ParseInbound(message.EM_MessageData);

					var responseHeader = parseResult.ResponseHeader;
					var procedureCode = GetProcedureCode(parseResult.ResponseHeader);

					errorCodesParser = new ErrorCodesParser(responseHeader);
					result.Add(CreateInboundCommonHeaderTable(parseResult));
				}
				else
				{
					IJPOutboundMessageHeader header;
					(header, parsedBody) = parser.ParseOutbound(message.EM_MessageData);
					result.Add(CreateOutboundCommonHeaderTable(header));
				}

				var groups = parsedBody.GroupBy(item => item.field.Repeat);

				foreach (var group in groups)
				{
					if (string.IsNullOrEmpty(group.Key))
					{
						result.Add(CreateTableHeader("Message Header"));
					}
					else
					{
						result.Add(CreateTableHeader($"Message Details - {group.Key}"));
					}

					result.Add(CreateBodyTable(group, errorCodesParser));
				}

				return string.Join(System.Environment.NewLine, result.Prepend(DefaultStyle));
			}
			catch (JPMessageSchemaException ex)
			{
				return $"Invalid message format: {ex.Message}";
			}
		}

		string CreateInboundCommonHeaderTable(IJPInboundMessageParseResult parseResult)
		{
			var tableCreator = GetErrorsTableCreator();

			if (parseResult.HasResultCode)
			{
				foreach (var error in parseResult.Errors)
				{
					tableCreator.WriteRow(error.ErrorCode, GetErrorCodeDescription(GetProcedureCode(parseResult.ResponseHeader), error.ErrorCode), error.FieldCode, GetFieldName(error.FieldDefinition), error.FieldIndex > 0 ? error.FieldIndex.ToString() : "");
				}
			}

			var tableCreatorCommonHeader = new HtmlTableCreator();
			tableCreatorCommonHeader.WriteRow("Procedure Code", parseResult.ResponseHeader.ProcedureCode);
			tableCreatorCommonHeader.WriteRow("Output Information Code", parseResult.ResponseHeader.OutputInformationCode);
			tableCreatorCommonHeader.WriteRow("Subject", parseResult.ResponseHeader.Subject);

			return (parseResult.HasResultCode ? (CreateTableHeader($"Results: {parseResult.ResultCode}") + tableCreator.ToHtml()) : string.Empty) + CreateTableHeader("Inbound Common Header") + tableCreatorCommonHeader.ToHtml();
		}

		HtmlTableCreator GetErrorsTableCreator()
		{
			return new HtmlTableCreator(new[] { "Result Code", "Description/Disposition", "Element ID", "Element Description", "Line Number" });
		}

		string GetErrorCodeDescription(string procedureCode, string errorCode)
		{
			var result = ZString.Empty;
			if (!string.IsNullOrEmpty(procedureCode))
			{
				var procedureCodes = JPResultCodeMapping.Mapping.TryGetValue(procedureCode.TrimEnd(ProcedureCodePaddingChar), out var mappedCodes) ? mappedCodes : new List<string> { procedureCode };
				result = ZZRefCusCodeListCombined.Loader.LoadForCodes(message.Factory, Core.Constants.CountryCodes.Japan, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NACCSResultCode, procedureCodes.Select(x => new ZString(x.PadRight(5, ProcedureCodePaddingChar) + errorCode)).ToArray(), ZDateTime.Now, null).FirstOrDefault()?.ZZD_Description ?? ZString.Empty;
			}
			return result;
		}

		string CreateOutboundCommonHeaderTable(IJPOutboundMessageHeader header)
		{
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow("Procedure Code", header.ProcedureCode);
			tableCreator.WriteRow("Message Reference", header.MessageTag);
			tableCreator.WriteRow("Input Reference", header.InputReference);
			return CreateTableHeader("Outbound Common Header") + tableCreator.ToHtml();
		}

		NameValueCollection Colspan(int span) => TableInterpretation.Attributes.GetColspanAttribute(span);

		IEnumerable<CellWithFormatting> GetCellWithFormattings(string[] cellValues, bool isTitle = false)
		{
			if (cellValues.Length == 4)
			{
				yield return new CellWithFormatting(cellValues[0], Colspan(1), isTitle);
				yield return new CellWithFormatting(cellValues[1], Colspan(2), isTitle);
				yield return new CellWithFormatting(cellValues[2], Colspan(1), isTitle);
				yield return new CellWithFormatting(cellValues[3], Colspan(3), isTitle);
			}
		}

		string CreateBodyTable(IEnumerable<(FieldDefinition field, string data)> bodyData, ErrorCodesParser errorCodesParser)
		{
			var result = new StringBuilder();

			var isReceivedMessage = message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive;

			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRowWithFormatting(GetCellWithFormattings(new[] { "NO", "項目名", isReceivedMessage ? "SEQ" : "ID", "" }, isTitle: true).ToArray());

			var extraResultRows = new List<(FieldDefinition Field, string Data)>();
			var needProcessExtraResults = isReceivedMessage && errorCodesParser != null;

			foreach (var row in bodyData)
			{
				var field = row.field;

				if (needProcessExtraResults && field.Name == ResultCodeName)
				{
					extraResultRows.Add(row);
				}
				else
				{
					WriteField(tableCreator, row.field, row.data);
				}
			}

			result.AppendLine(tableCreator.ToHtml());

			if (extraResultRows.Count > 0)
			{
				result.AppendLine();
				result.AppendLine(CreateExtraResultsTable(extraResultRows, errorCodesParser));
			}

			return result.ToString();
		}

		string CreateExtraResultsTable(IEnumerable<(FieldDefinition Field, string Data)> rows, ErrorCodesParser errorCodesParser)
		{
			var tableCreatorResults = GetErrorsTableCreator();

			foreach (var row in rows)
			{
				var errors = errorCodesParser.Parse(row.Data);

				foreach (var error in errors)
				{
					tableCreatorResults.WriteRow(error.ErrorCode, GetErrorCodeDescription(GetProcedureCode(parseResult.ResponseHeader), error.ErrorCode), error.FieldCode, GetFieldName(row.Field), error.FieldIndex > 0 ? error.FieldIndex.ToString() : string.Empty);
				}
			}

			return tableCreatorResults.ToHtml();
		}

		string GetProcedureCode(IJPInboundMessageHeader responseHeader) => string.IsNullOrWhiteSpace(responseHeader.ProcedureCode) ? string.Empty : responseHeader.ProcedureCode.PadRight(5, ProcedureCodePaddingChar);

		void WriteField(HtmlTableCreator tableCreator, FieldDefinition field, string data)
		{
			tableCreator.WriteRowWithFormatting(GetCellWithFormattings(new[]
			{
				field.No.ToString(),
				field.JPName,
				message.EM_ReceiveTransmit == ReceiveTransmitList.Codes.Receive ? field.Repeat1 : field.ID,
				data
			}).ToArray());
		}

		string CreateTableHeader(string title)
		{
			return $"<h3>{title}</h3>";
		}

		string GetFieldName(FieldDefinition field)
		{
			return Res.CurrentLanguage == SharedConstants.Languages.Japanese
				? FallbackName(field?.JPName, field?.Name)
				: FallbackName(field?.Name, field?.JPName);
		}

		string FallbackName(string firstName, string secondName)
		{
			return !string.IsNullOrEmpty(firstName) ? firstName : secondName;
		}

		public string ConvertXERXmlToHtmlTable()
		{
			var messageText = message.EM_MessageText;
			if (!messageText.StartsWith("<"))
			{
				return string.Empty;
			}

			try
			{
				var regex = new Regex(@"<Reason>\s*(.*?)\s*</Reason>", RegexOptions.Singleline);
				var match = regex.Match(messageText);

				if (match.Success)
				{
					var reason = match.Groups[1].Value.Trim();
					var replaceReason = SecurityElement.Escape(reason);
					messageText = messageText.Replace(reason, replaceReason);
				}
			}
			catch
			{
			}

			return TransformXMLToHTML("Enterprise.Customs.JP.Common.XSLT.XER.xslt", messageText);
		}

		string TransformXMLToHTML(string xsltResourcePath, string xmlString)
		{
			var xslCompiledTransform = new XslCompiledTransform();
			var xerXsltStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xsltResourcePath);
			using (var reader = XmlReader.Create(xerXsltStream))
			{
				xslCompiledTransform.Load(reader);
			}
			var stringWriter = new StringWriter();
			using (var reader = XmlReader.Create(new StringReader(xmlString)))
			{
				try
				{
					xslCompiledTransform.Transform(reader, null, stringWriter);
				}
				catch
				{
					return ZString.Empty;
				}
			}
			return stringWriter.ToString();
		}

		internal const string DefaultStyle = @"<style>
	body {width: 100%;}
	th, td {
		font-size: 30px;
        padding-left: 10px;
        padding-right: 10px;
	}
</style>";
	}
}
