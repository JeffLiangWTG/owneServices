namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class MiscellaneousOptionsUserControl
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
			this.BranchCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BranchCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// BranchCodeFindBox
			// 
			this.BranchCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchCodeFindBox, "BH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Lookups.Branches)));
			this.BranchCodeFindBox.BindToList = "Lookups.Branches";
			this.BranchCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d5be3362-9ad8-47cc-92e1-3ccf8d1e81e9", "Branch Code");
			this.BranchCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 3, true);
			this.BranchCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchCodeFindBox.Name = "BranchCodeFindBox";
			this.BranchCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BranchCodeFindBox.ParentType = null;
			this.BranchCodeFindBox.PreBoundMaxLength = 3;
			this.BranchCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BranchCodeFindBox.TabIndex = 0;
			// 
			// MiscellaneousOptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BranchCodeFindBox);
			this.Name = "MiscellaneousOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 291, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BranchCodeFindBox.ResumeLayout(true);
			this.BranchCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchCodeFindBox;

		#endregion
	}
}
