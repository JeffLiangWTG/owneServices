using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class CusInBondContainerCollection : ActiveBusinessObjectCollection<CusInBondContainer>
	{
		public CusInBondContainerCollection(CusInBondMoveDetail master) : base(master.Factory, master, new ZQuery(), CusInBondContainerSchema.BC_ParentID)
		{
		}
	}
}
