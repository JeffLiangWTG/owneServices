using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobProfitLossRequiringReasonParametersRegistryItem))]
	class JobProfitLossRequiringReasonParametersRegistryItemTest : StronglyTypedRegistryItemTestCase<JobProfitLossRequiringReasonParameters>
	{
		protected override StronglyTypedRegistryItem<JobProfitLossRequiringReasonParameters, JobProfitLossRequiringReasonParameters> GetNewRegistryItem()
		{
			return new JobProfitLossRequiringReasonParametersRegistryItem("", null, null, null, RegistryStorageFlags.System, new JobProfitLossRequiringReasonParameters());
		}
	}
}
