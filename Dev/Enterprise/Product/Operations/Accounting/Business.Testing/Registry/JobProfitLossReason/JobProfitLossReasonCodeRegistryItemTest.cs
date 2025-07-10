using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobProfitLossReasonCodeRegistryItem))]
	class JobProfitLossReasonCodeRegistryItemTest : StronglyTypedRegistryItemTestCase<JobProfitLossReasonCodeCollection>
	{
		protected override StronglyTypedRegistryItem<JobProfitLossReasonCodeCollection, JobProfitLossReasonCodeCollection> GetNewRegistryItem()
		{
			return new JobProfitLossReasonCodeRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobProfitLossReasonCodeCollection());
		}
	}
}
