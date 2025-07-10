using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FixedWidthFlatFileFieldPropertyTest : TestCase
	{
		public void TestConstructor()
		{
			FixedWidthFlatFileFieldProperty property = new FixedWidthFlatFileFieldProperty(1, 2, 3);
			AssertEquals("Name value", 1, property.Name);
			AssertEquals("Position value", 2, property.Position);
			AssertEquals("Length value", 3, property.Length);
		}
	}
}
