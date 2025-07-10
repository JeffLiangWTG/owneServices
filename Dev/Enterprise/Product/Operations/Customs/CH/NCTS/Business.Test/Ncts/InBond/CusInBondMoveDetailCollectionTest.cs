using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(CusInBondMoveDetailCollection))]
sealed class CusInBondMoveDetailCollectionTest : ActiveBusinessObjectCollectionTestCase<CusInBondMoveDetailCollection>
{
	protected override CusInBondMoveDetailCollection GetCollectionToTest() => new CusInBondMoveDetailCollection(NctsHeader.ArrivalMovementHeader);

	protected override BusinessObject GetNewElementToAddToTheCollection() => NctsHeader.ArrivalMovementHeader.MovementDetails.AddNew();

	NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
	NctsHeader nctsHeader;

	NctsHeader CreateNctsHeader()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		return nctsHeader;
	}
}
