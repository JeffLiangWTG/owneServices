using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseCollection : DependentBusinessObjectCollection<CusSCAHouse, CusSCAOceanBill>
	{
		public CusSCAHouseCollection(CusSCAOceanBill oceanBill, BusinessObjectFactory factory)
			: base(oceanBill, factory)
		{
			this.OceanBill = oceanBill;
			CountChanged += CusSCAHouseCollection_CountChanged;
		}

		public readonly CusSCAOceanBill OceanBill;

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			CusSCAHouse house = (CusSCAHouse)elementToDelete;
			if (house.CanDelete)
			{
				base.RemoveAndDelete(elementToDelete);
			}
			else
			{
				OnDeletingHouseBillWhenDisallowed(house);
			}
		}

		public event EventHandler DeletingHouseBillWhenDisallowed;

		void OnDeletingHouseBillWhenDisallowed(CusSCAHouse house)
		{
			if (DeletingHouseBillWhenDisallowed != null)
			{
				DeletingHouseBillWhenDisallowed(house, EventArgs.Empty);
			}
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			CusSCAHouse house = child as CusSCAHouse;
			if (house != null)
			{
				if (!OceanBill.CB_RL_NKPortOfLoading.IsEmpty)
				{
					house.CA_RL_NK_PortOfOrigin = OceanBill.CB_RL_NKPortOfLoading;
					house.CA_RN_NKGoodsOrigin = OceanBill.CB_RL_NKPortOfLoading.SubstringSafe(0, 2);
				}
				if (!OceanBill.CB_RL_NKPortOfDischarge.IsEmpty)
				{
					house.CA_RL_NK_PortOfDestination = OceanBill.CB_RL_NKPortOfDischarge;
				}
			}
		}

		void CusSCAHouseCollection_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			byHouseBill = null;
			if (e.ItemAdded)
			{
				((CusSCAHouse)e.BizObject).CA_HouseBillInfo.ValueChanged += CA_HouseBillInfo_ValueChanged;
			}
			else if (e.ItemRemoved)
			{
				((CusSCAHouse)e.BizObject).CA_HouseBillInfo.ValueChanged -= CA_HouseBillInfo_ValueChanged;
			}
		}

		void CA_HouseBillInfo_ValueChanged(object sender, EventArgs e)
		{
			byHouseBill = null;
		}

		public ILookup<ZString, CusSCAHouse> ByHouseBill => byHouseBill ?? (byHouseBill = this.Cast<CusSCAHouse>().ToLookup(house => house.CA_HouseBill));
#if DEBUG
		[CargoWise.EntityFramework.Testing.SuppressCollectionStateTest]
#endif
		ILookup<ZString, CusSCAHouse> byHouseBill;
	}
}
