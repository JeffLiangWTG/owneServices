using System.Linq;
using System.Security.Cryptography;
using FlexCel.Pdf;

namespace Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1
{
	class EMudhraPdfSigner : TPdfSigner
	{
		public EMudhraPdfSigner(EMudhraClient client)
		{
			Client = client;
			SHA256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
		}

		const int MaxSignatureLength = 1024 * 16;  //eMudhra default

		public override int EstimateLength() => MaxSignatureLength;

		public override byte[] GetSignature()
		{
			var pdfHash = SHA256.GetHashAndReset();
			var sig = Client.Sign(pdfHash);
			if (sig?.Value?.Any() ?? false)
			{
				var result = new byte[MaxSignatureLength];
				sig.Value.CopyTo(result, 0);
				return result;
			}
			else
			{
				//TODO:report error?
			}

			return null;
		}

		public override void Write(byte[] buffer, int offset, int count) => SHA256.AppendData(buffer, offset, count);

#if DEBUG
		internal
#endif
		readonly EMudhraClient Client;
		readonly IncrementalHash SHA256;
	}
}
