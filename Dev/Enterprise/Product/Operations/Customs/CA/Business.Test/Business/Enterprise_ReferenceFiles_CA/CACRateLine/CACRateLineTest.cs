using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACRateLine))]
	sealed class CACRateLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var rate = Factory.New<CACRate>();
			var rateLine = rate.RateLines.AddNew();
			AssertEquals("Rate", rate, rateLine.Rate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var rate = Factory.New<CACRate>();
			return rate.RateLines.AddNew();
		}
	}
}
