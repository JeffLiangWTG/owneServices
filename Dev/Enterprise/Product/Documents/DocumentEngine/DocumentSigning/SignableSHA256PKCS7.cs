using System;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;
using static Enterprise.DocumentEngine.DigitalSignature.PdfSignatureEditor;

namespace Enterprise.DocumentEngine.DocumentSigning
{
	public class SignableSHA256PKCS7
	{
		public SignableSHA256PKCS7(byte[] sha256Hash)
		{
			SHA256Hash = sha256Hash;
		}

		public byte[] GetSignableContent(SignerType signerType, byte[] signingCertificate)
		{
			if (signerType != SignerType.BytesToSignedSHA256Hash)
			{
				throw new ArgumentOutOfRangeException(nameof(signerType), signerType, "");
			}

			(PKCS7, SignableContent) = PKCS7Generator.GenerateSHA256PKCS7(SHA256Hash, signingCertificate);
			return SignableContent;
		}

		public void Sign(SignerType signerType, byte[] signature)
		{
			if (signerType != SignerType.BytesToSignedSHA256Hash)
			{
				throw new ArgumentOutOfRangeException(nameof(signerType), signerType, "Not valid signerType");
			}

			if (PKCS7 != null && signature != null && signature.Length == 256 && PKCS7.Length > 256)
			{
				Buffer.BlockCopy(signature, 0, PKCS7, PKCS7.Length - 256, 256);
			}
			else
			{
				throw new InvalidSignatureSizeForPKCS7Exception((NoResString)"The signature size is invalid and cannot be copied in the PKCS7.");
			}
		}

		public byte[] ToByteArray() => PKCS7;

		readonly byte[] SHA256Hash;
		byte[] PKCS7;
		byte[] SignableContent;
	}
}
