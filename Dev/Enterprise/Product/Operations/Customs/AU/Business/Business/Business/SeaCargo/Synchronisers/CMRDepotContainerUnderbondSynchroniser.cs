using System.Collections;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRDepotContainerUnderbondSynchroniser : CMRDepotUnderbondSynchroniser
	{
		public CMRDepotContainerUnderbondSynchroniser(CusUnderbond destination, CFSContainer source)
			: base(destination, source)
		{
			this.Underbond = destination;
			this.Container = source;
		}

		public readonly CusUnderbond Underbond;
		public readonly CFSContainer Container;

		#region Implmentation

		protected override void OnSynchronise(Customs.Business.SynchroniseEventArgs e)
		{
			if (e.Action == Enterprise.Customs.Business.SynchroniseAction.Force)
			{
				EnsureOutturnsExist();
			}
			base.OnSynchronise(e);
		}

		protected override void HookSynchronisers()
		{
			if (Container.DestinationCFSArrival != null)
			{
				Synchronisers.Add(new Customs.Business.FieldSynchroniser(Underbond.C4_DateOfArrivalIntoDestinationPremiseInfo, Container.DestinationCFSArrival.EU_PickupDeliveryTimeInfo));
			}
			HookCollectionSynchronisers();
		}

		void HookCollectionSynchronisers()
		{
			foreach (CusOutturn outturn in Underbond.Outturns)
			{
				if (outturn.C5_ParentID == Container.PK)
				{
					CMRDepotContainerOutturnSynchroniser outturnSynchroniser = new CMRDepotContainerOutturnSynchroniser(outturn, Container);
					OutturnSynchronisers.Add(outturnSynchroniser);
				}
			}
		}

		void EnsureOutturnsExist()
		{
			if (Container.DestinationCFSArrival != null && Container.DestinationCFSArrival.EU_PickupDeliveryTime != ZDateTime.Empty)
			{
				EnsureContainerGoodsReceipt();
			}
			if (Underbond.C4_MovementReason == CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination)
			{
				foreach (CFSPackLine shipmentPackLine in Container.PackLines)
				{
					if (shipmentPackLine.JL_Outturn != 0)
					{
						EnsureShipmentOutturnLines();
						break;
					}
				}
			}
		}

		void EnsureContainerGoodsReceipt()
		{
			ZQuery containerGoodsReceiptFilter = new ZQuery(CusOutturnSchema.C5_ReceiptOnlyIndicator, ZBool.True);
			containerGoodsReceiptFilter.AddToFilter(CusOutturnSchema.C5_ParentID, Container.PK);
			CusOutturn[] outturns = (CusOutturn[])Container.Factory.Load(typeof(CusOutturn), containerGoodsReceiptFilter);
			if (outturns.Length == 0)
			{
				CusOutturn newOutturn = NewGoodsReceiptLine(Container);
				OutturnSynchronisers.Add(new CMRDepotContainerOutturnSynchroniser(newOutturn, Container));
			}
			else if (outturns.Length == 1)
			{
				CMRDepotOutturnSynchroniser goodsReceiptSynchroniser = GetOutturnSynchroniser(Container, outturns[0]);
				if (goodsReceiptSynchroniser == null)
				{
					goodsReceiptSynchroniser = new CMRDepotContainerOutturnSynchroniser(outturns[0], Container);
					OutturnSynchronisers.Add(goodsReceiptSynchroniser);
				}
			}
			else if (outturns.Length > 1)
			{
				ErrorReporter.ReportOnce("DuplicateOutturnLineDetected", containerGoodsReceiptFilter.LiteralTextADO);
			}
		}

		void EnsureShipmentOutturnLines()
		{
			ArrayList shipments = new ArrayList();
			foreach (CFSPackLine packLine in Container.PackLines)
			{
				if (packLine.Shipment != null)
				{
					if (!shipments.Contains(packLine.Shipment))
					{
						shipments.Add(packLine.Shipment);
					}
				}
			}
			foreach (CFSShipment shipment in shipments)
			{
				EnsureShipmentOutturnLine(shipment);
			}
		}

		void EnsureShipmentOutturnLine(CFSShipment shipment)
		{
			ZQuery shipmentOutturnLineFilter = new ZQuery(CusOutturnSchema.C5_ReceiptOnlyIndicator, ZBool.False);
			shipmentOutturnLineFilter.AddToFilter(CusOutturnSchema.C5_ParentID, Container.PK);
			CusOutturn[] outturns = (CusOutturn[])Container.Factory.Load(typeof(CusOutturn), shipmentOutturnLineFilter);
			if (outturns.Length == 0)
			{
				CusOutturn newOutturn = NewOutturnLine(shipment);
				OutturnSynchronisers.Add(new CMRDepotShipmentOutturnSynchroniser(newOutturn, shipment));
			}
			else if (outturns.Length == 1)
			{
				CMRDepotOutturnSynchroniser shipmentOutturnLineSynchroniser = GetOutturnSynchroniser(shipment, outturns[0]);
				if (shipmentOutturnLineSynchroniser == null)
				{
					shipmentOutturnLineSynchroniser = new CMRDepotShipmentOutturnSynchroniser(outturns[0], shipment);
					OutturnSynchronisers.Add(shipmentOutturnLineSynchroniser);
				}
			}
			else if (outturns.Length > 1)
			{
				ErrorReporter.ReportOnce("DuplicateOutturnLineDetected", shipmentOutturnLineFilter.LiteralTextADO);
			}
		}

		protected CusOutturn NewGoodsReceiptLine(CFSContainer container)
		{
			CusOutturn result = Underbond.Outturns.AddNew();
			result.C5_ReceiptOnlyIndicator = true;
			result.C5_ParentID = container.PK;
			result.C5_ParentTableCode = JobContainerSchema.Constants.Prefix;
			return result;
		}

		protected CusOutturn NewOutturnLine(CFSShipment shipment)
		{
			CusOutturn result = Underbond.Outturns.AddNew();
			result.C5_ReceiptOnlyIndicator = false;
			result.C5_ParentID = shipment.PK;
			result.C5_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			int totalOutturnedPackages = 0;
			foreach (CFSPackLine packLine in shipment.OuterPackLines)
			{
				totalOutturnedPackages += packLine.JL_Outturn;
			}
			result.C5_PackagesOutturned = totalOutturnedPackages;
			return result;
		}

		#endregion

	}
}
