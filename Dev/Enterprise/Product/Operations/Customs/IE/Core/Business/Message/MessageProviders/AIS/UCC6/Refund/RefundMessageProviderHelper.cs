using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.Environment;

namespace Enterprise.Customs.IE.Business
{
	public static class RefundMessageProviderHelper
	{
		public static string GetNewRFApplicationReferenceId(BusinessObjectFactory factory)
		{
			return Env.NumberFountains.GetIENumberFountain("IEAISRFApplicationReferenceId").GetNext(factory).ToString(CultureInfo.InvariantCulture).PadLeft(22, '0');
		}

		public static string GetNewRDApplicationReferenceId(BusinessObjectFactory factory)
		{
			return Env.NumberFountains.GetIENumberFountain("IEAISRDApplicationReferenceId").GetNext(factory).ToString(CultureInfo.InvariantCulture).PadLeft(18, '0');
		}
	}
}
