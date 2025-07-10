using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(CusSupplyChainActorReferenceCollection))]
class CusSupplyChainActorReferenceCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestMaxCount()
	{
		AssertEquals(99, GetCollectionToTest().MaxCount);
	}

	protected override BusinessObjectCollection GetCollectionToTest() => new CusSupplyChainActorReferenceCollection(Factory.New<CusEntryInstruction>());
}
