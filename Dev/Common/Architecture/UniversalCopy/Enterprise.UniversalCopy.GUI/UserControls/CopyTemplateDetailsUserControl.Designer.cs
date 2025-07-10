using System.Windows.Forms;

namespace Enterprise.UniversalCopy.GUI
{
	partial class CopyTemplateDetailsUserControl
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
			this.dropEditConfigurationSource = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.checkBoxOrderByDescending = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.dropEditOrderBy = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.nominatedRecordFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.tabPageElementDetails.SuspendLayout();
			this.tabPageElementDetails.SuspendLayout();
			this.panelSort.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.dropEditOrderBy.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabPageElementDetails
			// 
			this.tabPageElementDetails.Controls.Add(this.dropEditConfigurationSource);
			this.tabPageElementDetails.Controls.SetChildIndex(this.dropEditConfigurationSource, 0);
			this.tabPageElementDetails.Controls.Add(this.nominatedRecordFindBox);
			this.tabPageElementDetails.Controls.SetChildIndex(this.nominatedRecordFindBox, 0);
			// 
			// panelFilter
			// 
			this.panelFilter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.panelFilter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 51, true);
			// 
			// panelSort
			// 
			this.panelSort.Controls.Add(this.checkBoxOrderByDescending);
			this.panelSort.Controls.Add(this.dropEditOrderBy);
			this.panelSort.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 26, true);
			this.panelSort.Visible = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo);
			// 
			// dropEditConfigurationSource
			// 
			this.dropEditConfigurationSource.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditConfigurationSource, "ConfigurationSourceDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo)(null)).ConfigurationSourceDescription)));
			this.dropEditConfigurationSource.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("7ED16000-01F6-4271-8A99-0DF334058CE6", "Source", "Source for selected element.");
			this.dropEditConfigurationSource.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 58, true);
			this.dropEditConfigurationSource.Name = "dropEditConfigurationSource";
			this.dropEditConfigurationSource.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.dropEditConfigurationSource.TabIndex = 6;
			this.dropEditConfigurationSource.CharacterCasing = CharacterCasing.Normal;
			this.dropEditConfigurationSource.ShowDescriptionBox = false;
			this.dropEditConfigurationSource.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.dropEditConfigurationSource.UseFullWidthForCodeBox = true;
			// 
			// checkBoxOrderByDescending
			// 
			this.checkBoxOrderByDescending.AutoSize = true;
			this.BindingSource.SetBindingMember(this.checkBoxOrderByDescending, "OrderByDescending");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo)(null)).OrderByDescending)));
			this.checkBoxOrderByDescending.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("b716b031-a495-403a-be3b-b1a5080b5017", "Descending", "Sort in descending order.");
			this.checkBoxOrderByDescending.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.checkBoxOrderByDescending.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(547, 5, true);
			this.checkBoxOrderByDescending.Name = "checkBoxOrderByDescending";
			this.checkBoxOrderByDescending.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 17, true);
			this.checkBoxOrderByDescending.TabIndex = 1;
			this.checkBoxOrderByDescending.UseVisualStyleBackColor = true;
			// 
			// dropEditOrderBy
			// 
			this.dropEditOrderBy.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.dropEditOrderBy, "OrderByField");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo)(null)).OrderByField)));
			this.dropEditOrderBy.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("bb7e48cd-6038-4e7a-b301-9b8127430692", "Order By", "Orders matched elements by specified property.");
			this.dropEditOrderBy.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.dropEditOrderBy.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 3, true);
			this.dropEditOrderBy.Name = "dropEditOrderBy";
			this.dropEditOrderBy.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 20, true);
			this.dropEditOrderBy.TabIndex = 0;
			// 
			// nominatedRecordFindBox
			// 
			this.nominatedRecordFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.nominatedRecordFindBox, "NominatedRecordPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo)(null)).NominatedRecordPk)));
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Business.CopyTemplateTreeBizo)(null)).ContactListTest)));
			//this.nominatedRecordFindBox.BindToList = "ContactListTest";
			this.nominatedRecordFindBox.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("F1D39340-D3FA-4F26-BEE6-37E7D6BC4113", "Nominated Record");
			this.nominatedRecordFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(480, 58, true);
			this.nominatedRecordFindBox.Name = "nominatedRecordFindBox";
			this.nominatedRecordFindBox.ShowDescriptionBox = false;
			this.nominatedRecordFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.nominatedRecordFindBox.TabIndex = 7;
			this.nominatedRecordFindBox.Visible = false;
			// 
			// CopyTemplateDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CopyTemplateDetailsUserControl";
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			this.tabPageElementDetails.ResumeLayout(false);
			this.tabPageElementDetails.PerformLayout();
			this.panelSort.ResumeLayout(false);
			this.panelSort.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.dropEditOrderBy.ResumeLayout(true);
			this.dropEditOrderBy.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZArchitecture.GUI.ZDropEdit dropEditConfigurationSource;
		private ZArchitecture.GUI.ZCheckBox checkBoxOrderByDescending;
		private ZArchitecture.GUI.ZDropEdit dropEditOrderBy;
		private ZArchitecture.GUI.ZGuidFindBox nominatedRecordFindBox;

		#endregion
	}
}
