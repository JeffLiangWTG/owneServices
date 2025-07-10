using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedAsNonPersistentBusinessObject]
	[TestedType(typeof(ScopeData))]
	public class ScopeDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestScopeDataTest_Success()
		{
			var obj = new ScopeData();
			obj.Name = "Test";
			AssertEquals("Test", obj.Name);
		}
	}
}
