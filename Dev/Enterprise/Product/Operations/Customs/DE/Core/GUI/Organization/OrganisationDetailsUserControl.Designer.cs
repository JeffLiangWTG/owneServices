namespace Enterprise.Customs.DE.GUI
{
	partial class OrganisationDetailsUserControl
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
			this.OrgCusAccountCollectionUserControl = new Enterprise.MasterFiles.GUI.OrgCusAccountCollectionUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OrgCusAccountCollectionUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// OrgCusAccountCollectionUserControl
			// 
			this.OrgCusAccountCollectionUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrgCusAccountCollectionUserControl, "DefermentAccountNumberCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgCusAccountCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).DefermentAccountNumberCollection)));
			this.OrgCusAccountCollectionUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrgCusAccountCollectionUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrgCusAccountCollectionUserControl.Name = "OrgCusAccountCollectionUserControl";
			this.OrgCusAccountCollectionUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 600, true);
			this.OrgCusAccountCollectionUserControl.TabIndex = 0;
			// 
			// OrganisationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OrgCusAccountCollectionUserControl);
			this.Name = "OrganisationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1178, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OrgCusAccountCollectionUserControl.ResumeLayout(true);
			this.OrgCusAccountCollectionUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.MasterFiles.GUI.OrgCusAccountCollectionUserControl OrgCusAccountCollectionUserControl;
	}
}
