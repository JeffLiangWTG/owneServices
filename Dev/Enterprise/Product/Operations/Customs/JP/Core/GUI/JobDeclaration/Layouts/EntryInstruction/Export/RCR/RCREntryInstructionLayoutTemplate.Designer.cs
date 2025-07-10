using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.GUI
{
	partial class RCREntryInstructionLayoutTemplate
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

		private void InitializeComponent()
		{
			this.RCRActionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviousBillNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViaLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RCRActionDropEdit.SuspendLayout();
			this.ViaLocationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// RCRActionDropEdit
			// 
			this.RCRActionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RCRActionDropEdit, "CEI_RCRAction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_RCRAction)));
			this.RCRActionDropEdit.Name = "RCRActionDropEdit";
			this.RCRActionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// PreviousBillNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousBillNumberTextBox, "CEI_PreviousBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_PreviousBillNumber)));
			this.PreviousBillNumberTextBox.Name = "PreviousBillNumberTextBox";
			this.PreviousBillNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// ViaLocationCodeFindBox
			// 
			this.ViaLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ViaLocationCodeFindBox, "CEI_ViaLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).CEI_ViaLocation)));
			this.ViaLocationCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.ViaLocationCodeFindBox.Name = "ViaLocationCodeFindBox";
			this.ViaLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			// 
			// RCREntryInstructionLayoutTemplate
			//
			this.Controls.Add(this.RCRActionDropEdit);
			this.Controls.Add(this.PreviousBillNumberTextBox);
			this.Controls.Add(this.ViaLocationCodeFindBox);
			this.Name = "RCREntryInstructionLayoutTemplate";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 258, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RCRActionDropEdit.ResumeLayout(true);
			this.RCRActionDropEdit.PerformLayout();
			this.ViaLocationCodeFindBox.ResumeLayout(true);
			this.ViaLocationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZDropEdit RCRActionDropEdit;
		ZTextBox PreviousBillNumberTextBox;
		ZCodeFindBox ViaLocationCodeFindBox;
	}
}
