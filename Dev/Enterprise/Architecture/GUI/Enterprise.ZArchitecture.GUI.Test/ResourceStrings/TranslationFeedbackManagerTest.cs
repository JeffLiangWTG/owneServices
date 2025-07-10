using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Testing;
using NUnit.Framework;
using MenuItem = System.Windows.Forms.MenuItem;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TranslationFeedbackManagerTest : TestCase
	{
		public void TestSimpleControls()
		{
			using (var label = new ZLabel() { CaptionResourceString = new ResourceStringData("ZL", "Zee Label") })
			using (SetupTestForm(label))
			{
				AssertTranslationFeedback(label.translationFeedbackManager, "ZL", "Zee Label");
			}

			using (var linkLabel = new ZLinkLabel() { CaptionResourceString = new ResourceStringData("ZLL", "Zee Link Label") })
			using (SetupTestForm(linkLabel))
			{
				linkLabel.LinkClicked += new LinkLabelLinkClickedEventHandler(ControlAction);
				AssertTranslationFeedback(linkLabel.translationFeedbackManager, "ZLL", "Zee Link Label");
			}

			using (var button = new ZButton() { CaptionResourceString = new ResourceStringData("ZB", "Zee Button") })
			using (SetupTestForm(button))
			{
				button.Click += new EventHandler(ControlAction);
				AssertTranslationFeedback(button.translationFeedbackManager, "ZB", "Zee Button");
			}

			using (var checkBox = new ZCheckBox() { CaptionResourceString = new ResourceStringData("ZC", "Zee Checkbox") })
			using (SetupTestForm(checkBox))
			{
				checkBox.CheckedChanged += new EventHandler(ControlAction);
				AssertTranslationFeedback(checkBox.translationFeedbackManager, "ZC", "Zee Checkbox");
			}
		}

		public void TestTabControl()
		{
			using (var tabControl = new ZTabControl())
			using (SetupTestForm(tabControl))
			{
				tabControl.TabIndexChanged += new EventHandler(ControlAction);
				tabControl.TabPages.Add(new ZTabPage() { CaptionResourceString = new ResourceStringData("1", "First Tab") });
				tabControl.TabPages.Add(new ZTabPage() { CaptionResourceString = new ResourceStringData("2", "Second Tab") });
				AssertTranslationFeedback(tabControl.translationFeedbackManager, Point.Add(tabControl.GetTabRect(0).Location, new Size(1, 1)), "1", "First Tab");
				AssertTranslationFeedback(tabControl.translationFeedbackManager, Point.Add(tabControl.GetTabRect(1).Location, new Size(1, 1)), "2", "Second Tab");
			}
		}

		public void TestDropEdit()
		{
			var dummyWithList = new BusinessObjectFactory().New<DummyWithSettableCodeDescriptionPairList>();
			var list = new CodeDescriptionPairList();
			list.AddPair("ONE", ResString.GetMultilingualString("111", "First Item"));
			list.AddPair("TWO", ResString.GetMultilingualString("222", "Second Item"));
			dummyWithList.SetDummyList(list);
			using (var testForm = new TestDropEditForm(dummyWithList))
			{
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				testForm.DropEdit.OnSelectedIndexChanged(0);
				AssertTranslationFeedback(testForm.DropEdit.descriptionBoxTranslationFeedbackManager, "111", "First Item");
				AssertTranslationFeedback(testForm.DropEdit.codeBoxTranslationFeedbackManager, "111", "First Item");

				testForm.DropEdit.OnSelectedIndexChanged(1);
				AssertTranslationFeedback(testForm.DropEdit.descriptionBoxTranslationFeedbackManager, "222", "Second Item");
				AssertTranslationFeedback(testForm.DropEdit.codeBoxTranslationFeedbackManager, "222", "Second Item");
			}
		}

		public void TestDropEditWithoutDescriptions()
		{
			var dummyWithList = new BusinessObjectFactory().New<DummyWithSettableCodeDescriptionPairList>();
			var list = new CodeDescriptionPairList();
			list.AddPair(ResString.GetMultilingualString("1", "ONE"), "First Item");
			list.AddPair(ResString.GetMultilingualString("2", "TWO"), "Second Item");
			dummyWithList.SetDummyList(list);
			using (var testForm = new TestDropEditForm(dummyWithList))
			{
				testForm.DropEdit.ShowDescriptionBox = false;
				testForm.DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
				testForm.Show();
				Application.DoEvents();
				UserIdleWorker.Flush();

				testForm.DropEdit.OnSelectedIndexChanged(0);
				AssertTranslationFeedback(testForm.DropEdit.descriptionBoxTranslationFeedbackManager, "1", "ONE");
				AssertTranslationFeedback(testForm.DropEdit.codeBoxTranslationFeedbackManager, "1", "ONE");

				testForm.DropEdit.OnSelectedIndexChanged(1);
				AssertTranslationFeedback(testForm.DropEdit.descriptionBoxTranslationFeedbackManager, "2", "TWO");
				AssertTranslationFeedback(testForm.DropEdit.codeBoxTranslationFeedbackManager, "2", "TWO");
			}
		}

		public void TestGrid()
		{
			using (var testForm = SetupTestForm(null))
			{
				((ZGridColumnInfo)testForm.Grid.ColumnStyles[1]).CaptionResourceString = new ResourceStringData("ZN", "Zee Number");
				testForm.Show();
				var initialSort = testForm.Grid.Sort;
				var x = testForm.Grid.TableStyles[0].RowHeaderWidth + 2;
				AssertTranslationFeedback(testForm.Grid.translationFeedbackManager, new Point(x, 4), "DummyBizo|Z0_Description", "Description");
				AssertEquals("sort not changed", initialSort, testForm.Grid.Sort);
				AssertTranslationFeedback(testForm.Grid.translationFeedbackManager, new Point(x + testForm.Grid.TableStyles[0].GridColumnStyles[0].Width, 4), "ZN", "Zee Number");
				AssertEquals("sort not changed", initialSort, testForm.Grid.Sort);

				using (var gridCustomize = new ZGridCustomiseTester(testForm.Grid.Columns, testForm.Grid.Columns, testForm.Grid))
				{
					gridCustomize.Show();
					AssertTranslationFeedback(gridCustomize.CurrentColumnsListBoxExposed.translationFeedbackManager, Point.Add(gridCustomize.CurrentColumnsListBoxExposed.GetItemRectangle(0).Location, new Size(2, 2)), "DummyBizo|Z0_Description", "Description");
					AssertTranslationFeedback(gridCustomize.CurrentColumnsListBoxExposed.translationFeedbackManager, Point.Add(gridCustomize.CurrentColumnsListBoxExposed.GetItemRectangle(1).Location, new Size(2, 2)), "ZN", "Zee Number");
				}
			}

			using (var testForm = SetupTestForm(null))
			{
				testForm.Show();
				testForm.Grid.Columns[0].ColumnStyle.Alignment = HorizontalAlignment.Right;
				((ZGridColumnStyle)testForm.Grid.Columns[0].ColumnStyle).PadRightForTesting = true;
				var initialSort = testForm.Grid.Sort;
				var x = testForm.Grid.TableStyles[0].RowHeaderWidth + 2;
				AssertTranslationFeedback(testForm.Grid.translationFeedbackManager, new Point(x, 4), "DummyBizo|Z0_Description", "Description");
			}
		}

		public void TestGroupName()
		{
			using (var testForm = SetupTestForm(null))
			{
				((ZGridColumnInfo)testForm.Grid.ColumnStyles[1]).CaptionResourceString = new ResourceStringData("ZN", "Zee Number");
				testForm.Show();

				testForm.Grid.Columns[0].GroupName = new ResourceStringData("DummyBizo|Z0_Description", "Description");
				testForm.Grid.Columns[1].GroupName = new ResourceStringData("ZN", "Zee Number");

				using (var gridCustomize = new ZGridCustomiseTester(testForm.Grid.Columns, testForm.Grid.Columns, testForm.Grid))
				{
					gridCustomize.Show();
					AssertTranslationFeedback(gridCustomize.CurrentColumnsListBoxExposed.translationFeedbackManager, Point.Add(gridCustomize.CurrentColumnsListBoxExposed.GetItemRectangle(0).Location, new Size(2, 2)), "DummyBizo|Z0_Description", "Description");
					AssertTranslationFeedback(gridCustomize.CurrentColumnsListBoxExposed.translationFeedbackManager, Point.Add(gridCustomize.CurrentColumnsListBoxExposed.GetItemRectangle(1).Location, new Size(2, 2)), "ZN", "Zee Number");
				}
			}
		}

		public void TestTreeView()
		{
			using (var treeView = new ZTreeView { Dock = DockStyle.Fill })
			{
				treeView.AfterSelect += new TreeViewEventHandler(ControlAction);

				var nodeA = new TreeNodeWithMultilingualDescription(ResString.GetMultilingualString("nodeA", "A"));
				treeView.Nodes.Add(nodeA);
				var nodeA1 = new TreeNodeWithMultilingualDescription(ResString.GetMultilingualString("nodeA1", "One"));
				nodeA.Nodes.Add(nodeA1);
				var nodeA2 = new TreeNodeWithMultilingualDescription(ResString.GetMultilingualString("nodeA2", "Two"));
				nodeA.Nodes.Add(nodeA2);
				var nodeB = new TreeNodeWithMultilingualDescription(ResString.GetMultilingualString("nodeB", "B"));
				treeView.Nodes.Add(nodeB);
				var nodeB1 = new TreeNodeWithMultilingualDescription(ResString.GetMultilingualString("nodeB1", "1"));
				nodeB.Nodes.Add(nodeB1);
				var nodeB1I = new TreeNodeWithMultilingualDescription(ResString.GetMultilingualString("nodeB1I", "I"));
				nodeB1.Nodes.Add(nodeB1I);

				using (var testForm = SetupTestForm(treeView))
				{
					testForm.Show();
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeA.Bounds.Location, new Size(2, 2)), "nodeA", "A");
					nodeA.Expand();
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeA1.Bounds.Location, new Size(2, 2)), "nodeA1", "One");
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeA2.Bounds.Location, new Size(2, 2)), "nodeA2", "Two");
					nodeB.ExpandAll();
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeB.Bounds.Location, new Size(2, 2)), "nodeB", "B");
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeB1.Bounds.Location, new Size(2, 2)), "nodeB1", "1");
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeB1I.Bounds.Location, new Size(2, 2)), "nodeB1I", "I");
				}
			}
		}

		public void TestTreeViewNotMultilingual()
		{
			using (var treeView = new ZTreeView { Dock = DockStyle.Fill })
			{
				treeView.AfterSelect += new TreeViewEventHandler(ControlAction);

				var nodeA = new TreeNode(Res.GetString("nodeA", "A"));
				treeView.Nodes.Add(nodeA);
				var nodeA1 = new TreeNode(Res.GetString("nodeA1", "One"));
				nodeA.Nodes.Add(nodeA1);
				var nodeA2 = new TreeNode(Res.GetString("nodeA2", "Two"));
				nodeA.Nodes.Add(nodeA2);
				var nodeB = new TreeNode(Res.GetString("nodeB", "B"));
				treeView.Nodes.Add(nodeB);
				var nodeB1 = new TreeNode(Res.GetString("nodeB1", "1"));
				nodeB.Nodes.Add(nodeB1);
				var nodeB1I = new TreeNode(Res.GetString("nodeB1I", "I"));
				nodeB1.Nodes.Add(nodeB1I);

				using (var testForm = SetupTestForm(treeView))
				{
					testForm.Show();
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeA.Bounds.Location, new Size(2, 2)), null, "A");
					nodeA.Expand();
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeA1.Bounds.Location, new Size(2, 2)), null, "One");
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeA2.Bounds.Location, new Size(2, 2)), null, "Two");
					nodeB.ExpandAll();
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeB.Bounds.Location, new Size(2, 2)), null, "B");
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeB1.Bounds.Location, new Size(2, 2)), null, "1");
					AssertTranslationFeedback(treeView.translationFeedbackManager, Point.Add(nodeB1I.Bounds.Location, new Size(2, 2)), null, "I");
				}
			}
		}

		public void TestLabelCaptionRender()
		{
			var provider = new MockTranslationFeedbackProvider();
			using (ObjectFactory.Substitute<ITranslationFeedbackProvider>(provider))
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			using (var testForm = SetupTestForm(null))
			{
				testForm.CalcEdit.Left = 100;
				testForm.TextBox.Left = 100;
				testForm.CalcEdit.CaptionResourceString = new ResourceStringData("ZN", "Zee Number");
				testForm.Show();
				Application.DoEvents();

				testForm.SendMouseMove(new MouseEventArgs(MouseButtons.None, 0, testForm.CalcEdit.Left - 5, testForm.CalcEdit.Top + 3, 0));
				testForm.SendClick(EventArgs.Empty);
				AssertEquals("ResourceString Key", "ZN", provider.LastFeedbackKey);
				AssertEquals("Caption", "Zee Number", provider.LastFeedbackCaption);

				testForm.SendMouseMove(new MouseEventArgs(MouseButtons.None, 0, testForm.TextBox.Left - 5, testForm.TextBox.Top + 3, 0));
				testForm.SendClick(EventArgs.Empty);
				AssertEquals("ResourceString Key", "DummyBizo|Z0_Description", provider.LastFeedbackKey);
				AssertEquals("Caption", "Description", provider.LastFeedbackCaption);
			}
		}

		public void TestToolStripButton()
		{
			using (var toolStrip = new ToolStrip())
			{
				var button = new ZToolStripButton() { CaptionResourceString = Res.GetData("k1", "My Button", "ToolTip") };
				button.Click += new EventHandler(ControlAction);
				toolStrip.Items.Add(button);

				using (var testForm = SetupTestForm(toolStrip))
				{
					testForm.Show();
					AssertTranslationFeedback(button, Point.Empty, "k1", "My Button", "ToolTip");
				}
			}
		}

		public void TestContextToolStripButton()
		{
			using (var testForm = SetupTestForm(null))
			using (testForm.ContextMenuStrip = new ContextMenuStrip())
			{
				var menuItem = new ZToolStripMenuItem(Res.GetData("k1", "Context Menu Item"), ControlAction);
				testForm.ContextMenuStrip.Items.Add(menuItem);
				testForm.Show();
				testForm.ContextMenuStrip.Show(Point.Empty);
				AssertTranslationFeedback(menuItem, Point.Empty, "k1", "Context Menu Item");
				testForm.ContextMenuStrip.Close();
			}
		}

		public void TestBalloonWindow()
		{
			using (var label = new ZLabel())
			using (var balloonWindow = new Balloons.BalloonWindow())
			{
				var notifications = new NotificationCollection();
				notifications.AddError("My Field: Your field has an error");
				notifications.AddWarning("This is a warning");
				notifications.AddWarning("This is another warning");
				balloonWindow.Descriptor = new Balloons.BalloonDescriptor(label, "The Caption", "The Description", notifications);
				balloonWindow.Show();
				balloonWindow.Update();
				AssertTranslationFeedback(balloonWindow.translationFeedbackManager, ControlDpiScalingHelper.NewScaledPoint(20, 30), null, "The Caption");
				AssertTranslationFeedback(balloonWindow.translationFeedbackManager, ControlDpiScalingHelper.NewScaledPoint(20, 45), null, "The Description");
				AssertTranslationFeedback(balloonWindow.translationFeedbackManager, ControlDpiScalingHelper.NewScaledPoint(40, 70), null, "Your field has an error");
				AssertTranslationFeedback(balloonWindow.translationFeedbackManager, ControlDpiScalingHelper.NewScaledPoint(40, 85), null, "This is a warning");
				AssertTranslationFeedback(balloonWindow.translationFeedbackManager, ControlDpiScalingHelper.NewScaledPoint(40, 100), null, "This is another warning");
			}
		}

		public void TestOverriddenCaptions()
		{
			using (var label = new ZLabel() { CaptionResourceString = new ResourceStringData("H", "Hidden"), Text = "Visible" })
			using (SetupTestForm(label))
			{
				AssertTranslationFeedback(label.translationFeedbackManager, "H", "Visible");
			}

			using (var label = new ZLabel() { CaptionResourceString = new ResourceStringData("H", "Hidden") })
			using (SetupTestForm(label))
			{
				label.GetExtension<ILabelCaptionRenderer>().Caption = "Visible";
				AssertTranslationFeedback(label.translationFeedbackManager, "H", "Visible");
			}

			var provider = new MockTranslationFeedbackProvider();
			using (ObjectFactory.Substitute<ITranslationFeedbackProvider>(provider))
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			using (var testForm = SetupTestForm(null))
			{
				testForm.TextBox.Left = 100;
				testForm.TextBox.CaptionResourceString = new ResourceStringData("H", "Hidden");
				testForm.TextBox.GetExtension<ILabelCaptionRenderer>().Caption = "Visible";
				testForm.Show();
				Application.DoEvents();
				testForm.TextBox.Text = "Text";

				testForm.SendMouseMove(new MouseEventArgs(MouseButtons.None, 0, testForm.TextBox.Left - 5, testForm.TextBox.Top + 3, 0));
				testForm.SendClick(EventArgs.Empty);
				AssertEquals("Caption", "Visible", provider.LastFeedbackCaption);
			}
		}

		public void TestZFormIgnoresFormVerb()
		{
			var provider = new MockTranslationFeedbackProvider();
			using (ObjectFactory.Substitute<ITranslationFeedbackProvider>(provider))
			using (var testForm = SetupTestForm(null))
			{
				testForm.Show();

				AssertEquals("New ZDummyForm", testForm.Text);
				TranslationFeedbackManager.OpenFeedbackForm(testForm);
				AssertEquals("Caption", "ZDummyForm", provider.LastFeedbackCaption);
			}
		}

		public void TestSecurityCheck()
		{
			try
			{
				using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
				{
					AssertEquals(true, TranslationFeedbackManager.InTranslationFeedbackMode());
					EnvProxy.Instance.Security.TranslationFeedback.IsAllowed = false;
					AssertEquals(false, TranslationFeedbackManager.InTranslationFeedbackMode());
				}
			}
			finally
			{
				EnvProxy.Instance.Security.TranslationFeedback.IsAllowed = true;
			}
		}

		public void TestToolStripItemsConvertedFromMenuItems()
		{
			using (var toolStrip = new ToolStrip())
			{
				var menuItem = new ZMenuItem(ResString.GetMultilingualString("k1", "&Parent"), new MenuItem[] { new ZMenuItem(ResString.GetMultilingualString("k2", "C&hild")) });
				var toolStripItem = (ToolStripDropDownItem)MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem, null);
				toolStrip.Items.Add(toolStripItem);
				using (var testForm = SetupTestForm(toolStrip))
				{
					testForm.Show();
					AssertTranslationFeedback(toolStripItem, Point.Empty, "k1", "&Parent");
					AssertTranslationFeedback(toolStripItem.DropDownItems[0], Point.Empty, "k2", "C&hild");
				}
			}
		}

		[ExpectNoExceptions]
		public void TestF2WithNullUserContext()
		{
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			using (Res.TemporarilySwitchLanguage(SharedConstants.Languages.Italian))
			{
				EnvProxy.Instance.ClearUserContext();
				TranslationFeedbackManager.InTranslationFeedbackMode();
			}
		}

		public void TestToString()
		{
			using (var form = new ZChildForm { Name = "Form1" })
			using (var panel = new ZPanel { Name = "Panel1" })
			using (var label = new ZLabel { Name = "Label1" })
			{
				form.Controls.Add(panel);
				panel.Controls.Add(label);

				using (var manager = new TranslationFeedbackManager(label))
				{
					AssertEquals("TranslationFeedbackManager [Form1 : Panel1 : Label1 (ZLabel)]", manager.ToString());
				}
			}
		}

		#region Implementation

		class TreeNodeWithMultilingualDescription : TreeNode, IMultilingualDescription
		{
			public TreeNodeWithMultilingualDescription(MultilingualString description)
				: base(description)
			{
				this.MultilingualDescription = description;
			}

			public MultilingualString MultilingualDescription
			{
				get;
				set;
			}
		}

		ZDummyForm SetupTestForm(Control control)
		{
			var testForm = new ZDummyForm(new BusinessObjectFactory().New<DummyBusinessObject>());
			testForm.CaptionRenderingEnabled = true;
			if (control != null)
			{
				testForm.Controls.Add(control);
			}
			return testForm;
		}

		void AssertTranslationFeedback(TranslationFeedbackManager manager, string resourceStringKey, string caption)
		{
			AssertTranslationFeedback(manager, Point.Empty, resourceStringKey, caption);
		}

		void AssertTranslationFeedback(TranslationFeedbackManager manager, Point p, string resourceStringKey, string caption)
		{
			var provider = new MockTranslationFeedbackProvider();
			using (ObjectFactory.Substitute<ITranslationFeedbackProvider>(provider))
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			{
				manager.control.GetType().InvokeMember("OnMouseMove", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
				Assert("Control should be in translation feedback mode", manager.InMode);
				manager.control.GetType().InvokeMember("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
				manager.control.GetType().InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
				manager.control.GetType().InvokeMember("OnClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, manager.control, new object[] { EventArgs.Empty });
				AssertEquals("ResourceString Key", resourceStringKey, provider.LastFeedbackKey);
				AssertEquals("Caption", caption, provider.LastFeedbackCaption);
			}
		}

		void AssertTranslationFeedback(ToolStripItem item, Point p, string resourceStringKey, string caption, string fullDescription = null)
		{
			var provider = new MockTranslationFeedbackProvider();
			using (ObjectFactory.Substitute<ITranslationFeedbackProvider>(provider))
			using (TranslationFeedbackManager.EnableTranslationFeedbackModeForTest())
			{
				item.GetType().InvokeMember("OnMouseMove", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, item, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
				item.GetType().InvokeMember("OnMouseUp", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, item, new object[] { new MouseEventArgs(MouseButtons.None, 0, p.X, p.Y, 0) });
				if (item is ToolStripSplitButton)
				{
					item.GetType().InvokeMember("OnButtonClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, item, new object[] { EventArgs.Empty });
				}
				else
				{
					item.GetType().InvokeMember("OnClick", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy, null, item, new object[] { EventArgs.Empty });
				}
				AssertEquals("ResourceString Key", resourceStringKey, provider.LastFeedbackKey);
				AssertEquals("Caption", caption, provider.LastFeedbackCaption);
				if (fullDescription != null)
				{
					AssertEquals("Full Description", fullDescription, provider.LastFeedbackDescription);
				}
				AssertNotNull("Control", provider.LastFeedbackControl);
			}
		}

		void ControlAction(object sender, EventArgs e)
		{
			throw new Exception("Control should not fire in translation feedback mode");
		}

		#endregion
	}
}
