using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Enterprise.ZArchitecture.Core;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

namespace Enterprise.Messaging.Business.EDICommunicationAuthInbound
{
	public static class CertificateUtility
	{
		#region Validation

		public static bool IsCsrValid(string csrPem)
		{
			return TryConvertCsrFromPem(csrPem, out Pkcs10CertificationRequest csr);
		}

		#endregion

		#region CovertFromPem

		static bool TryConvertCsrFromPem(string pem, out Pkcs10CertificationRequest csr)
		{
			try
			{
				using (var stringReader = new StringReader(pem))
				{
					var pemReader = new PemReader(stringReader);
					csr = (Pkcs10CertificationRequest)pemReader.ReadObject();

					if (csr == null)
					{
						return false;
					}

					var restOfTheContent = pemReader.Reader.ReadToEnd();
					return string.IsNullOrWhiteSpace(restOfTheContent);
				}
			}
			catch
			{
				csr = null;
				return false;
			}
		}

		public static bool TryReadCertificate(byte[] certificateByteArray, out X509Certificate2 certificate, out string pem, out Exception exception)
		{
			exception = null;
			pem = string.Empty;
			try
			{
				certificate = new X509Certificate2(certificateByteArray);

				if (certificateByteArray == null || certificate == null)
				{
					exception = new Exception((NoResString)"Input value is null");
					certificate = null;
					return false;
				}

				var builder = new StringBuilder();
				var certBytes = certificate.Export(X509ContentType.Cert);
				var base64Encoded = Convert.ToBase64String(certBytes);
				for (var i = 0; i < base64Encoded.Length; i += 64)
				{
					builder.Append(base64Encoded.Substring(i, Math.Min(64, base64Encoded.Length - i)));
					builder.Append((NoResString)"\r\n");
				}

				builder.Insert(0, (NoResString)"-----BEGIN CERTIFICATE-----\r\n");
				builder.Append((NoResString)"-----END CERTIFICATE-----");
				pem = builder.ToString();

				return true;
			}
			catch (Exception ex)
			{
				exception = new Exception($"Error in parsing the certificate: {ex.Message}");
				certificate = null;
				return false;
			}
		}

		#endregion

		#region Certificate Data
		public static string GetCertificateSubjectCN(X509Certificate2 certificate)
		{
			return GetCertificateField(certificate.Subject, (NoResString)"CN");
		}

		public static string GetCertificateIssuerCN(X509Certificate2 certificate)
		{
			return GetCertificateField(certificate.Issuer, (NoResString)"CN");
		}

		static string GetCertificateField(string certificateAttribute, string field)
		{
			var distinguishedFields = certificateAttribute.Split(',');

			foreach (var distinguishedField in distinguishedFields)
			{
				if (distinguishedField.Trim().StartsWith(field + (NoResString)"="))
				{
					return distinguishedField.Trim().Substring(field.Length + 1);
				}
			}

			return string.Empty;
		}

		#endregion
	}
}
