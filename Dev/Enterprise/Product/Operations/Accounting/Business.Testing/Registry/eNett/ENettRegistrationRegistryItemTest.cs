using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ENettRegistrationRegistryItem))]
	class ENettRegistrationRegistryItemTest : StronglyTypedRegistryItemTestCase<EnettRegistrationCode>
	{
		protected override StronglyTypedRegistryItem<EnettRegistrationCode, EnettRegistrationCode> GetNewRegistryItem()
		{
			return new ENettRegistrationRegistryItem("", null, null, null, RegistryStorageFlags.Company);
		}
	}
}
