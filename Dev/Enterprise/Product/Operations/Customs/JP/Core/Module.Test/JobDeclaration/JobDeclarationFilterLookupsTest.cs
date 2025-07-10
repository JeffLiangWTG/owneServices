using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterLookups))]
	sealed class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestEntryStatusList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			AssertSame(filterBO.Factory.GetCachedValue<CustomsStatusList>(), filterBO.Lookups.EntryStatusList());
		}

		public void TestMessageStatusList()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();
			AssertType<JPMessageStatusList>(filterBO.Lookups.MessageStatusList());
		}
	}
}
