using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusInBondContainer : Customs.Business.CusInBondContainer, Integration.Customs.EU.NCTS.INctsCusInBondContainer
	{
		public CusInBondContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusInBondMoveDetail MoveDetailCore => Factory.Load<CusInBondMoveDetail>(BC_ParentID);

		protected override Customs.Business.ICusInBondCargoDescCollection<Customs.Business.CusInBondCargoDesc> GetNewCommoditiesCollection() => new NctsCommonCargoDescCollection<NctsCommonCargoDesc>(this);

		public new CusInBondMoveDetail MoveDetail => (CusInBondMoveDetail)base.MoveDetail;
	}
}
