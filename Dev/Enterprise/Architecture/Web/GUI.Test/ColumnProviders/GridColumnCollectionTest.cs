using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	sealed class GridColumnCollectionTest : TestCaseWithFactory
	{
		public void TestGridColumnCollection()
		{
			GridColumnCollection collectionForTest = new GridColumnCollection();
			collectionForTest.Add(new ZTextEditColumn("text1", "bt1"));
			collectionForTest.Add(new ZTextEditColumn("text2", "bt2"));
			collectionForTest.Add(new ZTextEditColumn("text3", "bt3"));
			collectionForTest.Add(new ZTextEditColumn("text4", "bt4"));

			AssertEquals(collectionForTest["text1"].HeaderText, "text1");
			AssertEquals(collectionForTest["text2"].HeaderText, "text2");
			Assert(collectionForTest.ContainsKey("text3"));
			Assert(!collectionForTest.ContainsKey("blabla"));
		}
	}
}
