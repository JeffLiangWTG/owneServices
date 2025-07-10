using System.Collections.Generic;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Utils.Test
{
	class CollectionExtensionTest : TestCase
	{
		public void TestIsEmpty()
		{
			IEnumerable<string> collection = new List<string>();
			AssertEquals(true, collection.IsEmpty());

			collection = null;
			AssertEquals(true, collection.IsEmpty());

			collection = new string[] { "a", "b" };
			AssertEquals(false, collection.IsEmpty());
		}
	}
}
