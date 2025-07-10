using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ValuationDeclarationUserControl : ZUserControl
	{
		public ValuationDeclarationUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DynamicLayoutPanel.UpdateLayout(new ValuationDetailsLayout());
		}
	}
}
