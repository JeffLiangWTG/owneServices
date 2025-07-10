using WTG.TrustedMessaging.Models;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	public class TrustedRequestForTest
	{
		public TrustedRequest Request { get; set; }

		public string Signature { get; set; }

		public string IV { get; set; }
	}
}