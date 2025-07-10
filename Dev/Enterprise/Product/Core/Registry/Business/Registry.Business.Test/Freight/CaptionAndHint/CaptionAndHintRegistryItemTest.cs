using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CaptionAndHintRegistryItem))]
	sealed class CaptionAndHintRegistryItemTest : StronglyTypedRegistryItemTestCase<ICaptionAndHint, CaptionAndHint>
	{
		protected override StronglyTypedRegistryItem<ICaptionAndHint, CaptionAndHint> GetNewRegistryItem()
		{
			return new CaptionAndHintRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
