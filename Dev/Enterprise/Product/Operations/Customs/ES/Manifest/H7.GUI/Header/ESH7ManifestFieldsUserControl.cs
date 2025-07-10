using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class ESH7ManifestFieldsUserControl : ZUserControl
	{
		public ESH7ManifestFieldsUserControl()
		{
			InitializeComponent();
		}

		void NumericOnly_KeyPress(object sender, KeyPressEventArgs e)
		{
			if (!Char.IsControl(e.KeyChar) && !Char.IsDigit(e.KeyChar))
			{
				e.Handled = true;
			}
		}
	}
}

