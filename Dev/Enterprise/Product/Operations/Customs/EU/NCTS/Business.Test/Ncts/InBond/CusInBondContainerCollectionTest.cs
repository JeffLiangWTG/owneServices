using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(CusInBondContainerCollection))]
	class CusInBondContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondContainerCollection>
	{
		protected override CusInBondContainerCollection GetCollectionToTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var cusInBondMoveDetail = header.Bills.AddNew().MovementDetail;

			return new CusInBondContainerCollection(cusInBondMoveDetail);
		}
	}
}
