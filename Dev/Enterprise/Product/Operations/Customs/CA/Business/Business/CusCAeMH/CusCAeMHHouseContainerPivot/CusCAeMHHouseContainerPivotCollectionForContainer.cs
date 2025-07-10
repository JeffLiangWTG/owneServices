using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseContainerPivotCollectionForContainer : ActiveBusinessObjectCollection<CusCAeMHHouseContainerPivot>
	{
		public CusCAeMHHouseContainerPivotCollectionForContainer(CusCAeMHContainer master)
			: base(master.Factory, new ZQuery(CusCAeMHHouseContainerPivotSchema.BPA_BQ_Container, master.PK))
		{
			this.master = master;
		}
		readonly CusCAeMHContainer master;

		protected override void SetDefaultsForNewElementCore(CusCAeMHHouseContainerPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BPA_BQ_Container = master.PK;
		}

		protected override void SetRelationshipDefaultsForElementCore(CusCAeMHHouseContainerPivot newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BPA_BQ_Container = master.PK;
		}
	}
}
