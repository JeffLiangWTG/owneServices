namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	public partial class ExportManifestForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.oPostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.exportManifestDeclarationUserControl = new Enterprise.Customs.AU.ExportManifest.GUI.ExportManifestDeclarationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 23, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(417);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(417);
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.AllowDrop = true;
			this.oPostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(602, 699, true);
			this.oPostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.oPostingButtonsUserControl.Name = "oPostingButtonsUserControl";
			this.oPostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 27, true);
			this.oPostingButtonsUserControl.TabIndex = 3;
			// 
			// exportManifestDeclarationUserControl
			// 
			this.exportManifestDeclarationUserControl.AllowDrop = true;
			this.exportManifestDeclarationUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.exportManifestDeclarationUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.exportManifestDeclarationUserControl.Header = null;
			this.exportManifestDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.exportManifestDeclarationUserControl.Name = "exportManifestDeclarationUserControl";
			this.exportManifestDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 695, true);
			this.exportManifestDeclarationUserControl.TabIndex = 1;
			// 
			// ExportManifestForm
			// 

			this.Controls.Add(this.exportManifestDeclarationUserControl);
			this.Controls.Add(this.oPostingButtonsUserControl);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(856, 750, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 725, true);
			this.Name = "ExportManifestForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "ExportManifestForm";
			this.Controls.SetChildIndex(this.oPostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.exportManifestDeclarationUserControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl oPostingButtonsUserControl;
		private ExportManifestDeclarationUserControl exportManifestDeclarationUserControl;
	}
}
