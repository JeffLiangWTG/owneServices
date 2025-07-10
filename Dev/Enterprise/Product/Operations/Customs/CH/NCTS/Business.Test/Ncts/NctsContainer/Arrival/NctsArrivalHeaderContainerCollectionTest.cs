using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsArrivalHeaderContainerCollection))]
sealed class NctsArrivalHeaderContainerCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestAllowNew()
	{
		AssertEquals("AllowNew", false, GetCollectionToTest().AllowNew);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return Containers;
	}

	protected override System.Type GetExpectedCollectionType()
	{
		return typeof(NctsArrivalHeaderContainerCollection);
	}

	NctsArrivalHeaderContainerCollection Containers => containers ?? (containers = CreateContainers());
	NctsArrivalHeaderContainerCollection containers;

	NctsArrivalHeaderContainerCollection CreateContainers()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		return header.ArrivalHeaderContainers;
	}
}
