using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	internal class CommodityDetailLineTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be initialised in the constructor", ShipmentData, Line.ShipmentData);
			AssertEquals("Should be initialised in the constructor", CommodityData, Line.CommodityData);
		}

		public void TestLineAsString()
		{
			string expected = "SS102393   AU4000002005-04-12ADDGoods                                             Tariff                             AU00000000000000450000" + new string(' ', 161);
			AssertEquals(expected, Line.LineAsString);
		}

		#region Implementation
		CommodityDetailLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = new CommodityDetailLine(ShipmentData, CommodityData);
				}

				return fLine;
			}
		}

		CommodityDetailData CommodityData
		{
			get
			{
				if (fCommodityData == null)
				{
					fCommodityData = new CommodityDetailData("Goods", "Tariff", Core.Constants.CountryCodes.Australia, 4500.00m);
				}

				return fCommodityData;
			}
		}

		ShipmentDataForTest ShipmentData
		{
			get
			{
				if (fShipmentData == null)
				{
					fShipmentData = new ShipmentDataForTest();
					fShipmentData.ShipmentRef = "SS102393";
					fShipmentData.ImportDate = new ZDateTime(2005, 4, 12);
				}

				return fShipmentData;
			}
		}

		CommodityDetailLine fLine;
		CommodityDetailData fCommodityData;
		ShipmentDataForTest fShipmentData;
		#endregion
	}
}
