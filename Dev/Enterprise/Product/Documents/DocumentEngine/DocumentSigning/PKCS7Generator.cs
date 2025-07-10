using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Security.Certificates;
using Org.BouncyCastle.X509;
using Attribute = Org.BouncyCastle.Asn1.Cms.Attribute;

namespace Enterprise.DocumentEngine.DocumentSigning
{
	public static class PKCS7Generator
	{
		static Asn1EncodableVector GetEssV2(byte[] certificate)
		{
			var aaV2 = new Asn1EncodableVector();
			var algoId = new AlgorithmIdentifier(new DerObjectIdentifier(OID.DigestAlgorithmOid), null);
			aaV2.Add(algoId);

			var md = DigestUtilities.GetDigest("SHA256");
			var dig = new byte[md.GetDigestSize()];
			md.BlockUpdate(certificate, 0, certificate.Length);
			md.DoFinal(dig, 0);

			aaV2.Add(new DerOctetString(dig));
			return aaV2;
		}

		public static (byte[] PKCS7, byte[] SignableContent) GenerateSHA256PKCS7(byte[] sha256Hash, byte[] signingCertificate)
		{
			if (signingCertificate == null)
			{
				throw new ArgumentNullException("Certificate for PKCS7 cannot be null.");
			}

			X509Certificate cert;
			try
			{
				cert = new X509CertificateParser().ReadCertificate(signingCertificate);
			}
			catch (CertificateException)
			{
				throw new CertificateException("Certificate for PKCS7 cannot be parsed.");
			}

			var hashOid = OID.SHA256;
			var signedAttributesVector = new Asn1EncodableVector
			{
				new Attribute(attrType: new DerObjectIdentifier(OID.PKCS9AtContentType),
							 attrValues: new DerSet(new DerObjectIdentifier(OID.PKCS7IdData))),

				new Attribute(attrType: new DerObjectIdentifier(OID.PKCS9AtMessageDigest),
							attrValues: new DerSet(new DerOctetString(sha256Hash))),

				new Attribute(attrType: new DerObjectIdentifier(OID.PKCS7EssSigningCertificateV2),
							attrValues: new DerSet(new DerSequence(new DerSequence(new DerSequence(GetEssV2(cert.GetEncoded()))))))
			};

			var signedAttributes = new DerSet(signedAttributesVector);

			var signableContent = signedAttributes.GetDerEncoded();
			var digestSignature = new DerOctetString(new byte[256]);

			var digestSignatureAlgorithm = new AlgorithmIdentifier(
				algorithm: new DerObjectIdentifier(OID.PKCS1RsaEncryption),
				parameters: DerNull.Instance);

			var signerInfo = new SignerInfo(
				sid: new SignerIdentifier(new IssuerAndSerialNumber(cert.IssuerDN, cert.SerialNumber)),
				digAlgorithm: new AlgorithmIdentifier(
					algorithm: new DerObjectIdentifier(hashOid),
					parameters: DerNull.Instance),
				authenticatedAttributes: signedAttributes,
				digEncryptionAlgorithm: digestSignatureAlgorithm,
				encryptedDigest: digestSignature,
				unauthenticatedAttributes: null
			);

			var digestAlgorithmsVector = new Asn1EncodableVector
			{
				new AlgorithmIdentifier(
					algorithm: new DerObjectIdentifier(hashOid),
					parameters: DerNull.Instance)
			};

			var encapContentInfo = new ContentInfo(
				contentType: new DerObjectIdentifier(OID.PKCS7IdData),
				content: null);

			var certificatesVector = new Asn1EncodableVector
			{
				X509CertificateStructure.GetInstance(Asn1Object.FromByteArray(cert.GetEncoded()))
			};

			var signerInfosVector = new Asn1EncodableVector
			{
				signerInfo.ToAsn1Object()
			};

			var signedData = new SignedData(
				digestAlgorithms: new DerSet(digestAlgorithmsVector),
				contentInfo: encapContentInfo,
				certificates: new BerSet(certificatesVector),
				crls: null,
				signerInfos: new DerSet(signerInfosVector));

			var contentInfo = new ContentInfo(
				contentType: new DerObjectIdentifier(OID.PKCS7IdSignedData),
				content: signedData);

			return (contentInfo.GetDerEncoded(), signableContent);
		}

		static class OID
		{
			public const string PKCS9AtContentType = "1.2.840.113549.1.9.3";
			public const string PKCS9AtMessageDigest = "1.2.840.113549.1.9.4";
			public const string PKCS1RsaEncryption = "1.2.840.113549.1.1.1";
			public const string PKCS7IdData = "1.2.840.113549.1.7.1";
			public const string PKCS7IdSignedData = "1.2.840.113549.1.7.2";
			public const string SHA256 = "2.16.840.1.101.3.4.2.1";
			public const string PKCS7EssSigningCertificateV2 = "1.2.840.113549.1.9.16.2.47";
			public const string DigestAlgorithmOid = "1.2.840.10040.4.3";
		}
	}
}
