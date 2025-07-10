using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(KeyDataPairCollection))]
	sealed class KeyDataPairCollectionTest : NonPersistentBusinessObjectCollectionTestCase<KeyDataPairCollection>
	{
		#region Implementation

		protected override KeyDataPairCollection GetCollectionToTest()
		{
			return new KeyDataPairCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new KeyDataPair();
		}

		#endregion

		public void TestGetEnumerator()
		{
			var collection = GetCollectionToTest();

			collection.Add(new KeyDataPair
			{
				Key = "TestKey",
				Data = "TestData"
			});
			var enumerator = collection.GetEnumerator();
			AssertEquals(true, enumerator.MoveNext());
			var data = enumerator.Current;
			AssertEquals("TestKey", data.Key);
			AssertEquals("TestData", data.Data);
			AssertEquals(false, enumerator.MoveNext());
		}
	}
}
