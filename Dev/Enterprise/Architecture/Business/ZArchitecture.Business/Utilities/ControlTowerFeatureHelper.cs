using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.ZArchitecture.Business
{
	public static class ControlTowerFeatureHelper
	{
		public static bool IsControlTowerEnabled()
		{
#if DEBUG
			if (string.Equals(System.Environment.GetEnvironmentVariable("CTRLTOWER_Enabled"), "True", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (string.Equals(System.Environment.GetEnvironmentVariable("CTRLTOWER_Enabled"), "False", System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
#endif

			if (null != ObjectFactory.Get<IFeatureControlManager>()
								.GetFeatureData(LicenceFeatureCodeList.Codes.ControlTowerFeature))
			{
				return true;
			}

			return false;
		}
	}
}
