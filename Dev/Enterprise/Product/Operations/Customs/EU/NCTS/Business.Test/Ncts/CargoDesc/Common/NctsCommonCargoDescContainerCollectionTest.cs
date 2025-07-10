using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsCommonCargoDescContainerCollection))]
	public class NctsCommonCargoDescContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<NctsCommonCargoDescContainerCollection>
	{
		public void TestMaster()
		{
			AssertType<NctsArrivalAndUnloadingCargoDesc>(GetCollectionToTest().Master);
		}
		public void TestDefaultParentTableCode()
		{
			var collection = GetCollectionToTest();
			var container = collection.AddNew();
			AssertEquals(CusInBondCargoDescSchema.Constants.Prefix, container.BC_ParentTableCode);
		}

		protected override NctsCommonCargoDescContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.UnloadingMovementHeader.GoodsItems.AddNew().Containers;
		}
	}
}
