using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class PeriodForDischargeUserControl
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.PeriodForDischargeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.AutomaticExtensionCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.PeriodForDischargePeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.PeriodForDischargeDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.PeriodForDischargeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // PeriodForDischargeGroupBox
            // 
            this.PeriodForDischargeGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("78848C66-1538-49BF-9F82-0AC9B0B77C2D", "Period for Discharge");
            this.PeriodForDischargeGroupBox.Controls.Add(this.AutomaticExtensionCheckBox);
            this.PeriodForDischargeGroupBox.Controls.Add(this.PeriodForDischargePeriodCalcEdit);
            this.PeriodForDischargeGroupBox.Controls.Add(this.PeriodForDischargeDetailsTextBox);
            this.PeriodForDischargeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PeriodForDischargeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.PeriodForDischargeGroupBox.Name = "PeriodForDischargeGroupBox";
            this.PeriodForDischargeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 102, true);
            this.PeriodForDischargeGroupBox.TabIndex = 1;
            this.PeriodForDischargeGroupBox.TabStop = false;
            // 
            // AutomaticExtensionCheckBox
            // 
            this.AutomaticExtensionCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.AutomaticExtensionCheckBox, "CustomsEntryInstructions.ZG_PeriodForDischargeAutoExtension");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_PeriodForDischargeAutoExtension)));
            this.AutomaticExtensionCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 15, true);
            this.AutomaticExtensionCheckBox.Name = "AutomaticExtensionCheckBox";
            this.AutomaticExtensionCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 24, true);
            this.AutomaticExtensionCheckBox.TabIndex = 1;
            // 
            // PeriodForDischargePeriodCalcEdit
            // 
            this.PeriodForDischargePeriodCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.PeriodForDischargePeriodCalcEdit, "CustomsEntryInstructions.ZG_PeriodForDischarge");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_PeriodForDischarge)));
            this.PeriodForDischargePeriodCalcEdit.DecimalPlaces = 2;
            this.PeriodForDischargePeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 40, true);
            this.PeriodForDischargePeriodCalcEdit.Name = "PeriodForDischargePeriodCalcEdit";
            this.PeriodForDischargePeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 15, true);
            this.PeriodForDischargePeriodCalcEdit.TabIndex = 2;
            this.PeriodForDischargePeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.PeriodForDischargePeriodCalcEdit.TrackDisposedAccess = true;
            // 
            // PeriodForDischargeDetailsTextBox
            // 
            this.PeriodForDischargeDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.PeriodForDischargeDetailsTextBox, "CustomsEntryInstructions.PeriodForDischargeDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).PeriodForDischargeDetails)));
            this.PeriodForDischargeDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 65, true);
            this.PeriodForDischargeDetailsTextBox.Name = "PeriodForDischargeDetailsTextBox";
            this.PeriodForDischargeDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 15, true);
            this.PeriodForDischargeDetailsTextBox.TabIndex = 3;
            // 
            // PeriodForDischargeUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.PeriodForDischargeGroupBox);
            this.Name = "PeriodForDischargeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 102, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.PeriodForDischargeGroupBox.ResumeLayout(false);
            this.PeriodForDischargeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox PeriodForDischargeGroupBox;
		internal ZCheckBox AutomaticExtensionCheckBox;
		internal ZArchitecture.ZCalcEdit PeriodForDischargePeriodCalcEdit;
		internal ZArchitecture.ZTextBox PeriodForDischargeDetailsTextBox;
	}
}
