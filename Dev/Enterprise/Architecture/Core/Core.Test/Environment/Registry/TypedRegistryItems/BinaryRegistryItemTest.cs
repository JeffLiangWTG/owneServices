using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(BinaryRegistryItem))]
	sealed class BinaryRegistryItemTest : StronglyTypedRegistryItemTestCase<byte[]>
	{
		protected override StronglyTypedRegistryItem<byte[], byte[]> GetNewRegistryItem()
		{
			return new BinaryRegistryItem("", null, null, null, RegistryStorageFlags.All);
		}

		protected override byte[] ValidValue
		{
			get { return new byte[] { 1, 2, 3 }; }
		}
	}
}
