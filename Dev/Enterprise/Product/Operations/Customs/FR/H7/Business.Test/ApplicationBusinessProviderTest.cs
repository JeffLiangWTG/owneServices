using System.Collections.Generic;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.FR.H7.Business.Testing
{
	[TestedType(typeof(H7ApplicationBusinessProvider))]
	sealed class ApplicationBusinessProviderTest : EU.H7.Business.Testing.H7ApplicationBusinessProviderTest<H7ApplicationBusinessProvider, H7ManifestHeader>
	{
		protected override H7ManifestHeader CreateNewManifest()
		{
			var header = Factory.NewWithValidTestData<H7ManifestHeader>();
			header.Bills.AddNew();
			return header;
		}

		protected override IEnumerable<string> ExpectedCountryCodes => new[] { Core.Constants.CountryCodes.France };

		protected override ZString ExpectedPackedItemTariffDataGrouping => Core.Constants.CountryCodes.France;
	}
}
