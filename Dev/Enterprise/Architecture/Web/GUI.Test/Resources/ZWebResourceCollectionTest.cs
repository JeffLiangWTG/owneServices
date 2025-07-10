using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZWebResourceCollectionTest : TestCase
	{
		public void TestAdd()
		{
			ZWebResourceCollection collection = new ZWebResourceCollection();
			ZWebResource resource1 = new ZWebResource(GetType(), "resource1", null);
			collection.Add(resource1);

			AssertEquals(1, collection.Count);
			AssertSame(resource1, collection[0]);

			ZWebResource resource2 = new ZWebResource(GetType(), "resource2", null);
			collection.Add(resource2);

			AssertEquals(2, collection.Count);
			AssertSame(resource1, collection[0]);
			AssertSame(resource2, collection[1]);
		}
	}
}
