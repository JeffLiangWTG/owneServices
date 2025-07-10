using System;

namespace CargoWise.Integration
{
	public interface IClientHookLoader
	{
		event EventHandler ClientHookChanged;
		IClientHook ClientHook { get; }
#if DEBUG
		IDisposable OverrideClientHookForTest(IClientHook clientHook, bool loggedIn = false, bool overwriteToClientHook = false);
#endif
	}
}
