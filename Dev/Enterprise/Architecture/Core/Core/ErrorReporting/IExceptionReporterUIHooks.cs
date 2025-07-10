namespace Enterprise.ZArchitecture.Core
{
	public interface IExceptionReporterUIHooks
	{
		void HookThreadSpecificUnhandledExceptions();
		void UnHookUnhandledExceptions();
	}
}
