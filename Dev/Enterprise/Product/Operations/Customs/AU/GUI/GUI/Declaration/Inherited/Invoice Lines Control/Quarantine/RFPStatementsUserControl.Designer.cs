using System.Drawing.Text;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPStatementsUserControl
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
			this.StatementTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_StatementTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_StatementNumber5CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_StatementNumber4CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_StatementNumber3CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_StatementNumber2CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_StatementNumber1CalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.StatementTextGroupBox.SuspendLayout();
			this.zGroupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// StatementTextGroupBox
			// 
			this.StatementTextGroupBox.Controls.Add(this.QL_StatementTextTextBox);
			this.StatementTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(212, 6, true);
			this.StatementTextGroupBox.Name = "StatementTextGroupBox";
			this.StatementTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 209, true);
			this.StatementTextGroupBox.TabIndex = 1;
			this.StatementTextGroupBox.TabStop = false;
			this.StatementTextGroupBox.Text = "Statement Text";
			// 
			// QL_StatementTextTextBox
			// 
			this.QL_StatementTextTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_StatementTextTextBox, "QuarantineExDocLine+QL_StatementText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_StatementText)));
			this.QL_StatementTextTextBox.CaptionResourceString = null;
			this.QL_StatementTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 26, true);
			this.QL_StatementTextTextBox.Multiline = true;
			this.QL_StatementTextTextBox.Name = "QL_StatementTextTextBox";
			this.QL_StatementTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 164, true);
			this.QL_StatementTextTextBox.TabIndex = 1;
			// 
			// zGroupBox2
			// 
			this.zGroupBox2.Controls.Add(this.QL_StatementNumber5CalcEdit);
			this.zGroupBox2.Controls.Add(this.QL_StatementNumber4CalcEdit);
			this.zGroupBox2.Controls.Add(this.QL_StatementNumber3CalcEdit);
			this.zGroupBox2.Controls.Add(this.QL_StatementNumber2CalcEdit);
			this.zGroupBox2.Controls.Add(this.QL_StatementNumber1CalcEdit);
			this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.zGroupBox2.Name = "zGroupBox2";
			this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 209, true);
			this.zGroupBox2.TabIndex = 0;
			this.zGroupBox2.TabStop = false;
			this.zGroupBox2.Text = "Statement Numbers";
			// 
			// QL_StatementNumber5CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_StatementNumber5CalcEdit, "QuarantineExDocLine+QL_StatementNumber5");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_StatementNumber5)));
			this.QL_StatementNumber5CalcEdit.CaptionResourceString = null;
			this.QL_StatementNumber5CalcEdit.DecimalPlaces = 2;
			this.QL_StatementNumber5CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 170, true);
			this.QL_StatementNumber5CalcEdit.Name = "QL_StatementNumber5CalcEdit";
			this.QL_StatementNumber5CalcEdit.ShowGroupSeparators = false;
			this.QL_StatementNumber5CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.QL_StatementNumber5CalcEdit.TabIndex = 9;
			this.QL_StatementNumber5CalcEdit.Text = "0";
			this.QL_StatementNumber5CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_StatementNumber4CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_StatementNumber4CalcEdit, "QuarantineExDocLine+QL_StatementNumber4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_StatementNumber4)));
			this.QL_StatementNumber4CalcEdit.CaptionResourceString = null;
			this.QL_StatementNumber4CalcEdit.DecimalPlaces = 2;
			this.QL_StatementNumber4CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 134, true);
			this.QL_StatementNumber4CalcEdit.Name = "QL_StatementNumber4CalcEdit";
			this.QL_StatementNumber4CalcEdit.ShowGroupSeparators = false;
			this.QL_StatementNumber4CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.QL_StatementNumber4CalcEdit.TabIndex = 7;
			this.QL_StatementNumber4CalcEdit.Text = "0";
			this.QL_StatementNumber4CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_StatementNumber3CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_StatementNumber3CalcEdit, "QuarantineExDocLine+QL_StatementNumber3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_StatementNumber3)));
			this.QL_StatementNumber3CalcEdit.CaptionResourceString = null;
			this.QL_StatementNumber3CalcEdit.DecimalPlaces = 2;
			this.QL_StatementNumber3CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 98, true);
			this.QL_StatementNumber3CalcEdit.Name = "QL_StatementNumber3CalcEdit";
			this.QL_StatementNumber3CalcEdit.ShowGroupSeparators = false;
			this.QL_StatementNumber3CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.QL_StatementNumber3CalcEdit.TabIndex = 5;
			this.QL_StatementNumber3CalcEdit.Text = "0";
			this.QL_StatementNumber3CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_StatementNumber2CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_StatementNumber2CalcEdit, "QuarantineExDocLine+QL_StatementNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_StatementNumber2)));
			this.QL_StatementNumber2CalcEdit.CaptionResourceString = null;
			this.QL_StatementNumber2CalcEdit.DecimalPlaces = 2;
			this.QL_StatementNumber2CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 62, true);
			this.QL_StatementNumber2CalcEdit.Name = "QL_StatementNumber2CalcEdit";
			this.QL_StatementNumber2CalcEdit.ShowGroupSeparators = false;
			this.QL_StatementNumber2CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.QL_StatementNumber2CalcEdit.TabIndex = 3;
			this.QL_StatementNumber2CalcEdit.Text = "0";
			this.QL_StatementNumber2CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_StatementNumber1CalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_StatementNumber1CalcEdit, "QuarantineExDocLine+QL_StatementNumber1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_StatementNumber1)));
			this.QL_StatementNumber1CalcEdit.CaptionResourceString = null;
			this.QL_StatementNumber1CalcEdit.DecimalPlaces = 2;
			this.QL_StatementNumber1CalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(69, 26, true);
			this.QL_StatementNumber1CalcEdit.Name = "QL_StatementNumber1CalcEdit";
			this.QL_StatementNumber1CalcEdit.ShowGroupSeparators = false;
			this.QL_StatementNumber1CalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 17, true);
			this.QL_StatementNumber1CalcEdit.TabIndex = 1;
			this.QL_StatementNumber1CalcEdit.Text = "0";
			this.QL_StatementNumber1CalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RFPStatementsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.StatementTextGroupBox);
			this.Controls.Add(this.zGroupBox2);
			this.Name = "RFPStatementsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.StatementTextGroupBox.ResumeLayout(false);
			this.StatementTextGroupBox.PerformLayout();
			this.zGroupBox2.ResumeLayout(false);
			this.zGroupBox2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZGroupBox zGroupBox2;
		private ZCalcEdit QL_StatementNumber5CalcEdit;
		private ZCalcEdit QL_StatementNumber4CalcEdit;
		private ZCalcEdit QL_StatementNumber3CalcEdit;
		private ZCalcEdit QL_StatementNumber2CalcEdit;
		private ZCalcEdit QL_StatementNumber1CalcEdit;
		private ZGroupBox StatementTextGroupBox;
		private ZTextBox QL_StatementTextTextBox;

	}
}