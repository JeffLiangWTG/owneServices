using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondMoveDetailCollection : CusInBondMoveDetailCollection<CusInBondMoveDetail>
	{
		public CusInBondMoveDetailCollection(NctsArrivalMovementHeader master)
			: base(master)
		{
		}

		public CusInBondMoveDetailCollection(NctsDepartureMovementHeader master)
			: base(master)
		{
		}
	}
}
