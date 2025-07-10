using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7ApplicationBusinessProvider))]
sealed class ApplicationBusinessProviderTest : EU.H7.Business.Testing.H7ApplicationBusinessProviderTest<H7ApplicationBusinessProvider, AsycudaManifestHeader>
{
	protected override AsycudaManifestHeader CreateNewManifest()
	{
		var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.Bills.AddNew();
		return header;
	}

	protected override IEnumerable<string> ExpectedCountryCodes => new[] { Core.Constants.CountryCodes.Italy };
}
