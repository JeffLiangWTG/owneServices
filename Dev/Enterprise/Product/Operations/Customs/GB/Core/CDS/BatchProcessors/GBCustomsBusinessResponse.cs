using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class GBCustomsBusinessResponse
	{
		readonly XmlDocument xmlDoc;

		public GBCustomsBusinessResponse(ZString xml)
		{
			Xml = xml;
			xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xml);
		}

		public GBCustomsBusinessResponse(ZGuid conversationId, ZString responseBody)
			: this(GetXML(conversationId, responseBody))
		{
		}

		public ZString ConversationId => SelectSingleNode(ConversationIDNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString RequestID => SelectSingleNode(RequestIDNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString ServiceReference => SelectSingleNode(ServiceReferenceNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString NotificationBoxId => SelectSingleNode(NotificationBoxIdNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString MessageId => SelectSingleNode(MessageIdNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString EHubTrackingId => SelectSingleNode(EHubTrackingIDNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString Provider => SelectSingleNode(ProviderNodeXPath)?.Value ?? ZString.Empty;

		public ZString CorrelationId => SelectSingleNode(CorrelationIdNodeXPath)?.InnerText ?? ZString.Empty;

		public ZString GetMessageSubType(LoggingInformation logger)
		{
			var result = ZString.Empty;

			try
			{
				var xml = new XmlDocument();
				xml.LoadXml(ResponseBodyXml);
				result = xml.FirstChild.Name;
				result = result.Right(4).StartsWith("0") ? result.Right(3) : result.Right(4).Left(3);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				logger.Log("Failed to parse the MessageSubType : " + ex.Message); // Log entries are not localized
			}

			return result;
		}

		public MetaData MetaData
		{
			get
			{
				ZString metaDataContent = SelectSingleNode(MetaDataNodeXPath, EnvelopeBodyMetaDataNodeXPath)?.OuterXml ?? ZString.Empty;

				return metaDataContent.IsEmpty
					? null
					: XmlObjectSerializer.Deserialize<MetaData>(metaDataContent);
			}
		}

		public ZString InventoryMessage => SelectSingleNode(EnvelopeBodyInventoryMessageNodeXPath)?.InnerText ?? ZString.Empty;

		public virtual ZString ResponseBodyXml => ResponseBodyGetter(x => x.InnerText.Trim());

		public ZString ResponseBodyJson => ResponseBodyGetter(x => x.InnerXml);

		ZString ResponseBodyGetter(Func<XmlNode, string> innerGetter)
		{
			var bodyPath = SelectSingleNode(ResponseBodyNodeXPath);
			var bodyAttributes = bodyPath?.Attributes;
			if (bodyAttributes != null)
			{
				foreach (XmlAttribute attribute in bodyAttributes)
				{
					if (attribute.Name.ToLower() == "encoding") // xml node attribute name.
					{
						var encoding = attribute.Value.ToLower();
						if (encoding == "base64") // xml node attribute name.
						{
							return Encoding.UTF8.GetString(Convert.FromBase64String(bodyPath.InnerText));
						}
						else // e.g. Encoding=""None"
						{
							return innerGetter.Invoke(bodyPath);
						}
					}
				}
			}
			return ZString.Empty;
		}

		public IEnumerable<Response> Responses => MetaData?.GetResponses() ?? Enumerable.Empty<Response>();

		public SynchronousResponse SynchronousResponse
		{
			get
			{
				ZString responseContent = SelectSingleNode(SynchronousResponseNodeXPath)?.OuterXml ?? ZString.Empty;

				return responseContent.IsEmpty
					? null
					: new SynchronousResponse(responseContent);
			}
		}

		public InventoryLinkingControlResponse InventoryLinkingControlResponse
		{
			get
			{
				ZString responseContent = SelectSingleNode(InventoryLinkingControlResponseNodeXPath, EnvelopeBodyInventoryLinkingControlResponseNodeXPath)?.OuterXml ?? ZString.Empty;

				return responseContent.IsEmpty
					? null
					: new InventoryLinkingControlResponse(responseContent);
			}
		}

		public InventoryLinkingMovementResponse InventoryLinkingMovementResponse
		{
			get
			{
				ZString responseContent = SelectSingleNode(InventoryLinkingMovementResponseNodeXPath, EnvelopeBodyInventoryLinkingMovementResponseNodeXPath)?.OuterXml ?? ZString.Empty;

				return responseContent.IsEmpty
					? null
					: new InventoryLinkingMovementResponse(responseContent);
			}
		}

		public InventoryLinkingMovementTotalsResponse InventoryLinkingMovementTotalsResponse
		{
			get
			{
				ZString responseContent = SelectSingleNode(InventoryLinkingMovementTotalsResponseNodeXPath, EnvelopeBodyInventoryLinkingMovementTotalsResponseNodeXPath)?.OuterXml ?? ZString.Empty;

				return responseContent.IsEmpty
					? null
					: new InventoryLinkingMovementTotalsResponse(responseContent);
			}
		}

		public InventoryLinkingQueryResponse InventoryLinkingQueryResponse
		{
			get
			{
				ZString responseContent = SelectSingleNode(InventoryLinkingQueryResponseNodeXPath, EnvelopeBodyInventoryLinkingQueryResponseNodeXPath)?.OuterXml ?? ZString.Empty;

				return responseContent.IsEmpty
					? null
					: new InventoryLinkingQueryResponse(responseContent);
			}
		}

		public DeclarationInfoResponse DeclarationInfoResponse
		{
			get
			{
				ZString responseContent = SelectSingleNode(DeclarationInfoResponseResponseNodeXPath)?.OuterXml ?? ZString.Empty;

				return responseContent.IsEmpty
					? null
					: DeclarationInfoResponse.GetResponse(responseContent);
			}
		}

		public ZString DocumentUploadConfirmationRoot
		{
			get
			{
				var result = ZString.Empty;
				try
				{
					var responseContent = SelectSingleNode(ResponseBodyNodeXPath)?.InnerXml;
					var xml = new XmlDocument();
					xml.LoadXml(responseContent);
					if (xml["Root", "hmrc:fileupload"] != null)
					{
						result = xml.InnerXml;
					}
				}
				catch
				{
				}
				return result;
			}
		}

		public ZString Xml { get; }

		static ZString GetXML(ZGuid conversationId, ZString responseBody)
		{
			return Invariant(
$@"<GBCustomsBusinessResponse>
<ResponseHeader>
	<ConversationID>{conversationId.ToAlphanumericOnlyString()}</ConversationID>
</ResponseHeader>
<ResponseBody>{responseBody}</ResponseBody>
</GBCustomsBusinessResponse>");
		}

		protected XmlNode SelectSingleNode(string xPath) => xmlDoc.DocumentElement?.SelectSingleNode(xPath);

		protected XmlNode SelectSingleNode(params string[] xPaths) => xPaths.Select(SelectSingleNode).FirstOrDefault(node => node != null);

		const string RequestIDNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='RequestID']";
		const string ServiceReferenceNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='ServiceReference']";
		const string ConversationIDNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='ConversationID']";
		const string EHubTrackingIDNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='eHubTrackingId']";
		const string NotificationBoxIdNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='NotificationBoxId']";
		const string MessageIdNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='MessageId']";
		const string CorrelationIdNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='CorrelationID']";
		const string ProviderNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/@Provider";

		const string ResponseBodyNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']";
		const string SynchronousResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='SynchronousResponse']";
		const string MetaDataNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='MetaData']";
		const string EnvelopeBodyMetaDataNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='MetaData']";
		const string InventoryLinkingControlResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='inventoryLinkingControlResponse']";
		const string InventoryLinkingMovementResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='inventoryLinkingMovementResponse']";
		const string InventoryLinkingMovementTotalsResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='inventoryLinkingMovementTotalsResponse']";
		const string InventoryLinkingQueryResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='inventoryLinkingQueryResponse']";
		const string DeclarationInfoResponseResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='DeclarationStatusResponse']";

		const string EnvelopeBodyInventoryMessageNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='InventoryMessage']";
		const string EnvelopeBodyInventoryLinkingControlResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='inventoryLinkingControlResponse']";
		const string EnvelopeBodyInventoryLinkingMovementResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='inventoryLinkingMovementResponse']";
		const string EnvelopeBodyInventoryLinkingMovementTotalsResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='inventoryLinkingMovementTotalsResponse']";
		const string EnvelopeBodyInventoryLinkingQueryResponseNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']/*[local-name()='Envelope']/*[local-name()='Body']/*[local-name()='inventoryLinkingQueryResponse']";
	}
}
