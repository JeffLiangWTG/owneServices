using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EnableComplianceWiseRegistryItem))]
	sealed class EnableComplianceWiseRegistryItemTest : StronglyTypedRegistryItemTestCase<EnableComplianceWiseRegistryBusinessObject>
	{
		protected override StronglyTypedRegistryItem<EnableComplianceWiseRegistryBusinessObject, EnableComplianceWiseRegistryBusinessObject> GetNewRegistryItem()
		{
			return new EnableComplianceWiseRegistryItem("name", null, (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new EnableComplianceWiseRegistryBusinessObject());
		}
	}
}
