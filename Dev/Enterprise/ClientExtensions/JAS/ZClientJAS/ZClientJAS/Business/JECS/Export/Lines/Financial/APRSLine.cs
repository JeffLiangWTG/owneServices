using System;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export
{
	public class APRSLine : MessageLine
	{
		public APRSLine(JASForwardingConsol consol)
		{
			this.Consol = consol;
			if (consol == null)
			{
				throw new ArgumentNullException(nameof(consol));
			}
		}

		protected override ZString LineType
		{
			get { return JXCConstants.LineTypes.APRS; }
		}

		protected override int FieldCount
		{
			get { return JXCConstants.APRSFieldCount; }
		}

		protected override void SetFieldValues(JXCFlatFileDataRow dataRow)
		{
			dataRow.SetField(JXCConstants.APRSFieldPositions.TypeOfRecord, "N");
			dataRow.SetField(JXCConstants.APRSFieldPositions.AirportOfOrigin, AirportOfDeparture);
			dataRow.SetField(JXCConstants.APRSFieldPositions.AirportOfDestination, AirportOfDestination);
			dataRow.SetField(JXCConstants.APRSFieldPositions.AirlinePrefix, Consol.MasterBillAirlinePrefix);
			dataRow.SetField(JXCConstants.APRSFieldPositions.MAWBSerialNumber, Consol.MasterBillMAWB);
			dataRow.SetField(JXCConstants.APRSFieldPositions.DepartureDate, DepartureDate);
			dataRow.SetField(JXCConstants.APRSFieldPositions.OriginFileReference, Consol.JK_UniqueConsignRef);
			dataRow.SetField(JXCConstants.APRSFieldPositions.TotalNoOfHouseBills, Consol.Shipments.Count);
			dataRow.SetField(JXCConstants.APRSFieldPositions.TotalNoOfPieces, TotalNumberOfPieces);
			dataRow.SetField(JXCConstants.APRSFieldPositions.TotalChargeableWeight, TotalChargeableWeight);
			dataRow.SetField(JXCConstants.APRSFieldPositions.MasterFreightCost, MasterFreightCost);
			dataRow.SetField(JXCConstants.APRSFieldPositions.OtherMasterCosts, OtherMasterCosts);
			dataRow.SetField(JXCConstants.APRSFieldPositions.TotalHouseRevenue, TotalHouseRevenue);
			dataRow.SetField(JXCConstants.APRSFieldPositions.TotalProfitShareDueDestination, TotalProfitShareDueDestination);
			dataRow.SetField(JXCConstants.APRSFieldPositions.Currency, GlbCompany.CurrentCompany.LocalCurrency.RX_Code);
		}

		#region Calculated Properties

		ZString AirportOfDeparture
		{
			get { return (Consol.LoadPort != null) ? Consol.LoadPort.RL_IATA : ZString.Empty; }
		}

		ZString AirportOfDestination
		{
			get { return (Consol.DischargePort != null) ? Consol.DischargePort.RL_IATA : ZString.Empty; }
		}

		ZInt TotalNumberOfPieces
		{
			get
			{
				ZInt result = 0;

				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result += shipment.JS_OuterPacks;
				}

				return result;
			}
		}

		ZDecimal TotalChargeableWeight
		{
			get
			{
				ZDecimal result = 0;

				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result += shipment.JS_ActualChargeable;
				}

				return result;
			}
		}

		ZDecimal MasterFreightCost
		{
			get
			{
				ZDecimal result = 0;

				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result += shipment.TotalFreightCost;
				}

				return result;
			}
		}

		ZDecimal OtherMasterCosts
		{
			get
			{
				ZDecimal result = 0;

				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result += shipment.TotalOtherCosts;
				}

				return result;
			}
		}

		ZDecimal TotalHouseRevenue
		{
			get
			{
				ZDecimal result = 0;

				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result += shipment.TotalFreightRevenue;
				}

				return result;
			}
		}

		ZDecimal TotalProfitShareDueDestination
		{
			get
			{
				ZDecimal result = 0;

				foreach (JASForwardingShipment shipment in Consol.Shipments)
				{
					result += shipment.GetProfitShareDueDestination(Consol.ReceivingForwarder);
				}

				return result;
			}
		}

		Transport TransportToUse
		{
			get
			{
				return Consol.Transports.FirstTransportWithTransportMode(Core.Constants.TransportModes.Air) ?? Consol.Transports.MostInterestingTransport;
			}
		}

		ZDateTime DepartureDate
		{
			get { return (TransportToUse != null) ? TransportToUse.JW_ETD : ZDateTime.Empty; }
		}

		#endregion

		readonly JASForwardingConsol Consol;
	}
}
