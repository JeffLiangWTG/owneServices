
namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class MiscOptionsUserControl
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
			this.MiscGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MiscGroupBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// MiscGroupBox
			// 
			this.MiscGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("e38c4a77-87e3-4f28-9867-d26f2ce105f3", "Miscellaneous Options");
			this.MiscGroupBox.Controls.Add(this.BranchFindBox);
			this.MiscGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscGroupBox.Name = "MiscGroupBox";
			this.MiscGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 62, true);
			this.MiscGroupBox.TabIndex = 0;
			this.MiscGroupBox.TabStop = false;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "BH_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).BH_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Lookups.Branches)));
			this.BranchFindBox.BindToList = "Lookups.Branches";
			this.BranchFindBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("d5be3362-9ad8-47cc-92e1-3ccf8d1e81e9", "Branch Code");
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 19, true);
			this.BranchFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.PreBoundMaxLength = 3;
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.BranchFindBox.TabIndex = 0;
			// 
			// MiscOptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MiscGroupBox);
			this.Name = "MiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 385, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MiscGroupBox.ResumeLayout(false);
			this.MiscGroupBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox MiscGroupBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
	}
}
