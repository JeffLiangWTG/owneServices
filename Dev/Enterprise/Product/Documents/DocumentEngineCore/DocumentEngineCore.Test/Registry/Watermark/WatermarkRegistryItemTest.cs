using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(WatermarkRegistryItem))]
	class WatermarkRegistryItemTest : StronglyTypedRegistryItemTestCase<Watermark>
	{
		protected override StronglyTypedRegistryItem<Watermark, Watermark> GetNewRegistryItem()
		{
			return new WatermarkRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
