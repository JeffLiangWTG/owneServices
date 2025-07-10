using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class ZModuleButtonGridWithBlankDetachMessage : ZModuleButtonGrid
	{
		public ZModuleButtonGridWithBlankDetachMessage()
		{
			MessageBoxButtons = MessageBoxButtons.OK;
		}

		internal override DialogResult ConfirmDetach(string message)
		{
			return Globals.Message.Show(Res.GetString("1743B2D0-1D8E-42F9-8A4A-0640D49FF6B9", "{0}", message),
				Res.GetString("69363926-894F-49EF-8500-A3FB2A55FD69", "Confirm Detach..."), MessageBoxButtons, MessageBoxIcon.Information);
		}

		public MessageBoxButtons MessageBoxButtons { get; set; }
	}
}
