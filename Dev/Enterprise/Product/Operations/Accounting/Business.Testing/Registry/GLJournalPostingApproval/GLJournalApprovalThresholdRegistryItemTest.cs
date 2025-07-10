using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GLJournalApprovalThresholdRegistryItem))]
	class GLJournalApprovalThresholdRegistryItemTest : StronglyTypedRegistryItemTestCase<GLJournalApprovalThresholdCollection>
	{
		protected override StronglyTypedRegistryItem<GLJournalApprovalThresholdCollection, GLJournalApprovalThresholdCollection> GetNewRegistryItem()
		{
			return new GLJournalApprovalThresholdRegistryItem("", null, null, null, RegistryStorageFlags.System, new GLJournalApprovalThresholdCollection());
		}
	}
}
