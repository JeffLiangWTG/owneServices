using System;
using System.Drawing;
using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Provides extension methods to allow a form to be movable by 
	/// clicking and dragging anywhere within the form.
	/// </summary>
	public static class FormMoveByMouseDragExtensions
	{
		/// <summary>
		/// Allow the form to be movable by clicking and dragging anywhere on the form.
		/// Generally useful when the FormBorderStyle of your form is set to None.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
		public static void MoveFormByMouseDrag(this Form form, bool enabled)
		{
			form.MouseDown -= new MouseEventHandler(Form_MouseDown);
			form.MouseUp -= new MouseEventHandler(Form_MouseUp);
			form.MouseMove -= new MouseEventHandler(Form_MouseMove);

			if (enabled)
			{
				form.MouseDown += new MouseEventHandler(Form_MouseDown);
				form.MouseUp += new MouseEventHandler(Form_MouseUp);
			}
		}

		#region Implementation

		static void Form_MouseUp(object sender, MouseEventArgs e)
		{
			Form form = (Form)sender;
			form.MouseMove -= new MouseEventHandler(Form_MouseMove);
		}

		static void Form_MouseDown(object sender, MouseEventArgs e)
		{
			Form form = (Form)sender;
			mouseDownLocation = Form.MousePosition;
			mouseDownFormLocation = form.Location;

			form.MouseMove -= new MouseEventHandler(Form_MouseMove);
			form.MouseMove += new MouseEventHandler(Form_MouseMove);
		}

		internal static void Form_MouseMove(object sender, MouseEventArgs e)
		{
			Form form = (Form)sender;
			form.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(
				mouseDownFormLocation.X + (Form.MousePosition.X - mouseDownLocation.X),
				mouseDownFormLocation.Y + (Form.MousePosition.Y - mouseDownLocation.Y), false);
		}

		[ThreadStatic]
		static Point mouseDownLocation;

		[ThreadStatic]
		static Point mouseDownFormLocation;

		#endregion
	}
}
