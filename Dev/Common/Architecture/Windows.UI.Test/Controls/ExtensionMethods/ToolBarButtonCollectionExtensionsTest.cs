using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class ToolBarButtonCollectionExtensionsTest : TestCase
	{
		public void TestFindByText()
		{
			ToolBar toolbar = new ToolBar();
			ToolBarButton item1 = new ToolBarButton("Item1");
			ToolBarButton item2 = new ToolBarButton("Ite&m2");
			ToolBarButton item3 = new ToolBarButton("Item3(&3)");
			toolbar.Buttons.Add(item1);
			toolbar.Buttons.Add(item2);
			toolbar.Buttons.Add(item3);
			AssertEquals(item1, toolbar.Buttons.FindByText("Item1"));
			AssertEquals(null, toolbar.Buttons.FindByText("Item1(&1)"));
			AssertEquals(item2, toolbar.Buttons.FindByText("Item2"));
			AssertEquals(item3, toolbar.Buttons.FindByText("Item3(3)"));
			AssertEquals(null, toolbar.Buttons.FindByText("NoMatch"));
		}
	}
}
