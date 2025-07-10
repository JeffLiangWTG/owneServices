#if DEBUG

using Enterprise.Security;

namespace Enterprise.Accounting.GUI
{
	public partial class GLJournalSecurityOverrideProvider
	{
		public SecurityCore RequestLoginCredentials_ForTestOnly(SecurityCheckpoint checkPoint)
		{
			return RequestLoginCredentials(checkPoint);
		}

		public string GetSecurityOverrideMessageCore_ForTestOnly(SecurityCheckpoint checkPoint)
		{
			return GetSecurityOverrideMessageCore(checkPoint);
		}
	}
}

#endif
