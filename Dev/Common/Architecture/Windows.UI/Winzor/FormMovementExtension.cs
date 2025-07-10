using System.Windows.Forms;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// Provides extension methods to allow a form to be movable by 
	/// clicking and dragging anywhere within the form.
	/// </summary>
	public static class FormMoveByMouseDragExtensions
	{
		public static void MoveFormByMouseDrag(this Form form, bool enabled)
		{
			//In the original implementation, the window would be movable by clicking and dragging on the form. This was used for borderless windows.
			//In JavaScript, moveBy/moveTo are unreliable, so we are disabling borderless windows in this case (as this will allow it to be moved).
			form.DisableBorderlessWindow = enabled;
		}
	}
}
