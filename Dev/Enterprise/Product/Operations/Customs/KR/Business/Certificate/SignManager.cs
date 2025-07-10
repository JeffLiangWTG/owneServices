using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using CargoWise.Customs.KR.MessageDefinitions.SoapEnvelope.SOAP;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Customs.KR.Business.SoapHeaderBuilder.Constants;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("Soon to be used")]
	public static class SignManager
	{
		public static void Sign(XmlDocument xmlDocument, GlbExternalPassword password, Stream messageData = null)
		{
			(var x509Certificate2, var rsa) = SignManager.GetKeyInfo(password);
			if (x509Certificate2 != null && rsa != null)
			{
				var signedXml = new SignedXml(xmlDocument);
				signedXml.SigningKey = rsa;
				signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigCanonicalizationWithCommentsUrl;
				signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA256Url;
				signedXml.KeyInfo = CreateKeyInfo(x509Certificate2, rsa);
				signedXml.AddReference(CreateReference());
				if (messageData != null)
				{
					signedXml.AddReference(CreateReference(messageData));
				}
				signedXml.ComputeSignature();
				var signature = signedXml.GetXml();
				signature.SetPrefix(Constants.Prefixes.ds);
				xmlDocument.InsertSignature(signature);
				PostProsessingAfterSigning(xmlDocument, rsa);
			}
		}

		static void PostProsessingAfterSigning(XmlDocument xmlDocument, RSA rsa)
		{
			var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
			nsmgr.AddNamespace(Constants.Prefixes.ds, SignedXml.XmlDsigNamespaceUrl);
			var signedInfo = xmlDocument.SelectNodes($"//{Constants.Prefixes.ds}:{Constants.SignedXmlElementNames.SignedInfo}", nsmgr);
			var transform = new XmlDsigC14NWithCommentsTransform();
			transform.LoadInput(signedInfo);
			var transformedStream = (MemoryStream)transform.GetOutput(typeof(MemoryStream));
			var signedInfoXmlDocument = new XmlDocument();
			signedInfoXmlDocument.LoadXml(System.Text.Encoding.UTF8.GetString(transformedStream.ToArray()));
			foreach (XmlNode child in signedInfo[0].ChildNodes)
			{
				signedInfoXmlDocument.DocumentElement.AppendChild(signedInfoXmlDocument.ImportNode(child, true));
			}
			transform.LoadInput(signedInfoXmlDocument);
			transformedStream = (MemoryStream)transform.GetOutput(typeof(MemoryStream));
			var hashedSignedInfo = SHA256.Create().ComputeHash(transformedStream.ToArray());
			var rsaPkcs1SignatureFormatter = new RSAPKCS1SignatureFormatter(rsa);
			rsaPkcs1SignatureFormatter.SetHashAlgorithm("SHA256");
			var signature = rsaPkcs1SignatureFormatter.CreateSignature(hashedSignedInfo);
			var signatureEncoded = Convert.ToBase64String(signature);
			var signatureValueNode = xmlDocument.SelectSingleNode($"//{Constants.Prefixes.ds}:{Constants.SignedXmlElementNames.SignatureValue}", nsmgr);
			signatureValueNode.InnerXml = signatureEncoded;
		}

		static (X509Certificate2, RSA) GetKeyInfo(GlbExternalPassword password)
		{
			(X509Certificate2, RSA) result = (null, null);
			var store = new Pkcs12Store(password.GetGP_CertificateReader(), password.CurrentDecryptedCertificatePassphrase.ToString().ToCharArray());
			ZString localKeyIDRaw = CertificateManager.GetRawRandomValueAndLocalKeyID(password)?.Item2;
			var localKeyID = localKeyIDRaw.RemoveNonNumericCharacters();
			var alias = GetAlias(store, localKeyID);
			if (!string.IsNullOrEmpty(alias))
			{
				var certEntry = store.GetCertificateChain(alias).GetValue(0) as X509CertificateEntry;
				var bcX509Certificate = certEntry.Certificate;
				var key = store.GetKey(alias).Key;

				var rsaPriv = DotNetUtilities.ToRSA((Org.BouncyCastle.Crypto.Parameters.RsaPrivateCrtKeyParameters)key);

				var x509Certificate2 = new X509Certificate2(bcX509Certificate.GetEncoded(), password.CurrentDecryptedCertificatePassphrase, X509KeyStorageFlags.Exportable);

				result = (x509Certificate2, rsaPriv);
			}
			return result;
		}

		static string GetAlias(Pkcs12Store store, string keyID)
		{
			foreach (var item in store.Aliases)
			{
				var itemStr = item.ToString();
				if (itemStr == keyID)
				{
					return itemStr;
				}
			}
			return string.Empty;
		}

		static void InsertSignature(this XmlDocument xmlDocument, XmlElement signature)
		{
			if (xmlDocument.DocumentElement.LocalName == nameof(Envelope))
			{
				var header = xmlDocument.DocumentElement[Constants.SoapObjectLocalNames.SOAP.Header, Constants.Namespaces.SOAP];
				header.InsertAfter(signature, header[Constants.SoapObjectLocalNames.eb.SyncReply, Constants.Namespaces.eb]);
			}
			else
			{
				xmlDocument.DocumentElement.InsertAfter(signature, xmlDocument.DocumentElement.LastChild);
			}
		}

		static Reference CreateReference(Stream messageData = null)
		{
			Reference reference;
			var isSoapHeaderWithPayload = messageData != null;
			if (isSoapHeaderWithPayload)
			{
				reference = new Reference(messageData) { Uri = (NoResString)"cid:payload-1", DigestMethod = SignedXml.XmlDsigSHA256Url };
			}
			else
			{
				reference = new Reference() { Uri = "", DigestMethod = SignedXml.XmlDsigSHA256Url };
			}

			reference.AddTransform(new XmlDsigC14NTransform());
			if (!isSoapHeaderWithPayload)
			{
				reference.AddTransform(CreateTransform((NoResString)"not(ancestor-or-self::Signature)"));
			}
			return reference;
		}

		static string GetContentID(XmlDocument xmlDocument)
		{
			var result = string.Empty;
			if (xmlDocument != null)
			{
				var bodyElement = xmlDocument.DocumentElement[Constants.SoapObjectLocalNames.SOAP.Body, Constants.Namespaces.SOAP];
				var manifestElement = bodyElement?[Constants.SoapObjectLocalNames.eb.Manifest, Constants.Namespaces.eb];
				var refereceElement = manifestElement?[Constants.SoapObjectLocalNames.eb.Reference, Constants.Namespaces.eb];
				result = refereceElement?.Attributes[Constants.SoapObjectLocalNames.xlink.href, Constants.Namespaces.xlink]?.Value ?? string.Empty;
			}
			return result;
		}

		static Transform CreateTransform(string xPathString)
		{
			Transform result = null;
			if (!string.IsNullOrEmpty(xPathString))
			{
				var xPath = new XmlDocument().CreateElement(Constants.SoapObjectLocalNames.XPath);
				xPath.InnerText = xPathString;
				result = new XmlDsigXPathTransform();
				result.LoadInnerXml(xPath.SelectNodes("."));
			}
			return result;
		}

		static KeyInfo CreateKeyInfo(X509Certificate2 certificate, RSA rsa)
		{
			var keyInfo = new KeyInfo();
			keyInfo.AddClause(new RSAKeyValue(rsa));
			var keyinfoX509 = new KeyInfoX509Data();
			keyinfoX509.AddCertificate(certificate);
			keyinfoX509.AddIssuerSerial(certificate.Issuer, certificate.SerialNumber);
			keyinfoX509.AddSubjectName(certificate.SubjectName.Name);
			keyInfo.AddClause(keyinfoX509);
			return keyInfo;
		}

		static void SetPrefix(this XmlNode node, string prefix)
		{
			if (string.IsNullOrEmpty(node.Prefix))
			{
				node.Prefix = prefix;
				foreach (XmlNode childNode in node.ChildNodes)
				{
					SetPrefix(childNode, prefix);
				}
			}
		}
	}
}
