using System;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public static class CWNextFeatureHelper
	{
		public const string OptionForceCW1HomeScreen = "-ForceCW1HomeScreen";

		static Lazy<bool> isCWNextEnabled = new Lazy<bool>(IsCWNext);

		public static bool IsCWNextEnabled()
		{
			return isCWNextEnabled.Value;
		}

		public static void ResetIsCWNextEnabled()
		{
			isCWNextEnabled = new Lazy<bool>(IsCWNext);
		}

		/// <summary>
		/// Returns true if CWNext is enabled in the client license. False otherwise.
		/// ℹ️ Note: developers can set the CWNext environment variable; for local testing.
		/// </summary>
		static bool IsCWNext()
		{
			var arguments = CommandLineArguments.UsedToLaunchApplication;
			if (arguments != null && (bool)arguments.OptionalArgs[OptionForceCW1HomeScreen])
			{
				return false;
			}

#if DEBUG
			var envVar = System.Environment.GetEnvironmentVariable("CWNext_Enabled");
			if (envVar != null)
			{
				if (string.Equals(envVar, "True", System.StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				if (string.Equals(envVar, "False", System.StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
#endif

			return IsCWNextFeatureRulePresent();
		}

		static bool IsCWNextFeatureRulePresent()
			=> ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.CWNext) != null;
	}
}
