using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

[assembly: UniversalCustomsEDIMessagePacker(EDIMessage.ApplicationCodes.IC2, typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.EUICS2MessagePacker))]

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class EUICS2MessagePacker : IUniversalCustomsEDIMessagePacker
	{
		public bool AllowEmptyMessageBody => true;

		public ZString Pack(EDIMessage message, EDIInterchange interchange, LoggingInformation logger)
		{
			PopulateInterchange(message, interchange);
			logger.Log($"Message({message.EM_MessageNum}) packer finished successfully.");
			return ZString.Empty;
		}

		void PopulateInterchange(EDIMessage message, EDIInterchange interchange)
		{
			EDIMessagePackerUtils.PopulateInterchange(interchange, message.EM_ApplicationCode,
				message.EM_MessageText.IsEmpty ? EDIInterchangeTypeList.Codes.TST : message.EM_MessageType,
				message.ExternalPassword?.Company?.LicenceKeyIdentifier ?? GlbCompany.CurrentCompany.LicenceKeyIdentifier,
				MessageProviderHelper.MessageTo, message.EM_GB, message.EM_GP, ZGuid.NewZGuid(),
				receiveTransmit: message.EM_ReceiveTransmit);

			interchange.ContainedMessages.Add(message);
			PopulateEI_BodyData(interchange, message);

			message.EM_Status = EDIMessage.Status.Sent;
		}

		void PopulateEI_BodyData(EDIInterchange interchange, EDIMessage message)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);

			writer.Write(ConstructSoapHeaderMIMEText());
			writer.Flush();

			var attachments = new List<BinaryAttachment>();
			var attachmentContents = new List<string>();

			if (!message.EM_MessageText.IsEmpty)
			{
				var (body, attachment) = ConstructSoapHeaderMIMEAttachment(message);
				attachmentContents.Add(body);
				attachments.Add(attachment);
			}

			var binaryFiles = ExtractBinaryFilesFromMessage(message);
			foreach (var binaryFile in binaryFiles)
			{
				var (body, attachment) = ConstructDocMIMEAttachment(binaryFile);
				attachmentContents.Add(body);
				attachments.Add(attachment);
			}

			SignSoapMessage(interchange, binaryFiles, attachments).CopyTo(stream);

			foreach (var attachmentContent in attachmentContents)
			{
				writer.Write(attachmentContent);
			}

			writer.WriteLine();
			writer.WriteLine("--MIME_boundary--");

			writer.Flush();

			interchange.SetEI_BodyDataSource(new StreamSource(stream));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "MIME strings")]
		string ConstructSoapHeaderMIMEText()
		{
			var result = new ZStringBuilder();

			result.AppendLine("MIME-Version: 1.0 ");
			result.AppendLine("Content-Type: Multipart/Related; boundary=\"MIME_boundary\" ");
			result.AppendLine();
			result.AppendLine("--MIME_boundary");
			result.AppendLine("Content-Type: Application/soap+xml; charset=UTF-8");
			result.AppendLine("Content-ID: <SOAP_ENVELOPE> ");
			result.AppendLine();

			return result.ToString();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Name space strings")]
		Stream SignSoapMessage(EDIInterchange interchange, IReadOnlyCollection<IBinaryFile> binaryFiles, IEnumerable<BinaryAttachment> attachments)
		{
			const string BinarySecurityTokenReference = "X509-Cert00";

			var xmlDocument = GetSoapHeaderBuilder(interchange, binaryFiles).GetSoapStreamXmlDocument();
			var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
			nsmgr.AddNamespace("eb", "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/");
			nsmgr.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");

			var credential = GetCertificate(interchange);
			if (credential != null)
			{
				var wssSecurityNode = xmlDocument.CreateElement("wss:Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
				wssSecurityNode.SetAttribute("xmlns:wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");
				wssSecurityNode.SetAttribute("mustUnderstand", nsmgr.LookupNamespace("soap"), "true");
				var wssBinarySecurityTokenNode = xmlDocument.CreateElement("wss", "BinarySecurityToken", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
				wssBinarySecurityTokenNode.SetAttribute("EncodingType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary");
				wssBinarySecurityTokenNode.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509PKIPathv1");
				wssBinarySecurityTokenNode.SetAttribute("Id", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd", BinarySecurityTokenReference);

				var x509Certificate = new X509Certificate2(credential.GP_Certificate, credential.CurrentDecryptedCertificatePassphrase);
				wssBinarySecurityTokenNode.InnerXml = GetPKIPath(x509Certificate);
				wssSecurityNode.AppendChild(wssBinarySecurityTokenNode);
				xmlDocument.DocumentElement.ChildNodes[0].AppendChild(wssSecurityNode);

				var signedXml = new SignedXmlWithId(xmlDocument);
				signedXml.SigningKey = x509Certificate.GetRSAPrivateKey();
				signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;
				(signedXml.SignedInfo.CanonicalizationMethodObject as XmlDsigExcC14NTransform).InclusiveNamespacesPrefixList = "soap";
				signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;

				var keyInfo = new KeyInfo();
				var keyNode = xmlDocument.CreateElement("wss", "SecurityTokenReference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
				keyNode.SetAttribute("xmlns:wsse11", "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd");
				keyNode.SetAttribute("TokenType", "http://docs.oasis-open.org/wss/oasis-wss-wssecurity-secext-1.1.xsd", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509PKIPathv1");
				var keyReferenceNode = xmlDocument.CreateElement("wss", "Reference", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");
				keyReferenceNode.SetAttribute("URI", "#" + BinarySecurityTokenReference);
				keyReferenceNode.SetAttribute("ValueType", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509PKIPathv1");
				keyNode.AppendChild(keyReferenceNode);
				keyInfo.AddClause(new KeyInfoNode { Value = keyNode });
				signedXml.KeyInfo = keyInfo;

				var referenceMessaging = new Reference() { Uri = "#Messaging", DigestMethod = SignedXml.XmlDsigSHA256Url };
				SetIdToReference(xmlDocument.SelectSingleNode("//eb:Messaging", nsmgr) as XmlElement, "Messaging");
				referenceMessaging.AddTransform(new XmlDsigExcC14NTransform());
				var referenceBody = new Reference() { Uri = "#Body", DigestMethod = SignedXml.XmlDsigSHA256Url };
				SetIdToReference(xmlDocument.SelectSingleNode("//soap:Body", nsmgr) as XmlElement, "Body");
				referenceBody.AddTransform(new XmlDsigExcC14NTransform());
				signedXml.AddReference(referenceMessaging);
				signedXml.AddReference(referenceBody);

				foreach (var attachment in attachments)
				{
					if (attachment.ID != null && attachment.Data != null)
					{
						var referenceAttachment = new Reference(ToStream(attachment.Data)) { Uri = BinaryFileProvider.ContentIDPrefix + attachment.ID, DigestMethod = SignedXml.XmlDsigSHA256Url };
						referenceAttachment.AddTransform(new NoTransform() { Algorithm = "http://docs.oasis-open.org/wss/oasis-wss-SwAProfile-1.1#Attachment-Content-Signature-Transform" });
						signedXml.AddReference(referenceAttachment);
					}
				}

				signedXml.ComputeSignature();
				var signature = signedXml.GetXml();
				wssSecurityNode.AppendChild(signature);
			}

			return ToStream(xmlDocument);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Element strings")]
		void SetIdToReference(XmlElement element, string id)
		{
			const string wssNameSpace = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
			element.SetAttribute("xmlns:wsu", wssNameSpace);
			element.SetAttribute("Id", wssNameSpace, id);
		}

		string GetPKIPath(X509Certificate cert)
		{
			var x509Asn1 = cert.Export(X509ContentType.Cert);
			var x509LengthBytes = BitConverter.IsLittleEndian ? BitConverter.GetBytes((ushort)x509Asn1.Length).Reverse().ToArray() : BitConverter.GetBytes((ushort)x509Asn1.Length);
			x509LengthBytes = x509LengthBytes.SkipWhile(b => b == 0).ToArray();
			if (x509LengthBytes.Length > 1)
			{
				var pkiPathAsn1 = new byte[x509Asn1.Length + 2 + x509LengthBytes.Length];
				pkiPathAsn1[0] = 0x30;
				pkiPathAsn1[1] = (byte)(x509LengthBytes.Length + 0x80);
				Array.Copy(x509LengthBytes, 0, pkiPathAsn1, 2, x509LengthBytes.Length);
				Array.Copy(x509Asn1, 0, pkiPathAsn1, 2 + x509LengthBytes.Length, x509Asn1.Length);
				return Convert.ToBase64String(pkiPathAsn1);
			}
			else
			{
				var pkiPathAsn1 = new byte[x509Asn1.Length + 1 + x509LengthBytes.Length];
				pkiPathAsn1[0] = 0x30;
				Array.Copy(x509LengthBytes, 0, pkiPathAsn1, 1, x509LengthBytes.Length);
				Array.Copy(x509Asn1, 0, pkiPathAsn1, 2, x509Asn1.Length);
				return Convert.ToBase64String(pkiPathAsn1);
			}
		}

		#region MIME Attachment

		(string attachmentMIMEBody, BinaryAttachment Attachment) ConstructDocMIMEAttachment(CommonBinaryFile binaryFile)
		{
			var id = binaryFile.Identification;
			var fileName = binaryFile.Filename;
			var fileType = binaryFile.MIME;
			var fileData = binaryFile.Doc.ImageData;

			return ConstructMIMEAttachmentCore(id, fileName, fileType, fileData);
		}

		(string attachmentMIMEBody, BinaryAttachment Attachment) ConstructSoapHeaderMIMEAttachment(EDIMessage message)
		{
			var id = $"attachment1@{MessageProviderHelper.DomainName}";
			var fileName = $"IE3{message.EM_MessageType}.xml";
			var fileType = "application/gzip";
			var fileData = GetMessageBinary(message.EM_MessageText);

			return ConstructMIMEAttachmentCore(id, fileName, fileType, fileData);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "MIME strings")]
		(string attachmentMIMEBody, BinaryAttachment Attachment) ConstructMIMEAttachmentCore(string id, string fileName, string fileType, byte[] fileData)
		{
			var result = new ZStringBuilder();
			var attachmentText = Convert.ToBase64String(fileData);

			result.AppendLine();
			result.AppendLine();
			result.AppendLine("--MIME_boundary");
			result.AppendLine($"Content-Type: {fileType}");
			result.AppendLine($"Content-ID: <{id}>");
			result.AppendLine($"Content-Disposition: attachment; filename={fileName} ");
			result.AppendLine(string.Format("Content-Transfer-Encoding: base64"));
			result.AppendLine();
			result.AppendLine(attachmentText);

			return (result.ToString(), new BinaryAttachment { Data = fileData, ID = id });
		}

		byte[] GetMessageBinary(string datastring)
		{
			MemoryStream cms = null;
			try
			{
				cms = new MemoryStream();
				System.IO.Compression.GZipStream gzip = null;
				try
				{
					gzip = new System.IO.Compression.GZipStream(cms, System.IO.Compression.CompressionMode.Compress);
					var bytes = Encoding.UTF8.GetBytes(datastring);
					gzip.Write(bytes, 0, bytes.Length);
				}
				finally
				{
					if (gzip != null)
					{
						gzip.Close();
					}
				}
				return cms.ToArray();
			}
			finally
			{
				cms?.Close();
			}
		}

		Stream ToStream(byte[] binary)
		{
			var stream = new MemoryStream();
			var writer = new BinaryWriter(stream);
			writer.Write(binary);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		Stream ToStream(XmlDocument xml)
		{
			var signedSoapStream = new MemoryStream();
			var xmlWriter = new XmlTextWriter(signedSoapStream, null);
			xml.WriteTo(xmlWriter);
			xmlWriter.Flush();
			signedSoapStream.Position = 0;
			return signedSoapStream;
		}

		#endregion

		#region Binary Files

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Element strings")]
		IReadOnlyCollection<CommonBinaryFile> ExtractBinaryFilesFromMessage(EDIMessage message)
		{
			var result = new List<CommonBinaryFile>();

			if (message.EM_LinkedObject is IDocManagerSupport docManagerSupport)
			{
				using (var reader = message.GetEM_MessageTextReader())
				{
					XDocument document;

					try
					{
						document = XDocument.Load(reader);
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						document = null;
					}

					var binaryFileElements = document != null
						? document.XPathSelectElements("//*[local-name()='binaryFile']").Concat(document.XPathSelectElements("//*[local-name()='binaryAttachment']"))
						: Enumerable.Empty<XElement>();

					if (binaryFileElements.Any())
					{
						var collections = EDocsHelper.GetEDocCollections(docManagerSupport).ToArray();

						foreach (var binaryFileElement in binaryFileElements)
						{
							var binaryFile = new CommonBinaryFile
							{
								Identification = FindValue(binaryFileElement, "identification").Replace(BinaryFileProvider.ContentIDPrefix, string.Empty),
								Filename = FindValue(binaryFileElement, "filename"),
								MIME = FindValue(binaryFileElement, "MIME"),
								Description = FindValue(binaryFileElement, "description")
							};

							if (Guid.TryParse(binaryFile.Identification, out var docKey))
							{
								binaryFile.Doc = collections
									.Select((IStorageDocsBaseCollection c) => c.GetFromUniqueKey(docKey))
									.FirstOrDefault((IeDoc c) => c != null);

								result.Add(binaryFile);
							}
						}
					}
				}
			}

			return result;
		}

		static string FindValue(XElement element, string localName)
		{
			return element.Elements().FirstOrDefault(c => c.Name.LocalName.EqualIgnoringOrder(localName))?.Value ?? string.Empty;
		}

		sealed class CommonBinaryFile : IBinaryFile
		{
			public string Identification { get; set; }

			public string Filename { get; set; }

			public string MIME { get; set; }

			public string Description { get; set; }

			public IeDoc Doc { get; set; }
		}

		sealed class BinaryAttachment
		{
			public string ID { get; set; }

			public byte[] Data { get; set; }
		}

		#endregion

		protected virtual SoapHeaderBuilder GetSoapHeaderBuilder(EDIInterchange interchange, IReadOnlyCollection<IBinaryFile> binaryFiles)
		{
			var message = interchange.ContainedMessages[0];
			return new SoapHeaderBuilder(new SoapHeaderProvider(message.EM_LinkedObject as AsycudaManifestHeader, interchange, binaryFiles));
		}

		protected virtual GlbExternalPasswordWithCertificate GetCertificate(EDIInterchange interchange) => (interchange.ContainedMessages[0].EM_LinkedObject as AsycudaManifestHeader).ICS2Credential;

		class NoTransform : Transform
		{
			public override Type[] InputTypes => new[] { typeof(Stream) };

			public override Type[] OutputTypes => new[] { typeof(Stream) };

			public override object GetOutput() => inputStream;

			public override object GetOutput(Type type)
			{
				throw new NotImplementedException();
			}

			public override void LoadInnerXml(XmlNodeList nodeList)
			{
				throw new NotImplementedException();
			}

			public override void LoadInput(object obj)
			{
				inputStream = (Stream)obj;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Element and attribute strings")]
			protected override XmlNodeList GetInnerXml()
			{
				XmlDocument xmlDocument = new XmlDocument();
				XmlElement xmlElement = xmlDocument.CreateElement("Transform", "http://www.w3.org/2000/09/xmldsig#");
				if (!string.IsNullOrEmpty(base.Algorithm))
				{
					xmlElement.SetAttribute("Algorithm", base.Algorithm);
				}
				return xmlElement.ChildNodes;
			}

			Stream inputStream;
		}

		class SignedXmlWithId : SignedXml
		{
			public SignedXmlWithId(XmlDocument xml) : base(xml)
			{
			}

			public SignedXmlWithId(XmlElement xmlElement)
				: base(xmlElement)
			{
			}

			public override XmlElement GetIdElement(XmlDocument doc, string id)
			{
				// check to see if it's a standard ID reference
				XmlElement idElem = base.GetIdElement(doc, id);

				if (idElem == null)
				{
					XmlNamespaceManager nsManager = new XmlNamespaceManager(doc.NameTable);
					nsManager.AddNamespace((NoResString)"wsu", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd");

					idElem = doc.SelectSingleNode((NoResString)"//*[@wsu:Id=\"" + id + "\"]", nsManager) as XmlElement;
				}

				return idElem;
			}
		}
	}
}
