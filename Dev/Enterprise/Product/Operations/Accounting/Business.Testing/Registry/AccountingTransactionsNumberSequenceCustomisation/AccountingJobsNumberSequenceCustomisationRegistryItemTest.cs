using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(JobsNumberSequenceCustomisationRegistryItem))]
	class AccountingJobsNumberSequenceCustomisationRegistryItemTest : StronglyTypedRegistryItemTestCase<TransactionNumberSequenceCustomisationCollection>
	{
		protected override StronglyTypedRegistryItem<TransactionNumberSequenceCustomisationCollection, TransactionNumberSequenceCustomisationCollection> GetNewRegistryItem()
		{
			return new JobsNumberSequenceCustomisationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
