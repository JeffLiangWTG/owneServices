using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(OrgHeaderCodeListCollection))]
	sealed class OrgHeaderCodeListCollectionTest : NonPersistentBusinessObjectCollectionTestCase<OrgHeaderCodeListCollection>
	{
		public void TestToGuidArray()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid[] guidList = new Guid[] { guid1, guid2 };

			OrgHeaderCodeListCollection collection = new OrgHeaderCodeListCollection(guidList);
			Guid[] newList = collection.ToGuidArray();
			AssertNotNull("NewList", newList);
			AssertEquals("NewList.Length", 2, newList.Length);
			AssertCollectionContains("NewList should contain Guid1", guid1, newList);
			AssertCollectionContains("NewList should contain Guid2", guid2, newList);
		}

		public void TestToString()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid[] guidList = new Guid[] { guid1, guid2 };

			OrgHeaderCodeListCollection collection = new OrgHeaderCodeListCollection(guidList);
			string expectedString = guid1.ToString() + "," + guid2.ToString();
			Guid[] newList = collection.ToGuidArray();
			AssertNotNull("NewList", newList);
			AssertEquals("ToString", expectedString, collection.ToString());
		}

		protected override OrgHeaderCodeListCollection GetCollectionToTest()
		{
			return new OrgHeaderCodeListCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgHeaderCodeListElement(ZGuid.NewZGuid(), new OrgHeaderCodeListCollection(Factory));
		}
	}
}
