namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	using System.Linq;
	using System.Net.Http;
	using Enterprise.Client.EDI.TrustedMessaging.Business;
	using WTG.TrustedMessaging;

	public static class TrustedMessageResponseHelper
	{
		public static string ReadMessage(HttpResponseMessage response, EdiTrustedSystem trustedSystem)
		{
			var signHeader = response.Headers.GetValues("SIGNED").FirstOrDefault();
			var iVString = response.Headers.GetValues("WTG_I").FirstOrDefault();

			var trustedMessage = new TrustedMessage()
			{
				EncryptedContent = response.Content.ReadAsStringAsync().Result,
				Signature = signHeader,
				IV = iVString
			};

			var certProvider = new CertificatesProviderForTest() { IsServer = false };
			var messenger = new TrustedMessenger(certProvider);
			return messenger.ReadMessageContent(trustedMessage, trustedSystem.ETS_SecretKey);
		}
	}
}
