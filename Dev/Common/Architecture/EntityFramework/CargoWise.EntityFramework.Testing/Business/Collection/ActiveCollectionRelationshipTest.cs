using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ActiveCollectionRelationshipTest : TestCase
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ActiveCollectionRelationship(null, new CollectionRelationship(typeof(DummyBusinessObject))));
			AssertExceptionThrown<ArgumentNullException>(() => new ActiveCollectionRelationship(typeof(DummyBusinessObject), null));
		}
	}
}
