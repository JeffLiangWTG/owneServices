using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CACRateCollection))]
	sealed class CACRateCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCollection()
		{
			var rateHeader = Factory.New<CACRateHeader>();
			rateHeader.Rates.AddNew();
			rateHeader.Rates.Add(Factory.New<CACRate>());
			rateHeader.Rates.AddRange(new[] { Factory.New<CACRate>(), Factory.New<CACRate>() });
			AssertRates(rateHeader, rateHeader.Rates);

			rateHeader = Factory.New<CACRateHeader>();
			rateHeader.Rates.AddNew();
			rateHeader.Rates.AddNew();

			var collection = new CACRateCollection(rateHeader);
			collection.Load();
			AssertEquals("Count", 2, collection.Count);
			AssertRates(rateHeader, collection);
		}

		void AssertRates(CACRateHeader rateHeader, CACRateCollection rates)
		{
			foreach (CACRate rate in rates)
			{
				AssertEquals("Parent", rateHeader.PK, rate.ZC_ParentID);
				AssertEquals("Parent Table", rateHeader.TablePrefix, rate.ZC_ParentTableCode);
			}
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new CACRateCollection(Factory.New<CACRateHeader>());
		}
	}
}
