using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.MailManager.ExternalMailInterface;
using Enterprise.ZArchitecture.Core;
using MimeKit;

namespace Enterprise.Customs.CH.Business;

internal static class MIMETypeXTParser
{
	const string SoapEnvelopeNamespace = "http://schemas.xmlsoap.org/soap/envelope/";
	const int LF = 10;
	const int CR = 13;

	readonly static Regex ContentTypeRegex = new Regex(@"^content-type:[ ""]* (?<value>[^"";\r\n]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
	readonly static Regex BoundaryRegex = new Regex(@"boundary=""(?<value>[^""]*)""", RegexOptions.Multiline | RegexOptions.IgnoreCase);
	readonly static Regex ContentDescriptionRegex = new Regex(@"^content-description:[ ""]* (?<value>[^"";\r\n]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
	readonly static Regex ContentTransferEncodingRegex = new Regex(@"^content-transfer-encoding:[ ""]* (?<value>[^"";\r\n]*)", RegexOptions.Multiline | RegexOptions.IgnoreCase);

	public static IEnumerable<ResponseParsingResult> ParseTextHttp(string content)
	{
		using (TextReader reader = new StringReader(content))
		{
			return ParseTextHttp(reader);
		}
	}

	public static IEnumerable<ResponseParsingResult> ParseTextHttp(TextReader reader)
	{
		var resultList = new List<ResponseParsingResult>();

		void addResultIfNotNull(ResponseParsingResult result)
		{
			if (result != null)
			{
				resultList.Add(result);
			}
		}

		if (ExtractTillFirstEmptyLine(reader, out string header))
		{
			var contentMatch = ContentTypeRegex.Match(header);
			if (contentMatch.Success)
			{
				var contentType = contentMatch.Groups[(NoResString)"value"].Value.ToLower().Trim();

				if (contentType.Contains((NoResString)"multipart"))
				{
					foreach (var multipartContent in ParseContentMultipart(header, reader.ReadToEnd()))
					{
						resultList.AddRange(ParseTextHttp(multipartContent));
					}
				}
				else if (contentType.Contains((NoResString)"xml"))
				{
					addResultIfNotNull(ParseContentXml(contentType, header, reader.ReadToEnd()));
				}
				else if (contentType.Contains((NoResString)"pdf"))
				{
					addResultIfNotNull(ParseContentPdf(contentType, header, reader.ReadToEnd()));
				}
				else
				{
					addResultIfNotNull(ParseContentText(contentType, reader.ReadToEnd()));
				}
			}
			else
			{
				addResultIfNotNull(ParseContentText(string.Empty, $"{header}{reader.ReadToEnd()}"));
			}
		}
		else
		{
			addResultIfNotNull(ParseContentText(string.Empty, header));
		}
		return resultList;
	}

	static bool ExtractTillFirstEmptyLine(TextReader reader, out string header)
	{
		var contentTillFirstEmptyLine = new StringBuilder();
		var emptyLineFound = false;
		int character;
		int nextCharacter;

		while ((character = reader.Read()) != -1)
		{
			contentTillFirstEmptyLine.Append((char)character);

			if (NewLineFound(character, reader, contentTillFirstEmptyLine))
			{
				emptyLineFound = true;
				if ((nextCharacter = reader.Read()) != -1)
				{
					contentTillFirstEmptyLine.Append((char)nextCharacter);
					if (NewLineFound(nextCharacter, reader, contentTillFirstEmptyLine))
					{
						break;
					}
				}
				else
				{
					break;
				}
			}
		}
		header = contentTillFirstEmptyLine.ToString();
		return emptyLineFound;
	}

	static bool NewLineFound(int character, TextReader reader, StringBuilder readContent)
	{
		if (character == CR || character == LF)
		{
			if (character == CR && reader.Peek() == LF)
			{
				readContent.Append((char)reader.Read());
			}
			return true;
		}
		return false;
	}

	static string[] ParseContentMultipart(string header, string content)
	{
		var boundaryMatch = BoundaryRegex.Match(header);
		if (boundaryMatch.Success)
		{
			var splitter = $"--{boundaryMatch.Groups["value"].Value}";
			return CutOffFirstSafe(content, splitter).Split(new[] { splitter }, StringSplitOptions.RemoveEmptyEntries).Select(c => CutOffFirstSafe(c, "\r\n")).ToArray();
		}
		else
		{
			return Array.Empty<string>();
		}
	}

	static ResponseParsingResult ParseContentText(string contentType, string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			return new ResponseParsingResult()
			{
				Type = string.IsNullOrEmpty(contentType) ? "text/plain" : contentType,
				BodyText = content,
			};
		}
		return null;
	}

	static ResponseParsingResult ParseContentXml(string contentType, string header, string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			return new ResponseParsingResult()
			{
				Type = contentType,
				Description = GetContentDescriptionFromHeader(header),
				BodyText = content,
			};
		}
		return null;
	}

	static ResponseParsingResult ParseContentPdf(string contentType, string header, string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			return new ResponseParsingResult()
			{
				Type = contentType,
				Description = GetContentDescriptionFromHeader(header),
				BodyText = GetBodyTextFromContent(header, content),
				BodyData = GetBodyDataFromContent(header, content),
			};
		}
		return null;
	}

	public static IEnumerable<ResponseParsingResult> ParseTextMail(string content)
	{
		using (TextReader reader = new StringReader(content))
		{
			return ParseTextMail(reader);
		}
	}

	public static IEnumerable<ResponseParsingResult> ParseTextMail(TextReader reader)
	{
		ResponseParsingResult addParsingResult(MimeEntity attachment, bool isXml, bool isPdf, string fileName, string fileMimeType)
		{
			var fileData = attachment.GetData();
			return new ResponseParsingResult()
			{
				Type = fileMimeType,
				IsXml = isXml,
				IsPdf = isPdf,
				Description = !string.IsNullOrEmpty(fileName) ? Path.GetFileNameWithoutExtension(fileName) : string.Empty,
				BodyText = isXml ? Encoding.UTF8.GetString(fileData) : string.Empty,
				BodyData = isPdf ? fileData : Array.Empty<byte>(),
			};
		}

		var resultList = new List<ResponseParsingResult>();

		var message = MimeMessageExtensions.CreateMessageFromEml(reader.ReadToEnd());
		var attachments = message.GetFullAttachments();
		foreach (var attachment in attachments)
		{
			var fileName = attachment.GetName();
			var fileMimeType = attachment?.ContentType?.MimeType ?? string.Empty;
			if (Path.GetExtension(fileName).ToLower() == ".xml" && fileMimeType == (NoResString)"application/octet-stream" && (fileName.Contains("_edecResponse_") || fileName.Contains("_edecComplaintRequest_")))
			{
				resultList.Add(addParsingResult(attachment, true, false, fileName, fileMimeType));
			}
			if (fileMimeType == "application/pdf")
			{
				resultList.Add(addParsingResult(attachment, false, true, fileName, fileMimeType));
			}
		}
		return resultList;
	}

	public static string ParseSoapEnvelope(string text)
	{
		var root = XDocument.Parse(text);

		var ns = new XmlNamespaceManager(new NameTable());
		ns.AddNamespace((NoResString)"env", SoapEnvelopeNamespace);
		var targetNode = root.XPathSelectElement("//env:Envelope/env:Body/env:Fault/detail", ns)
			?? root.XPathSelectElement("//env:Envelope/env:Body", ns);

		return targetNode?.Elements().FirstOrDefault()?.ToString();
	}

	internal static string CutOffFirstSafe(string content, string trimString)
	{
		if (!string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(trimString))
		{
			var firstOccurrence = content.IndexOf(trimString);
			var lastOccurrence = content.LastIndexOf(trimString);

			if (firstOccurrence == lastOccurrence)
			{
				firstOccurrence = firstOccurrence <= content.Length / 2 ? firstOccurrence : -1;
				lastOccurrence = lastOccurrence > content.Length / 2 ? lastOccurrence : -1;
			}

			var startIndex = firstOccurrence >= 0 ? firstOccurrence + trimString.Length : 0;
			var length = lastOccurrence >= 0 ? lastOccurrence - startIndex : content.Length - startIndex;

			return content.Substring(startIndex, length);
		}
		else
		{
			return content;
		}
	}

	static string GetContentDescriptionFromHeader(string header)
	{
		var descriptionMatch = ContentDescriptionRegex.Match(header);
		return descriptionMatch.Success ? Path.GetFileNameWithoutExtension(descriptionMatch.Groups["value"].Value) : string.Empty;
	}

	static string GetBodyTextFromContent(string header, string content)
	{
		var encodinMatch = ContentTransferEncodingRegex.Match(header);
		return !IsContentBase64Encoded(encodinMatch) ? content : string.Empty;
	}

	static byte[] GetBodyDataFromContent(string header, string content)
	{
		var encodinMatch = ContentTransferEncodingRegex.Match(header);
		return IsContentBase64Encoded(encodinMatch) ? Convert.FromBase64String(content) : Array.Empty<byte>();
	}

	static bool IsContentBase64Encoded(Match encodingMatch) => encodingMatch.Success && encodingMatch.Groups["value"].Value.ToLower().Contains("base64");
}
