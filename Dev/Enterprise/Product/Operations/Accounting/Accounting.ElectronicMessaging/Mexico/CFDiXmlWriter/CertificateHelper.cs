using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICertificateHelper
	{
		void SignCFDiXmlDocument(XmlDocument doc, EInvoicingCertificateCredential certificateCredential);
		(ZString Certificate, ZString Password) GetCredentialDataForCancellation(EInvoicingCertificateCredential certificateCredential);
	}

	class CertificateHelper : ICertificateHelper
	{
		#region SuppressResourceStringsCheckRegion

		void ICertificateHelper.SignCFDiXmlDocument(XmlDocument doc, EInvoicingCertificateCredential certificateCredential)
		{
			if (certificateCredential != null)
			{
				var serialNumberForMexico = TransformSerialNumberFromHexStringForMexico(certificateCredential.GP_CertificateSerialNumber.IsEmpty ? certificateCredential.SerialNumber : certificateCredential.GP_CertificateSerialNumber);
				doc.DocumentElement.SetAttribute("NoCertificado", serialNumberForMexico);

				var mexicoX509 = GetDecryptedCertificate(certificateCredential);
				doc.DocumentElement.SetAttribute("Certificado", Convert.ToBase64String(mexicoX509.GetRawCertData()));

				using var rsa = new RSACryptoServiceProvider();
				var securityProviderPrivateKey = mexicoX509.GetRSAPrivateKey().ToXmlString(true);
				rsa.FromXmlString(securityProviderPrivateKey);

				var cadenaOriginalCFDI = GenerateCadenaOriginal(doc);
				var data = Encoding.UTF8.GetBytes(cadenaOriginalCFDI);
				var signature = rsa.SignData(data, CryptoConfig.MapNameToOID("SHA256"));

				if (signature != null)
				{
					doc.DocumentElement.SetAttribute("Sello", Convert.ToBase64String(signature));
				}
			}
		}

		(ZString Certificate, ZString Password) ICertificateHelper.GetCredentialDataForCancellation(EInvoicingCertificateCredential certificateCredential)
		{
			if (certificateCredential != null)
			{
				var blobToByteCertificate = (byte[])certificateCredential.GP_Certificate;
				var certString = Convert.ToBase64String(blobToByteCertificate, Base64FormattingOptions.None);
				return (certString, certificateCredential.CurrentDecryptedCertificatePassphrase);
			}

			return (string.Empty, string.Empty);
		}

		#endregion

		static ZString GenerateCadenaOriginal(XmlDocument xml)
		{
			var xsltResource = "Enterprise.Accounting.ElectronicMessaging.Mexico.CFDiXmlWriter.Transformation.cadenaoriginal_4_0.xslt";
			Argument.NotNull(xml, "xmlDocument");

			var xsltStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(xsltResource);
			XmlDocument xsltDoc = new XmlDocument();
			xsltDoc.Load(xsltStream);

			var transform = new XslCompiledTransform();
			transform.Load(xsltDoc, null, new EmbeddedResourceXsltResolver());

			using (var sw = new StringWriter())
			{
				transform.Transform(xml, null, sw);
				return sw.ToString();
			}
		}

		static ZString TransformSerialNumberFromHexStringForMexico(ZString serialNumber)
		{
			if (serialNumber.Length > 20)
			{
				var result = new byte[serialNumber.Length / 2];
				for (int i = 0; i < result.Length; i++)
				{
					result[i] = byte.Parse(serialNumber.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
				}
				return Encoding.UTF8.GetString(result);
			}
			else
			{
				return serialNumber;
			}
		}

		static X509Certificate2 GetDecryptedCertificate(EInvoicingCertificateCredential certificateCredential)
		{
			var binaryCertificate = (byte[])certificateCredential.GP_Certificate;
			var passwordCertificate = certificateCredential.CurrentDecryptedCertificatePassphrase;
			return new X509Certificate2(binaryCertificate, passwordCertificate, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable);
		}
	}
}
