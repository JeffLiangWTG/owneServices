using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseContainerPivotCollectionForHouse : ActiveBusinessObjectCollection<CusCAeMHHouseContainerPivot>
	{
		public CusCAeMHHouseContainerPivotCollectionForHouse(CusCAeMHHouse master)
			: base(master.Factory, new ZQuery(CusCAeMHHouseContainerPivotSchema.BPA_BW_House, master.PK))
		{
			this.master = master;
		}
		readonly CusCAeMHHouse master;

		protected override void SetDefaultsForNewElementCore(CusCAeMHHouseContainerPivot newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BPA_BW_House = master.PK;
		}

		protected override void SetRelationshipDefaultsForElementCore(CusCAeMHHouseContainerPivot newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BPA_BW_House = master.PK;
		}
	}
}
