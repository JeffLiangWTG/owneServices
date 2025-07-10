using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CTOCusHAWBAndPartShipCollection : BusinessObjectCollection<BusinessObject>
	{
		public CTOCusHAWBAndPartShipCollection(CTOCusMAWB mAWB)
			: base(mAWB.Factory)
		{
			this.mAWB = mAWB;
			mAWB.CM_ArrivalDateInfo.ValueChanged += new EventHandler(CM_ArrivalDateInfo_ValueChanged);
			mAWB.CM_FlightNoInfo.ValueChanged += new EventHandler(CM_FlightNoInfo_ValueChanged);
			mAWB.CM_RL_NKDischargePortInfo.ValueChanged += new EventHandler(CM_RL_NKDischargePortInfo_ValueChanged);
		}

		readonly CTOCusMAWB mAWB;

		public new ICusHAWBBase this[int index]
		{
			get { return (ICusHAWBBase)Elements[index]; }
		}

		public new ICusHAWBBase AddNew()
		{
			return (ICusHAWBBase)base.AddNew();
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CTOCusHAWB);
		}

		public override void Load()
		{
			mAWB.ChildBills.Load();
			LoadAnyUnremovableElements();
		}

		public override void Load(ZQuery alternativeAdditionalFilter)
		{
			mAWB.ChildBills.Load(alternativeAdditionalFilter);
			LoadAnyUnremovableElements();
		}

		public void HookCollectionChanged(CTOCusHAWBCollection collection)
		{
			collection.CountChanged += new CollectionCountChangedEventHandler(ChildrenBills_CountChanged);
		}

		void LoadAnyUnremovableElements()
		{
			foreach (BusinessObject element in this.ToArray())
			{
				if (!((ICusHAWBBase)element).CanDeleteFromICusHAWBCollection)
				{
					Remove(element);
				}
			}
			if (!mAWB.CM_ArrivalDate.IsEmpty && mAWB.CM_ArrivalDate.IsValid && !mAWB.CM_FlightNo.IsEmpty && !mAWB.CM_RL_NKDischargePort.IsEmpty)
			{
				ZQuery partShipsFilter = new ZQuery();
				partShipsFilter.AddToFilter(CusPartShipSchema.CG_ArrivalDate, mAWB.CM_ArrivalDate);
				partShipsFilter.AddToFilter(CusPartShipSchema.CG_FlightNo, mAWB.CM_FlightNo);
				partShipsFilter.AddToFilter(CusPartShipSchema.CG_RL_NKDischargePort, mAWB.CM_RL_NKDischargePort);
				CusPartShip[] partShips = (CusPartShip[])Factory.Load(typeof(CusPartShip), partShipsFilter);
				foreach (CusPartShip partShip in partShips)
				{
					if (partShip.HouseBill is CTOCusHAWB)
					{
						Add(partShip);
					}
				}
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			if (((ICusHAWBBase)elementToDelete).CanDeleteFromICusHAWBCollection)
			{
				base.RemoveAndDelete(elementToDelete);
			}
			else
			{
				throw new CannotDeleteException("You can't delete this line because it comes from a Part Shipment from another CTO form.  To remove the item remove it from the CTO form it was created on.");
			}
		}

		void ChildrenBills_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && !Contains(e.BizObject))
			{
				this.Add(e.BizObject);
			}
			else if (e.ItemRemoved && Contains(e.BizObject))
			{
				this.Remove(e.BizObject);
			}
		}

		void CM_ArrivalDateInfo_ValueChanged(object sender, EventArgs e)
		{
			LoadAnyUnremovableElements();
		}

		void CM_FlightNoInfo_ValueChanged(object sender, EventArgs e)
		{
			LoadAnyUnremovableElements();
		}

		void CM_RL_NKDischargePortInfo_ValueChanged(object sender, EventArgs e)
		{
			LoadAnyUnremovableElements();
		}
	}
}
