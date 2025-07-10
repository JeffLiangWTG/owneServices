using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.IL.Business.Constants;

[assembly: UniversalCustomsEDIMessagePacker(EDIMessage.ApplicationCodes.ILCustoms, typeof(Enterprise.Customs.IL.Business.ILMessagePacker))]

namespace Enterprise.Customs.IL.Business
{
	public class ILMessagePacker : IUniversalCustomsEDIMessagePacker
	{
		public bool AllowEmptyMessageBody => false;

		public ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
		{
			try
			{
				PopulateInterchange(message, interchange);
				logger.Log($"Message({message.EM_MessageNum}) packer finished successfully.");
				return ZString.Empty;
			}
			catch (Exception ex)
			{
				logger.LogError($"Message({message.EM_MessageNum}) packer failed:{ex.Message}");
				return ex.Message;
			}
		}

		protected virtual ZGuid CreateNewSessionGUID()
		{
			return ZGuid.NewZGuid();
		}

		void PopulateInterchange(EDIMessage message, EDIInterchange interchange)
		{
			EDIMessagePackerUtils.PopulateInterchange(
				interchange,
				message.EM_ApplicationCode,
				message.EM_MessageType,
				message.ExternalPassword?.Company?.LicenceKeyIdentifier ?? GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				MessageProviderHelper.MessageTo,
				message.EM_GB,
				message.EM_GP,
				CreateNewSessionGUID(),
				receiveTransmit: message.EM_ReceiveTransmit,
				transportType: EDIInterchangeTransportTypeList.Codes.xT);

			interchange.ContainedMessages.Add(message);

			var dictionary = GetAttributesDictionary(message);
			dictionary.Add(Enterprise.xTMessaging.Shared.Constants.CustomMsgAttributes.MessageSubType, message.EM_MessageSubType);
			interchange.SetHeaderTextWithAttributeDictionary(dictionary);

			PopulateEI_BodyData(interchange, message);

			message.EM_Status = EDIMessage.Status.Sent;
		}

		static Dictionary<string, string> GetAttributesDictionary(EDIMessage message)
		{
			if (!message.EM_GP.IsEmpty)
			{
				var credential = message.ExternalPassword;
			}

			return new Dictionary<string, string>();
		}

		void PopulateEI_BodyData(EDIInterchange interchange, EDIMessage message)
		{
			var soapMessage = CreateSoapEnvelopeWithRawBody(interchange, message);
			var soapStream = ToStream(soapMessage);
			interchange.SetEI_BodyDataSource(new StreamSource(soapStream));
		}

		static XDocument CreateSoapEnvelopeWithRawBody(EDIInterchange interchange, EDIMessage message)
		{
			XNamespace soap12 = "http://www.w3.org/2003/05/soap-envelope";
			XNamespace requestHeader = "http://MalamTeam.Inf.ESB.Schemas.RequestHeader";

			var rawBodyXml = message.EM_MessageText;
			// Parse the raw XML string into an XElement
			XElement bodyContent = XElement.Parse(rawBodyXml);

			// Extract the namespace URI from the root element of the raw body XML
			XNamespace myns = bodyContent?.Name.NamespaceName;

			// Create the envelope element
			var envelope = myns != null && !string.IsNullOrEmpty(myns.NamespaceName)
				? new XElement(soap12 + (NoResString)"Envelope",
					new XAttribute(XNamespace.Xmlns + (NoResString)"soap12", soap12),
					new XAttribute(XNamespace.Xmlns + (NoResString)"myns", myns),
					new XAttribute(XNamespace.Xmlns + (NoResString)"mal", requestHeader))
				: new XElement(soap12 + (NoResString)"Envelope",
					new XAttribute(XNamespace.Xmlns + (NoResString)"soap12", soap12),
					new XAttribute(XNamespace.Xmlns + (NoResString)"mal", requestHeader));

			// Create and add the header element
			var header = new XElement(soap12 + (NoResString)"Header",
				new XElement(requestHeader + (NoResString)"RequestHeader",
					new XElement(requestHeader + (NoResString)"ServiceName", MessageServiceNameProvider.GetServiceName(message.EM_MessageSubType)),
					new XElement(requestHeader + (NoResString)"SoftwareProvider", CustomsRequestHeader.SoftwareProvider),
					new XElement(requestHeader + (NoResString)"SoftwareVersion", CustomsRequestHeader.SoftwareVersion),
					new XElement(requestHeader + (NoResString)"WSDLVersion", CustomsRequestHeader.WSDLVersion),
					new XElement(requestHeader + (NoResString)"ExternalId", interchange.EI_SessionGUID),
					new XElement(requestHeader + (NoResString)"ConsumerId", message.ExternalPassword?.GP_MailBoxID)
				)
			);
			envelope.Add(header);

			// Create and add the body element, incorporating the raw XML string
			var body = new XElement(soap12 + (NoResString)"Body", bodyContent);
			envelope.Add(body);

			// Create the XDocument and return the string
			var soapDocument = new XDocument(new XDeclaration("1.0", "utf-8", null), envelope);
			return soapDocument;
		}

		Stream ToStream(XDocument xml)
		{
			var signedSoapStream = new MemoryStream();
			var xmlWriter = new XmlTextWriter(signedSoapStream, null);
			xml.WriteTo(xmlWriter);
			xmlWriter.Flush();
			signedSoapStream.Position = 0;
			return signedSoapStream;
		}
	}
}
