using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHItemCollection : ActiveBusinessObjectCollection<CusCAeMHItem>, Customs.Business.IOverrideDefaultValuesCollection
	{
		public CusCAeMHItemCollection(CusCAeMHHouse master)
			: base(master.Factory, new ZQuery(CusCAeMHItemSchema.BX_BW_House, master.PK))
		{
			this.master = master;
		}
		readonly CusCAeMHHouse master;

		protected override void SetDefaultsForNewElementCore(CusCAeMHItem newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BX_BW_House = master.PK;
		}

		protected override void SetRelationshipDefaultsForElementCore(CusCAeMHItem newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BX_BW_House = master.PK;
		}

		protected override void OnAdded(CusCAeMHItem businessObject)
		{
			base.OnAdded(businessObject);
			AllocateLineNumber(businessObject);
		}

		void AllocateLineNumber(CusCAeMHItem item)
		{
			var highestLineNo = master.Items.Max(x => x.BX_LineNumber);
			item.BX_LineNumber = highestLineNo + 1;
		}

		bool Customs.Business.IOverrideDefaultValuesCollection.IsOverrideDefaultValuesEnabled => master.Shipment != null;
		ZPropertyInfoBool Customs.Business.IOverrideDefaultValuesCollection.OverrideDefaultValuesInfo => (ZPropertyInfoBool)master.BW_OverrideFreightDefaultsInfo;
	}
}
