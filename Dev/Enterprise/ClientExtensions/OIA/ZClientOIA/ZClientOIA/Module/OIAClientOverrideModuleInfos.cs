using Enterprise.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.OIA.Module
{
	/// <summary>
	/// A country generic GL module override.
	/// </summary>
	internal class OIAGLClientOverrideModuleInfo : ClientOverrideModuleInfo
	{
		public OIAGLClientOverrideModuleInfo()
			: base(new ClientOverrideModuleIdentifier(ModuleIDs.GLJournal), typeof(OIAGLJournalModule).Assembly.FullName,
			typeof(OIAGLJournalModule).FullName, string.Empty)
		{
		}
	}

	/// <summary>
	/// A work-around to bypass China override in core.
	/// </summary>
	internal class OIAGLChinaClientOverrideModuleInfo : ClientOverrideModuleInfo
	{
		public OIAGLChinaClientOverrideModuleInfo()
			: base(new ClientOverrideModuleIdentifier(ModuleIDs.GLJournal), typeof(OIAGLJournalModule).Assembly.FullName,
				typeof(OIAGLJournalModule).FullName, Constants.CountryCodes.China)
		{
		}
	}
}
