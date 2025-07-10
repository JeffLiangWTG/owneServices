using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Layout.Testing
{
	sealed class RowLayoutDependentControlManagerTest : TestCase
	{
		public void TestSetRowVisibleDependentControls()
		{
			using (UserControlWithVisibilityRelationship basePanel = new UserControlWithVisibilityRelationship())
			using (RowLayoutPanel panel = new RowLayoutPanel())
			{
				basePanel.Controls.Add(panel);

				TextBox ctrl1 = new TextBox();
				TextBox ctrl2 = new TextBox();
				panel.Controls.Add(ctrl1);
				panel.Controls.Add(ctrl2);

				panel.SetRow(ctrl1, 1);
				panel.SetRow(ctrl2, 3);

				basePanel.VisibilityRelationshipProvider.SetDependency(ctrl2, ctrl1);

				RowLayoutDependentControlManager.SetRowAndMoveDependentRows(panel.LayoutEngine as RowLayout, ctrl1, 3, basePanel);
				AssertEquals(3, panel.GetRow(ctrl1));
				AssertEquals(5, panel.GetRow(ctrl2));
			}
		}

		public void TestSetRowVisibleDependentControls_WhenRowSettingError()
		{
			using (var basePanel = new UserControlWithVisibilityRelationship())
			using (var panel = new RowLayoutPanel())
			{
				basePanel.Controls.Add(panel);

				var ctrl1 = new TextBox();
				var ctrl2 = new TextBox();
				panel.Controls.Add(ctrl1);
				panel.Controls.Add(ctrl2);

				panel.SetRow(ctrl1, 1);
				panel.SetRow(ctrl2, -1);

				basePanel.VisibilityRelationshipProvider.SetDependency(ctrl2, ctrl1);

				AssertExceptionThrown<RowLayoutDependentControlManager.SetRowAndMoveDependentRowsException>(() => RowLayoutDependentControlManager.SetRowAndMoveDependentRows(panel.LayoutEngine as RowLayout, ctrl1, -1, basePanel));
			}
		}

		#region TestPrepareSpaceForDependentRows

		public void TestPrepareSpaceForDependentRows()
		{
			using (UserControlWithVisibilityRelationship basePanel = new UserControlWithVisibilityRelationship())
			using (RowLayoutPanel panel = new RowLayoutPanel())
			{
				basePanel.Controls.Add(panel);

				var ctrl1 = new TextBox();
				var ctrl1a = new TextBox();
				var ctrl1b = new TextBox();
				var ctrl2 = new TextBox();
				var ctrl3 = new TextBox();
				var ctrl3a = new TextBox();
				var ctrl4 = new TextBox();
				var ctrl4a = new TextBox();

				panel.Controls.Add(ctrl1);
				panel.Controls.Add(ctrl1a);
				panel.Controls.Add(ctrl1b);
				panel.Controls.Add(ctrl2);
				panel.Controls.Add(ctrl3);
				panel.Controls.Add(ctrl3a);
				panel.Controls.Add(ctrl4);
				panel.Controls.Add(ctrl4a);

				panel.SetRow(ctrl1, 1);
				panel.SetRow(ctrl1a, 2);
				panel.SetRow(ctrl1b, 4);
				panel.SetRow(ctrl2, 5);
				panel.SetRow(ctrl3, 6);
				panel.SetRow(ctrl3a, 6);
				panel.SetRow(ctrl4, 7);
				panel.SetRow(ctrl4a, 8);

				basePanel.VisibilityRelationshipProvider.SetDependency(ctrl1a, ctrl1);
				basePanel.VisibilityRelationshipProvider.SetDependency(ctrl1b, ctrl1);
				basePanel.VisibilityRelationshipProvider.SetDependency(ctrl3a, ctrl3);
				basePanel.VisibilityRelationshipProvider.SetDependency(ctrl4a, ctrl4);

				Dictionary<Control, int> orderList = new Dictionary<Control, int> { { ctrl1, 2 }, { ctrl2, 3 }, { ctrl3, 1 }, { ctrl4, 4 } };

				RowLayoutDependentControlManager.PrepareSpaceForDependentRows(panel.LayoutEngine as RowLayout, basePanel, orderList);

				AssertEquals(2, orderList[ctrl1]);
				AssertEquals(6, orderList[ctrl2]);
				AssertEquals(1, orderList[ctrl3]);
				AssertEquals(7, orderList[ctrl4]);
			}
		}

		#endregion

		#region Implementation

		class UserControlWithVisibilityRelationship : Control, IControlVisibilityRelationshipProviderSource
		{
			public ControlVisibilityRelationshipProvider VisibilityRelationshipProvider
			{
				get { return fVisibilityRelationshipProvider ?? (fVisibilityRelationshipProvider = new ControlVisibilityRelationshipProvider()); }
			}
			ControlVisibilityRelationshipProvider fVisibilityRelationshipProvider;
		}

		#endregion
	}
}
