using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Main.Navigation;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace CargoWise.Main.Test.Navigation;

public class ToolStripItemExtensionsTest : TestCase
{
	public void TestToPopupMenuMenu_WhenToolStripItemIsNull()
	{
		ToolStripItem item = null;
		AssertNull(item.ToPopupMenu());
	}

	public void TestToPopupMenuMenu_WhenSeparator()
	{
		var item = new ToolStripSeparator();
		var uut = item.ToPopupMenu();

		AssertNotNull(uut);
		AssertEquals(0, uut.SubMenuItems.Count);
		Assert("Should be separator", uut.IsSeparator);
	}

	public void TestToPopupMenuMenu_WhenToolStripMenuItem()
	{
		var item = new ZToolStripMenuItem(text: "Test");
		var uut = item.ToPopupMenu();

		AssertNotNull(uut);
		AssertEquals(item.Text, uut.Title);
		AssertEquals(item, uut.ToolStripDropDown);
		AssertEquals(string.Empty, uut.InputGestureText);
		AssertEquals(0, uut.SubMenuItems.Count);
		Assert("Should NOT be a separator", !uut.IsSeparator);
	}

	public void TestToPopupMenuMenu_WhenToolStripMenuItem_WhenHasShortcutKeys()
	{
		var item = new ZToolStripMenuItem(text: "Test") { ShortcutKeys = Keys.Control | Keys.Shift | Keys.B };
		var uut = item.ToPopupMenu();

		AssertNotNull(uut);
		AssertEquals("Ctrl+Shift+B", uut.InputGestureText);
		AssertEquals(0, uut.SubMenuItems.Count);
		Assert("Should NOT be a separator", !uut.IsSeparator);
	}

	public void TestToPopupMenuMenu_WhenClassAttribute()
	{
		var item = new ZToolStripMenuItem(text: "Test");
		TypeDescriptor.AddAttributes(item, new SuppressFormsLocalizedTestAttribute());

		var uut = item.ToPopupMenu();

		var attributes = TypeDescriptor.GetAttributes(uut);
		var hasAttribute = attributes[typeof(SuppressFormsLocalizedTestAttribute)] != null;

		Assert("Should have the Class Attribute.", hasAttribute);
	}

	public void TestToPopupMenuMenu_WhenToolStripMenuItem_WhenHasOnClickHandler()
	{
		var count = 0;
		var item = new ZToolStripMenuItem(text: "Test", (s, e) => count++);

		var uut = item.ToPopupMenu();
		uut.Command.Execute(null);

		AssertEquals(1, count);
		AssertEquals(0, uut.SubMenuItems.Count);
	}

	public void TestToPopupMenuMenu_WhenToolStripMenuItem_WhenDoesNotHaveOnClickHandler()
	{
		var item = new ZToolStripMenuItem(text: "Test");

		var uut = item.ToPopupMenu();

		AssertNoExceptionThrown(() => uut.Command.Execute(null));
	}

	public void TestToPopupMenuMenu_WhenToolStripMenuItem_WhenHasSubMenuItems()
	{
		var item = new ZToolStripMenuItem(text: "Test")
		{
			DropDownItems =
			{
				new ZToolStripMenuItem(text: "SubItem1"),
				new ZToolStripMenuItem(text: "SubItem2"),
			},
		};

		var uut = item.ToPopupMenu();

		AssertNoExceptionThrown(() => uut.Command.Execute(null));
		AssertEquals(2, uut.SubMenuItems.Count);
		AssertEquals("SubItem1", uut.SubMenuItems[0].Title);
		AssertEquals("SubItem2", uut.SubMenuItems[1].Title);
		AssertEquals(0, uut.SubMenuItems[0].SubMenuItems.Count);
		AssertEquals(0, uut.SubMenuItems[1].SubMenuItems.Count);
	}

	public void TestToPopupMenuMenu_WhenToolStripMenuItem_WhenHasOnClickHandler_WhenHasSubMenuItems()
	{
		var onClickCalls = new List<string>();
		var item = new ZToolStripMenuItem(text: "Test", (s, e) => onClickCalls.Add("Test"))
		{
			DropDownItems =
			{
				new ZToolStripMenuItem(text: "SubItem1", (s, e) => onClickCalls.Add("SubItem1")),
				new ZToolStripMenuItem(text: "SubItem2")
				{
					DropDownItems =
					{
						new ZToolStripMenuItem(text: "SubSubItem2.1", (s, e) => onClickCalls.Add("SubSubItem2.1")),
					},
				},
			},
		};

		var uut = item.ToPopupMenu();

		uut.Command.Execute(null);

		AssertEquals(2, uut.SubMenuItems.Count);

		uut.SubMenuItems[0].Command.Execute(null);
		uut.SubMenuItems[1].Command.Execute(null);

		AssertEquals(0, uut.SubMenuItems[0].SubMenuItems.Count);
		AssertEquals(1, uut.SubMenuItems[1].SubMenuItems.Count);

		uut.SubMenuItems[1].SubMenuItems[0].Command.Execute(null);
		AssertEquals(0, uut.SubMenuItems[1].SubMenuItems[0].SubMenuItems.Count);

		AssertEquals(3, onClickCalls.Count);
		AssertCollectionContains(onClickCalls, (i) => i == "Test");
		AssertCollectionContains(onClickCalls, (i) => i == "SubItem1");
		AssertCollectionContains(onClickCalls, (i) => i == "SubSubItem2.1");
	}

	public void TestToPopupMenu_WhenAfterCreateActionPassed_ActionIsCalled()
	{
		bool wasCalled = false;
		Action<PopupMenu> afterCreate = _ => wasCalled = true;

		var item = new ZToolStripMenuItem(text: "Test");

		item.ToPopupMenu(afterCreate);

		AssertEquals(wasCalled, true);
	}
}
