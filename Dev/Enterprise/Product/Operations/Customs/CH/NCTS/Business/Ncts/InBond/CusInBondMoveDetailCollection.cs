namespace Enterprise.Customs.CH.NCTS.Business;

public class CusInBondMoveDetailCollection : EU.NCTS.Business.CusInBondMoveDetailCollection
{
	public CusInBondMoveDetailCollection(EU.NCTS.Business.NctsArrivalMovementHeader master) : base(master)
	{
	}

	public CusInBondMoveDetailCollection(EU.NCTS.Business.NctsDepartureMovementHeader master) : base(master)
	{
	}

	public new CusInBondMoveDetail AddNew() => (CusInBondMoveDetail)base.AddNew();

	public new CusInBondMoveDetail this[int index] => (CusInBondMoveDetail)base[index];
}
