using Enterprise.ZArchitecture;

namespace Enterprise.Core.Forms
{
	public partial class CodeDescriptionListEditControl
	{

		#region Component Designer generated code

		public ZGrid CodeDescriptionGrid;

		void InitializeComponent()
		{
			this.CodeDescriptionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// CodeDescriptionGrid
			// 
			this.CodeDescriptionGrid.AllowNavigation = false;
			this.CodeDescriptionGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.CodeDescriptionGrid.CaptionVisible = false;
			this.CodeDescriptionGrid.GridId = "52969035-8fe6-493f-aae5-1fb1ef3803bc";
			this.CodeDescriptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionGrid.IsCustomiseMenuVisible = false;
			this.CodeDescriptionGrid.LayoutKey = "CodeDescriptionGrid";
			this.CodeDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionGrid.Name = "CodeDescriptionGrid";
			this.CodeDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 288, true);
			this.CodeDescriptionGrid.TabIndex = 0;
			// 
			// CodeDescriptionListEditControl
			// 
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionGrid);
			this.Name = "CodeDescriptionListEditControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionGrid)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

	}
}
