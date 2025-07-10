using System;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class HeaderDetailsUserControl : ZUserControl
	{
		public HeaderDetailsUserControl()
		{
			InitializeComponent();

			DynamicHeaderDetailsPanel.UpdateLayout(new HeaderDetailsLayouts());
		}

		CusReconDeclaration CusReconDeclaration => (CusReconDeclaration)CurrentDataItem;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (FSimplifiedDeclarationFilterUserControl == null)
			{
				SuspendLayout();
				FSimplifiedDeclarationFilterUserControl = new FSimplifiedDeclarationFilterUserControl(CusReconDeclaration);
				this.bottomPanel.Controls.Add(FSimplifiedDeclarationFilterUserControl);
				FSimplifiedDeclarationFilterUserControl.Dock = DockStyle.Fill;
				FSimplifiedDeclarationFilterUserControl.DockPadding.All = 5;
				FSimplifiedDeclarationFilterUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				FSimplifiedDeclarationFilterUserControl.TabIndex = 0;
				FSimplifiedDeclarationFilterUserControl.AutoSize = true;
				ResumeLayout(false);
			}
		}
	}
}
