using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

sealed partial class MessageStatusUserControl : ZUserControl, IExtendedControl
{
	public MessageStatusUserControl()
	{
		InitializeComponent();
		Extensions = new DefaultControlExtensionCollection(this);
	}

	public Control Host => this;

	[Browsable(false)]
	public IControlExtensionCollection Extensions { get; }

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			Extensions.Dispose();
		}

		base.Dispose(disposing);
	}
}
