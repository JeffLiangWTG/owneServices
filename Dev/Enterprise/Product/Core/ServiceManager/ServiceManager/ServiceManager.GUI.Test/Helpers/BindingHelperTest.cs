using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	public class BindingHelperTest : TestCase
	{
		public void TestUpdateControlBindTos()
		{
			// Arrange
			using var parentControl = new StmServiceTaskRecurrenceControl { BindTo = "parent" };
			using var childControl = new StmServiceTaskRecurrenceControl { BindTo = "child" };

			parentControl.SetBindingMember(parentControl.BindTo);
			childControl.SetBindingMember(childControl.BindTo);
			childControl.Parent = parentControl;

			// Act
			BindingHelper.UpdateControlBindTos(parentControl, parentControl.BindTo);

			// Assert
			AssertEquals("parent.child", childControl.GetBindingMember());
		}

		public void TestUpdateControlBindTosMultiLevel()
		{
			// Arrange
			using var parentControl = new StmServiceTaskRecurrenceControl { BindTo = "parent" };
			parentControl.SetBindingMember(parentControl.BindTo);

			using var childControl1 = AddChildControl(parentControl, 1);
			using var childControl2 = AddChildControl(childControl1, 2);
			using var childControl3 = AddChildControl(childControl2, 3);

			// Act
			BindingHelper.UpdateControlBindTos(parentControl, parentControl.BindTo);

			// Assert
			AssertEquals("parent.child1", childControl1.GetBindingMember());
			AssertEquals("parent.child2", childControl2.GetBindingMember());
			AssertEquals("parent.child3", childControl3.GetBindingMember());

			Control AddChildControl(Control parent, int level)
			{
				var childControl = new StmServiceTaskRecurrenceControl { BindTo = $"child{level}" };
				childControl.SetBindingMember(childControl.BindTo);
				childControl.Parent = parent;

				return childControl;
			}
		}
	}
}
