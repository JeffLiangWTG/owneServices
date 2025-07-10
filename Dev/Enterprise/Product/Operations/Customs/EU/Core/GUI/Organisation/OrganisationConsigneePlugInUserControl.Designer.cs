using System;
using Enterprise.ZArchitecture.GUI;
using CargoWiseOne.ResourceStrings;
using static Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.GUI
{
	partial class OrganisationConsigneePlugInUserControl : ZUserControl
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
			this.components = new System.ComponentModel.Container();
			this.EUPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zCheckBoxRepresentationType = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zDropEditOtherDeferralType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zCheckBoxUseFr3FiscalRepresentation = new Enterprise.ZArchitecture.GUI.ZCheckBox();

			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EUPanel.SuspendLayout();
			this.zCheckBoxRepresentationType.SuspendLayout();
			this.zDropEditOtherDeferralType.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = GetDataSourceType();
			// 
			// EUPanel
			// 
			this.EUPanel.Controls.Add(this.zCheckBoxRepresentationType);
			this.EUPanel.Controls.Add(this.zDropEditOtherDeferralType);
			this.EUPanel.Controls.Add(this.zCheckBoxUseFr3FiscalRepresentation);
			this.EUPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.EUPanel.Name = "EUTabPage";
			this.EUPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.EUPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 141, true);
			this.EUPanel.TabIndex = 1;
			this.EUPanel.Text = "EU";
			// 
			// zDropEditOtherDeferralType
			// 
			this.zDropEditOtherDeferralType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditOtherDeferralType, "ZO_OtherDeferType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.EUOrgImpAddInfo)(null)).ZO_OtherDeferType)));
			this.zDropEditOtherDeferralType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 0, true);
			this.zDropEditOtherDeferralType.Name = "zDropEditOtherDeferralType";
			this.zDropEditOtherDeferralType.PreBoundMaxLength = 1;
			this.zDropEditOtherDeferralType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 17, true);
			this.zDropEditOtherDeferralType.TabIndex = 0;
			// 
			// zCheckBoxRepresentationType
			// 
			this.BindingSource.SetBindingMember(this.zCheckBoxRepresentationType, "ZO_Box14UseIndirectRepresentation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.EUOrgImpAddInfo)(null)).ZO_Box14UseIndirectRepresentation)));
			this.zCheckBoxRepresentationType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 26, true);
			this.zCheckBoxRepresentationType.Name = "zDropEditRepresentationType";
			this.zCheckBoxRepresentationType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.zCheckBoxRepresentationType.TabIndex = 2;
			this.zCheckBoxRepresentationType.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("586b9afb-bef1-43f7-bade-79a554640c9b", "Use indirect representation?");
			// 
			// zCheckBoxUseFr3FiscalRepresentation
			// 
			this.BindingSource.SetBindingMember(this.zCheckBoxUseFr3FiscalRepresentation, "ZO_UseFr3FiscalRepresentation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.EUOrgImpAddInfo)(null)).ZO_UseFr3FiscalRepresentation)));
			this.zCheckBoxUseFr3FiscalRepresentation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 43, true);
			this.zCheckBoxUseFr3FiscalRepresentation.Name = "zCheckBoxUseFr3FiscalRepresentation";
			this.zCheckBoxUseFr3FiscalRepresentation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.zCheckBoxUseFr3FiscalRepresentation.TabIndex = 3;
			this.zCheckBoxUseFr3FiscalRepresentation.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("15EB9200-269D-4E6E-BCED-0B6AF9D9E1E8", "Use postponed VAT accounting?");
			// 
			// OrganisationConsigneePlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EUPanel);
			this.Name = "OrganisationConsigneePlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(648, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EUPanel.ResumeLayout(false);
			this.EUPanel.PerformLayout();
			this.zCheckBoxRepresentationType.ResumeLayout(true);
			this.zCheckBoxRepresentationType.PerformLayout();
			this.zDropEditOtherDeferralType.ResumeLayout(true);
			this.zDropEditOtherDeferralType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZPanel EUPanel;
		private ZDropEdit zDropEditOtherDeferralType;
		private ZCheckBox zCheckBoxRepresentationType;
		private ZCheckBox zCheckBoxUseFr3FiscalRepresentation;
	}
}
