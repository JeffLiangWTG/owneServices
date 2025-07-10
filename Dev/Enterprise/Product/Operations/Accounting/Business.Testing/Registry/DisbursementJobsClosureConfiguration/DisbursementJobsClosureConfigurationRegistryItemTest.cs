using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(DisbursementJobsClosureConfigurationRegistryItem))]
	class DisbursementJobsClosureConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<IDisbursementJobsClosureConfiguration, DisbursementJobsClosureConfiguration>
	{
		protected override StronglyTypedRegistryItem<IDisbursementJobsClosureConfiguration, DisbursementJobsClosureConfiguration> GetNewRegistryItem()
		{
			return new DisbursementJobsClosureConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System | RegistryStorageFlags.Company, RegistryOptions.Default);
		}
	}
}
