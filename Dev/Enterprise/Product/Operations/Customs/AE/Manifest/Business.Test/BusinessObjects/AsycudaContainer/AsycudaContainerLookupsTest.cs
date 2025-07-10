using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

class AsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
{
	[ExpectNoExceptions]
	public void TestAEContainerTemperatureUnitCodes()
	{
		var asycudaManifestHeader = Factory.New<AsycudaManifestHeader>();
		var container = asycudaManifestHeader.Containers.AddNew();
		var lookedUpList = container.Lookups.AEContainerTemperatureUnitCodes;
		var cachedCodes = Factory.GetCachedValue<AEContainerTemperatureUnitCodes>();

		CombineAssertions(() =>
		{
			NUnit.Framework.Assert.That(lookedUpList.CodesAsString, Is.EqualTo("C, F"), "AEContainerTemperatureUnitCodes values");
			NUnit.Framework.Assert.That(lookedUpList, Is.SameAs(cachedCodes), "AEContainerTemperatureUnitCodes is cached");
		});
	}
}
