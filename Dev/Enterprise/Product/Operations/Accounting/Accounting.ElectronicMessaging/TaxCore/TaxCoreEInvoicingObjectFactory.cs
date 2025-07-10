using System;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Fiji;
using Enterprise.Accounting.ElectronicMessaging.Samoa;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.TaxCore
{
	public static class TaxCoreEInvoicingObjectFactory
	{
		public static ITaxCoreCountryEInvoicingObjectFactory GetICountryEInvoicingObjectFactory(ZString countryCode) => GetGlobalEInvoicingObjectFactory(countryCode);

		static ITaxCoreCountryEInvoicingObjectFactory GetGlobalEInvoicingObjectFactory(ZString countryCode)
		{
			switch (countryCode)
			{
				case CountryCodes.Fiji:
					return new FijiEInvoicingObjectFactory();
				case CountryCodes.WesternSamoa:
					return new SamoaEInvoicingObjectFactory();
				default:
					throw new NotSupportedException(FormattableString.Invariant($"Country with code '{countryCode}' is not a TaxCore compliant country"));
			}
		}
	}
}
