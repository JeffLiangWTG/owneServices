namespace Enterprise.Customs.KR.GUI
{
	partial class CarnetMiscUserControl
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
            this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BranchGuidFindBox.SuspendLayout();
            this.BrokerCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // BranchGuidFindBox
            // 
            this.BranchGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GB)));
            this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 12, true);
            this.BranchGuidFindBox.Name = "BranchGuidFindBox";
            this.BranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BranchGuidFindBox.ParentType = null;
            this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(159, 17, true);
            this.BranchGuidFindBox.TabIndex = 0;
            // 
            // BrokerCodeFindBox
            // 
            this.BrokerCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_GS_NKCusAgent)));
            this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 12, true);
            this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
            this.BrokerCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BrokerCodeFindBox.ParentType = null;
            this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 17, true);
            this.BrokerCodeFindBox.TabIndex = 1;
            // 
            // CarnetMiscUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.BrokerCodeFindBox);
            this.Controls.Add(this.BranchGuidFindBox);
            this.Name = "CarnetMiscUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 46, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BranchGuidFindBox.ResumeLayout(true);
            this.BranchGuidFindBox.PerformLayout();
            this.BrokerCodeFindBox.ResumeLayout(true);
            this.BrokerCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
	}
}
