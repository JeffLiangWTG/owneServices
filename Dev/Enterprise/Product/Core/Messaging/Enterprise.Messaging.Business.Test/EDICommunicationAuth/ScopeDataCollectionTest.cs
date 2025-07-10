using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedAsNonPersistentBusinessObject]
	[TestedType(typeof(ScopeDataCollection))]
	public class ScopeDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ScopeDataCollection>
	{
		protected override ScopeDataCollection GetCollectionToTest() => new ScopeDataCollection("c1");

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ScopeData();
	}
}
