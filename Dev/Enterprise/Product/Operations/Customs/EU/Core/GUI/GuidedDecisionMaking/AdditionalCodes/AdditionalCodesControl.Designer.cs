namespace Enterprise.Customs.EU.GUI
{
	partial class AdditionalCodesControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AdditionalCodesFlowLayoutPanelGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingAdditionalCodeCollection);
			// 
			// AdditionalCodesFlowLayoutPanelGroupBox
			// 
			this.AdditionalCodesFlowLayoutPanelGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalCodesFlowLayoutPanelGroupBox.AutoSize = true;
			this.AdditionalCodesFlowLayoutPanelGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalCodesFlowLayoutPanelGroupBox.Name = "AdditionalCodesFlowLayoutPanelGroupBox";
			this.AdditionalCodesFlowLayoutPanelGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 50, true);
			this.AdditionalCodesFlowLayoutPanelGroupBox.TabIndex = 0;
			this.AdditionalCodesFlowLayoutPanelGroupBox.TabStop = false;
			// 
			// AdditionalCodesControl
			// 
			this.AutoSize = true;
			this.Controls.Add(this.AdditionalCodesFlowLayoutPanelGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, 0, 0, 10, true);
			this.Name = "AdditionalCodesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(960, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox AdditionalCodesFlowLayoutPanelGroupBox;
	}
}
