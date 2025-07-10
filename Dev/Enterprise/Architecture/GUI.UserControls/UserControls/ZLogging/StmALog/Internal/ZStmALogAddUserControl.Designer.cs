using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmALogAddUserControl
	{
		#region Designer Generated Code

		Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		Enterprise.ZArchitecture.GUI.Internal.ZStmALogAddDropEdit EventsDropEdit;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit SL_EventTimeBoundDateEdit;
		ZButton ExtraInfoButton;
		Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;

		protected virtual void InitializeComponent()
		{
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.EventsDropEdit = new Enterprise.ZArchitecture.GUI.Internal.ZStmALogAddDropEdit();
			this.SL_EventTimeBoundDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExtraInfoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EventsDropEdit.SuspendLayout();
			this.SL_EventTimeBoundDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.Internal.StmALogAsAddedByUser);
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "SL_Reference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.Internal.StmALogAsAddedByUser)(null)).SL_Reference);
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 32, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 17, true);
			this.DescriptionTextBox.TabIndex = 1;
			// 
			// zCheckBox1
			// 
			this.zCheckBox1.AutoSize = true;
			this.BindingSource.SetBindingMember(this.zCheckBox1, "SL_IsEstimate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.Internal.StmALogAsAddedByUser)(null)).SL_IsEstimate);
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(204, 64, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 16, true);
			this.zCheckBox1.TabIndex = 3;
			// 
			// EventsDropEdit
			// 
			this.EventsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EventsDropEdit, "SL_SE_NKEvent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.Internal.StmALogAsAddedByUser)(null)).SL_SE_NKEvent);
			this.EventsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 4, true);
			this.EventsDropEdit.MaxItemsToShowInDropDown = 20;
			this.EventsDropEdit.Name = "EventsDropEdit";
			this.EventsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 17, true);
			this.EventsDropEdit.TabIndex = 0;
			// 
			// SL_EventTimeBoundDateEdit
			// 
			this.SL_EventTimeBoundDateEdit.AllowDrop = true;
			this.SL_EventTimeBoundDateEdit.AutoCompleteMonthThreshold = 1;
			this.SL_EventTimeBoundDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SL_EventTimeBoundDateEdit, "SL_EventTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.Internal.StmALogAsAddedByUser)(null)).SL_EventTime);
			this.SL_EventTimeBoundDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SL_EventTimeBoundDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 60, true);
			this.SL_EventTimeBoundDateEdit.Name = "SL_EventTimeBoundDateEdit";
			this.SL_EventTimeBoundDateEdit.TabIndex = 2;
			// 
			// ExtraInfoButton
			// 
			this.ExtraInfoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 32, true);
			this.ExtraInfoButton.Name = "ExtraInfoButton";
			this.ExtraInfoButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ExtraInfoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 20, true);
			this.ExtraInfoButton.TabIndex = 4;
			this.ExtraInfoButton.Text = "...";
			this.ExtraInfoButton.UseVisualStyleBackColor = true;
			this.ExtraInfoButton.Click += new System.EventHandler(this.ExtraInfoButton_Click);
			// 
			// ZStmALogAddUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExtraInfoButton);
			this.Controls.Add(this.SL_EventTimeBoundDateEdit);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.zCheckBox1);
			this.Controls.Add(this.EventsDropEdit);
			this.Name = "ZStmALogAddUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 84, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EventsDropEdit.ResumeLayout(true);
			this.EventsDropEdit.PerformLayout();
			this.SL_EventTimeBoundDateEdit.ResumeLayout(true);
			this.SL_EventTimeBoundDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
