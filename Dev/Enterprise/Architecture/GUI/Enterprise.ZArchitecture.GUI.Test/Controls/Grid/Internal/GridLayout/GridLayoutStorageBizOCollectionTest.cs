using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(GridLayoutStorageBizOCollection))]
	sealed class GridLayoutStorageBizOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GridLayoutStorageBizOCollection>
	{
		public void TestCollection()
		{
			var layout1 = Factory.New<StmModuleFilter>();
			layout1.S9_ModuleID = "1";
			layout1.S9_FilterName = "A";

			var layout2 = Factory.New<StmModuleFilter>();
			layout2.S9_ModuleID = "1";
			layout2.S9_FilterName = "B";

			var layout3 = Factory.New<StmModuleFilter>();
			layout3.S9_ModuleID = "2";
			layout3.S9_FilterName = "C";

			var defaultLayout = Factory.New<StmData>();
			defaultLayout.SD_Name = "1";
			defaultLayout.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
			defaultLayout.SD_BinaryValue = ZBlob.FromAscii("Juio45897kljrew");
			Factory.Save();

			var collection = new GridLayoutStorageBizOCollection(new string[] { "1" }, new string[] { "1" }, ZGuid.Empty, Factory);
			AssertEquals("Three elements expected", 3, collection.Count);

			AssertEquals("A", collection[0].LayoutNameDisplay);
			AssertEquals("B", collection[1].LayoutNameDisplay);
			AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, collection[2].LayoutNameDisplay);
		}

		#region Implementation

		protected override GridLayoutStorageBizOCollection GetCollectionToTest()
		{
			return new GridLayoutStorageBizOCollection(new string[] { "1" }, new string[] { "1" }, ZGuid.Empty, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var filter = Factory.New<StmModuleFilter>();
			filter.S9_ModuleID = "1";
			filter.S9_FilterName = "2";
			return new GridLayoutStorageBizO(filter, Factory);
		}

		#endregion
	}
}
