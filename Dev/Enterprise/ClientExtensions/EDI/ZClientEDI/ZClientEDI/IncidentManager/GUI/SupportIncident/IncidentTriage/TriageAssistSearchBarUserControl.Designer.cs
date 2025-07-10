using System.Windows.Forms;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class TriageAssistSearchBarUserControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			this.ShowSearchOptionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShouldSearchKeywordsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShouldSearchDescriptionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ShouldSearchSuggestedListCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SearchOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CriteriaTypeFilterDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SearchOptionsGroupBox.SuspendLayout();
			this.CriteriaTypeFilterDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject);
			// 
			// ShowSearchOptionsCheckBox
			// 
			this.ShowSearchOptionsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShowSearchOptionsCheckBox, "ShowSearchOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShowSearchOptions)));
			this.ShowSearchOptionsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShowSearchOptionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShowSearchOptionsCheckBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.ShowSearchOptionsCheckBox.Name = "ShowSearchOptionsCheckBox";
			this.ShowSearchOptionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ShowSearchOptionsCheckBox.TabIndex = 1;
			this.ShowSearchOptionsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShowSearchOptionsCheckBox.UseVisualStyleBackColor = true;
			this.ShowSearchOptionsCheckBox.CheckedChanged += new System.EventHandler(this.ShowSearchOptionsCheckBox_CheckedChanged);
			// 
			// ShouldSearchKeywordsCheckBox
			// 
			this.ShouldSearchKeywordsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ShouldSearchKeywordsCheckBox, "ShouldSearchKeywords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShouldSearchKeywords)));
			this.ShouldSearchKeywordsCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldSearchKeywordsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 12, true);
			this.ShouldSearchKeywordsCheckBox.Name = "ShouldSearchKeywordsCheckBox";
			this.ShouldSearchKeywordsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 17, true);
			this.ShouldSearchKeywordsCheckBox.TabIndex = 4;
			this.ShouldSearchKeywordsCheckBox.Text = "Search in:    Keywords";
			this.ShouldSearchKeywordsCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShouldSearchKeywordsCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShouldSearchDescriptionCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ShouldSearchDescriptionCheckBox, "ShouldSearchDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShouldSearchDescription)));
			this.ShouldSearchDescriptionCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldSearchDescriptionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(361, 12, true);
			this.ShouldSearchDescriptionCheckBox.Name = "ShouldSearchDescriptionCheckBox";
			this.ShouldSearchDescriptionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 18, true);
			this.ShouldSearchDescriptionCheckBox.TabIndex = 5;
			this.ShouldSearchDescriptionCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShouldSearchDescriptionCheckBox.UseVisualStyleBackColor = true;
			// 
			// ShouldSearchSuggestedListCheckBox
			// 
			this.ShouldSearchSuggestedListCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShouldSearchSuggestedListCheckBox, "ShouldSearchSuggestedList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).ShouldSearchSuggestedList)));
			this.ShouldSearchSuggestedListCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShouldSearchSuggestedListCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(525, 13, true);
			this.ShouldSearchSuggestedListCheckBox.Name = "ShouldSearchSuggestedListCheckBox";
			this.ShouldSearchSuggestedListCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(164, 17, true);
			this.ShouldSearchSuggestedListCheckBox.TabIndex = 6;
			this.ShouldSearchSuggestedListCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ShouldSearchSuggestedListCheckBox.UseVisualStyleBackColor = true;
			// 
			// SearchOptionsGroupBox
			// 
			this.SearchOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SearchOptionsGroupBox.Controls.Add(this.ShouldSearchSuggestedListCheckBox);
			this.SearchOptionsGroupBox.Controls.Add(this.ShouldSearchDescriptionCheckBox);
			this.SearchOptionsGroupBox.Controls.Add(this.ShouldSearchKeywordsCheckBox);
			this.SearchOptionsGroupBox.Controls.Add(this.CriteriaTypeFilterDropEdit);
			this.SearchOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 11, true);
			this.SearchOptionsGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SearchOptionsGroupBox.Name = "SearchOptionsGroupBox";
			this.SearchOptionsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SearchOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 34, true);
			this.SearchOptionsGroupBox.TabIndex = 2;
			this.SearchOptionsGroupBox.TabStop = false;
			this.SearchOptionsGroupBox.Visible = false;
			// 
			// CriteriaTypeFilterDropEdit
			// 
			this.CriteriaTypeFilterDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CriteriaTypeFilterDropEdit, "CriteriaTypeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.IncidentManager.Business.TriageAssistBusinessObject)(null)).CriteriaTypeFilter)));
			this.CriteriaTypeFilterDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CriteriaTypeFilterDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 10, true);
			this.CriteriaTypeFilterDropEdit.Name = "CriteriaTypeFilterDropEdit";
			this.CriteriaTypeFilterDropEdit.ShouldResizeByMaxLength = false;
			this.CriteriaTypeFilterDropEdit.ShowDescriptionBox = false;
			this.CriteriaTypeFilterDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CriteriaTypeFilterDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.CriteriaTypeFilterDropEdit.TabIndex = 3;
			// 
			// TriageAssistSearchBarUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.Transparent;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ShowSearchOptionsCheckBox);
			this.Controls.Add(this.SearchOptionsGroupBox);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "TriageAssistSearchBarUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 47, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SearchOptionsGroupBox.ResumeLayout(false);
			this.SearchOptionsGroupBox.PerformLayout();
			this.CriteriaTypeFilterDropEdit.ResumeLayout(true);
			this.CriteriaTypeFilterDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZCheckBox ShowSearchOptionsCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShouldSearchKeywordsCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShouldSearchDescriptionCheckBox;
		private ZArchitecture.GUI.ZCheckBox ShouldSearchSuggestedListCheckBox;
		private ZArchitecture.GUI.ZGroupBox SearchOptionsGroupBox;
		private ZArchitecture.GUI.ZDropEdit CriteriaTypeFilterDropEdit;
	}
}
