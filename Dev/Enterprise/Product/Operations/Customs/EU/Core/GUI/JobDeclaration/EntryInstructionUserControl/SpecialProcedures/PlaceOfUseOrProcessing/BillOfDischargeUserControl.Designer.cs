using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class BillOfDischargeUserControl
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
            this.BillOfDischargeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.NecessaryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.BillOfDischargeDeadlineCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.BillOfDischargeDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BillOfDischargeGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
            // 
            // BillOfDischargeGroupBox
            // 
            this.BillOfDischargeGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("FD1A5487-04A3-4690-8277-FE8382CC0D64", "Bill Of Discharge");
            this.BillOfDischargeGroupBox.Controls.Add(this.NecessaryCheckBox);
            this.BillOfDischargeGroupBox.Controls.Add(this.BillOfDischargeDeadlineCalcEdit);
            this.BillOfDischargeGroupBox.Controls.Add(this.BillOfDischargeDetailsTextBox);
            this.BillOfDischargeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BillOfDischargeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.BillOfDischargeGroupBox.Name = "BillOfDischargeGroupBox";
            this.BillOfDischargeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 110, true);
            this.BillOfDischargeGroupBox.TabIndex = 1;
            this.BillOfDischargeGroupBox.TabStop = false;
            // 
            // NecessaryCheckBox
            // 
            this.NecessaryCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.NecessaryCheckBox, "CustomsEntryInstructions.ZG_BillOfDischargeIsNecessary");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_BillOfDischargeIsNecessary)));
            this.NecessaryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 15, true);
            this.NecessaryCheckBox.Name = "NecessaryCheckBox";
            this.NecessaryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 24, true);
            this.NecessaryCheckBox.TabIndex = 1;
            // 
            // BillOfDischargeDeadlineCalcEdit
            // 
            this.BillOfDischargeDeadlineCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.BillOfDischargeDeadlineCalcEdit, "CustomsEntryInstructions.ZG_BillOfDischargeDeadline");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).ZG_BillOfDischargeDeadline)));
            this.BillOfDischargeDeadlineCalcEdit.DecimalPlaces = 2;
            this.BillOfDischargeDeadlineCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 39, true);
            this.BillOfDischargeDeadlineCalcEdit.Name = "BillOfDischargeDeadlineCalcEdit";
            this.BillOfDischargeDeadlineCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 15, true);
            this.BillOfDischargeDeadlineCalcEdit.TabIndex = 2;
            this.BillOfDischargeDeadlineCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.BillOfDischargeDeadlineCalcEdit.TrackDisposedAccess = true;
            // 
            // BillOfDischargeDetailsTextBox
            // 
            this.BillOfDischargeDetailsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.BillOfDischargeDetailsTextBox, "CustomsEntryInstructions.BillOfDischargeDetails");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryInstructions)).SyncRoot)).BillOfDischargeDetails)));
            this.BillOfDischargeDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 65, true);
            this.BillOfDischargeDetailsTextBox.Name = "BillOfDischargeDetailsTextBox";
            this.BillOfDischargeDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 15, true);
            this.BillOfDischargeDetailsTextBox.TabIndex = 3;
            // 
            // BillOfDischargeUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.BillOfDischargeGroupBox);
            this.Name = "BillOfDischargeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(483, 110, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BillOfDischargeGroupBox.ResumeLayout(false);
            this.BillOfDischargeGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZGroupBox BillOfDischargeGroupBox;
		internal ZCheckBox NecessaryCheckBox;
		internal ZArchitecture.ZCalcEdit BillOfDischargeDeadlineCalcEdit;
		internal ZArchitecture.ZTextBox BillOfDischargeDetailsTextBox;
	}
}
