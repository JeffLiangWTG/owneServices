using System.Collections.Generic;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZDropEditListTest : TestCaseWithFactory
	{
		#region Implementation

		ZDropEditListForTest DropEditList
		{
			get
			{
				if (dropEditList == null)
				{
					dropEditList = new ZDropEditListForTest();
				}
				return dropEditList;
			}
		}

		ZDropEditListForTest dropEditList;

		ZDropEditListForTestWithRelatedControl DropEditListWithRelatedControl
		{
			get
			{
				if (dropEditListWithRelatedControl == null)
				{
					dropEditListWithRelatedControl = new ZDropEditListForTestWithRelatedControl();
				}
				return dropEditListWithRelatedControl;
			}
		}

		ZDropEditListForTestWithRelatedControl dropEditListWithRelatedControl;

		protected override void SetUp()
		{
			base.SetUp();
			DropEditList.OnPreRenderForTest();
		}

		#endregion

		#region TestOnBlurEventHandler

		public void TestOnClickEventHandler()
		{
			AssertEquals("onclick event handler should be assigned", DropEditList.SelectItemHandlerForTest, DropEditList.ListBoxControlForTest.Attributes["onclick"]);
			AssertEquals(DropEditList.SelectItemHandlerForTest, "ZDropEditList_SelectItem('" + DropEditList.TextBoxControlForTest.ClientID + "')");
		}

		#endregion

		#region TestOnKeyDownEventHandler

		public void TestOnKeyDownEventHandler()
		{
			AssertEquals("onkeydown event handler should be assigned", DropEditList.KeysHandlerForTest, DropEditList.ListBoxControlForTest.Attributes["onkeydown"]);
			AssertEquals(DropEditList.KeysHandlerForTest, "return ZDropEditList_KeyPress('" + DropEditList.TextBoxControlForTest.ClientID + "')");
		}

		#endregion

		#region TestButtonBackgroundStyle

		public void TestButtonBackgroundStyle()
		{
			DropEditList.OnPreRenderForTest();
			AssertNull("Button's background should not be assigned", DropEditList.ButtonControlForTest.Style["background"]);
		}

		#endregion

		#region TestButtonClickHandler

		public void TestButtonClickHandler()
		{
			AssertEquals("onclick event handler for button should be assigned", DropEditList.ButtonClickHandlerForTest, DropEditList.ButtonControlForTest.Attributes["onclick"]);
			AssertEquals(DropEditList.ButtonClickHandlerForTest, "ZDropEditList_ShowPopup('" + DropEditList.PopupIDForTest + "')");
			AssertEquals(DropEditListWithRelatedControl.ButtonClickHandlerForTest, "ZDropEditList_ShowPopup('" + DropEditList.PopupIDForTest + "','some_related_control_identifier')");
		}

		#endregion

		#region TestResources

		public void TestResources()
		{
			AssertNotNull("Resources should be not equal to null", DropEditList.Resources);
			Assert("Should contain at least 1 resource", DropEditList.Resources.Count >= 1);
			AssertCollectionContains("ZDropEditScriptBlock should be in Resources", DropEditList.ScriptFileForTest, DropEditList.Resources);
		}

		#endregion

		public void TestBindListControl()
		{
			var myDummy = Factory.New<DummyBusinessObject>();

			var child1 = myDummy.Collection.AddNew();
			child1.Z0_Description = "description1";
			child1.Z0_Code = "C1";
			child1.Z0_Number = 1;

			var child5A = myDummy.Collection.AddNew();
			child5A.Z0_Description = "description5A";
			child5A.Z0_Code = "C5A";
			child5A.Z0_Number = 5;

			var child5B = myDummy.Collection.AddNew();
			child5B.Z0_Description = "description5B";
			child5B.Z0_Code = "C5B";
			child5B.Z0_Number = 5;

			var child7 = myDummy.Collection.AddNew();
			child7.Z0_Description = "description7";
			child7.Z0_Code = "C7";
			child7.Z0_Number = 7;

			Factory.Save();

			DropEditList.BindTo = "Z0_Code";
			DropEditList.BindToList = "FilteredCollection";
			DropEditList.Bind(myDummy);

			var itemsForFilteredCollection = DropEditList.ListBoxControlForTest.Items;
			AssertEquals("There should be as many items in the DropEditList as in the bound filtered collection.", myDummy.FilteredCollection.Count, DropEditList.ListBoxControlForTest.Items.Count);
			AssertEquals("2 items expeteced", 2, DropEditList.ListBoxControlForTest.Items.Count);

			SortListItemsAscending(itemsForFilteredCollection);
			AssertEquals("C5A", itemsForFilteredCollection[0].Value);
			AssertEquals("C5B", itemsForFilteredCollection[1].Value);

			DropEditList.UnBind();
			DropEditList.BindTo = "Z0_Code";
			DropEditList.BindToList = "Collection";
			DropEditList.Bind(myDummy);
			AssertEquals("There should be as many items in the DropEditList as in the bound unfiltered collection.", myDummy.Collection.Count, DropEditList.ListBoxControlForTest.Items.Count);
		}

		void SortListItemsAscending(ListItemCollection items)
		{
			var list = new List<ListItem>();
			foreach (ListItem item in items)
			{
				list.Add(item);
			}
			list.Sort(delegate(ListItem x, ListItem y)
			{ return x.Text.CompareTo(y.Text); });
			items.Clear();
			items.AddRange(list.ToArray());
		}
	}
}
