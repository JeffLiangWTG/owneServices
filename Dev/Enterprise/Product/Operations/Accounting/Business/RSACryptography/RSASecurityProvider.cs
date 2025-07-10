using System.IO;
using System.Reflection;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Business.RSACryptography
{
	[CodeAlive("Used in Portugal document signing process")]
	public class RSASecurityProvider
	{
		const string publicKeyXmlResource = "Enterprise.Accounting.Business.RSACryptography.Resources.RSA-public-key.xml";

		public RSAEncryptor Encryptor
		{
			get
			{
				if (encryptor == null)
				{
					using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(publicKeyXmlResource))
					{
						using (var reader = new StreamReader(stream))
						{
							encryptor = new RSAEncryptor(reader.ReadToEnd());
						}
					}
				}
				return encryptor;
			}
		}
		RSAEncryptor encryptor;

		public RSADecryptor Decryptor
		{
			get
			{
				if (decryptor == null)
				{
					decryptor = new RSADecryptor(PrivateKeyXMLFromRegistry);
				}
				return decryptor;
			}
		}
		RSADecryptor decryptor;

		string PrivateKeyXMLFromRegistry
		{
			get
			{
				var registryValue = AccountingConfigurationRegistry.Instance.PortugalCertificationKey.Value;
				return (new AESCrypto()).DecryptStringAES(registryValue, AESCrypto.RANDOM_SHAREDSECRET);
			}
		}

		public RSASignatureSigner SignatureSigner
		{
			get
			{
				if (signatureSigner == null)
				{
					signatureSigner = new RSASignatureSigner(PrivateKeyXMLFromRegistry);
				}
				return signatureSigner;
			}
		}
		RSASignatureSigner signatureSigner;

		public RSASignatureVerifier SignatureVerifier
		{
			get
			{
				if (signatureVerifier == null)
				{
					using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(publicKeyXmlResource))
					{
						using (var reader = new StreamReader(stream))
						{
							signatureVerifier = new RSASignatureVerifier(reader.ReadToEnd());
						}
					}
				}
				return signatureVerifier;
			}
		}
		RSASignatureVerifier signatureVerifier;
	}
}
