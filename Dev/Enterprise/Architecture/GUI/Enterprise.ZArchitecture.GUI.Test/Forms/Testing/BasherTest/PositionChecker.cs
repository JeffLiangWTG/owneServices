using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public class PositionChecker
	{
		public PositionChecker(BasherTest test)
		{
			this.test = test;
		}

		readonly BasherTest test;

		bool ControlIsValidForTabTest(Control control)
		{
			return !(control is Label)
				&& !(control is ZDateRangeControl)
				&& control.TabStop
				&& control.Visible
				&& (control.CanFocus || control.Controls.Count > 0);
		}

		public void CheckControlPosition(Control control)
		{
			if (control.Visible)
			{
				CheckControlParentRelationship(control);
				CheckControlSiblingRelationship(control);
			}

			CheckTabOrder(control);
		}

		void CheckTabOrder(Control control)
		{
			if (ControlIsValidForTabTest(control))
			{
				var previousControl = GetPreviousEnabledControl(control);
				if (previousControl != null)
				{
					const int movingRightDelta = 5;

					var controlScreenLocation = control.PointToScreen(control.Location);
					var previousControlScreenLocation = control.PointToScreen(previousControl.Location);
					var movingLeft = controlScreenLocation.X < previousControlScreenLocation.X;
					var movingUpOrSameLevel = controlScreenLocation.Y <= previousControlScreenLocation.Y;
					var movingRight = (controlScreenLocation.X - previousControlScreenLocation.X) > movingRightDelta;

					var previousControlIsAGrid = previousControl is ZGrid;
					var completelyToTheRightOfPreviousControl = (previousControl.Left + previousControl.Width) <= control.Left;

					var tabOrderError = ZString.Empty;
					if (movingUpOrSameLevel)
					{
						if (movingLeft || !movingRight)
						{
							tabOrderError = "Tab order problem in control";
						}
						if (previousControlIsAGrid && !completelyToTheRightOfPreviousControl)
						{
							tabOrderError = "Tab order problem in control going back up from a Grid";
						}
					}
					if (!tabOrderError.IsEmpty && (test == null || !test.AllowTabBackward(control, previousControl)))
					{
						throw new ZFormBashingExceptionWhereCallStackIsMeaningless(tabOrderError,
							"Control: " + GetControlDescriptionAndTabIndexWithParentNameAndType(control) + System.Environment.NewLine +
							"Previous control : " + GetControlDescriptionAndTabIndexWithParentNameAndType(previousControl));
					}
				}
			}
		}

		void CheckControlParentRelationship(Control control)
		{
			if (control.Parent != null &&
				control.Parent.Width > 0 &&
				control.Parent.Height > 0 &&
				(control.Top < 0 ||
				control.Left < 0 ||
				control.Left + control.Width > control.Parent.Width ||
				(!ControlHeightIsAllowedToGoOutsideBoundsOfParent(control) && control.Top + control.Height > control.Parent.Height)))
			{
				var parentAsScrollableControl = control.Parent as ScrollableControl;
				if (parentAsScrollableControl == null || !parentAsScrollableControl.AutoScroll)
				{
					if (test == null || !test.AllowOutsideOfParentControl(control))
					{
						throw new ZFormBashingExceptionWhereCallStackIsMeaningless("Control positioned incorrectly (outside the bounds of the parent control)",
							"Control : " + GetControlDescriptionWithParentAndItsBounds(control));
					}
				}
			}
		}

		void CheckControlSiblingRelationship(Control control)
		{
			if (control.Parent != null)
			{
				foreach (Control siblingControl in new ArrayList(control.Parent.Controls))
				{
					if (siblingControl != control && siblingControl.Visible && control.Visible)
					{
						if (ControlsOverlap(control, siblingControl))
						{
							if (test == null || !test.AllowOverlap(control, siblingControl))
							{
								throw new ZFormBashingExceptionWhereCallStackIsMeaningless(
									"Control overlaps another visible control",
									"Control : " + GetControlDescriptionWithParentNameAndType(control) + System.Environment.NewLine +
									"Sibling control : " + GetControlDescriptionWithParentNameAndType(siblingControl) + System.Environment.NewLine +
									"Call control.AllowOverlap(siblingControl) in the Form or UserControl production initialization code to declare allowed overlaps if appropriate.");
							}
						}
					}
				}
			}
		}

		bool ControlsOverlap(Control control, Control siblingControl)
		{
			bool result;
			if (control.Parent == null || control.Parent.Controls.GetChildIndex(control) > control.Parent.Controls.GetChildIndex(siblingControl))
			{
				result = false;
			}
			else
			{
				var rectangle1 = GetControlRectangle(control);
				var rectangleSiblingControl = GetControlRectangle(siblingControl);
				if (siblingControl is Label)
				{
					rectangleSiblingControl.Height -= 3;    // fudge factor for Tahoma font
				}
				result = rectangle1.IntersectsWith(rectangleSiblingControl);
			}
			return result;
		}

		const int EmptySpaceAtEndOfLabel = 4;

		Rectangle GetControlRectangle(Control control)
		{
			var controlHeight = control.Height;
			var controlWidth = control.Width;
			if (control is Label)
			{
				controlWidth -= ControlDpiScalingHelper.ScaleToCurrentDpiX(EmptySpaceAtEndOfLabel);
			}
			return ControlDpiScalingHelper.NewScaledRectangle(control.Location.X, control.Location.Y, controlWidth, controlHeight, false);
		}

		bool ControlHeightIsAllowedToGoOutsideBoundsOfParent(Control control)
		{
			var result = false;
			var toolBar = control as ToolBar;

			if (toolBar != null && toolBar.Parent != null)
			{
				var allowableSpace = toolBar.Height - toolBar.ButtonSize.Height;
				result = toolBar.Bottom >= toolBar.Parent.Height + allowableSpace;
			}

			return result;
		}

		string GetControlDescriptionAndTabIndexWithParentNameAndType(Control control)
		{
			return control.Name + " (" + control.GetType().FullName + ")     in "
				+ control.Parent.Name + " (" + control.Parent.GetType().FullName + ")" + System.Environment.NewLine
				+ "Bounds :" + control.Bounds.ToString() + " (TabIndex: " + control.TabIndex.ToString() + ")";
		}

		string GetControlDescriptionWithParentNameAndType(Control control)
		{
			return control.Name + " (" + control.GetType().FullName + ")     in "
				+ control.Parent.Name + " (" + control.Parent.GetType().FullName + ")" + System.Environment.NewLine
				+ "Bounds :" + control.Bounds.ToString();
		}

		string GetControlDescription(Control control)
		{
			return control.Name + " (" + control.GetType().FullName + ")" + System.Environment.NewLine
				+ "Bounds :" + control.Bounds.ToString();
		}

		string GetControlDescriptionWithParentAndItsBounds(Control control)
		{
			return GetControlDescription(control) + System.Environment.NewLine + "Parent Control: " + GetControlDescription(control.Parent);
		}

		Control GetPreviousEnabledControl(Control control)
		{
			Control result = null;
			var parentControl = control.Parent;
			if (parentControl != null)
			{
				var currentControl = control;
				while (currentControl != null)
				{
					currentControl = parentControl.GetNextControl(currentControl, false);
					if (currentControl != null && currentControl.Parent == parentControl && ControlIsValidForTabTest(currentControl))
					{
						result = currentControl;
						break;
					}
				}
			}
			return result;
		}

		#region Test

		class Test : TestCase
		{
			[ExpectNoExceptions]
			public void TestCheckControlSiblingRelationshipGood()
			{
				using (var form = new Form())
				{
					var control = new Control("", 0, 0, 10, 10);
					var siblingControl = new Control("", 10, 0, 10, 10);
					form.Controls.Add(control);
					form.Controls.Add(siblingControl);
					form.Show();
					new PositionChecker(null).CheckControlSiblingRelationship(control);
				}
			}

			[ExpectException(typeof(ZFormBashingExceptionWhereCallStackIsMeaningless))]
			public void TestCheckControlSiblingRelationshipBad()
			{
				using (var form = new Form())
				{
					var control = new Control("", 0, 0, 10, 10);
					var siblingControl = new Control("", 9, 0, 10, 10);
					form.Controls.Add(control);
					form.Controls.Add(siblingControl);
					form.Show();

					new PositionChecker(null).CheckControlSiblingRelationship(control);
				}
			}
		}

		#endregion
	}
}
