using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

[TestedType(typeof(CGMAsycudaContainerCollection))]
sealed class CGMAsycudaContainerCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestSetDefaultContainerAgentCode()
	{
		var orgHeader = Factory.New<OrgHeader>();
		var address = orgHeader.MainAddress;

		var containerCollection = (CGMAsycudaContainerCollection)Collection;
		var container1 = containerCollection.AddNew();
		container1.ContainerAgentCode = address.PK;
		var container2 = containerCollection.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("ContainerAgentCode", address.PK, container2.ContainerAgentCode);
			AssertEquals("ContainerAgentCodeOrgPK", orgHeader.PK, container2.ContainerAgentCodeOrgPK);
		});
	}

	protected override Type GetExpectedCollectionType()
	{
		return typeof(CGMAsycudaContainerCollection);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var header = Factory.New<CGMAsycudaManifestHeader>();
		return header.Containers;
	}
}
