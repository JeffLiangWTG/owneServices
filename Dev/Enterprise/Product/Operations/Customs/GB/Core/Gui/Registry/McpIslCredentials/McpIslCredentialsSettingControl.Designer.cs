namespace Enterprise.Customs.GB.GUI.Registry
{
	partial class McpIslCredentialsSettingControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.McpIslCredentialsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.McpIslCredentialsGrid)).BeginInit();
			this.McpIslCredentialsGrid.SuspendLayout();
			this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.GB.Registry.McpIslCredentialsSetting);
			// 
			// McpIslCredentialsGrid
			// 
			this.McpIslCredentialsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.McpIslCredentialsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.GB.Registry.McpIslCredentialsSetting)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.McpIslCredentialsSetting)(null)).McpIslCompanyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.McpIslCredentialsSetting)(null)).McpIslPassword)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.McpIslCredentialsSetting)(null)).McpIslDevice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.GB.Registry.McpIslCredentialsSetting)(null)).McpIslUsername)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("0d951617-dcc2-4eb0-a911-ed406c3bb01e", "Badge (Company) Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "McpIslCompanyCode";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("88d58544-0baf-41f9-a5a5-831f4251c370", "Username");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo2.ColumnName = "McpIslUsername";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("e65623be-8c8d-459c-87b1-bb5ed5542f55", "Password");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo3.ColumnName = "McpIslPassword";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.PasswordChar = '*';
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("232ad64e-6b5d-44e6-93d5-978e0a65f82f", "Device");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.ColumnName = "McpIslDevice";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			this.McpIslCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.McpIslCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.McpIslCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.McpIslCredentialsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.McpIslCredentialsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.McpIslCredentialsGrid.GridId = "420749EA-D327-4A8F-A7A5-B61092E1D8F6";
			this.McpIslCredentialsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.McpIslCredentialsGrid.LayoutKey = "McpIslCredentialsGrid";
			this.McpIslCredentialsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.McpIslCredentialsGrid.Name = "McpIslCredentialsGrid";
			this.McpIslCredentialsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.McpIslCredentialsGrid.TabIndex = 0;
			// 
			// McpIslCredentialsSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.McpIslCredentialsGrid);
            this.Name = "McpIslCredentialsSettingControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.McpIslCredentialsGrid)).EndInit();
			this.McpIslCredentialsGrid.ResumeLayout(false);
			this.McpIslCredentialsGrid.PerformLayout();
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZGrid McpIslCredentialsGrid;
	}
}
