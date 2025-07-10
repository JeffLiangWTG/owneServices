using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NonPersistentJobTypeOptionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestJobTypeList()
		{
			var jobTypeOption = new NonPersistentJobTypeOption(Factory);
			var list = jobTypeOption.Lookups.JobTypeList;
			CombineAssertions(() =>
			{
				AssertEquals("NCT, NC5", list.CodesAsString);
				AssertSame(list, jobTypeOption.Lookups.JobTypeList);
			});
		}
	}
}
