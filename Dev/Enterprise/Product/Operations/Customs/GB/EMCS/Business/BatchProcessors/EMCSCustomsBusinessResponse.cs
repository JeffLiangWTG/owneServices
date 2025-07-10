using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.GB.EMCS
{
	public class EMCSCustomsBusinessResponse
	{
		readonly XmlDocument xmlDoc;

		public EMCSCustomsBusinessResponse(ZString xml)
		{
			try
			{
				Xml = xml;
				xmlDoc = new XmlDocument();
				xmlDoc.LoadXml(xml);
			}
			catch
			{
			}
		}

		public ZString Xml { get; }

		public ZString BodyXml
		{
			get
			{
				var responseBodyContent = xmlDoc.DocumentElement?.SelectSingleNode(ResponseBodyNodeXPath)?.InnerText ?? ZString.Empty;
				try
				{
					var jsonOrXml = DecodeResponse(responseBodyContent);
					try
					{
						var bodyXmlDoc = new XmlDocument();
						bodyXmlDoc.LoadXml(jsonOrXml);
						return jsonOrXml;
					}
					catch (XmlException)
					{
						var messageDataObject = GetMessageDataObject(jsonOrXml);
						var encodedMessage = messageDataObject?.encodedMessage ?? ZString.Empty;
						return DecodeResponse(encodedMessage);
					}
				}
				catch (SystemException ex) when (ex is FormatException || ex is ArgumentException)
				{
					return ZString.Empty;
				}
			}
		}

		ZString DecodeResponse(ZString encodedMessage)
		{
			var contentBytes = Convert.FromBase64String(encodedMessage);
			return Encoding.UTF8.GetString(contentBytes);
		}

		EMCSMessageDataObject GetMessageDataObject(string messageText)
		{
			var dataObjectList = new List<EMCSMessageDataObject>();

			try
			{
				dataObjectList = JsonSerializer.Deserialize<List<EMCSMessageDataObject>>(messageText);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// Nothing to do here, this may occur if we receive a text error instead of json
			}
			return dataObjectList.FirstOrDefault();
		}

		public ZString ServiceReference => xmlDoc.DocumentElement?.SelectSingleNode(ServiceReferenceNodeXPath)?.InnerText ?? ZString.Empty;
		public ZString EHubTrackingID => xmlDoc.DocumentElement?.SelectSingleNode(EHubTrackingIDNodeXPath)?.InnerText ?? ZString.Empty;
		public ZString JobNumber => xmlDoc.DocumentElement?.SelectSingleNode(JobNumberNodeXPath)?.InnerText ?? ZString.Empty;

		const string ServiceReferenceNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='ServiceReference']";
		const string EHubTrackingIDNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='eHubTrackingId']";
		const string JobNumberNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseHeader']/*[local-name()='JobNumber']";
		const string ResponseBodyNodeXPath = "//*[local-name()='GBCustomsBusinessResponse']/*[local-name()='ResponseBody']";
	}
}
