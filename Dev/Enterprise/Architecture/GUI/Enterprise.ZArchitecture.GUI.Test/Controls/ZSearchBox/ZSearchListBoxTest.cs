using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.SearchBox;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZSearchListBoxTest : TestCase
	{
		IList<IDisplayItem> TestItems_Normal => new List<IDisplayItem>
		{
			DisplayItemFactory.CreateHeadingItem("Heading 1"),
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 1", "Description 1"),
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 2", "Description 2"),
			DisplayItemFactory.CreateHeadingItem("Heading 2"),
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 3", "Description 3"),
		};

		IList<IDisplayItem> TestItems_NoHeadings => new List<IDisplayItem>
		{
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 1", "Description 1"),
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 2", "Description 2"),
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 3", "Description 3"),
			DisplayItemFactory.CreateSearchItemWithEmptyAction("Item 4", "Description 4"),
		};

		IList<IDisplayItem> TestItems_HeadingsOnly => new List<IDisplayItem>
		{
			DisplayItemFactory.CreateHeadingItem("Heading 1", "Description 1"),
			DisplayItemFactory.CreateHeadingItem("Heading 2", "Description 2"),
			DisplayItemFactory.CreateHeadingItem("Heading 3", "Description 3"),
		};

		IList<IDisplayItem> TestItems_Error => new List<IDisplayItem>
		{
			DisplayItemFactory.CreateErrorItem("Error 1", "Description 1"),
		};

		IList<IDisplayItem> TestItems_Empty => new List<IDisplayItem>
		{ };

		#region Designer

		public void TestDesignerPropertiesAreCorrect()
		{
			var slb = new ZSearchListBox();
			AssertEquals(DrawMode.OwnerDrawVariable, slb.DrawMode);
		}

		#endregion

		#region Event Overrides

		public void TestOnVisibleChanged()
		{
			// arrange
			var slb = new ZSearchListBox
			{
				DataSource = TestItems_Normal,
				SelectedIndex = 4,
				Visible = false
			};
			AssertEquals(4, slb.SelectedIndex);

			// act
			slb.Visible = true;

			// assert
			AssertEquals(1, slb.SelectedIndex);
		}

		public void TestOnMeasureItem()
		{
			// arrange
			var slb = new ZSearchListBoxForTest
			{
				DataSource = TestItems_Normal
			};

			// act
			var miea = new MeasureItemEventArgs(slb.CreateGraphics(), 1, 32);
			slb.OnMeasureItem(miea);

			// assert
			AssertEquals("search item height is wrong", 32, miea.ItemHeight);

			// act
			miea = new MeasureItemEventArgs(slb.CreateGraphics(), 0, 32);
			slb.OnMeasureItem(miea);

			// assert
			AssertEquals("heading item height is wrong", 16, miea.ItemHeight);
		}

		public void TestOnKeyPress()
		{
			// arrange
			var slb = new ZSearchListBox();
			var executeTriggered = false;
			var oncloseTriggered = false;
			var data = TestItems_Normal;
			data.Add(DisplayItemFactory.CreateSearchItem(new Action(() => executeTriggered = true), "Item 33", "Order 66"));
			slb.DataSource = data;
			slb.SelectedIndex = data.Count - 1;
			slb.OnClose += (a, b) => oncloseTriggered = true;

			// act - assert
			KeySender.SendKeyPress(slb, Keys.Enter);
			Assert("Enter keypress didn't trigger item execution", executeTriggered);

			// act - assert
			KeySender.SendKeyPress(slb, Keys.Escape);
			Assert("Exit keypress didn't invoke the OnClose event", oncloseTriggered);
		}

		public void TestOnKeyDown()
		{
			// arrange
			var slb = new ZSearchListBoxForTest
			{
				DataSource = TestItems_Normal,
				SelectedIndex = 2,
				Visible = true
			};
			_ = slb.Focus();

			// act
			KeySender.PostKeyDown(slb, Keys.Up);
			Application.DoEvents();

			// assert
			AssertEquals("SelectedIndex was incorrect", 1, slb.SelectedIndex);

			// first selectable item is 1 so this should remain at 1
			KeySender.PostKeyDown(slb, Keys.Up);
			Application.DoEvents();
			AssertEquals("SelectedIndex was incorrect", 1, slb.SelectedIndex);

			KeySender.PostKeyDown(slb, Keys.Down);
			Application.DoEvents();
			AssertEquals("SelectedIndex was incorrect", 2, slb.SelectedIndex);

			// skip index 3 since its a heading; non-selectable

			KeySender.PostKeyDown(slb, Keys.Down);
			Application.DoEvents();
			AssertEquals("SelectedIndex was incorrect", 4, slb.SelectedIndex);

			// end of list is 4 so pressing down again should have no change
			KeySender.PostKeyDown(slb, Keys.Down);
			Application.DoEvents();
			AssertEquals("SelectedIndex was incorrect", 4, slb.SelectedIndex);
		}

		public void TestOnKeyDownErrorList() => OnKeyDownListCore(TestItems_Error);
		public void TestOnKeyDownEmptyList() => OnKeyDownListCore(TestItems_Empty);
		public void TestOnKeyDownHeadingOnlyList() => OnKeyDownListCore(TestItems_HeadingsOnly);

		void OnKeyDownListCore(IList<IDisplayItem> testList)
		{
			// arrange
			var slb = new ZSearchListBoxForTest
			{
				DataSource = testList,
				Visible = true
			};
			_ = slb.Focus();

			// act
			KeySender.PostKeyDown(slb, Keys.Up);
			Application.DoEvents();

			// assert
			AssertEquals("SelectedIndex was incorrect", -1, slb.SelectedIndex);

			// first selectable item is 1 so this should remain at 1
			KeySender.PostKeyDown(slb, Keys.Up);
			Application.DoEvents();
			AssertEquals("SelectedIndex was incorrect", -1, slb.SelectedIndex);

			KeySender.PostKeyDown(slb, Keys.Down);
			Application.DoEvents();
			AssertEquals("SelectedIndex was incorrect", -1, slb.SelectedIndex);
		}

		public void TestOnMouseClick_Zero()
		{
			// arrange
			var slb = new ZSearchListBoxForTest();
			AssertEquals("List should be empty as a precondition to this test", 0, slb.Items.Count);
			var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);

			// act-assert
			AssertNoExceptionThrown("", () => slb.OnMouseClick(mouseEventArgs));
		}

		public void TestOnMouseClick_Negative()
		{
			// arrange
			var slb = new ZSearchListBoxForTest();
			AssertEquals("List should be empty as a precondition to this test", 0, slb.Items.Count);
			var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, -1, -1, 0);

			// act-assert
			AssertNoExceptionThrown("", () => slb.OnMouseClick(mouseEventArgs));
		}

		public void TestOnMouseClick_Offscreen()
		{
			// arrange
			var slb = new ZSearchListBoxForTest();
			AssertEquals("List should be empty as a precondition to this test", 0, slb.Items.Count);
			var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, 65535, 1234567, 0);

			// act-assert
			AssertNoExceptionThrown("", () => slb.OnMouseClick(mouseEventArgs));
		}

		public void TestOnMouseClick_Normal()
		{
			// arrange
			var executeItemCalled = false;
			var listCopy = new List<IDisplayItem> { DisplayItemFactory.CreateSearchItem(() => executeItemCalled = true, "Item 1", "Description 1") };
			var mouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, 58, 6, 0);
			var slb = new ZSearchListBoxForTest
			{
				DataSource = listCopy,
				Visible = true,
			};

			// act-assert
			AssertNoExceptionThrown("", () => slb.OnMouseClick(mouseEventArgs));
			Assert("List item was clicked on but not executed", executeItemCalled);
		}

		#endregion

		#region Functionality

		public void TestFirstSelectableItemIndex() => _TestFirstSelectableItemIndex(TestItems_Normal, 1);
		public void TestFirstSelectableItemIndexNoHeadings() => _TestFirstSelectableItemIndex(TestItems_NoHeadings, 0);

		static void _TestFirstSelectableItemIndex(IList<IDisplayItem> items, int expectedIndex)
		{
			// arrange
			var slb = new ZSearchListBox
			{
				DataSource = items
			};

			// assert
			AssertEquals("Initial SelectedIndex was wrong", expectedIndex, slb.SelectedIndex);
			AssertEquals("FirstSelectableItemIndex was wrong", expectedIndex, slb.FirstSelectableItemIndex());
		}

		public void TestNextSelectableItemIndex() => _TestNextSelectableItemIndex(TestItems_Normal, 1, 2, 4);
		public void TestNextSelectableItemIndexNoHeadings() => _TestNextSelectableItemIndex(TestItems_NoHeadings, 1, 2, 3);

		static void _TestNextSelectableItemIndex(IList<IDisplayItem> items, params int[] expectedIndices)
		{
			// arrange
			var slb = new ZSearchListBox
			{
				DataSource = items,
				SelectedIndex = 1
			};

			// assert
			AssertEquals("Initial SelectedIndex was wrong", expectedIndices[0], slb.SelectedIndex);
			AssertEquals("NextSelectableItemIndex was wrong", expectedIndices[1], slb.NextSelectableItemIndex());

			slb.SelectedIndex = slb.NextSelectableItemIndex();
			AssertEquals("SelectedIndex was wrong", expectedIndices[1], slb.SelectedIndex);
			AssertEquals("NextSelectableItemIndex was wrong", expectedIndices[2], slb.NextSelectableItemIndex());

			// don't go past end
			slb.SelectedIndex = slb.NextSelectableItemIndex();
			AssertEquals("SelectedIndex was wrong", expectedIndices[2], slb.SelectedIndex);
			AssertEquals("NextSelectableItemIndex was wrong", expectedIndices[2], slb.NextSelectableItemIndex());
		}

		public void TestPreviousSelectableItemIndex() => _TestPreviousSelectableItemIndex(TestItems_Normal, 1, 1);
		public void TestPreviousSelectableItemIndexNoHeadings() => _TestPreviousSelectableItemIndex(TestItems_NoHeadings, 1, 0);

		static void _TestPreviousSelectableItemIndex(IList<IDisplayItem> items, params int[] expectedIndices)
		{
			// arrange
			var slb = new ZSearchListBox
			{
				DataSource = items,
				SelectedIndex = 1
			};

			// assert
			AssertEquals("Initial SelectedIndex was wrong", expectedIndices[0], slb.SelectedIndex);
			AssertEquals("PreviousSelectableItemIndex was wrong", expectedIndices[1], slb.PreviousSelectableItemIndex());

			// don't go past start
			slb.SelectedIndex = slb.PreviousSelectableItemIndex();
			AssertEquals("SelectedIndex was wrong", expectedIndices[1], slb.SelectedIndex);
			AssertEquals("PreviousSelectableItemIndex was wrong", expectedIndices[1], slb.PreviousSelectableItemIndex());
		}

		public void TestExecuteSelectedItem()
		{
			// arrange
			var slb = new ZSearchListBox();
			var triggered = false;
			var data = TestItems_Normal;
			data.Add(DisplayItemFactory.CreateSearchItem(new Action(() => triggered = true), "Item 33", "Order 66"));
			slb.DataSource = data;
			slb.SelectedIndex = data.Count - 1;

			// act
			slb.ExecuteSelectedItem();

			// assert
			Assert("Execute action was not triggered", triggered);
		}

		public void TestExecuteActionIsNull() => AssertNoExceptionThrown(() => DisplayItemFactory.CreateSearchItem(null, "Item 33", "Order 66"));

		#endregion
	}
}
