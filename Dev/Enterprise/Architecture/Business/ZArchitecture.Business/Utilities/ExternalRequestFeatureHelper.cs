using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.ZArchitecture.Business
{
	public static class ExternalRequestFeatureHelper
	{
		public static bool IsExternalRequestEnabled()
		{
#if DEBUG
			if (string.Equals(System.Environment.GetEnvironmentVariable("EXTREQ_Enabled"), "True", System.StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			else if (string.Equals(System.Environment.GetEnvironmentVariable("EXTREQ_Enabled"), "False", System.StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
#endif

			if (null != ObjectFactory.Get<IFeatureControlManager>()
								.GetFeatureData(LicenceFeatureCodeList.Codes.ExternaRequestFeature))
			{
				return true;
			}

			return false;
		}
	}
}
