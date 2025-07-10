using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Enterprise.MasterFiles.Business;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Enterprise.Customs.KR.Business
{
	public static class CertificateManager
	{
		public static string EncodeRandomValue(byte[] randomValue)
		{
			var result = string.Empty;
			var publicKeyData = KRCustomsRegistry.Instance.CustomsCertificate.Value;
			if (publicKeyData != null)
			{
				var certificate = new X509Certificate2(publicKeyData);
				var rsa = certificate.GetRSAPublicKey();
				var encryptedRvalue = rsa.Encrypt(randomValue,RSAEncryptionPadding.Pkcs1);
				result = Convert.ToBase64String(encryptedRvalue, 0, encryptedRvalue.Length);
			}
			return result;
		}

		public static Tuple<byte[], string> GetRawRandomValueAndLocalKeyID(GlbExternalPassword glbExternalPassword)
		{
			var password = glbExternalPassword.CurrentDecryptedCertificatePassphrase.ToString().ToCharArray();
			var asn1Object = Asn1Object.FromByteArray(glbExternalPassword.GP_Certificate);
			var pfx = Pfx.GetInstance(asn1Object);
			var info = pfx.AuthSafe;

			if (info.ContentType.Equals(PkcsObjectIdentifiers.Data))
			{
				var content = Asn1OctetString.GetInstance(info.Content);
				var authSafe = AuthenticatedSafe.GetInstance(content.GetOctets());
				var contentInfos = authSafe.GetContentInfo();

				foreach (var contentInfo in contentInfos)
				{
					var oid = contentInfo.ContentType;
					byte[] octets = null;
					if (oid.Equals(PkcsObjectIdentifiers.Data))
					{
						octets = Asn1OctetString.GetInstance(contentInfo.Content).GetOctets();
					}
					else if (oid.Equals(PkcsObjectIdentifiers.EncryptedData))
					{
						if (password != null)
						{
							var data = EncryptedData.GetInstance(contentInfo.Content);
							octets = CryptPbeData(false, data.EncryptionAlgorithm, password, data.Content.GetOctets());
						}
					}

					if (octets != null)
					{
						var seq = Asn1Sequence.GetInstance(octets);
						foreach (var subSeq in seq)
						{
							var bag = SafeBag.GetInstance(subSeq);
							PrivateKeyInfo privateKeyInfo = null;
							if (bag.BagID.Equals(PkcsObjectIdentifiers.Pkcs8ShroudedKeyBag))
							{
								privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(password, false, EncryptedPrivateKeyInfo.GetInstance(bag.BagValue));
							}
							else if (bag.BagID.Equals(PkcsObjectIdentifiers.KeyBag))
							{
								privateKeyInfo = PrivateKeyInfo.GetInstance(bag.BagValue);
							}

							if (privateKeyInfo != null && privateKeyInfo.Attributes != null)
							{
								if (privateKeyInfo.Attributes.Count != 1)
								{
									return null;
								}
								var keyAttributeSeq = (Asn1Sequence)privateKeyInfo.Attributes[0].ToAsn1Object();
								if (keyAttributeSeq.Count == 2 && keyAttributeSeq[0] is DerObjectIdentifier idRandomNumOID)
								{
									if (idRandomNumOID.ToString() == oidKrKisaNpki)
									{
										var derset = (DerSet)keyAttributeSeq[1];
										var derString = (DerBitString)derset[0];
										var bagAttributeSeq = (Asn1Sequence)bag.BagAttributes[0].ToAsn1Object();
										if (bagAttributeSeq.Count == 2 && bagAttributeSeq[1] is DerSet localKeyID)
										{
											return new Tuple<byte[], string>(derString.GetOctets(), localKeyID[0].ToString());
										}
									}
								}
							}
						}
					}
				}
			}
			return null;
		}

		static byte[] CryptPbeData(bool forEncryption, AlgorithmIdentifier algId, char[] password, byte[] data)
		{
			var cipher = PbeUtilities.CreateEngine(algId) as IBufferedCipher
				?? throw new Exception("Unknown encryption algorithm: " + algId.Algorithm);

			if (algId.Algorithm.Equals(PkcsObjectIdentifiers.IdPbeS2))
			{
				var pbeParameters = PbeS2Parameters.GetInstance(algId.Parameters);
				var cipherParams = PbeUtilities.GenerateCipherParameters(algId.Algorithm, password, pbeParameters);
				cipher.Init(forEncryption, cipherParams);
				return cipher.DoFinal(data);
			}
			else
			{
				var pbeParameters = Pkcs12PbeParams.GetInstance(algId.Parameters);
				var cipherParams = PbeUtilities.GenerateCipherParameters(algId.Algorithm, password, false, pbeParameters);
				cipher.Init(forEncryption, cipherParams);
				return cipher.DoFinal(data);
			}
		}

		const string oidKrKisaNpki = "1.2.410.200004.10.1.1.3";
	}
}
