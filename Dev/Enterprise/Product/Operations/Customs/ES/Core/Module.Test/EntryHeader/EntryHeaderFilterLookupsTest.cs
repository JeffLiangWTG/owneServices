using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ES.Module.Testing
{
	class EntryHeaderFilterLookupsTest : TestCaseWithFactory
	{
		public void TestParallelListCodeList()
		{
			var lookups = new EntryHeaderFilterLookups(new EntryHeaderFilterBusinessObject());
			CombineAssertions(() =>
			{
				var list = lookups.ParallelList;
				AssertEquals(typeof(YesNoList), list.GetType());
				AssertEquals("N, Y", list.CodesAsString);
				AssertEquals("Yes", list.GetDescriptionFromCode("Y"));
				AssertEquals("ParallelList returns the same object", true, ReferenceEquals(list, lookups.ParallelList));
			});
		}
	}
}
