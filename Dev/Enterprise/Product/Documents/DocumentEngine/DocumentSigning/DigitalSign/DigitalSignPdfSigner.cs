using System.Security.Cryptography;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.DigitalSignature.DigitalSign
{
	class DigitalSignPdfSigner : TPdfSigner
	{
		public DigitalSignPdfSigner(DigitalSignClient client)
		{
			Client = client;
			SHA256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		}

		const int MaxSignatureLength = 1024 * 16;  //client default

		public override int EstimateLength() => MaxSignatureLength;

		public override byte[] GetSignature()
		{
			// TODO in next Work Item
			return null;
		}

		public override void Write(byte[] buffer, int offset, int count) => SHA256.AppendData(buffer, offset, count);

#if DEBUG
		internal
#endif
		readonly DigitalSignClient Client;
		readonly IncrementalHash SHA256;
	}
}
