using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using WinzorFramework;

namespace Enterprise.BufferManagement.GUI
{
	public partial class DateAcceptabilityPicture : ZUserControl
	{
		protected override EventAttribute EventAttributes => EventAttribute.MouseUp;

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (!parent.IsPreview && e.Button == MouseButtons.Right)
			{
				base.OnClick(e);
				ShowDateAcceptabilityLegend();
			}
		}
	}
}
