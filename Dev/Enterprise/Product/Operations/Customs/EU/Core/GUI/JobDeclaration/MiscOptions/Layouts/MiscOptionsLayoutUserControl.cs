using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class MiscOptionsLayoutUserControl : ZUserControl
	{
		public MiscOptionsLayoutUserControl()
		{
			InitializeComponent();

			InitializeRelatedDeclarationUserControl();
		}

		void InitializeRelatedDeclarationUserControl()
		{
			this.RelatedDeclarationsUserControl = GetRelatedDeclarationsUserControl();
			this.RelatedDeclarationsUserControl.SuspendLayout();
			// 
			// RelatedDeclarationsUserControl
			// 
			this.RelatedDeclarationsUserControl.AllowDrop = true;
			this.RelatedDeclarationsUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RelatedDeclarationsUserControl, ".");
			this.RelatedDeclarationsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(399, 260, true);
			this.RelatedDeclarationsUserControl.Name = "RelatedDeclarationsUserControl";
			this.RelatedDeclarationsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 267, true);
			this.RelatedDeclarationsUserControl.TabIndex = 3;
			this.Controls.Add(this.RelatedDeclarationsUserControl);
			this.Controls.SetChildIndex(this.RelatedDeclarationsUserControl, 0);
			this.RelatedDeclarationsUserControl.ResumeLayout(true);
			this.RelatedDeclarationsUserControl.PerformLayout();
		}

		protected virtual RelatedDeclarationsUserControl GetRelatedDeclarationsUserControl() => new RelatedDeclarationsUserControl();
	}
}
