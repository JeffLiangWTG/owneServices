using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuMenuPivotBaseCollection))]
	sealed class StmMenuMenuPivotBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuMenuPivotBaseCollection(Factory);
		}

		public void TestEditingMode()
		{
			var sysPivot = Factory.New<StmMenuMenuPivotBase>();
			sysPivot.SF_IsSystemDefined = true;

			var userPivot = Factory.New<StmMenuMenuPivotBase>();
			userPivot.SF_IsSystemDefined = false;

			var clientPivot = Factory.New<StmMenuMenuPivotBase>();
			clientPivot.SF_IsSystemDefined = true;
			clientPivot.SF_IsClientSpecific = true;

			var collection = new StmMenuMenuPivotBaseCollection(Factory);
			collection.Load();

			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, collection.EditingMode);
			AssertEquals("SysPivot ReadOnly", true, collection.FindByPK(sysPivot.PK).ReadOnly);
			AssertEquals("UserPivot ReadOnly", false, collection.FindByPK(userPivot.PK).ReadOnly);
			AssertEquals("ClientPivot ReadOnly", true, collection.FindByPK(clientPivot.PK).ReadOnly);

			AssertEquals("SysPivot CanDelete", false, ((StmMenuMenuPivotBase)collection.FindByPK(sysPivot.PK)).CanDelete);
			AssertEquals("UserPivot CanDelete", true, ((StmMenuMenuPivotBase)collection.FindByPK(userPivot.PK)).CanDelete);
			AssertEquals("ClientPivot CanDelete", false, ((StmMenuMenuPivotBase)collection.FindByPK(clientPivot.PK)).CanDelete);
		}

		public void TestIsManagedByDataRefresh()
		{
			var collection1 = new StmMenuMenuPivotBaseCollection(Factory);
			AssertEquals("IsManagedForDataRefresh", true, collection1.IsManagedForDataRefresh);

			var collection2 = new StmMenuMenuPivotBaseCollection(Factory, new ZQuery());
			AssertEquals("IsManagedForDataRefresh", true, collection2.IsManagedForDataRefresh);
		}

		public void TestINotifyCollectionChanged()
		{
			var collection = new StmMenuTemplatePivotBaseCollection(Factory);
			int collectionChangedHit = 0;
			collection.CollectionChanged += (s, e) =>
			{
				collectionChangedHit++;
			};
			AssertEquals(0, collectionChangedHit);

			var pivot = collection.AddNew();
			AssertEquals(1, collectionChangedHit);

			collection.Remove(pivot);
			AssertEquals(2, collectionChangedHit);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
		}

		#endregion
	}
}
