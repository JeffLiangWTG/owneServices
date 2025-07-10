using Enterprise.ZClientWebCargoWiseEDI;

namespace ZClientWebEDI.Test.WebApi.Controllers.IdentityAndSecurity
{
	public class SystemToSystemTrustHelperForTest : SystemToSystemTrustHelper
	{
		public SystemToSystemTrustHelperForTest(string authorityUrl) : base()
		{
			this.authorityUrl = authorityUrl;
		}

		readonly string authorityUrl;

		protected override string GetAuthorityUrlCore(NLogWrapper logger)
		{
			return authorityUrl;
		}
	}
}
