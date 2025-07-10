namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ShapeEntityDetailsForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ShapeDetailsControl = new Enterprise.BufferManagement.NetworkVisualisation.GUI.ShapeDetailsUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ShapeDetailsControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 608, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("5c081a73-d08e-4680-8e81-df00da55e482", "OK");
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(767, 577, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 25, true);
            this.OKButton.TabIndex = 2;
            this.OKButton.UseVisualStyleBackColor = false;
            // 
            // ShapeDetailsControl
            // 
            this.ShapeDetailsControl.AllowDrop = true;
            this.ShapeDetailsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.ShapeDetailsControl, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            //CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeNetworkEntity)(((Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape)(null)))));
            this.ShapeDetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ShapeDetailsControl.Name = "ShapeDetailsControl";
            this.ShapeDetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 571, true);
            this.ShapeDetailsControl.TabIndex = 1;
            // 
            // ShapeEntityDetailsForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(882, 632, true);
            this.Controls.Add(this.ShapeDetailsControl);
            this.Controls.Add(this.OKButton);
            this.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.BMNCNShape);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 669, true);
            this.Name = "ShapeEntityDetailsForm";
            this.Text = "ShapeEntityDetailsForm";
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.OKButton, 0);
            this.Controls.SetChildIndex(this.ShapeDetailsControl, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ShapeDetailsControl.ResumeLayout(true);
            this.ShapeDetailsControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton OKButton;
		private ShapeDetailsUserControl ShapeDetailsControl;
	}
}