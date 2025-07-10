using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class TermsProgressForm : ProcessStatusProgressForm
	{
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EnterpriseLogo.SizeMode = PictureBoxSizeMode.CenterImage;
			EnterpriseLogo.Image = Icons.GetImage(IconTypes.Loader);
		}
	}
}
