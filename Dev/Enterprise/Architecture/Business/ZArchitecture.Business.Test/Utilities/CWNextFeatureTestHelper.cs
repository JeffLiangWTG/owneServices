using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business.Test
{
	public static class CWNextFeatureTestHelper
	{
		const string CWNextEnabledEnvVar = "CWNext_Enabled";
		public static IDisposable SetIsCWNextEnabled(bool isEnabled)
		{
			CWNextFeatureHelper.ResetIsCWNextEnabled();
			var oldValue = System.Environment.GetEnvironmentVariable(CWNextEnabledEnvVar);
			System.Environment.SetEnvironmentVariable(CWNextEnabledEnvVar, isEnabled.ToString());

			return new DisposableAction(() =>
			{
				System.Environment.SetEnvironmentVariable(CWNextEnabledEnvVar, oldValue);
				CWNextFeatureHelper.ResetIsCWNextEnabled();
			});
		}

		public static IDisposable EnableCWNext()
		{
			return SetIsCWNextEnabled(true);
		}

		public static IDisposable DisableCWNext()
		{
			return SetIsCWNextEnabled(false);
		}
	}
}
