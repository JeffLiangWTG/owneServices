#if DEBUG

using System.Security.Cryptography;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.TrustedMessaging.Business;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	class SecurityKeyTestHelper
	{
		public byte[] SecretKey
		{
			get
			{
				if (secretKey == null)
				{
					var aes = Aes.Create();
					aes.KeySize = 256;
					aes.Padding = PaddingMode.PKCS7;
					secretKey = aes.Key;
				}
				return secretKey;
			}
		}
		byte[] secretKey;

		public void SetSecretKey(EdiTrustedSystem trustedSystem)
		{
			trustedSystem.ETS_SecretKey = SecretKey;
		}

		public void SetSecretKey(LicenceDatabase database)
		{
			database.GetOrCreateTrustedSystem().ETS_SecretKey = SecretKey;
		}
	}
}

#endif
