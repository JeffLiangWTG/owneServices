namespace Enterprise.Customs.KR.GUI
{
	partial class RefundForCancelUserControl
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
            this.CancelReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DisposalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DisposalNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsLocationDescriptionLongTextControl = new Customs.GUI.LongTextControl();
			this.ResidualSubstanceDescriptionLongTextControl = new Customs.GUI.LongTextControl();
			this.DamageSituationLongTextControl = new Customs.GUI.LongTextControl();
			this.ExportEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ExportEntryLineNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.CancelReasonDropEdit.SuspendLayout();
            this.DisposalDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusReconDeclaration);
            // 
            // CancelReasonDropEdit
            // 
            this.CancelReasonDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CancelReasonDropEdit, "CusReconEntryLines.ContractRevocations.CSI_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).CSI_Code)));
            this.CancelReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 13, true);
            this.CancelReasonDropEdit.Name = "CancelReasonDropEdit";
            this.CancelReasonDropEdit.PreBoundMaxLength = 1;
            this.CancelReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 21, true);
            this.CancelReasonDropEdit.TabIndex = 0;
            // 
            // DisposalDateEdit
            // 
            this.DisposalDateEdit.AllowDrop = true;
            this.DisposalDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DisposalDateEdit, "CusReconEntryLines.ContractRevocations.CSI_DateOfExpiry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).CSI_DateOfExpiry)));
            this.DisposalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 65, true);
            this.DisposalDateEdit.Name = "DisposalDateEdit";
            this.DisposalDateEdit.TabIndex = 1;
            // 
            // DisposalNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.DisposalNumberTextBox, "CusReconEntryLines.ContractRevocations.DisposalNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).DisposalNumber)));
            this.DisposalNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 43, true);
            this.DisposalNumberTextBox.Name = "DisposalNumberTextBox";
            this.DisposalNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 21, true);
            this.DisposalNumberTextBox.TabIndex = 2;
			// 
			// GoodsLocationDescriptionLongTextControl
			// 
			this.BindingSource.SetBindingMember(this.GoodsLocationDescriptionLongTextControl, "CusReconEntryLines.ContractRevocations.CSI_AdditionalDescription");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).CSI_AdditionalDescription)));
            this.GoodsLocationDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 86, true);
            this.GoodsLocationDescriptionLongTextControl.Name = "GoodsLocationDescriptionLongTextControl";
            this.GoodsLocationDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 21, true);
			this.GoodsLocationDescriptionLongTextControl.TabIndex = 3;
			// 
			// ResidualSubstanceDescriptionLongTextControl
			// 
			this.BindingSource.SetBindingMember(this.ResidualSubstanceDescriptionLongTextControl, "CusReconEntryLines.ContractRevocations.CSI_ReferenceNumber2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).CSI_ReferenceNumber2)));
            this.ResidualSubstanceDescriptionLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 119, true);
            this.ResidualSubstanceDescriptionLongTextControl.Name = "ResidualSubstanceDescriptionLongTextControl";
            this.ResidualSubstanceDescriptionLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 21, true);
            this.ResidualSubstanceDescriptionLongTextControl.TabIndex = 4;
			// 
			// DamageSituationLongTextControl
			// 
			this.BindingSource.SetBindingMember(this.DamageSituationLongTextControl, "CusReconEntryLines.ContractRevocations.CSI_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).CSI_Description)));
            this.DamageSituationLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 151, true);
            this.DamageSituationLongTextControl.Name = "DamageSituationLongTextControl";
            this.DamageSituationLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(183, 21, true);
            this.DamageSituationLongTextControl.TabIndex = 5;
            // 
            // ExportEntryNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.ExportEntryNumberTextBox, "CusReconEntryLines.ContractRevocations.ExportEntryNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).ExportEntryNumber)));
            this.ExportEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("550000A4-A567-4AA5-BB54-5D9B9BF0EDEA", "EXP Entry No./ Line No.");
            this.ExportEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 192, true);
            this.ExportEntryNumberTextBox.Name = "ExportEntryNumberTextBox";
            this.ExportEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(123, 21, true);
            this.ExportEntryNumberTextBox.TabIndex = 6;
            // 
            // ExportEntryLineNumberTextBox
            // 
            this.BindingSource.SetBindingMember(this.ExportEntryLineNumberTextBox, "CusReconEntryLines.ContractRevocations.FormattedLineNo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.ContractRevocation5UL)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ContractRevocations)).SyncRoot)).FormattedLineNo)));
            this.ExportEntryLineNumberTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("AF56204F-6601-4ED1-879C-B66B45537C9A", "/");
            this.ExportEntryLineNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 192, true);
            this.ExportEntryLineNumberTextBox.Name = "ExportEntryLineNumberTextBox";
            this.ExportEntryLineNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 21, true);
            this.ExportEntryLineNumberTextBox.TabIndex = 7;
            // 
            // RefundForCancelUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.ExportEntryLineNumberTextBox);
            this.Controls.Add(this.ExportEntryNumberTextBox);
            this.Controls.Add(this.DamageSituationLongTextControl);
            this.Controls.Add(this.ResidualSubstanceDescriptionLongTextControl);
            this.Controls.Add(this.GoodsLocationDescriptionLongTextControl);
            this.Controls.Add(this.DisposalNumberTextBox);
            this.Controls.Add(this.DisposalDateEdit);
            this.Controls.Add(this.CancelReasonDropEdit);
            this.Name = "RefundForCancelUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 252, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.CancelReasonDropEdit.ResumeLayout(true);
            this.CancelReasonDropEdit.PerformLayout();
            this.DisposalDateEdit.ResumeLayout(true);
            this.DisposalDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CancelReasonDropEdit;
		internal ZArchitecture.GUI.ZDateEdit DisposalDateEdit;
		internal ZArchitecture.ZTextBox DisposalNumberTextBox;
		internal Customs.GUI.LongTextControl GoodsLocationDescriptionLongTextControl;
		internal Customs.GUI.LongTextControl ResidualSubstanceDescriptionLongTextControl;
		internal Customs.GUI.LongTextControl DamageSituationLongTextControl;
		internal ZArchitecture.ZTextBox ExportEntryNumberTextBox;
		internal ZArchitecture.ZTextBox ExportEntryLineNumberTextBox;
	}
}
