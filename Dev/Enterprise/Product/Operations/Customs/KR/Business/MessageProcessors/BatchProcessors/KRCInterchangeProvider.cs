using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.InterchangeProviders;
using Constants = Enterprise.Customs.KR.Business.SoapHeaderBuilder.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class KRCInterchangeProvider : InterchangeProviderBase
	{
		public const string UserID = "Custom.KR.UserID";
		public const string RandomValueEncoded = "Custom.KR.RandomValueEncoded";

		public KRCInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override string InstructionHowToSetInterchangeSenderID => ZString.Empty;

		protected override string GetCollationKey(Enterprise.Messaging.Business.EDIMessage message) => DoNotCollateType;

		protected override Type InterchangeType => typeof(EDIInterchange);

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			var message = messages.OfType<EDIMessage>().Single();
			interchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			interchange.EI_SessionGUID = ZGuid.NewZGuid();
			SetInterchangeValuesForTransmit(interchange, messages, message.EM_MessageType, EDIInterchange.InterchangePartyIDs.KRCustomsMailbox, GetLicenceCode());
			PopulateEI_BodyData(interchange, message);
		}

		void PopulateEI_BodyData(Enterprise.Messaging.Business.EDIInterchange interchange, EDIMessage message)
		{
			var isSoapHeaderWithPayload = interchange.EI_InterchangeType != Messaging.Constants.EDIInterchangeType.DLT &&
										interchange.EI_InterchangeType != Messaging.Constants.EDIInterchangeType.DOC;
			var password = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).CertificateForUnipass;
			var boundary = ZDateTime.Now.ToString(Messaging.Constants.DateFormatType.DateWithMicroSeconds);
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(ConstructSoapHeaderMIMEText(isSoapHeaderWithPayload, password, boundary));
			writer.Flush();

			var ediMessageStream = isSoapHeaderWithPayload ? SignPayload(message, password) : null;
			SignSoapMessage(interchange, password, ediMessageStream).CopyTo(stream);

			if (isSoapHeaderWithPayload)
			{
				writer.Write(ConstructPayloadMIMEText(boundary));
				writer.Flush();
				ediMessageStream.Position = 0;
				ediMessageStream.CopyTo(stream);
				writer.Write($"\r\n--{boundary}--");
				writer.Flush();
			}
			interchange.SetEI_BodyDataSource(new StreamSource(stream));
		}

		string ConstructSoapHeaderMIMEText(bool isSoapHeaderWithPayload, GlbCompanyCredential password, string boundary)
		{
			var soapHeaderMIMEText = new ZStringBuilder();
			soapHeaderMIMEText.AppendLine(Constants.HttpElements.SoapAction);

			if (password != null && password.CertificateStatus == GlbExternalPasswordWithCertificate.CertificateLoaded)
			{
				var userID = MessageEncoding.UTF8WithoutBOM.GetBytes(password.GP_Name);
				var userIDEncoded = Convert.ToBase64String(userID, 0, userID.Length);
				var randomValueEncoded = CertificateManager.EncodeRandomValue(CertificateManager.GetRawRandomValueAndLocalKeyID(password)?.Item1);
				soapHeaderMIMEText.AppendLine($"Authorization: Basic {userIDEncoded}:{randomValueEncoded}");
			}

			if (isSoapHeaderWithPayload)
			{
				soapHeaderMIMEText.AppendLine(Constants.HttpElements.ContentTypeMultipart + $" boundary=\"{boundary}\"");
				soapHeaderMIMEText.AppendLine($"\r\n--{boundary}");
				soapHeaderMIMEText.AppendLine(Constants.HttpElements.ContentTypeText);
				soapHeaderMIMEText.AppendLine(Constants.HttpElements.ContentIdSoapPart);
				soapHeaderMIMEText.AppendLine();
			}
			else
			{
				soapHeaderMIMEText.AppendLine(Constants.HttpElements.ContentTypeText);
				soapHeaderMIMEText.AppendLine();
			}
			return soapHeaderMIMEText.ToString();
		}

		string ConstructPayloadMIMEText(string boundary)
		{
			var payloadMIMEText = new ZStringBuilder();
			payloadMIMEText = new ZStringBuilder();
			payloadMIMEText.AppendLine($"\r\n--{boundary}");
			payloadMIMEText.AppendLine(Constants.HttpElements.ContentTypeText);
			payloadMIMEText.AppendLine(Constants.HttpElements.ContentIdPayload);
			payloadMIMEText.AppendLine();
			return payloadMIMEText.ToString();
		}

		Stream SignPayload(EDIMessage message, GlbCompanyCredential password)
		{
			var ediMessageStream = new MemoryStream();
			var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
			xmlDocument.Load(message.GetEM_MessageDataReader());
			SignManager.Sign(xmlDocument, password);
			InsertLineBreaksToBase64String(xmlDocument, tagsToModify);
			xmlDocument.Save(ediMessageStream);
			ediMessageStream.Position = 0;
			return ediMessageStream;
		}

		Stream SignSoapMessage(Enterprise.Messaging.Business.EDIInterchange interchange, GlbCompanyCredential password, Stream ediMessageStream)
		{
			var soapMessage = new SoapHeaderBuilder(interchange).GenerateMessage();
			var soapStream = EnvelopeSerializeAsStream(soapMessage);
			var xmlDocument = new XmlDocument() { PreserveWhitespace = true };
			xmlDocument.Load(soapStream);
			SignManager.Sign(xmlDocument, password, ediMessageStream);
			InsertLineBreaksToBase64String(xmlDocument, tagsToModify);
			var signedSoapStream = new MemoryStream();
			xmlDocument.Save(signedSoapStream);
			signedSoapStream.Position = 0;
			return signedSoapStream;
		}

		readonly List<string> tagsToModify = new List<string>()
		{
			$"{Constants.Prefixes.ds}:{Constants.SignedXmlElementNames.SignatureValue}",
			$"{Constants.Prefixes.ds}:{Constants.SignedXmlElementNames.Modulus}",
			$"{Constants.Prefixes.ds}:{Constants.SignedXmlElementNames.X509Certificate}"
		};

		void InsertLineBreaksToBase64String(XmlDocument xmlDocument, List<string> tagNames)
		{
			foreach (var tag in tagNames)
			{
				InsertLineBreaksToBase64String(xmlDocument, tag);
			}
		}

		void InsertLineBreaksToBase64String(XmlDocument xmlDocument, string tagName)
		{
			var elements = xmlDocument.GetElementsByTagName(tagName);
			foreach (XmlNode element in elements)
			{
				var originalBase64SignatureValue = Convert.FromBase64String(element.InnerText);
				element.InnerText = Convert.ToBase64String(originalBase64SignatureValue, Base64FormattingOptions.InsertLineBreaks);
			}
		}

		protected override ZString GetFooterText(Enterprise.Messaging.Business.EDIInterchange interchange, NonDependentEDIMessageCollection messages) => string.Empty;

		string GetLicenceCode()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			return registrationKey.EnterpriseCode + registrationKey.ServerCode;
		}

		public static Stream EnvelopeSerializeAsStream<T>(T xmlObject)
		{
			var namespaces = new XmlSerializerNamespaces();
			namespaces.Add(SoapHeaderBuilder.Constants.Prefixes.SOAP, SoapHeaderBuilder.Constants.Namespaces.SOAP);
			namespaces.Add(SoapHeaderBuilder.Constants.Prefixes.xlink, SoapHeaderBuilder.Constants.Namespaces.xlink);
			namespaces.Add(SoapHeaderBuilder.Constants.Prefixes.xsi, SoapHeaderBuilder.Constants.Namespaces.xsi);

			var settings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

			return SoapHeaderBuilder.SerializeAsStream(xmlObject, settings, namespaces);
		}
	}
}
