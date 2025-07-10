using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class FSimplifiedDeclarationFilterUserControl : ZUserControl
	{
		public FSimplifiedDeclarationFilterUserControl()
		{
			InitializeComponent();
		}

		public FSimplifiedDeclarationFilterUserControl(CusReconDeclaration declaration)
		{
			this.declaration = declaration;

			InitializeComponent();
			SetupSimplifiedDeclarationFilterControl();
		}

		public CusReconDeclaration declaration;
		FSimplifiedDeclarationFilterStripControl filterControl;

		void SetupSimplifiedDeclarationFilterControl()
		{
			filterControl = new FSimplifiedDeclarationFilterStripControl(declaration);
			SuspendLayout();
			try
			{
				Controls.Add(filterControl);
				filterControl.Dock = DockStyle.Fill;
				filterControl.DockPadding.All = 5;
				filterControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				filterControl.FilteredGrid.ReadOnly = false;
				filterControl.FilteredGrid.AllowDrop = true;
				filterControl.FilteredGrid.AllowNavigation = false;
				filterControl.Name = "FSimplifiedDeclarationFilterStripControl";
			}
			finally
			{
				ResumeLayout(false);
			}
		}
	}
}
