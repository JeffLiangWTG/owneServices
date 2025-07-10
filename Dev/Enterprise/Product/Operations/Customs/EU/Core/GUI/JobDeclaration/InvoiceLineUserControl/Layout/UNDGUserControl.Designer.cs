namespace Enterprise.Customs.EU.GUI
{
	partial class UNDGUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.DGLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.DGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.FlashpointUserControl = new Enterprise.Customs.EU.GUI.UNDGFlashpointUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DGGuidFindBox.SuspendLayout();
			this.FlashpointUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// DGLinkLabel
			// 
			this.DGLinkLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("b0b7ab53-c505-413a-9b90-a4afd7c09e9e", "Link label");
			this.DGLinkLabel.IsFontBold = false;
			this.DGLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 63, true);
			this.DGLinkLabel.Name = "DGLinkLabel";
			this.DGLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 24, true);
			this.DGLinkLabel.TabIndex = 2;
			this.DGLinkLabel.TabStop = false;
			// 
			// DGGuidFindBox
			// 
			this.DGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DGGuidFindBox, "FilteredInvoiceLines.UNDGs+FirstItemForBinding.DI_DG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)).DI_DG)));
			this.DGGuidFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("UNDGUserControl|433f18ad-b05a-4fcb-b5a4-c708e52a63ec", "UNDG", "UNDG No.", "UNDG Number", "");
			this.DGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 14, true);
			this.DGGuidFindBox.Name = "DGGuidFindBox";
			this.DGGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DGGuidFindBox.ParentType = null;
			this.DGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DGGuidFindBox.TabIndex = 0;
			// 
			// FlashpointUserControl
			// 
			this.FlashpointUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FlashpointUserControl, "FilteredInvoiceLines.UNDGs+FirstItemForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.UNDGDataItem)(((Enterprise.MasterFiles.Business.UNDGDataItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).UNDGs.FirstItemForBinding)).SyncRoot)))));
			this.FlashpointUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 40, true);
			this.FlashpointUserControl.Name = "FlashpointUserControl";
			this.FlashpointUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.FlashpointUserControl.TabIndex = 1;
			// 
			// UNDGUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.FlashpointUserControl);
			this.Controls.Add(this.DGLinkLabel);
			this.Controls.Add(this.DGGuidFindBox);
			this.Name = "UNDGUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 108, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DGGuidFindBox.ResumeLayout(true);
			this.DGGuidFindBox.PerformLayout();
			this.FlashpointUserControl.ResumeLayout(true);
			this.FlashpointUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZLinkLabel DGLinkLabel;
		internal ZArchitecture.GUI.ZGuidFindBox DGGuidFindBox;
		internal UNDGFlashpointUserControl FlashpointUserControl;
	}
}
