using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsUnloadingMovementHeader))]
	class NctsUnloadingMovementHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGoodsItems()
		{
			AssertType<NctsArrivalAndUnloadingCargoDescCollection<NctsArrivalAndUnloadingCargoDesc>>(unloadingMovement.GoodsItems);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			return nctsHeader.UnloadingMovementHeader;
		}

		protected override BusinessObject GetNewBusinessObject() => unloadingMovement;

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			unloadingMovement = nctsHeader.UnloadingMovementHeader;
		}
		NctsUnloadingMovementHeader unloadingMovement;
	}
}
