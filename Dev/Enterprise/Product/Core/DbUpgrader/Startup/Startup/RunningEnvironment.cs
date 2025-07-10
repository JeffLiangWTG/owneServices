using CargoWise.Common;

namespace Enterprise.DbUpgrader.Startup
{
	static class RunningEnvironment
	{
		public static bool IsDebugMode
		{
			get
			{
#if DEBUG
				return OverridableDebugModeIfDebug.Value;
#else
				return false;
#endif
			}
		}

		internal static readonly Overridable<bool> OverridableDebugModeIfDebug = new Overridable<bool>(true);
	}
}
