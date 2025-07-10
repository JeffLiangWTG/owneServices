using System;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class UPEQueueFiltersTreeViewTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			AssertEquals("There should be 1 root node in the tree view", 1, TreeView.Nodes.Count);
			AssertEquals("The first and only node should be the queue list", "Queues", TreeView.Nodes[0].Text);
			TreeView.Nodes[0].Expand();
			AssertEquals("The queue name nodes should be bound to a list", true, TreeView.Nodes[0].Nodes.Count > 2);
		}

		#region TestExpandsOnFocus
		public void TestExpandsOnFocus()
		{
			DoTestExpandsOnFocus();
			PutTreeViewIntoGroupBoxAndDockFill();
			DoTestExpandsOnFocus();
		}

		void PutTreeViewIntoGroupBoxAndDockFill()
		{
			ZGroupBox treeViewGroupBox = new ZGroupBox();
			Form.Controls.Add(treeViewGroupBox);
			treeViewGroupBox.Parent = Form;
			treeViewGroupBox.Location = TreeView.Location;
			TreeView.Parent = treeViewGroupBox;
			TreeView.Dock = DockStyle.Fill;
			OtherControlOnForm.Focus();
			treeViewGroupBox.Height += 100 - TreeView.Height;
			AssertEquals("Height of the TreeView should remain 100 for the test", 100, TreeView.Height);
		}

		void DoTestExpandsOnFocus()
		{
			TreeView.OverriddenMousePosition = PointWithinTreeView;
			OtherControlOnForm.Focus();
			AssertEquals("TreeView shouldn't be expanded as it is not focused", 100, TreeView.Height);
			TreeView.Parent.Focus();
			TreeView.Focus();
			int expectedHeight = 100 + ControlDpiScalingHelper.RoundToNearestRoundHalfTowardNegativeInfinity(300 * ControlDpiScalingHelper.DpiY / ControlDpiScalingHelper.BaseDpiY);
			AssertEquals("TreeView should be expanded when focused", expectedHeight, TreeView.Height);
			OtherControlOnForm.Focus();
			AssertEquals("TreeView should be shrunk when unfocused", 100, TreeView.Height);
			TestExpandedStateUpdatesOnMouseEvent(TreeView, "OnMouseEnter", expectedHeight);
			TestExpandedStateUpdatesOnMouseEvent(TreeView, "OnMouseLeave", expectedHeight);
			TestExpandedStateUpdatesOnMouseEvent(TreeView, "FireNCMouseLeave", expectedHeight);
			TestExpandedStateUpdatesOnMouseEvent(TreeView, "FireNCMouseMove", expectedHeight);
		}

		void TestExpandedStateUpdatesOnMouseEvent(TestUPEQueueFiltersTreeView treeView, string eventHandlerMethodName, int expectedHeight)
		{
			treeView.OverriddenMousePosition = PointWithinTreeView;
			treeView.Parent.Focus();
			treeView.Focus();
			AssertEquals(eventHandlerMethodName + "; TreeView should be expanded initially for the test", expectedHeight, treeView.Height);
			treeView.OverriddenMousePosition = PointOutsideTreeView;
			FireControlEvent(treeView, eventHandlerMethodName);
			AssertEquals(eventHandlerMethodName + "; TreeView should be shrunk as the mouse moved out of the control", 100, treeView.Height);
			treeView.OverriddenMousePosition = PointWithinTreeView;
			FireControlEvent(treeView, eventHandlerMethodName);
			AssertEquals(eventHandlerMethodName + "; TreeView should be expanded again as the mouse moved into the control", expectedHeight, treeView.Height);
		}

		#endregion
		#region Test Classes
		class TestUPEQueueFiltersTreeView : UPEQueueFiltersTreeView
		{
			public Point OverriddenMousePosition;
			protected override Point MousePosition
			{
				get
				{
					return OverriddenMousePosition;
				}
			}

			public void FireNCMouseLeave(EventArgs e)
			{
#if !WINZOR
				UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), WindowsMessage.WM_NCMOUSEMOVE, 0, 0);
#endif
			}

			public void FireNCMouseMove(EventArgs e)
			{
#if !WINZOR
				UnsafeNativeMethods.SendMessage(new HandleRef(this, Handle), WindowsMessage.WM_NCMOUSELEAVE, 0, 0);
#endif
			}
		}

#endregion
		#region Implementation
		void FireControlEvent(Control control, string eventHandlerMethodName)
		{
			MethodInfo eventMethod = control.GetType().GetMethod(eventHandlerMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
			eventMethod.Invoke(control, new object[] { EventArgs.Empty });
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPEAirCargoFilterBusinessObject filterBizObj = new UPEAirCargoFilterBusinessObject();
			Form = new ZForm();
			TreeView = new TestUPEQueueFiltersTreeView();
			TreeView.FilterBizObj = filterBizObj;
			OtherControlOnForm = new TextBox();
			Form.Controls.Add(OtherControlOnForm);
			Form.Controls.Add(TreeView);
			Form.Show();
			Application.DoEvents();
			Form.Size = new Size(200, 200);
			TreeView.Location = new Point(50, 50);
			TreeView.Size = new Size(100, 100);
			PointWithinTreeView = TreeView.PointToScreen(new Point(50, 50));
			PointOutsideTreeView = new Point(0, 0);
		}

		protected override void TearDown()
		{
			base.TearDown();
			Form.Dispose();
		}

		Point PointWithinTreeView;
		Point PointOutsideTreeView;
		Control OtherControlOnForm;
		ZForm Form;
		TestUPEQueueFiltersTreeView TreeView;
		#endregion
	}
}
