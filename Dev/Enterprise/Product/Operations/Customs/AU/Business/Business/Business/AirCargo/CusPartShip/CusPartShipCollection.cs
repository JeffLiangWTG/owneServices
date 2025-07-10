using System;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusPartShipCollection : Customs.Business.CusPartShipCollection<CusPartShip>
	{
		public CusPartShipCollection(CusHAWBBase parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
			HouseBill = parent;
		}

		public CusPartShip PartShipToSend
		{
			get
			{
				CusPartShip result = null;
				foreach (CusPartShip partShipment in this)
				{
					if (partShipment.CG_CustomsStatus.ToUpper().Trim() == AirCargoMessage.NewStatus.NotSent)
					{
						result = partShipment;
						break;
					}
				}
				return result;
			}
		}

		/// <summary>
		/// The latest part shipment that went through
		/// </summary>
		public CusPartShip PrevSuccPartShip
		{
			get
			{
				CusPartShip result = null;
				this.Sort(CusPartShip.Schema.CG_MessageReference, System.ComponentModel.ListSortDirection.Descending);
				foreach (CusPartShip partShip in this)
				{
					if (partShip.CG_CustomsStatus != AirCargoMessage.NewStatus.NotSent
						&& partShip.CG_CustomsStatus != AirCargoMessage.NewStatus.Waiting
						&& partShip.CG_CustomsStatus != AirCargoMessage.NewStatus.Rejected)
					{
						result = partShip;
						break;
					}
				}
				return result;
			}
		}

		public ZString LastFlightNo
		{
			get { return PrevSuccPartShip == null ? HouseBill.MAWB.CM_FlightNo : PrevSuccPartShip.CG_FlightNo; }
		}

		public ZDateTime LastArrivalDate
		{
			get { return PrevSuccPartShip == null ? HouseBill.MAWB.CM_ArrivalDate : PrevSuccPartShip.CG_ArrivalDate; }
		}

		public ZShort NextPiecesManifested
		{
			get
			{
				ZShort result;
				CusPartShip previousSuccess = PrevSuccPartShip;
				if (previousSuccess != null)
				{
					result = previousSuccess.CG_PiecesManifested - previousSuccess.CG_PiecesLanded;
				}
				else
				{
					result = HouseBill.CS_PiecesManifested - HouseBill.CS_PiecesLanded;
				}
				return Math.Max(result, ZShort.Zero);
			}
		}

		#region Implementation
		protected readonly CusHAWBBase HouseBill;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			CusPartShip partShip = child as CusPartShip;
			partShip.CG_CustomsStatus = AirCargoMessage.NewStatus.NotSent;
		}

		#endregion
	}
}
