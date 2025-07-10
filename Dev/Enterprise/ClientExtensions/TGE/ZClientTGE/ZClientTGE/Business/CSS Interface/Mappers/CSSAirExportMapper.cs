using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSAirExportMapper : CSSMapper
	{
		protected override void MapCoreSpecific(CSSDataRow result, BusinessObject bizObj)
		{
			if (bizObj.GetType() == typeof(JobDeclaration))
			{
				JobDeclaration jobDec = bizObj as JobDeclaration;
				result.MAWBNumber = jobDec.JE_MasterBill;
				result.FlightNumber = jobDec.JE_VoyageFlightNo;
				result.OriginCode = jobDec.JE_RL_NKOrigin;
				result.DestinationCode = jobDec.JE_RL_NKFinalDestination;
				result.HAWBNumber = jobDec.JE_HouseBill;
				result.ProductCodeOrDescription = jobDec.JE_GoodsDescription;
				result.GoodsValue = jobDec.GoodsValue.Amount;
				result.GoodsCurrency = jobDec.GoodsValue.Currency.Code;
				result.DepartureOrArrivalDate = jobDec.JE_DateOfArrival;
				result.ShipmentCodeOrDescriptionStatus = "CES_" + jobDec.JE_EntryStatus;
			}
			else if (bizObj.GetType() == typeof(ForwardingShipment))
			{
				ForwardingShipment shipment = bizObj as ForwardingShipment;
				result.OriginCode = shipment.JS_RL_NKOrigin;
				result.DestinationCode = shipment.JS_RL_NKDestination;
				result.HAWBNumber = shipment.JS_HouseBill;
				result.ProductCodeOrDescription = shipment.JS_GoodsDescription;
				result.GoodsValue = shipment.JS_GoodsValue;
				result.GoodsCurrency = shipment.JS_RX_NKGoodsValueCurr;
				result.ShipmentCodeOrDescriptionStatus = shipment.CustomsEntryNumberType;
				result.CustomsAuthorityNumber = shipment.CustomsEntryNumber;

				if (shipment.Consols != null && shipment.Consols.Count > 0)
				{
					MapConsolDetails(result, shipment.Consols[0]);
				}
			}
		}

		protected override ZString Direction
		{
			get { return direction; }
		}
		const string direction = "O";

		void MapConsolDetails(CSSDataRow result, ForwardingConsol forwardingConsol)
		{
			result.MAWBNumber = forwardingConsol.JK_MasterBillNum;
			if (forwardingConsol.MostInterestingTransportForBinding != null)
			{
				result.FlightNumber = forwardingConsol.MostInterestingTransportForBinding[0].JW_VoyageFlight;
				result.DepartureOrArrivalDate = forwardingConsol.MostInterestingTransportForBinding[0].JW_ATD;
			}
		}
	}
}
