using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(UnloadingRemarksCollection))]
class UnloadingRemarksCollectionTest : SingleCusCodeDataCollectionTest<UnloadingRemarks>
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
		var seal = nctsHeader.ArrivalHeaderContainers.AddNew().Seals.AddNew();
		seal.BK_SealNumber = "X";
		return new UnloadingRemarksCollection(seal);
	}
}
