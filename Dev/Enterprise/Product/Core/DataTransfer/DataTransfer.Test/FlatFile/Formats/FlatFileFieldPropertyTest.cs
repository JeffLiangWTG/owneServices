using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class FlatFileFieldPropertyTest : TestCase
	{
		public void TestConstructor()
		{
			FlatFileFieldProperty property = new FlatFileFieldProperty(1, 3);
			AssertEquals("Name value", 1, property.Name);
			AssertEquals("Length value", 3, property.Length);
		}
	}
}
