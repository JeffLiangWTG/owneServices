using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class CFSColumnsAndFiltersProviderTest : ZFilterStripControlTest
	{
		public void TestFilterAndColumnsOnlyVisibleForCanadianCompanies()
		{
			var filters = new ModuleFilterCollection();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				new CACFSShipmentModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);

				AssertNull("RNS Release Status filter", filters["RNS Release Status"]);
				AssertNull("RNS Release Date filter", filters["RNS Release Date"]);
				AssertNull("Arrival Certification Status filter", filters["Arrival Certification Status"]);
				AssertNull("Arrival Certification Date filter", filters["Arrival Certification Date"]);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				new CACFSShipmentModuleColumnsAndFiltersProvider().AddFilters(filters, Factory);

				AssertNotNull("RNS Release Status filter", filters["RNS Release Status"]);
				AssertNotNull("RNS Release Date filter", filters["RNS Release Date"]);
				AssertNotNull("Arrival Certification Status filter", filters["Arrival Certification Status"]);
				AssertNotNull("Arrival Certification Date filter", filters["Arrival Certification Date"]);
			}
		}
	}
}
