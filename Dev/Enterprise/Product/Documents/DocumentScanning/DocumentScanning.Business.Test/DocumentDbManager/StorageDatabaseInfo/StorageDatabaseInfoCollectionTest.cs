using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(StorageDatabaseInfoCollection))]
	internal sealed class StorageDatabaseInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<StorageDatabaseInfoCollection>
	{
		public void TestAddNewNotAllowed()
		{
			StorageDatabaseInfoCollection testCollection = new StorageDatabaseInfoCollection(Factory);
			AssertEquals("AllowNew?", false, testCollection.AllowNew);
		}

		public void TestRemoveNotAllowed()
		{
			StorageDatabaseInfoCollection testCollection = new StorageDatabaseInfoCollection(Factory);
			AssertEquals("AllowRemove?", false, testCollection.AllowRemove);
		}

		public void TestLoad()
		{
			StorageDatabaseInfoCollection testCollection = new StorageDatabaseInfoCollection(Factory);
			AssertEquals("Initial count", 0, testCollection.Count);
			AssertEquals("Has changes?", false, testCollection.HasChanges);

			var storageDocDbs = Db.Connection.GetDatabases(DatabaseType.SD);
			testCollection.Load();
			AssertEquals("Count", storageDocDbs.Count(), testCollection.Count);
			AssertEquals("Has changes?", false, testCollection.HasChanges);

			AssertEquals("1st DB Name", storageDocDbs.First(), testCollection[0].DatabaseName);
		}

		#region Implementation

		protected override StorageDatabaseInfoCollection GetCollectionToTest()
		{
			return new StorageDatabaseInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new StorageDatabaseInfo();
		}

		#endregion
	}
}
