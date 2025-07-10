using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaContainer))]
sealed class CGMAsycudaContainerIDocAddressesTest : TestCaseWithFactory
{
	public void TestGetDocAddressRequirement()
	{
		var docAddresses = GetDocAddresses();
		AssertNull(docAddresses.GetDocAddressRequirement(DocAddressType.ContainerAgentCodeAddress));
	}

	public void TestSupportedAddressTypes()
	{
		var docAddresses = GetDocAddresses();
		AssertContainsExactElementsInExactOrder(new[] { DocAddressType.ContainerAgentCodeAddress }, docAddresses.SupportedAddressTypes);
	}

	public void TestGetCanOverrideCheckpoint()
	{
		var docAddresses = GetDocAddresses();
		AssertEquals(Env.Security.None, docAddresses.GetCanOverrideCheckpoint(null));
	}

	public void TestCanDeleteAddress()
	{
		var docAddresses = GetDocAddresses();
		AssertEquals(false, docAddresses.CanDeleteAddress(null));
	}

	public void TestGetOrgHeaderList()
	{
		var docAddresses = GetDocAddresses();
		AssertNull(docAddresses.GetOrgHeaderList(DocAddressType.ContainerAgentCodeAddress));
	}

	public void TestPiggyBackedDocAddressValidation()
	{
		var docAddresses = GetDocAddresses();
		AssertNull(docAddresses.PiggyBackedDocAddressValidation(null));
	}

	IDocAddresses GetDocAddresses()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		header.SuspendCheckBusinessObjectType();
		var container = header.Containers.AddNew();
		container.ACN_ContainerNumber = "Test";
		return container;
	}
}
