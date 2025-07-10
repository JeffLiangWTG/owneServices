namespace Enterprise.Customs.CA.GUI
{
	partial class B2AdjustmentsUserControl
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MiscGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PaymentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MiscGroupBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.BrokerCodeFindBox.SuspendLayout();
			this.PaymentCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.PaymentCodeDropEdit);
			// MiscGroupBox
			// 
			this.MiscGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7d8628ae-4476-4c3a-aea7-b03f2b4ad01d", "Misc.");
			this.MiscGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.MiscGroupBox.Controls.Add(this.BrokerCodeFindBox);
			this.MiscGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 62, true);
			this.MiscGroupBox.Name = "MiscGroupBox";
			this.MiscGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 68, true);
			this.MiscGroupBox.TabIndex = 18;
			this.MiscGroupBox.TabStop = false;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "JE_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_GB)));
			this.BranchGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("f95c21a6-04b4-4416-819c-92220f981010", "Branch Code");
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 11, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.BranchGuidFindBox.TabIndex = 19;
			// 
			// BrokerCodeFindBox
			// 
			this.BrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BrokerCodeFindBox, "JE_GS_NKCusAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_GS_NKCusAgent)));
			this.BrokerCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d0dbcc55-5ac9-443f-ab35-b47f5e40d48d", "Broker");
			this.BrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 37, true);
			this.BrokerCodeFindBox.Name = "BrokerCodeFindBox";
			this.BrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(223, 20, true);
			this.BrokerCodeFindBox.TabIndex = 20;
			// 
			// PaymentCodeDropEdit
			// 
			this.PaymentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentCodeDropEdit, "JE_PaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.JobDeclaration)(null)).JE_PaymentMethod)));
			this.PaymentCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d452f2d9-cbd1-4c63-abb8-1eec7a8f054c", "Payment Code");
			this.PaymentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 213, true);
			this.PaymentCodeDropEdit.Name = "PaymentCodeDropEdit";
			this.PaymentCodeDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 20, true);
			this.PaymentCodeDropEdit.TabIndex = 22;
			// 
			// B2AdjustmentsUserControl
			// 
			this.Controls.Add(this.MiscGroupBox);
			this.Controls.SetChildIndex(this.MiscGroupBox, 0);
			this.Name = "B2AdjustmentsUserControl";
			this.MiscGroupBox.ResumeLayout(false);
			this.MiscGroupBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.BrokerCodeFindBox.ResumeLayout(true);
			this.BrokerCodeFindBox.PerformLayout();
			this.PaymentCodeDropEdit.ResumeLayout(true);
			this.PaymentCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox MiscGroupBox;
		protected ZArchitecture.GUI.ZCodeFindBox BrokerCodeFindBox;
		protected ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		protected ZArchitecture.GUI.ZDropEdit PaymentCodeDropEdit;
	}
}
