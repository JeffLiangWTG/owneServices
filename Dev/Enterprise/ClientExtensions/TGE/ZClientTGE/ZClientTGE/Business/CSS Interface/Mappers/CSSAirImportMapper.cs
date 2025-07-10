using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Client.TGE.Business
{
	internal class CSSAirImportMapper : CSSMapper
	{
		protected override void MapCoreSpecific(CSSDataRow result, BusinessObject bizObj)
		{
			CusHAWB cusHawb = bizObj as CusHAWB;
			result.MAWBNumber = cusHawb.MAWB.CM_MAWB;
			result.FlightNumber = cusHawb.CS_FlightNo;
			result.OriginCode = cusHawb.CS_RL_NKOrigin;
			result.DestinationCode = cusHawb.CS_RL_NKDestination;
			result.HAWBNumber = cusHawb.CS_HAWB;
			result.ProductCodeOrDescription = cusHawb.GoodsDescription;
			result.GoodsValue = cusHawb.CS_GoodsValue;
			result.GoodsCurrency = cusHawb.GoodsCurrency.RX_Code;
			result.DepartureOrArrivalDate = cusHawb.CS_ArrivalDate;
			result.ShipmentCodeOrDescriptionStatus = cusHawb.CS_CustomsStatus;
		}

		protected override ZString Direction
		{
			get { return direction; }
		}
		const string direction = "I";
	}
}
