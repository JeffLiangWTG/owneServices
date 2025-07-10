using CargoWise.EntityFramework;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondMoveDetailCollection : ActiveBusinessObjectCollection<CusInBondMoveDetail>
	{
		public CusInBondMoveDetailCollection(CusInBondMoveHeader master) : base(master)
		{
		}
	}
}
