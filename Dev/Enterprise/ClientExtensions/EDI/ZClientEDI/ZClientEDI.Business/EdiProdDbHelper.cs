using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;

namespace ZClientEDI.Business
{
	public static class EdiProdDbHelper
	{
		public static bool IsRunningOnEdiProdDatabase
		{
			get
			{
				var result = false;
				var ediProdLicenceIdentifier = SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value;

				if (!string.IsNullOrEmpty(ediProdLicenceIdentifier) && ediProdLicenceIdentifier.Length == 9)
				{
					var ediProdEnterpriseCode = ediProdLicenceIdentifier.Substring(0, 3);
					var ediProdServerCode = ediProdLicenceIdentifier.Substring(6, 3);

					var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
					var systemEntperpriseCode = registrationKey.EnterpriseCode;
					var systemServerCode = registrationKey.ServerCode;

					result = ((ZString)ediProdEnterpriseCode).EqualsIgnoringCase(systemEntperpriseCode) && ((ZString)ediProdServerCode).EqualsIgnoringCase(systemServerCode);
				}
				return result;
			}
		}
	}
}
