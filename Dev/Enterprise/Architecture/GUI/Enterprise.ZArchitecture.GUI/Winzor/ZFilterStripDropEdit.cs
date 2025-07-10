using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Controls.Interfaces;

namespace Enterprise.ZArchitecture.GUI.Internal;

public partial class ZFilterStripDropEdit
{
	public void SetCommandKeyHandler(IKeyEventHandler commandKeyHandler)
	{
		((ZFilterStripDropCodeBox)CodeBox).CommandKeyHandler = commandKeyHandler;
	}

	public class CommandKeyHandler : IKeyEventHandler
	{
		readonly ZFilterStripDropEdit dropEdit;
		readonly ZFilterStrip filterStrip;

		public CommandKeyHandler(ZFilterStripDropEdit dropEdit, ZFilterStrip filterStrip)
		{
			this.dropEdit = dropEdit;
			this.filterStrip = filterStrip;
		}

		public void Handle(KeyEventArgs e)
		{
			var control = filterStrip.CurrentFilterControls.FirstOrDefault();
			dropEdit.HandleCommandKey(e);
			if (e.KeyCode == Keys.Tab && control != filterStrip.CurrentFilterControls.FirstOrDefault())
			{
				if (control is null)
				{
					filterStrip.SetFocusAfterRenderControl();
				}
				else
				{
					dropEdit.Focus();
				}
			}
		}
	}
}
