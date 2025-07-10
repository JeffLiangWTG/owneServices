using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	sealed class CountrySwitcher : IDisposable
	{
		readonly ZString previous;

		public CountrySwitcher(string countryCode)
		{
			previous = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
		}

		public void Dispose()
		{
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = previous;
		}
	}
}
