namespace Enterprise.Customs.CA.GUI
{
	partial class IM2UserControl
	{
		#region Component Designer generated code

		private void InitializeComponent()
		{
			this.VersionNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OriginalJobNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreviousB2TransactionNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreviousB2JobNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SuspendLayout();
			// 
			// B2TypeDropEdit
			// 
			this.B2TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 10, true);
			this.B2TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.VersionNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.OriginalJobNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.PreviousB2TransactionNoTextBox);
			this.DetailsGroupBox.Controls.Add(this.PreviousB2JobNumberTextBox);
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 285, true);
			// 
			// AmendmentToDropEdit
			// 
			this.AmendmentToDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 235, true);
			this.AmendmentToDropEdit.TabIndex = 23;
			// 
			// AnySightDepositAmountCalcEdit
			//
			this.AnySightDepositAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 210, true);
			this.AnySightDepositAmountCalcEdit.TabIndex = 22;
			// 
			// ClaimedInterestAmountCalcEdit
			//
			this.ClaimedInterestAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 185, true);
			this.ClaimedInterestAmountCalcEdit.TabIndex = 21;
			// 
			// DocsAttachedCheckBox
			//
			this.DocsAttachedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(58, 160, true);
			this.DocsAttachedCheckBox.TabIndex = 19;
			// 
			// SecurityNoTextBox
			//
			this.SecurityNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 135, true);
			this.SecurityNoTextBox.TabIndex = 18;
			// 
			// AuthorisationDateDateEdit1
			// 
			this.AuthorisationDateDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 110, true);
			this.AuthorisationDateDateEdit1.TabIndex = 17;
			// 
			// OriginalAccountingDateDateEdit
			// 
			this.OriginalAccountingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 110, true);
			this.OriginalAccountingDateDateEdit.TabIndex = 16;
			// 
			// ClearancePortCodeFindBox
			// 
			this.ClearancePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 85, true);
			this.ClearancePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 20, true);
			this.ClearancePortCodeFindBox.TabIndex = 15;
			// 
			// OriginalTransactionNumberCodeFindBox
			//
			this.OriginalTransactionNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 35, true);
			// 
			// VersionNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.VersionNumberTextBox, "CA_Version");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).CA_Version)));
			this.VersionNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("F5BD3061-7B8A-4676-8996-2A271289EC64", "Ver.");
			this.VersionNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 35, true);
			this.VersionNumberTextBox.Name = "VersionNumberTextBox";
			this.VersionNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(22, 20, true);
			this.VersionNumberTextBox.TabIndex = 11;
			// 
			// OriginalJobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.OriginalJobNumberTextBox, "OriginalDeclaration+JE_DeclarationReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).OriginalDeclaration.JE_DeclarationReference)));
			this.OriginalJobNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9B6F3D15-10D7-434B-90C8-6E4278270A37", "Orig.Job");
			this.OriginalJobNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 35, true);
			this.OriginalJobNumberTextBox.Name = "OriginalJobNumberTextBox";
			this.OriginalJobNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.OriginalJobNumberTextBox.TabIndex = 12;
			// 
			// PreviousB2TransactionNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousB2TransactionNoTextBox, "PreviousTransactionNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).PreviousTransactionNumber)));
			this.PreviousB2TransactionNoTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("38C41FDD-79E7-4D94-994E-3804E76DB8E3", "Previous B2 Transaction No.");
			this.PreviousB2TransactionNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 60, true);
			this.PreviousB2TransactionNoTextBox.Name = "PreviousB2TransactionNoTextBox";
			this.PreviousB2TransactionNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(121, 20, true);
			this.PreviousB2TransactionNoTextBox.TabIndex = 13;
			// 
			// PreviousB2JobNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousB2JobNumberTextBox, "PreviousJobNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).PreviousJobNumber)));
			this.PreviousB2JobNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("080774B4-1E80-4370-810E-AAD8EC928612", "Previous B2 Job No.");
			this.PreviousB2JobNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 60, true);
			this.PreviousB2JobNumberTextBox.Name = "PreviousB2JobNumberTextBox";
			this.PreviousB2JobNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.PreviousB2JobNumberTextBox.TabIndex = 14;
			// 
			// DatesGroupBox
			// 
			this.DatesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(746, 62, true);
			this.DatesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 100, true);
			// 
			// IsOurFaultCheckBox
			// 
			this.IsOurFaultCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 160, true);
			this.IsOurFaultCheckBox.TabIndex = 20;
			// 
			// InitiatedByDropEdit
			// 
			this.InitiatedByDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(864, 170, true);
			// 
			// ChequeNumberTextBox
			// 
			this.ChequeNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(864, 195, true);
			// 
			// ChequeDateDateEdit
			// 
			this.ChequeDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(864, 220, true);
			this.AmountDueImporterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(864, 245, true);
			this.AmountDueCBSACalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(864, 270, true);
			// 
			// IM2UserControl
			// 
			this.Name = "IM2UserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 425, true);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.ZTextBox VersionNumberTextBox;
		private ZArchitecture.ZTextBox OriginalJobNumberTextBox;
		private ZArchitecture.ZTextBox PreviousB2TransactionNoTextBox;
		private ZArchitecture.ZTextBox PreviousB2JobNumberTextBox;
	}
}
