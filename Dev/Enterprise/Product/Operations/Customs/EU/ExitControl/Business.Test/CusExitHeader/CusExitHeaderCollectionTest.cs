using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitHeaderCollection<CusExitHeader>))]
	sealed class CusExitHeaderCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitHeaderCollection<CusExitHeader>>
	{
		public void TestGetApplicationCodeFilter()
		{
			var xit = Factory.New<CusExitHeader>();
			var dac = Factory.New<CusExitHeader>();
			dac.CXH_ApplicationCode = "DAC";

			var collection = new CusExitHeaderCollection<CusExitHeader>(Factory);
			var collectionFilter = collection.CompleteFilter;

			CombineAssertions(() =>
			{
				AssertEquals("XIT", expected: true, xit.MatchesFilter(collectionFilter));
				AssertEquals("DAC", expected: false, dac.MatchesFilter(collectionFilter));
			});
		}

		public void TestGetApplicationCodeFilterWithAdditionalFilter()
		{
			var xit1 = Factory.New<CusExitHeader>();
			xit1.CXH_CustomsProfile = "1";
			var xit2 = Factory.New<CusExitHeader>();
			xit2.CXH_CustomsProfile = "2";
			var dac1 = Factory.New<CusExitHeader>();
			dac1.CXH_ApplicationCode = "DAC";
			dac1.CXH_CustomsProfile = "1";
			var dac2 = Factory.New<CusExitHeader>();
			dac2.CXH_ApplicationCode = "DAC";
			dac2.CXH_CustomsProfile = "2";

			var collection = new CusExitHeaderCollection<CusExitHeader>(Factory, new ZQuery(CusExitHeaderSchema.CXH_CustomsProfile, "1"));
			var collectionFilter = collection.CompleteFilter;

			CombineAssertions(() =>
			{
				AssertEquals("XIT 1", expected: true, xit1.MatchesFilter(collectionFilter));
				AssertEquals("DAC 1", expected: false, dac1.MatchesFilter(collectionFilter));
				AssertEquals("XIT 2", expected: false, xit2.MatchesFilter(collectionFilter));
				AssertEquals("DAC 2", expected: false, dac2.MatchesFilter(collectionFilter));
			});
		}
	}
}
