using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusInBondContainerCollection))]
	public class CusInBondContainerCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondContainerCollection>
	{
		protected override CusInBondContainerCollection GetCollectionToTest()
		{
			var moveDetail = GetMovementDetail();
			return new CusInBondContainerCollection(moveDetail);
		}

		CusInBondMoveDetail GetMovementDetail()
		{
			var header = Factory.New<CusInBondHeader>();
			header.BH_GB = GlbBranch.CurrentBranch.PK;
			var moveHeader = Factory.New<CusInBondMoveHeader>();
			moveHeader.BM_BH = header.PK;
			var moveDetail = moveHeader.MovementDetails.AddNew();
			return moveDetail;
		}
	}
}
