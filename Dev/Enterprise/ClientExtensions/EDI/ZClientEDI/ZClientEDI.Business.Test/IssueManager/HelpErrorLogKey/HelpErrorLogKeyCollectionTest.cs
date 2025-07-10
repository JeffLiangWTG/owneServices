using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Business.Test
{
	[TestedType(typeof(HelpErrorLogKeyCollection))]
	class HelpErrorLogKeyCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestContainsKey()
		{
			var collection = Collection as HelpErrorLogKeyCollection;
			string key = "ABCD1234";
			int hash = HelpErrorLogKey.GetKeyHashCode(key);

			AssertEquals(false, collection.ContainsKey(key, hash));

			collection.AddNew();
			collection[0].HK_Key = key;
			collection[0].HK_HashCode = hash;

			AssertEquals(true, collection.ContainsKey(key, hash));
			AssertEquals("Should match even if passed hash is zero", true, collection.ContainsKey(key, 0));

			collection[0].HK_HashCode = 0;
			AssertEquals("Should match even if db hash is zero", true, collection.ContainsKey(key, hash));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new HelpErrorLogKeyCollection(Factory.New<EdiHelpErrorLog>(), Factory);
		}
	}
}
