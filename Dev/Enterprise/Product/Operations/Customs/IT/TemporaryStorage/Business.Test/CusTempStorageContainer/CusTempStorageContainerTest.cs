using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(CusTempStorageContainer))]
sealed class CusTempStorageContainerTest : CusCodeDataTest<CusTempStorageContainer>
{
	public void TestSetDefaultValues()
	{
		AssertEquals("TSC", tempStorageContainer.CY_Type);
	}

	public void TestParent()
	{
		AssertType<CusTempStorageRegLine>(tempStorageContainer.Parent);
		AssertNotNull("Parent", tempStorageContainer.Parent);
	}

	#region Implementation

	protected override IEnumerable<CusTempStorageContainer> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		yield return GetNewTempStorageContainer(factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewTempStorageContainer(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewTempStorageContainer();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	#endregion

	protected override void SetUp()
	{
		base.SetUp();
		tempStorageContainer = GetNewTempStorageContainer();
	}
	CusTempStorageContainer tempStorageContainer;

	CusTempStorageContainer GetNewTempStorageContainer(BusinessObjectFactory factory = null)
	{
		var currentFactory = factory ?? Factory;
		var storageRegHeader = currentFactory.New<CusTempStorageRegHeader>();
		storageRegHeader.SRH_Reference = "TEST";
		var storageRegLine = storageRegHeader.CusTempStorageRegLines.AddNew();
		storageRegLine.SRL_LineNumber = 1;
		return storageRegLine.Containers.AddNew();
	}
}
