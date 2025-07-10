using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	internal class CSSAirExportMapperTest : CSSMapperTest<CSSAirExportMapper>
	{
		protected override IList<BusinessObject> GetPopulatedBizos()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			result.Add(TestHelper.GetJobDec());
			result.Add(TestHelper.GetShipment());
			return result;
		}

		protected override void AssertSpecificContents(CSSDataRow dataRow, Type bizoSource)
		{
			if (bizoSource == typeof(JobDeclaration))
			{
				AssertEquals("JobDec MAWBNumber", "0813333", dataRow.MAWBNumber);
				AssertEquals("JobDec FlightNumber", "QF123", dataRow.FlightNumber);
				AssertEquals("JobDec OriginCode", "AUSYD", dataRow.OriginCode);
				AssertEquals("JobDec DestinationCode", "NZAKL", dataRow.DestinationCode);
				AssertEquals("JobDec HAWBNumber", "HOUSEBILL", dataRow.HAWBNumber);
				AssertEquals("JobDec ProductCodeOrDescription", "BOOKS", dataRow.ProductCodeOrDescription);
				AssertEquals("JobDec GoodsValue", 525.25M, dataRow.GoodsValue);
				AssertEquals("JobDec GoodsCurrency", "AUD", dataRow.GoodsCurrency);
				AssertEquals("JobDec DepartureOrArrivalDate", new ZDateTime(2009, 10, 13, 11, 34, 0), dataRow.DepartureOrArrivalDate);
				AssertEquals("JobDec ShipmentCodeOrDescriptionStatus", "CES_CLR", dataRow.ShipmentCodeOrDescriptionStatus);
				AssertEquals("JobDec CustomsAuthorityNumber", "", dataRow.CustomsAuthorityNumber);
				AssertEquals("JobDec Direction", "O", dataRow.Direction);
			}
			else if (bizoSource == typeof(ForwardingShipment))
			{
				AssertEquals("Shipment MAWBNumber", "0813333", dataRow.MAWBNumber);
				AssertEquals("Shipment FlightNumber", "QF123", dataRow.FlightNumber);
				AssertEquals("Shipment OriginCode", "AUSYD", dataRow.OriginCode);
				AssertEquals("Shipment DestinationCode", "NZAKL", dataRow.DestinationCode);
				AssertEquals("Shipment HAWBNumber", "HOUSEBILL", dataRow.HAWBNumber);
				AssertEquals("Shipment ProductCodeOrDescription", "BOOKS", dataRow.ProductCodeOrDescription);
				AssertEquals("Shipment GoodsValue", 525.25M, dataRow.GoodsValue);
				AssertEquals("Shipment GoodsCurrency", "AUD", dataRow.GoodsCurrency);
				AssertEquals("Shipment DepartureOrArrivalDate", new ZDateTime(2009, 10, 13, 11, 34, 0), dataRow.DepartureOrArrivalDate);
				AssertEquals("Shipment ShipmentCodeOrDescriptionStatus", "CAN", dataRow.ShipmentCodeOrDescriptionStatus);
				AssertEquals("Shipment CustomsAuthorityNumber", "3333", dataRow.CustomsAuthorityNumber);
				AssertEquals("Shipment Direction", "O", dataRow.Direction);
			}
		}
	}
}
