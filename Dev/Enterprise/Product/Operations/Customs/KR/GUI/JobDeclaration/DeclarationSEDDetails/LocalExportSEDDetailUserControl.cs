using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class LocalExportSEDDetailUserControl : ZUserControl
	{
		public LocalExportSEDDetailUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			MainPanel.UpdateLayout(new LEXSEDDetailsLayout());
		}
	}
}
