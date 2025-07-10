using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(DigitalSignatureRegistryItem))]
	sealed class DigitalSignatureRegistryItemTest : StronglyTypedRegistryItemTestCase<DigitalSignatureRegistry>
	{
		protected override StronglyTypedRegistryItem<DigitalSignatureRegistry, DigitalSignatureRegistry> GetNewRegistryItem()
		{
			return new DigitalSignatureRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", DigitalSignatureTestHelper.GetDigitalSignatureRegistry());
		}
	}
}
