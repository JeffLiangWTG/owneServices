using System.Drawing;
using System.Windows.Forms;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class FormMoveByMouseDragExtensionsTest : TestCase
	{
		public void TestMouseMove()
		{
			using (Form form = new Form())
			{
				form.Location = new Point(10, 15);

				Point existingFormMousePosition = Form.MousePosition;
				FormMoveByMouseDragExtensions.Form_MouseMove(form, new MouseEventArgs(MouseButtons.Left, 1, 11, 16, 0));

				Point expectedPoint = new Point(
					form.Location.X + (Form.MousePosition.X - existingFormMousePosition.X),
					form.Location.Y + (Form.MousePosition.Y - existingFormMousePosition.Y));

				AssertEquals(expectedPoint, form.Location);
			}
		}
	}
}
