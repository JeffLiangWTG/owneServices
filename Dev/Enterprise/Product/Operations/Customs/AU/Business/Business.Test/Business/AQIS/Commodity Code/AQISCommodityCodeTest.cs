using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISCommodityCode))]
	sealed class AQISCommodityCodeTest : AQISSingleValueBusinessObjectTest
	{
		public void TestLookups()
		{
			AQISCommodityCode commodityCode = new AQISCommodityCode(Factory);
			AssertNotNull("Lookups", commodityCode.Lookups);
		}

		AQISCommodityCode aqisCommodityCode;
		public override AQISSingleValueBusinessObject BusinessObjectToTest => aqisCommodityCode ?? (aqisCommodityCode = new AQISCommodityCode(Factory));

		protected override BusinessObject GetNewBusinessObject() => new AQISCommodityCode(Factory);
	}
}
