using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Manifest.Business.Testing;

[TestedType(typeof(AsycudaManifestHeaderLookups))]
sealed class AsycudaManifestHeaderLookupsTest : BusinessObjectLookupsTestCase
{
	[ExpectNoExceptions]
	public void TestCustomsOriginPortList()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		NUnit.Framework.Assert.That(header.Lookups.CustomsOriginPortList, Is.TypeOf<RefUNLOCOCollection>());
	}
}
