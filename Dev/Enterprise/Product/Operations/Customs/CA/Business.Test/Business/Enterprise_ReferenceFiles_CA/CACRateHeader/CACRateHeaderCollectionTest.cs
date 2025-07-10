using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACRateHeaderCollection))]
	sealed class CACRateHeaderCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCLSCollection()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var collection = new CACRateHeaderCollection(classHeader, CACRateHeader.RateType.ClassificationRate);
			collection.AddNew();
			collection.Add(Factory.New<CACRateHeader>());
			collection.AddRange(new[] { Factory.New<CACRateHeader>() });
			AssertEquals("Count", 3, collection.Count);

			foreach (CACRateHeader rate in collection)
			{
				AssertEquals("RateType", CACRateHeader.RateType.ClassificationRate, rate.ZB_RateType);
			}
		}

		public void TestEXSCollection()
		{
			var classHeader = Factory.New<CACClassHeader>();
			var collection = new CACRateHeaderCollection(classHeader, CACRateHeader.RateType.ExciseDutyRate);
			collection.AddNew();
			collection.Add(Factory.New<CACRateHeader>());
			collection.AddRange(new[] { Factory.New<CACRateHeader>() });
			AssertEquals("Count", 3, collection.Count);

			foreach (CACRateHeader rate in collection)
			{
				AssertEquals("RateType", CACRateHeader.RateType.ExciseDutyRate, rate.ZB_RateType);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CACRateHeaderCollection(Factory.New<CACClassHeader>(), CACRateHeader.RateType.ClassificationRate);
		}
	}
}
