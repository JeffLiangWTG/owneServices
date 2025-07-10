using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.MessageCalculators
{
	public static class TransportCountryCalculator
	{
		public static string TransportCountry(JobDeclaration declaration)
		{
			string result = string.Empty;
			if (declaration.IsSea)
			{
				if (declaration.Vessel != null)
				{
					result = declaration.Vessel.RV_RN_NKCountryOfReg;
				}
			}
			else
			{
				OrgHeader org = declaration.ShippingLine;
				if (org != null)
				{
					result = org.OH_RL_NKClosestPort.Left(2);
				}
			}
			return result;
		}
	}
}
