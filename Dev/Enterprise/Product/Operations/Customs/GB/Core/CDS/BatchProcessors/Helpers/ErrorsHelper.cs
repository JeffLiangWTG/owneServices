using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using CargoWise.Customs.GB.MessageDefinitions.CDS;
using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS
{
	class ErrorsHelper
	{
		public ErrorsHelper(string messageText, string xpathPrefix)
		{
			XmlDocument = new XmlDocument();
			XmlDocument.LoadXml(messageText);

			errorResponses = new List<CDSErrorResponse>();
			var errors = XmlDocument.SelectNodes(xpathPrefix + ErrorXPathSuffix);
			ProcessErrors(errorResponses, errors);
		}

		public XmlDocument XmlDocument { get; }

		public IReadOnlyList<CDSErrorResponse> ErrorResponses => errorResponses;

		void ProcessErrors(List<CDSErrorResponse> errorResponses, XmlNodeList errors)
		{
			foreach (XmlNode error in errors)
			{
				ProcessError(errorResponses, error);
			}
		}

		void ProcessError(List<CDSErrorResponse> errorResponses, XmlNode error)
		{
			var errorMessage = (ZString)(error.SelectSingleNode(MessageXPath)?.InnerText ?? string.Empty);
			var messageType = errorMessage.Substring(0, errorMessage.IndexOf(':'));
			try
			{
				switch (messageType)
				{
					case CVCPatternValid:
						ProcessCVCPatterValidError(messageType, errorResponses, errorMessage);
						break;
					case CVCComplexType:
					case CVCEnumerationValid:
					case CVCAttribute:
					default:
						ProcessGeneralError(messageType, errorResponses, errorMessage);
						break;
				}
			}
			catch
			{
				AddDefaultErrorRowWithVerbatimText(errorResponses, errorMessage);
			}
		}

		static void AddDefaultErrorRowWithVerbatimText(List<CDSErrorResponse> errorResponses, ZString errorMessage)
		{
			errorResponses.Add(new CDSErrorResponse { ErrorReason = errorMessage });
		}

		void ProcessGeneralError(ZString messageType, List<CDSErrorResponse> errorResponses, ZString errorMessage)
		{
			var error = errorMessage.SubstringSafe(messageType.Length + 1).Trim();
			AddDefaultErrorRowWithVerbatimText(errorResponses, error);
		}

		void ProcessCVCPatterValidError(string messageType, List<CDSErrorResponse> errorResponses, string errorMessage)
		{
			var split = errorMessage.Split(new string[] { AnonErrorType }, StringSplitOptions.RemoveEmptyEntries);
			if (split.Length == 1 && errorMessage.IndexOf(ForType, StringComparison.OrdinalIgnoreCase) > -1)
			{
				split = errorMessage.Split(new string[] { ForType }, StringSplitOptions.RemoveEmptyEntries);
			}
			if (split.Length == 2)
			{
				var complexType = split[1].Replace("'.", string.Empty).Replace("'", string.Empty).Trim();

				var xPath = string.Format(CultureInfo.InvariantCulture, WCONameXPath, complexType);
				var wcoName = new PointerParser().GetWCONameFromXPath(xPath);

				var firstSplit = split[0];
				var errorReasonSplit = ((ZString)split[0]).SubstringSafe(firstSplit.IndexOf("Value", StringComparison.OrdinalIgnoreCase)).ToString()?.Replace("for type '", string.Empty)?.Trim();

				errorResponses.Add(new CDSErrorResponse { FieldInError = wcoName, ErrorReason = errorReasonSplit });
			}
			else
			{
				ProcessGeneralError(messageType, errorResponses, errorMessage);
			}
		}

		const string WCONameXPath = @"//*[local-name()='element'][@type='ds:{0}']//*[local-name()='documentation']/*[local-name()='WCOName']/text()";
		const string AnonErrorType = "#AnonType_";
		const string MessageXPath = "message";
		const string CVCPatternValid = "cvc-pattern-valid";
		const string CVCComplexType = "cvc-complex-type.2.2";
		const string CVCEnumerationValid = "cvc-enumeration-valid";
		const string CVCAttribute = "cvc-attribute.3";
		const string ForType = "for type";
		const string ErrorXPathSuffix = "/errors/error";

		readonly List<CDSErrorResponse> errorResponses;
	}
}
