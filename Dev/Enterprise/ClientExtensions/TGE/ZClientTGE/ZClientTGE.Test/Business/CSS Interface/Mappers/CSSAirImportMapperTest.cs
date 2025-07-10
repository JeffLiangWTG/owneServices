using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.TGE.Business.CSSInterface.Testing
{
	internal class CSSAirImportMapperTest : CSSMapperTest<CSSAirImportMapper>
	{
		protected override IList<BusinessObject> GetPopulatedBizos()
		{
			List<BusinessObject> result = new List<BusinessObject>();
			result.Add(TestHelper.GetCusHawb());
			return result;
		}

		protected override void AssertSpecificContents(CSSDataRow dataRow, Type bizoSource)
		{
			AssertEquals("MAWBNumber", "MAWB101", dataRow.MAWBNumber);
			AssertEquals("FlightNumber", "QF123", dataRow.FlightNumber);
			AssertEquals("OriginCode", "SGSIN", dataRow.OriginCode);
			AssertEquals("DestinationCode", "AUSYD", dataRow.DestinationCode);
			AssertEquals("HAWBNumber", "HAWB1011", dataRow.HAWBNumber);
			AssertEquals("ProductCodeOrDescription", "BOOKS", dataRow.ProductCodeOrDescription);
			AssertEquals("GoodsValue", 525.25M, dataRow.GoodsValue);
			AssertEquals("GoodsCurrency", "AUD", dataRow.GoodsCurrency);
			AssertEquals("DepartureOrArrivalDate", new ZDateTime(2009, 10, 13, 11, 34, 0), dataRow.DepartureOrArrivalDate);
			AssertEquals("ShipmentCodeOrDescriptionStatus", "NOT", dataRow.ShipmentCodeOrDescriptionStatus);
			AssertEquals("Direction", "I", dataRow.Direction);
		}
	}
}
