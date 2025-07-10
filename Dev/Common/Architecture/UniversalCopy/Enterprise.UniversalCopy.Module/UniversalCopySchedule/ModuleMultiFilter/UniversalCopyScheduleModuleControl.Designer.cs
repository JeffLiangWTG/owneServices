
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter
{
	partial class UniversalCopyScheduleModuleControl
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
			this.labelObject = new Enterprise.ZArchitecture.ZLabel();
			this.modulesPairListDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.operatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.objectDescirptionEditText = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.modulesPairListDropEdit.SuspendLayout();
			this.operatorDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter.UniversalCopyScheduleModuleFilter);
			// 
			// modulesPairListDropEdit
			// 
			this.modulesPairListDropEdit.AllowDrop = true;
			this.modulesPairListDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.modulesPairListDropEdit, "ModulesPairList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter.UniversalCopyScheduleModuleFilter)(null)).ModulesPairList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter.UniversalCopyScheduleModuleFilter)(null)).ModulesPair_List)));
			this.modulesPairListDropEdit.BindToList = "ModulesPair_List";
			this.modulesPairListDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.modulesPairListDropEdit.Name = "modulesPairListDropEdit";
			this.modulesPairListDropEdit.ShowDescriptionBox = false;
			this.modulesPairListDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.modulesPairListDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 0, true);
			this.modulesPairListDropEdit.TabIndex = 0;
			this.modulesPairListDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 18, true);
			// 
			// ObjectLabel
			// 
			this.labelObject.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 25, true);
			this.labelObject.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 18, true);
			this.labelObject.CaptionResourceString = Enterprise.UniversalCopy.Module.Res.GetData("923962cf-aad7-4fac-89b9-148dd1d76498", "Object");
			this.labelObject.Name = "ObjectLabel";
			this.labelObject.TabIndex = 22;
			// 
			// operatorDropEdit
			// 
			this.operatorDropEdit.AllowDrop = true;
			this.operatorDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.operatorDropEdit, "ComparisonOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter.UniversalCopyScheduleModuleFilter)(null)).ComparisonOperator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter.UniversalCopyScheduleModuleFilter)(null)).ComparisonOperator_List)));
			this.operatorDropEdit.BindToList = "ComparisonOperator_List";
			this.operatorDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.operatorDropEdit.Name = "operatorDropEdit";
			this.operatorDropEdit.ShowDescriptionBox = false;
			this.operatorDropEdit.PreBoundMaxLength = 7;
			this.operatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 18, true);
			this.operatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 25, true);
			this.operatorDropEdit.TabIndex = 1;
			// 
			// objectDescirptionEditText
			// 
			this.BindingSource.SetBindingMember(this.objectDescirptionEditText, "CopyObjectCode");
			//// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Module.UniversalCopySchedule.ModuleMultiFilter.UniversalCopyScheduleModuleFilter)(null)).CopyObjectCode)));
			this.objectDescirptionEditText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(370, 25, true);
			this.objectDescirptionEditText.Name = "objectDescirptionEditText";
			this.objectDescirptionEditText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 18, true);
			this.objectDescirptionEditText.TabIndex = 1;
			this.objectDescirptionEditText.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			// 
			// UniversalCopyScheduleModuleControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.labelObject);
			this.Controls.Add(this.modulesPairListDropEdit);
			this.Controls.Add(this.operatorDropEdit);
			this.Controls.Add(this.objectDescirptionEditText);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 55, true);
			this.Name = "UniversalCopyScheduleModuleControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 55, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.modulesPairListDropEdit.ResumeLayout(true);
			this.modulesPairListDropEdit.PerformLayout();
			this.operatorDropEdit.ResumeLayout(true);
			this.operatorDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZDropEdit modulesPairListDropEdit;
		private ZArchitecture.GUI.ZDropEdit operatorDropEdit;
		private ZArchitecture.ZTextBox objectDescirptionEditText;
		private ZArchitecture.ZLabel labelObject;
	}
}
