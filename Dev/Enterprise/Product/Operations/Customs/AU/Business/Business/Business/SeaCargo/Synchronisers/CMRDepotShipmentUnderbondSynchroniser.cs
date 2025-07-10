using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;
using FieldSynchroniser = Enterprise.Customs.Business.FieldSynchroniser;
using SynchroniseAction = Enterprise.Customs.Business.SynchroniseAction;
using SynchroniseEventArgs = Enterprise.Customs.Business.SynchroniseEventArgs;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDepotShipmentUnderbondSynchroniser : CMRDepotUnderbondSynchroniser
	{
		public CMRDepotShipmentUnderbondSynchroniser(CusUnderbond destination, CFSShipment source)
			: base(destination, source)
		{
			this.Shipment = source;
			this.Underbond = destination;
		}

		public readonly CFSShipment Shipment;
		public readonly CusUnderbond Underbond;

		#region Implmnetation

		protected override void OnSynchronise(SynchroniseEventArgs e)
		{
			if (e.Action == SynchroniseAction.Force)
			{
				EnsureOutturnsExist();
			}
			base.OnSynchronise(e);
		}

		protected override void HookSynchronisers()
		{
			HookContainerLegsSynchronisers();
		}

		protected override void UnHookSynchronisers()
		{
			base.UnHookSynchronisers();
			UnHookContainerLegs();
		}

		bool arrivalLegHooked;
		protected void HookContainerLegsSynchronisers()
		{
			foreach (CommonPickupDeliveryConfirm leg in Shipment.DestinationCFSArrivals)
			{
				HookArrivalContainerLeg(leg);
				break;
			}

			if (!arrivalLegHooked && Shipment.Containers.Any())
			{
				HookArrivalContainerLeg(Shipment.Containers.First().DestinationCFSArrival);
			}

			if (!arrivalLegHooked)
			{
				Shipment.DestinationCFSArrivals.CountChanged += new EventHandler(NoArrival_WaitForPossibleNew_CountChanged);
				Shipment.ContainerAdded += ContainerAdded;
			}
		}

		void ContainerAdded(object sender, EventArgs e)
		{
			CountChanged();
		}

		void NoArrival_WaitForPossibleNew_CountChanged(object sender, EventArgs e)
		{
			CountChanged();
		}

		void CountChanged()
		{
			foreach (CommonPickupDeliveryConfirm leg in Shipment.DestinationCFSArrivals)
			{
				HookNewArrivalContainerLeg(leg);
				Shipment.DestinationCFSArrivals.CountChanged -= new EventHandler(NoArrival_WaitForPossibleNew_CountChanged);
				Shipment.ContainerAdded -= ContainerAdded;
				break;
			}

			if (!arrivalLegHooked && Shipment.Containers.Any())
			{
				HookNewArrivalContainerLeg(Shipment.Containers.First().DestinationCFSArrival);
				Shipment.DestinationCFSArrivals.CountChanged -= new EventHandler(NoArrival_WaitForPossibleNew_CountChanged);
				Shipment.ContainerAdded -= ContainerAdded;
			}
		}

		protected void UnHookContainerLegs()
		{
			if (!arrivalLegHooked)
			{
				Shipment.DestinationCFSArrivals.CountChanged -= new EventHandler(NoArrival_WaitForPossibleNew_CountChanged);
				Shipment.ContainerAdded -= ContainerAdded;
			}
			else
			{
				UnHookArrivalContainerLeg();
			}
		}

		FieldSynchroniser arrivalLegDateSynchroniser;
		protected void HookArrivalContainerLeg(CommonPickupDeliveryConfirm arrivalLeg)
		{
			arrivalLegHooked = true;
			arrivalLegDateSynchroniser = new FieldSynchroniser(Underbond.C4_ArrivalDateInfo, arrivalLeg.EU_PickupDeliveryTimeInfo);
			arrivalLegDateSynchroniser.SetEnabled(IsEnabled, DetectEnabled);
			Synchronisers.Add(arrivalLegDateSynchroniser);
		}

		protected void HookNewArrivalContainerLeg(CommonPickupDeliveryConfirm arrivalLeg)
		{
			HookArrivalContainerLeg(arrivalLeg);
			if (Underbond.Outturns.Count == 0)
			{
				CusOutturn outturn = Underbond.Outturns.AddNew();
				outturn.C5_ReceiptOnlyIndicator = true;
				outturn.C5_ParentID = Shipment.PK;
				outturn.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				CMRDepotShipmentOutturnSynchroniser newOutturnSynchroniser = new CMRDepotShipmentOutturnSynchroniser(outturn, Shipment);
				newOutturnSynchroniser.Synchronise(new SynchroniseEventArgs(SynchroniseAction.Force));
				OutturnSynchronisers.Add(newOutturnSynchroniser);
			}
		}

		protected void UnHookArrivalContainerLeg()
		{
			if (arrivalLegDateSynchroniser != null)
			{
				arrivalLegDateSynchroniser.SetEnabled(false, arrivalLegDateSynchroniser.DetectEnabled);
				if (Synchronisers.Contains(arrivalLegDateSynchroniser))
				{
					Synchronisers.Remove(arrivalLegDateSynchroniser);
				}
			}
		}

		void EnsureOutturnsExist()
		{
			CommonPickupDeliveryConfirm arrivalLeg = GetArrivalLeg(Shipment);
			if (arrivalLeg != null && arrivalLeg.EU_PickupDeliveryTime != ZDateTime.Empty)
			{
				EnsureShipmentGoodsReceipt();
			}
		}

		CommonPickupDeliveryConfirm GetArrivalLeg(CFSShipment shipment)
		{
			CommonPickupDeliveryConfirm result = null;
			foreach (CommonPickupDeliveryConfirm leg in shipment.DestinationCFSArrivals)
			{
				result = leg;
				if (!result.EU_PickupDeliveryTime.IsEmpty)
				{
					break;
				}
			}

			if (result == null)
			{
				result = shipment.Containers.Select(x => x.DestinationCFSArrival).FirstOrDefault();
			}

			return result;
		}

		void EnsureShipmentGoodsReceipt()
		{
			ZQuery shipmentGoodsReceiptFilter = new ZQuery(CusOutturnSchema.C5_ReceiptOnlyIndicator, ZBool.True);
			shipmentGoodsReceiptFilter.AddToFilter(CusOutturnSchema.C5_ParentID, Shipment.PK);
			CusOutturn[] outturns = (CusOutturn[])Shipment.Factory.Load(typeof(CusOutturn), shipmentGoodsReceiptFilter);
			if (outturns.Length == 0)
			{
				CusOutturn newOutturn = NewGoodsReceiptLine(Shipment);
				OutturnSynchronisers.Add(new CMRDepotShipmentOutturnSynchroniser(newOutturn, Shipment));
			}
			else if (outturns.Length == 1)
			{
				CMRDepotOutturnSynchroniser goodsReceiptSynchroniser = GetOutturnSynchroniser(Shipment, outturns[0]);
				if (goodsReceiptSynchroniser == null)
				{
					goodsReceiptSynchroniser = new CMRDepotShipmentOutturnSynchroniser(outturns[0], Shipment);
					OutturnSynchronisers.Add(goodsReceiptSynchroniser);
				}
			}
			else if (outturns.Length > 1)
			{
				ErrorReporter.ReportOnce("DuplicateOutturnLineDetected", shipmentGoodsReceiptFilter.LiteralTextADO);
			}
		}

		protected CusOutturn NewGoodsReceiptLine(CFSShipment shipment)
		{
			CusOutturn result = Underbond.Outturns.AddNew();
			result.C5_ReceiptOnlyIndicator = true;
			result.C5_ParentID = shipment.PK;
			result.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			result.C5_HouseBill = shipment.JS_HouseBill;
			if (shipment.OuterPackLines.Count > 0)
			{
				result.C5_ContainerNumber = shipment.OuterPackLines[0].GetContainerNumForConsol(shipment.ArrivalConsol);
			}
			return result;
		}

		#endregion
	}
}
