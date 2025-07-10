using CargoWise.Types;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public static class SecurityTraderCountryGroupHelper
	{
		public static SecurityTraderCountryGroup GetSecurityTraderCountryGroup(JobDocAddress jobDocAddress)
		{
			var result = SecurityTraderCountryGroup.None;

			if ((jobDocAddress?.E2_RN_NKCountryCode ?? ZString.Empty) != ZString.Empty)
			{
				if (jobDocAddress.Factory.IsCountryConsideredInEuForSafetyAndSecurity(jobDocAddress.E2_RN_NKCountryCode))
				{
					result = SecurityTraderCountryGroup.EuForSafetyAndSecurity;
				}
				else
				{
					var unLoco = jobDocAddress.Address?.RelatedPortCode ?? jobDocAddress.Address?.HeaderClosestPort;

					if (unLoco?.IsInNorthernIreland ?? false)
					{
						result = SecurityTraderCountryGroup.NorthernIreland;
					}
					else if (jobDocAddress.Factory.IsCountryEuOrCtCountry(jobDocAddress.E2_RN_NKCountryCode))
					{
						result = SecurityTraderCountryGroup.NotEU;
					}
				}
			}

			return result;
		}
	}
}
