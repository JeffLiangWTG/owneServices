namespace Enterprise.Customs.CN.GUI
{
	partial class OrgSupBuyLinkTrnModeControl
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
			this.ZO_CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ZO_OfficeOfEntryExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ZO_CIQOfficeOfEntryExitCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.OfficeOfDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ZO_CustomsOfficeCodeFindBox.SuspendLayout();
			this.ZO_OfficeOfEntryExitCodeFindBox.SuspendLayout();
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.SuspendLayout();
			this.OfficeOfDestinationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj);
			// 
			// ZO_CustomsOfficeCodeFindBox
			// 
			this.ZO_CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZO_CustomsOfficeCodeFindBox, "ZO_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj)(null)).ZO_CustomsOffice)));
			this.ZO_CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 10, true);
			this.ZO_CustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ZO_CustomsOfficeCodeFindBox.Name = "ZO_CustomsOfficeCodeFindBox";
			this.ZO_CustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ZO_CustomsOfficeCodeFindBox.ParentType = null;
			this.ZO_CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ZO_CustomsOfficeCodeFindBox.TabIndex = 0;
			// 
			// ZO_OfficeOfEntryExitCodeFindBox
			// 
			this.ZO_OfficeOfEntryExitCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZO_OfficeOfEntryExitCodeFindBox, "ZO_OfficeOfEntryExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj)(null)).ZO_OfficeOfEntryExit)));
			this.ZO_OfficeOfEntryExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 36, true);
			this.ZO_OfficeOfEntryExitCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ZO_OfficeOfEntryExitCodeFindBox.Name = "ZO_OfficeOfEntryExitCodeFindBox";
			this.ZO_OfficeOfEntryExitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ZO_OfficeOfEntryExitCodeFindBox.ParentType = null;
			this.ZO_OfficeOfEntryExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ZO_OfficeOfEntryExitCodeFindBox.TabIndex = 1;
			// 
			// ZO_CIQOfficeOfEntryExitCodeFindBox
			// 
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZO_CIQOfficeOfEntryExitCodeFindBox, "ZO_CIQOfficeOfEntryExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj)(null)).ZO_CIQOfficeOfEntryExit)));
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 88, true);
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.Name = "ZO_CIQOfficeOfEntryExitCodeFindBox";
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.ParentType = null;
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.PreBoundMaxLength = 6;
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.TabIndex = 3;
			// 
			// OfficeOfDestinationCodeFindBox
			// 
			this.OfficeOfDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OfficeOfDestinationCodeFindBox, "OfficeOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.OrgSupBuyLinkTrnModeAddInfoBizObj)(null)).OfficeOfDestination)));
			this.OfficeOfDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 62, true);
			this.OfficeOfDestinationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.OfficeOfDestinationCodeFindBox.Name = "OfficeOfDestinationCodeFindBox";
			this.OfficeOfDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OfficeOfDestinationCodeFindBox.ParentType = null;
			this.OfficeOfDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.OfficeOfDestinationCodeFindBox.TabIndex = 2;
			// 
			// OrgSupBuyLinkTrnModeControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OfficeOfDestinationCodeFindBox);
			this.Controls.Add(this.ZO_CIQOfficeOfEntryExitCodeFindBox);
			this.Controls.Add(this.ZO_OfficeOfEntryExitCodeFindBox);
			this.Controls.Add(this.ZO_CustomsOfficeCodeFindBox);
			this.Name = "OrgSupBuyLinkTrnModeControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 121, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ZO_CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.ZO_CustomsOfficeCodeFindBox.PerformLayout();
			this.ZO_OfficeOfEntryExitCodeFindBox.ResumeLayout(true);
			this.ZO_OfficeOfEntryExitCodeFindBox.PerformLayout();
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.ResumeLayout(true);
			this.ZO_CIQOfficeOfEntryExitCodeFindBox.PerformLayout();
			this.OfficeOfDestinationCodeFindBox.ResumeLayout(true);
			this.OfficeOfDestinationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion

		private Enterprise.ZArchitecture.GUI.ZCodeFindBox ZO_CustomsOfficeCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ZO_OfficeOfEntryExitCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox ZO_CIQOfficeOfEntryExitCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox OfficeOfDestinationCodeFindBox;
	}
}
