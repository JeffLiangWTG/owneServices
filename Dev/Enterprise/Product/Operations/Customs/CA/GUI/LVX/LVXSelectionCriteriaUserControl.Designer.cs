namespace Enterprise.Customs.CA.GUI
{
	partial class LVXSelectionCriteriaUserControl
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
			this.PeriodMonthEdit = new Enterprise.Customs.Module.MonthEdit();
			this.PeriodYearEdit = new Enterprise.ZArchitecture.GUI.ZYearEdit();
			this.PeriodLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ProvinceOfClearanceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImporterOrganisationControl.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.ProvinceOfClearanceDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO);
			// 
			// PeriodMonthEdit
			// 
			this.PeriodMonthEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodMonthEdit, "PeriodMonth");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO)(null)).PeriodMonth)));
			this.PeriodMonthEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0c83f9d8-2bb8-4d1b-b199-e5145ca6b866", "Period");
			this.PeriodMonthEdit.DecimalPlaces = 2;
			this.PeriodMonthEdit.IsCalculatorEnabled = false;
			this.PeriodMonthEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 18, true);
			this.PeriodMonthEdit.Name = "PeriodMonthEdit";
			this.PeriodMonthEdit.ShowGroupSeparators = false;
			this.PeriodMonthEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.PeriodMonthEdit.TabIndex = 0;
			this.PeriodMonthEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodYearEdit
			// 
			this.PeriodYearEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PeriodYearEdit, "PeriodYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO)(null)).PeriodYear)));
			this.PeriodYearEdit.DecimalPlaces = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodYearEdit, false);
			this.PeriodYearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(168, 18, true);
			this.PeriodYearEdit.Name = "PeriodYearEdit";
			this.PeriodYearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.PeriodYearEdit.TabIndex = 1;
			this.PeriodYearEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodLabel
			// 
			this.PeriodLabel.AutoSize = true;
			this.PeriodLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(157, 21, true);
			this.PeriodLabel.Name = "PeriodLabel";
			this.PeriodLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 13, true);
			this.PeriodLabel.TabIndex = 3;
			this.PeriodLabel.Text = "/";
			// 
			// ImporterOrganisationControl
			// 
			this.ImporterOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImporterOrganisationControl, "OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO)(null)).OH_Importer)));
			this.ImporterOrganisationControl.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b1aaed9d-00c8-4596-ac43-280ca6a403bd", "Importer");
			this.ImporterOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.ImporterOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 42, true);
			this.ImporterOrganisationControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 0, true);
			this.ImporterOrganisationControl.Name = "ImporterOrganisationControl";
			this.ImporterOrganisationControl.PopupCaption = "";
			this.ImporterOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 39, true);
			this.ImporterOrganisationControl.TabIndex = 2;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BrokerCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "GS_NKBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO)(null)).GS_NKBroker)));
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fa7b7d78-8434-45b3-9838-a84c7cb8cad2", "Broker");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 145, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.PreBoundMaxLength = 3;
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BrokerCodeFindBox.TabIndex = 5;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BranchGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO)(null)).Branch)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b20618bf-8916-44fe-aa2d-3b2bc4709442", "Branch");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 89, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.BranchGuidFindBox.TabIndex = 3;
			// 
			// ProvinceOfClearanceTextBox
			// 
			this.ProvinceOfClearanceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProvinceOfClearanceDropEdit, "ProvinceOfClearance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.LVXSelectionCriteriaBO)(null)).ProvinceOfClearance)));
			this.ProvinceOfClearanceDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ff114ad0-ec71-4b4a-8db5-c1b0c2719e31", "Province of Clearance");
			this.ProvinceOfClearanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 117, true);
			this.ProvinceOfClearanceDropEdit.Name = "ProvinceOfClearanceTextBox";
			this.ProvinceOfClearanceDropEdit.PreBoundMaxLength = 2;
			this.ProvinceOfClearanceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 20, true);
			this.ProvinceOfClearanceDropEdit.TabIndex = 4;
			// 
			// LVXSelectionCriteriaUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PeriodMonthEdit);
			this.Controls.Add(this.PeriodYearEdit);
			this.Controls.Add(this.PeriodLabel);
			this.Controls.Add(this.BrokerCodeFindBox);
			this.Controls.Add(this.BranchGuidFindBox);
			this.Controls.Add(this.ImporterOrganisationControl);
			this.Controls.Add(this.ProvinceOfClearanceDropEdit);
			this.Name = "LVXSelectionCriteriaUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 182, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImporterOrganisationControl.ResumeLayout(true);
			this.ImporterOrganisationControl.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.ProvinceOfClearanceDropEdit.ResumeLayout(true);
			this.ProvinceOfClearanceDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.Customs.Module.MonthEdit PeriodMonthEdit;
		private ZArchitecture.ZLabel PeriodLabel;
		private ZArchitecture.GUI.ZYearEdit PeriodYearEdit;
		private MasterFiles.GUI.ZOrganisationControl ImporterOrganisationControl;
		private ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		private ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit ProvinceOfClearanceDropEdit;
	}
}
