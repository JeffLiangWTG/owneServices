using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuTemplatePivotBaseCollection))]
	sealed class StmMenuTemplatePivotBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuTemplatePivotBaseCollection(Factory);
		}

		public void TestEditingMode()
		{
			var sysPivot = Factory.New<StmMenuTemplatePivotBase>();
			sysPivot.SI_DocumentTitle = "SysPivot";
			sysPivot.SI_IsSystemDefined = true;

			var userPivot = Factory.New<StmMenuTemplatePivotBase>();
			userPivot.SI_DocumentTitle = "UserPivot";
			userPivot.SI_IsSystemDefined = false;

			var clientPivot = Factory.New<StmMenuTemplatePivotBase>();
			clientPivot.SI_DocumentTitle = "ClientPivot";
			clientPivot.SI_IsSystemDefined = true;
			clientPivot.SI_IsClientSpecific = true;

			var collection = new StmMenuTemplatePivotBaseCollection(Factory);
			collection.Load();

			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, collection.EditingMode);
			AssertMultilineASCIIEquals("SysPivot ReadOnly"
				, "SI_IsPasswordProtected\r\nSI_IsPasswordProtectedForOpening\r\nSI_PrintByDefault"
				, string.Join("\r\n", StmMenuTemplatePivotBaseTest.GetNonReadOnlyPropertyNames(collection.FindByPK(sysPivot.PK) as StmMenuTemplatePivotBase)));
			AssertEquals("UserPivot ReadOnly", false, collection.FindByPK(userPivot.PK).ReadOnly);
			AssertMultilineASCIIEquals("ClientPivot ReadOnly"
				, "SI_IsPasswordProtected\r\nSI_IsPasswordProtectedForOpening\r\nSI_PrintByDefault"
				, string.Join("\r\n", StmMenuTemplatePivotBaseTest.GetNonReadOnlyPropertyNames(collection.FindByPK(clientPivot.PK) as StmMenuTemplatePivotBase)));

			AssertEquals("SysPivot CanDelete", false, ((StmMenuTemplatePivotBase)collection.FindByPK(sysPivot.PK)).CanDelete);
			AssertEquals("UserPivot CanDelete", true, ((StmMenuTemplatePivotBase)collection.FindByPK(userPivot.PK)).CanDelete);
			AssertEquals("ClientPivot CanDelete", false, ((StmMenuTemplatePivotBase)collection.FindByPK(clientPivot.PK)).CanDelete);
		}

		public void TestDocManagerFilter()
		{
			var collection = new StmMenuTemplatePivotBaseCollection(Factory);
			var pivot1 = collection.AddNew();
			var pivot2 = collection.AddNew();

			Assert("DocManager filter empty by default", collection.DocManagerFilter.IsEmpty);
			AssertEquals("DocManagerFilter empty on the Pivots by default", collection.DocManagerFilter, pivot1.DocManagerFilter);

			collection.DocManagerFilter = "SHP";
			AssertEquals("DocManager filter should be SHP", "SHP", collection.DocManagerFilter);
			AssertEquals("DocManagerFilter on the individual elements should be the same as parent", collection.DocManagerFilter, pivot1.DocManagerFilter);
			AssertEquals("DocManagerFilter on the individual elements should be the same as parent", collection.DocManagerFilter, pivot2.DocManagerFilter);
		}

		public void TestAddNewRow()
		{
			var collection = new StmMenuTemplatePivotBaseCollection(Factory);
			var pivot1 = collection.AddNew();
			AssertEquals("Pivot1 DocManagerFilter should be blank", ZString.Empty, pivot1.DocManagerFilter);

			collection.DocManagerFilter = "SHP";
			var pivot2 = collection.AddNew();
			AssertEquals("Pivot1 DocManagerFilter should be SHP", "SHP", pivot1.DocManagerFilter);
			AssertEquals("Pivot2 DocManagerFilter should be SHP", "SHP", pivot2.DocManagerFilter);
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
