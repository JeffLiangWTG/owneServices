using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHHouseCollection : ActiveBusinessObjectCollection<CusCAeMHHouse>
	{
		public CusCAeMHHouseCollection(CusCAeMHMaster master)
			: base(master.Factory, new ZQuery(CusCAeMHHouseSchema.BW_BP_Master, master.PK))
		{
			this.master = master;
		}
		readonly CusCAeMHMaster master;

		protected override void SetRelationshipDefaultsForElementCore(CusCAeMHHouse newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BW_BP_Master = master.PK;
		}

		protected override void SetDefaultsForNewElementCore(CusCAeMHHouse newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BW_BP_Master = master.PK;
			var nonContainerized = master.Containers.Where(x => !x.IsNonContainerized).ToArray();
			if (nonContainerized.Length == 1)
			{
				var pivot = newElement.Pivots.AddNew();
				using (pivot.SuspendSettingHasChanges())
				{
					pivot.BPA_BQ_Container = nonContainerized[0].PK;
				}
			}
		}
	}
}
