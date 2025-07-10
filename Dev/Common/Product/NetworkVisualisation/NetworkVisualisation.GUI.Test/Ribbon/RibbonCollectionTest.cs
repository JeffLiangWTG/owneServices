using NUnit.Framework;

namespace CargoWise.NetworkVisualisation.GUI.Test
{
	public class RibbonCollectionTest : TestCase
	{
		public void TestInsertAfterItem()
		{
			var collection = new RibbonCollection<RibbonTabViewModel>();
			var homeTab = new RibbonTabViewModel(ResString.GetMultilingualString("TestCollection|Home", "Home"));
			var viewTab = new RibbonTabViewModel(ResString.GetMultilingualString("TestCollection|View", "View"));

			collection.Add(homeTab);

			collection.InsertAfterItem(homeTab, viewTab);
			AssertEquals(viewTab, collection[1]);
		}

		public void TestInsertAtStart()
		{
			var collection = new RibbonCollection<RibbonTabViewModel>();
			var homeTab = new RibbonTabViewModel(ResString.GetMultilingualString("TestCollection|Home", "Home"));
			var viewTab = new RibbonTabViewModel(ResString.GetMultilingualString("TestCollection|View", "View"));

			collection.Add(homeTab);

			collection.InsertAtStart(viewTab);
			AssertEquals(viewTab, collection[0]);
		}
	}
}
