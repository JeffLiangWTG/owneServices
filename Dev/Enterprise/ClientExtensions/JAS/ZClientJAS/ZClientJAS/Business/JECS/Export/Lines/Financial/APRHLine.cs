using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class APRHLine : MessageLine
	{
		public APRHLine(IJXCExportHeader exportHeader, JASForwardingShipment shipment)
		{
			this.ExportHeader = exportHeader;
			this.Shipment = shipment;
			CheckForNullArguments(exportHeader, shipment);
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.APRH; }
		}

		protected override int FieldCount
		{
			get { return JXCConstants.APRHFieldCount; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.APRHFieldPositions.HAWBSerialNumber, Shipment.JS_HouseBill);
			dataRow.SetField(JXCConstants.APRHFieldPositions.NoOfPieces, Shipment.JS_OuterPacks);
			dataRow.SetField(JXCConstants.APRHFieldPositions.ChargeableWeight, Shipment.JS_ActualChargeable);
			dataRow.SetField(JXCConstants.APRHFieldPositions.CollectOrPrepaid, Shipment.IsCollect ? "C" : "P");
			dataRow.SetField(JXCConstants.APRHFieldPositions.FreightRevenue, Shipment.TotalFreightRevenue);
			dataRow.SetField(JXCConstants.APRHFieldPositions.TotalCollectCharges, Shipment.GetTotalCollectChargesWithoutProfitShare(ExportHeader.ReceivingForwarder));
			dataRow.SetField(JXCConstants.APRHFieldPositions.AllocatedFreightCost, Shipment.TotalFreightCost);
			dataRow.SetField(JXCConstants.APRHFieldPositions.AllocatedOtherCost, Shipment.TotalOtherCosts);
			dataRow.SetField(JXCConstants.APRHFieldPositions.GrossProfit, Shipment.AgentDeclaredGrossProfit);
			dataRow.SetField(JXCConstants.APRHFieldPositions.DestinationProfitSplitPercentage, JASDataRegistry.Instance.ProfitSplitDueDestinationPercentage);
			dataRow.SetField(JXCConstants.APRHFieldPositions.ProfitSplitDueDestination, Shipment.GetProfitShareDueDestination(ExportHeader.ReceivingForwarder));
			dataRow.SetField(JXCConstants.APRHFieldPositions.Currency, GlbCompany.CurrentCompany.LocalCurrency.RX_Code);
		}

		void CheckForNullArguments(IJXCExportHeader exportHeader, JASForwardingShipment shipment)
		{
			if (exportHeader == null)
			{
				throw new ArgumentNullException(nameof(exportHeader));
			}

			if (shipment == null)
			{
				throw new ArgumentNullException(nameof(shipment));
			}
		}

		readonly IJXCExportHeader ExportHeader;
		readonly JASForwardingShipment Shipment;
	}
}

#region Implementation
#endregion
