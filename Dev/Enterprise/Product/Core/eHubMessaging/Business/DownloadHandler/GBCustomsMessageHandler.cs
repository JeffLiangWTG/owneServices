using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.eHub.Common.Extensions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.GBCustomsBusinessResponse)]
	public class GBCustomsMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			Message.MessageStream.SeekBegin();
			var xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(Message.MessageStream);
			}
			catch
			{
			}

			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = GetApplicationCode(xmlDocument);
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.GBCustoms;
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			interchange.SetEI_BodyTextSource(new StreamReaderSource(Message.MessageStream));

			return interchange;
		}

		ZString GetApplicationCode(XmlDocument xmlDocument)
		{
			var result = ApplicationCodeList.Codes.GbCustomsDeclarationServices;
			var provider = GetProvider(xmlDocument);
			if (provider == CTCProvider && IsPhase5Transaction(xmlDocument))
			{
				result = ApplicationCodeList.Codes.GbCustomsNCTS;
			}
			else
			{
				if (AttributeAndApplicationCodeMapping.TryGetValue(provider, out var applicationCode))
				{
					result = applicationCode;
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		string GetProvider(XmlDocument xmlDocument)
		{
			var provider = string.Empty;
			var headerAttributes = xmlDocument.DocumentElement?.SelectSingleNode(ResponseHeaderNodeXPath)?.Attributes;
			if (headerAttributes != null)
			{
				foreach (XmlAttribute attribute in headerAttributes)
				{
					if (attribute.Name == "Provider")
					{
						provider = attribute.Value;
						break;
					}
				}
			}

			if (string.IsNullOrEmpty(provider))
			{
				provider = xmlDocument.DocumentElement?.SelectSingleNode(ProviderNodeXPath)?.FirstChild?.Value ?? string.Empty;
			}

			return provider;
		}

		string GetServiceReference(XmlDocument xmlDocument) => xmlDocument.DocumentElement?.SelectSingleNode(ServiceReferenceNodeXPath)?.InnerText ?? string.Empty;

		bool IsPhase5Transaction(XmlDocument xmlDocument)
		{
			var isPhase5 = false;
			var serviceReference = GetServiceReference(xmlDocument);
			if (!string.IsNullOrEmpty(serviceReference))
			{
				isPhase5 = !int.TryParse(serviceReference, out _);
			}
			if (!isPhase5)
			{
				var body = xmlDocument.DocumentElement?.SelectSingleNode(ResponseBodyNodeXPath)?.InnerXml;
				if (!string.IsNullOrEmpty(body))
				{
					isPhase5 = Regex.IsMatch(body, @"PhaseID="" *NCTS5.*""");
				}
			}
			return isPhase5;
		}

		readonly Dictionary<string, string> AttributeAndApplicationCodeMapping = new Dictionary<string, string>()
		{
			{ "GVMS", ApplicationCodeList.Codes.GbCustomsGVMSManifest },
			{ CTCProvider, ApplicationCodeList.Codes.GbCommonTransitConvention },
			{ "ICSNI", ApplicationCodeList.Codes.GbMessageICSNorthernIreland },
			{ "ICSGB", ApplicationCodeList.Codes.GbMessageICSGreatBritain },
			{ "EMCS", ApplicationCodeList.Codes.GbCustomsEMCS },
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ResponseHeaderNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ProviderNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='Provider']";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ResponseBodyNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "const string")]
		const string ServiceReferenceNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='ServiceReference']";
		const string CTCProvider = "CTCGB";
	}
}
