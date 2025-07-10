using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaContainerLookups))]
sealed class CGMAsycudaContainerLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestContainerAgentCodeOrganisations()
	{
		AssertType<OrganisationsFindBoxCollection>(container.Lookups.ContainerAgentCodeOrganisations);
	}

	public void TestISOCodeList()
	{
		AssertSame(Factory.GetCachedValue<ISOContainerCodeList>(), container.Lookups.ISOCodeList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		var header = Factory.New<CGMAsycudaManifestHeader>();
		container = header.Containers.AddNew();
	}

	CGMAsycudaContainer container;
}
