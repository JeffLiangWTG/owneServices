using System.Windows.Forms;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZTabPageControlBaseTest : ZControlBaseTestCase<ZTabPage>
	{
		protected override bool IsDragDropHandledByEDocs
		{
			get { return true; }
		}

		protected override void AddControlToForm(ZForm form, Control control)
		{
			TabControl.TabPages.Add((ZTabPage)control);
			form.Controls.Add(TabControl);
		}

		ZTabControl TabControl
		{
			get { return tabControl ?? (tabControl = new ZTabControl()); }
		}
		ZTabControl tabControl;
	}
}
