using System.ComponentModel;
using System.Windows.Forms;

namespace CargoWise.Windows.UI.Testing
{
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public class GenericExtendedControl : Control, IExtendedControl
	{
		public GenericExtendedControl()
		{
		}

		public GenericExtendedControl(IControlExtensionCollection extensions)
		{
			Extensions = extensions;
		}

		Control IExtendedControl.Host
		{
			get { return this; }
		}

		public IControlExtensionCollection Extensions { get; set; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Extensions != null)
				{
					Extensions.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
