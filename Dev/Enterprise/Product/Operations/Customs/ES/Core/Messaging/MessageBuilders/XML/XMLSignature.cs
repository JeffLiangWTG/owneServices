using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders
{
	public static class XMLSignature
	{
		public static ZString Sign(XmlDocument xmlToSign, X509Certificate2 certificate)
		{
			var bodyNode = xmlToSign.LastChild.LastChild;
			var root = (XmlElement)bodyNode.FirstChild;
			var signatureElements = xmlToSign.GetElementsByTagName((NoResString)"Signature");
			if (signatureElements.Count > 0)
			{
				var oldSignature = signatureElements[0];
				root.RemoveChild(oldSignature);
			}
			var hashBody = CanonicalizeAndCalculateSHA1(root.OuterXml);

			var keyinfo = new KeyInfo();
			keyinfo.AddClause(new KeyInfoX509Data(certificate));
			keyinfo.Id = "CertificadoFirmante";
			var keyXml = keyinfo.GetXml();
			var hashKey = CanonicalizeAndCalculateSHA1(keyXml.OuterXml.Replace(@"xmlns=""http://www.w3.org/2000/09/xmldsig#""", ""));

			string signingDateTime = ZDateTime.Now.ToCustomsFormatString(CustomsDateTimeExtension.DateTimeFormatForSignature);
			var objectNode = CreateObjectNode(signingDateTime);
			var hashSignProperties = CanonicalizeAndCalculateSHA1(objectNode.FirstChild.FirstChild.OuterXml);

			var signatureDocument = new XmlDocument();
			var bodyName = bodyNode.FirstChild.LocalName;
			signatureDocument.LoadXml(EmptySignatureString(bodyName));

			var digestValues = signatureDocument.GetElementsByTagName("DigestValue");
			digestValues[0].InnerText = Convert.ToBase64String(hashBody);
			digestValues[1].InnerText = Convert.ToBase64String(hashKey);
			digestValues[2].InnerText = Convert.ToBase64String(hashSignProperties);

			var signingTimeElement = signatureDocument.GetElementsByTagName("etsi:SigningTime")[0];
			signingTimeElement.InnerText = signingDateTime;

			UTF8Encoding byteConverter = new UTF8Encoding();
			byte[] originalData = byteConverter.GetBytes(signatureDocument.OuterXml);

			using (RSA rsa = certificate.GetRSAPrivateKey())
			{
				byte[] signature = rsa.SignData(originalData, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
				var signedValueElement = signatureDocument.GetElementsByTagName("SignatureValue")[0];
				signedValueElement.InnerText = Convert.ToBase64String(signature);
			}

			var certificateElement = signatureDocument.GetElementsByTagName("X509Certificate")[0];
			certificateElement.InnerText = keyXml.GetElementsByTagName("X509Certificate")[0].InnerText;

			var finalSignatureNode = xmlToSign.ImportNode(signatureDocument.DocumentElement, true);
			root.AppendChild(finalSignatureNode);

			return xmlToSign.OuterXml;
		}

		static byte[] CanonicalizeAndCalculateSHA1(string xml)
		{
			var myDoc = new XmlDocument();
			myDoc.LoadXml(xml);
			var transform = new XmlDsigEnvelopedSignatureTransform();

			transform.LoadInput(myDoc);
			var document = (XmlDocument)transform.GetOutput(typeof(XmlDocument));
			var transform2 = new XmlDsigC14NWithCommentsTransform();

			transform2.LoadInput(document);
			var document2 = (Stream)transform2.GetOutput(typeof(Stream));
			return SHA1.Create().ComputeHash(document2);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Text expected")]
		static XmlElement CreateObjectNode(ZString signingDateTime)
		{
			string xmlContent = string.Format(@"<Object>
	<etsi:QualifyingProperties Target=""#Firma"" xmlns:etsi=""http://uri.etsi.org/01903/v1.2.2#"">
		<etsi:SignedProperties Id=""SignedProperties"">
			<etsi:SignedSignatureProperties>
				<etsi:SigningTime>{0}</etsi:SigningTime>
				<etsi:SignaturePolicyIdentifier>
					<etsi:SignaturePolicyId>
						<etsi:SigPolicyId>
							<etsi:Identifier>http://administracionelectronica.gob.es/es/ctt/politicafirma/politica_firma_AGE_v1_8.pdf</etsi:Identifier>
						</etsi:SigPolicyId>
						<etsi:SigPolicyHash>
							<DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
							<DigestValue>VYICYpNOjso9g1mBiXDVxNORpKk=</DigestValue>
						</etsi:SigPolicyHash>
					</etsi:SignaturePolicyId>
				</etsi:SignaturePolicyIdentifier>
			</etsi:SignedSignatureProperties>
		</etsi:SignedProperties>
	</etsi:QualifyingProperties>
</Object>", signingDateTime);
			XmlDocument objectDoc = new XmlDocument();
			objectDoc.LoadXml(xmlContent);
			return objectDoc.DocumentElement;
		}

		static string EmptySignatureString(ZString bodyName) => (NoResString)ZString.Format(@"<Signature Id=""Firma"" xmlns=""http://www.w3.org/2000/09/xmldsig#"">
	<SignedInfo>
		<CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315#WithComments""/>
		<SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/>
		<Reference URI=""#{0}"">
			<Transforms>
				<Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/>
			</Transforms>
			<DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
			<DigestValue></DigestValue>
		</Reference>
		<Reference URI=""#CertificadoFirmante"">
			<DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
			<DigestValue></DigestValue>
		</Reference>
		<Reference URI=""#SignedProperties"">
			<DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/>
			<DigestValue></DigestValue>
		</Reference>
	</SignedInfo>
	<SignatureValue></SignatureValue>
	<KeyInfo Id=""CertificadoFirmante"">
		<X509Data>
			<X509Certificate></X509Certificate>
		</X509Data>
	</KeyInfo>" + CreateObjectNode("").OuterXml + "</Signature>", bodyName);
	}
}
