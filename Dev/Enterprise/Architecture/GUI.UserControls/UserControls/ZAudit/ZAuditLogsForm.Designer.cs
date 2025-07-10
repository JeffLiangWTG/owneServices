namespace Enterprise.ZArchitecture.GUI
{
	partial class ZAuditLogsForm
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
		new void InitializeComponent()
		{
            this.auditUserControl = new Enterprise.ZArchitecture.GUI.ZAudit.PlugIn.ZAuditUserControl();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.auditUserControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.IAutoAdminLogTarget);
            // 
            // auditUserControl
            // 
            this.auditUserControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.auditUserControl, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.AuditDataServices.Business.Audit)(((Enterprise.ZArchitecture.Business.IAutoAdminLogTarget)(null)))));
            this.auditUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.auditUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.auditUserControl.Name = "auditUserControl";
            this.auditUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 299, true);
            this.auditUserControl.TabIndex = 1;
            // 
            // DataAuditLogsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("DataAuditLogsForm|9b068567-1084-4d05-a7ab-23743a22d62a", "Data Audit Log");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 299, true);
            this.Controls.Add(this.auditUserControl);
            this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.IAutoAdminLogTarget);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 140, true);
            this.Name = "DataAuditLogsForm";
            this.Text = "DataAuditLogsForm";
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.DataAuditLogsForm_KeyPress);
            this.Controls.SetChildIndex(this.auditUserControl, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.auditUserControl.ResumeLayout(true);
            this.auditUserControl.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZAudit.PlugIn.ZAuditUserControl auditUserControl;
	}
}
