using System.Windows;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.DragDrop.Testing;

[System.Diagnostics.Contracts.ContractVerification(false)]
public class UIHelperTest : TestCase
{
	[RequiresSTA]
	public void TestGetUIElement()
	{
		using (var testHelper = new DragDropTestHelper())
		{
			testHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			AssertEquals("Got item 1", testHelper.GetItemsControlItem(0), UIHelper.GetUIElement(testHelper.ItemsControl, new Point(10, 10)));
			AssertEquals("Got item 2", testHelper.GetItemsControlItem(1), UIHelper.GetUIElement(testHelper.ItemsControl, new Point(10, 60)));
			AssertEquals("Got item 3", testHelper.GetItemsControlItem(2), UIHelper.GetUIElement(testHelper.ItemsControl, new Point(10, 110)));
			AssertNull("Didn't get anything", UIHelper.GetUIElement(testHelper.ItemsControl, new Point(10, 1000)));
		}
	}

	[RequiresSTA]
	public void TestIsAboveElement()
	{
		using (var testHelper = new DragDropTestHelper())
		{
			testHelper.ShowWindow();
			DragDropTestHelper.DoEvents();
			var firstItem = testHelper.GetItemsControlItem(0);
			AssertEquals("Got item 1", firstItem, UIHelper.GetUIElement(testHelper.ItemsControl, new Point(10, 10)));
			Assert("Point is above element", UIHelper.IsPositionAboveElement(firstItem, new Point(5, 5)));
			Assert("Point is below element", !UIHelper.IsPositionAboveElement(firstItem, new Point(5, firstItem.ActualHeight / 2 + 5)));
		}
	}
}
