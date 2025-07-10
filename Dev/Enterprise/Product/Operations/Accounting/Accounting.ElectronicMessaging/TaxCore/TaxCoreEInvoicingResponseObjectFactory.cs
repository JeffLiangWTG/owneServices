using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Fiji;
using Enterprise.Accounting.ElectronicMessaging.Samoa;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public static class TaxCoreEInvoicingResponseObjectFactory
	{
		public static ITaxCoreCountryEInvoicingResponseObjectFactory GetICountryEInvoicingResponseObjectFactory(ZString countryCode) => GetEInvoicingResponseObjectFactory(countryCode);

		static ITaxCoreCountryEInvoicingResponseObjectFactory GetEInvoicingResponseObjectFactory(ZString countryCode)
		{
			switch (countryCode)
			{
				case CountryCodes.Fiji:
					return new FijiEInvoicingResponseObjectFactory();
				case CountryCodes.WesternSamoa:
					return new SamoaEInvoicingResponseObjectFactory();
				default:
					return null;
			}
		}
	}
}
