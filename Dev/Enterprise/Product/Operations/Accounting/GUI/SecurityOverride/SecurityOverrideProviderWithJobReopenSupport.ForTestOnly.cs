#if DEBUG

using Enterprise.Security;

namespace Enterprise.Accounting.GUI
{
	public partial class SecurityOverrideProviderWithJobReopenSupport
	{
		public string GetSecurityGrantedMessage_ForTestOnly(SecurityCheckpoint checkPoint)
		{
			return GetSecurityGrantedMessage(checkPoint);
		}

		public string GetReopenClosedJobSecurityOverrideMessage_ForTestOnly()
		{
			return GetReopenClosedJobSecurityOverrideMessage();
		}

		public string GetReopenRestrictedClosedJobSecurityOverrideMessage_ForTestOnly()
		{
			return GetReopenRestrictedClosedJobSecurityOverrideMessage();
		}

		public string GetReopenClosedJobSecurityGrantedMessage_ForTestOnly()
		{
			return GetReopenClosedJobSecurityGrantedMessage();
		}
	}
}

#endif
