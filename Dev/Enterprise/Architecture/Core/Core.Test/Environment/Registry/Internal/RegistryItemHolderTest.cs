using System;
using Enterprise.Core.Environment.Internal;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryItemHolderTest : TestCase
	{
		public void TestProperties()
		{
			StringRegistryItem item = new StringRegistryItem("", null, null, null, RegistryStorageFlags.System);
			RegistryItemHolder holder = new RegistryItemHolder(item);
			AssertEquals("Item", item, holder.Item);
			Assert("LastUsed", holder.ElapsedSinceLastUse.TotalSeconds < 1);
			holder.ElapsedSinceLastUse = new TimeSpan(1432, 9, 2, 3);
			AssertEquals("LastUsed", new TimeSpan(1432, 9, 2, 3), holder.ElapsedSinceLastUse);
		}

		public void TestItemIsNull()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new RegistryItemHolder(null));
		}
	}
}
