using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(JsonStringArrayRegistryItem))]
	sealed class JsonStringArrayRegistryItemTest : StronglyTypedRegistryItemTestCase<string[]>
	{
		protected override StronglyTypedRegistryItem<string[], string[]> GetNewRegistryItem()
		{
			return new JsonStringArrayRegistryItem("a", (NoResString)"b", (NoResString)"c", (NoResString)"d", RegistryStorageFlags.System);
		}

		public void TestMaxLengthScopedToIndividualRegistryItem()
		{
			var a = new JsonStringArrayRegistryItem("worm", (NoResString)"Worms", (NoResString)"A worm", (NoResString)"It's a worm.", RegistryStorageFlags.System);
			var b = new JsonStringArrayRegistryItem("squishy", (NoResString)"Worms", (NoResString)"A different worm", (NoResString)"It's another worm.", RegistryStorageFlags.System);
			var squishyMaxLength = b.DataType.MaximumLength;

			a.DataType.MaximumLength = 123;
			AssertEquals("First registry item should have updated max length.", 123, a.DataType.MaximumLength);
			AssertEquals("Second registry item should have original max length.", squishyMaxLength, b.DataType.MaximumLength);
		}
	}
}
