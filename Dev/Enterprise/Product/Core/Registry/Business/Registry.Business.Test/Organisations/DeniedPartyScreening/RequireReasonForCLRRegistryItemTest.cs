using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RequireReasonForCLRRegistryItem))]
	sealed class RequireReasonForCLRRegistryItemTest : StronglyTypedRegistryItemTestCase<RequireReasonForCLRWrapper>
	{
		protected override StronglyTypedRegistryItem<RequireReasonForCLRWrapper, RequireReasonForCLRWrapper> GetNewRegistryItem()
		{
			var wrapper = new RequireReasonForCLRWrapper();
			return new RequireReasonForCLRRegistryItem("", null, null, null, RegistryStorageFlags.All, RegistryOptions.Default, wrapper);
		}
	}
}
