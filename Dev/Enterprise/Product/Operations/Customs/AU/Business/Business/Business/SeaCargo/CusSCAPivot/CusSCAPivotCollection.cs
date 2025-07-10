using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAPivotCollection : ActiveBusinessObjectCollection<CusSCAPivot>
	{
		public CusSCAPivotCollection(CusSCAOceanBill oceanBill)
			: base(oceanBill)
		{
			OceanBill = oceanBill;
			CollectionCountChange += CusSCAPivotCollection_CountChanged;
		}

		void CusSCAPivotCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			byHouseBill = null;
			byContainer = null;

			if (e.BizObject is CusSCAPivot pivot)
			{
				if (e.ItemAdded)
				{
					pivot.CV_CAInfo.ValueChanged += CV_CAInfo_ValueChanged;
					pivot.CV_CNInfo.ValueChanged += CV_CNInfo_ValueChanged;
				}
				else if (e.ItemRemoved)
				{
					pivot.CV_CAInfo.ValueChanged -= CV_CAInfo_ValueChanged;
					pivot.CV_CNInfo.ValueChanged -= CV_CNInfo_ValueChanged;
				}
			}
		}

		public override void Delete(CusSCAPivot businessObject)
		{
			if (businessObject.CanDelete)
			{
				base.Delete(businessObject);
			}
		}

		void CV_CAInfo_ValueChanged(object sender, System.EventArgs e) => byHouseBill = null;
		void CV_CNInfo_ValueChanged(object sender, System.EventArgs e) => byContainer = null;

		public ILookup<ZGuid, CusSCAPivot> ByHouseBill => byHouseBill ?? (byHouseBill = this.ToLookup(pivot => pivot.CV_CA));
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ILookup<ZGuid, CusSCAPivot> byHouseBill;

		public ILookup<ZGuid, CusSCAPivot> ByContainer => byContainer ?? (byContainer = this.ToLookup(pivot => pivot.CV_CN));
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ILookup<ZGuid, CusSCAPivot> byContainer;

		public readonly CusSCAOceanBill OceanBill;
	}
}
