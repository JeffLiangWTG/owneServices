using System.Drawing;
using System.Windows.Forms;

using CargoWise.Windows.UI;

namespace Enterprise.DocumentScanning.OCR
{
	/// <summary>
	/// A one pixel form which constrains the cursor to its boundaries.
	/// </summary>
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis] // This is setup with values the designer cannot handle.
	public partial class CursorConstrainingForm : KForm
	{
		public CursorConstrainingForm()
		{
			InitializeComponent();
		}

		protected Rectangle originalClip = Rectangle.Empty;
		bool fFormWasLoaded;

		void CursorConstrainingForm_Activated(object sender, System.EventArgs e)
		{
			if (fFormWasLoaded)
			{
				Cursor.Clip = this.RectangleToScreen(this.ClientRectangle);
			}
		}

		void CursorConstrainingForm_Load(object sender, System.EventArgs e)
		{
			originalClip = Cursor.Clip;
			Cursor.Clip = this.RectangleToScreen(this.ClientRectangle);
			fFormWasLoaded = true;
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (fFormWasLoaded)
				{
					Cursor.Clip = originalClip;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
