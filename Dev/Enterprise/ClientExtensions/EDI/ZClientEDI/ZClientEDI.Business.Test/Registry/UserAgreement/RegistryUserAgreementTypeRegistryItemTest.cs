using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(RegistryUserAgreementTypeRegistryItem))]
	class RegistryUserAgreementTypeRegistryItemTest : StronglyTypedRegistryItemTestCase<RegistryUserAgreementTypeCollection>
	{
		protected override StronglyTypedRegistryItem<RegistryUserAgreementTypeCollection, RegistryUserAgreementTypeCollection> GetNewRegistryItem()
		{
			return new RegistryUserAgreementTypeRegistryItem("UserAgreementTypes",
				(NoResString)"",
				(NoResString)"User Agreement Types",
				(NoResString)"List of User Agreement Types that can be implemented in the User Agreement Module",
				RegistryStorageFlags.System,
				RegistryOptions.Default,
				new RegistryUserAgreementTypeCollection());
		}
	}
}
