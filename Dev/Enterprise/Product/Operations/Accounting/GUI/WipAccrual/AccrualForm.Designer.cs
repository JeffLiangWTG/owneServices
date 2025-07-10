namespace Enterprise.Accounting.GUI.WipAccrual
{
	public partial class AccrualForm
	{


		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ConsolNumFindBox = new ZArchitecture.ZTextBox();
			this.ChargeCodeFindBox.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.JobFindBox.SuspendLayout();
			this.AccountFindBox.SuspendLayout();
			this.CreatedDateDateEdit.SuspendLayout();
			this.AmountCalcFindBox.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.GLAccountFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// ChargeCodeFindBox
			// 
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 157, true);
			this.ChargeCodeFindBox.TabIndex = 7;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 109, true);
			this.DepartmentFindBox.TabIndex = 5;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 85, true);
			this.BranchFindBox.TabIndex = 4;
			// 
			// JobFindBox
			// 
			this.BindingSource.SetBindingMember(this.JobFindBox, "AL_JH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.WIPAccrual.Accrual)(null)).AL_JH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.WIPAccrual.Accrual)(null)).JobCollection)));
			// 
			// AccountFindBox
			// 
			this.AccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 133, true);
			this.AccountFindBox.TabIndex = 6;
			// 
			// CreatingUserTextBox
			// 
			this.CreatingUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 230, true);
			this.CreatingUserTextBox.TabIndex = 10;
			// 
			// CreatedDateDateEdit
			// 
			this.CreatedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 230, true);
			this.CreatedDateDateEdit.TabIndex = 11;
			// 
			// AmountCalcFindBox
			// 
			this.AmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 206, true);
			this.AmountCalcFindBox.TabIndex = 9;
			// 
			// TabControl
			// 
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 286, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.ConsolNumFindBox);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 259, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.ConsolNumFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.AccountFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.ChargeCodeFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.CreatingUserTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.DepartmentFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.CreatedDateDateEdit, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.BranchFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.AmountCalcFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.JobFindBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.GLAccountFindBox, 0);
			// 
			// PostButton
			// 
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 295, true);
			// 
			// CloseButton
			// 
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 295, true);
			// 
			// GLAccountFindBox
			// 
			this.GLAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 181, true);
			this.GLAccountFindBox.TabIndex = 8;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 322, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WIPAccrual.Accrual);
			// 
			// ConsolNumFindBox
			// 
			this.ConsolNumFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolNumFindBox, "JK_UniqueConsignRef");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.WIPAccrual.Accrual)(null)).JK_UniqueConsignRef)));
			this.ConsolNumFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("e66e1b0e-cbd8-4ffa-aaf1-ef6f5e9d6f3a", "Consol Number");
			this.ConsolNumFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 61, true);
			this.ConsolNumFindBox.Name = "ConsolNumFindBox";
			this.ConsolNumFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ConsolNumFindBox.TabIndex = 3;
			// 
			// AccrualForm
			// 
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccrualForm|094cf333-343b-4bda-9313-b4a19c14b215", "Accrual");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 344, true);
			this.DataSourceType = typeof(Business.WIPAccrual.Accrual);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.WIPAccrual.Accrual";
			this.Name = "AccrualForm";
			this.ChargeCodeFindBox.ResumeLayout(true);
			this.ChargeCodeFindBox.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.JobFindBox.ResumeLayout(true);
			this.JobFindBox.PerformLayout();
			this.AccountFindBox.ResumeLayout(true);
			this.AccountFindBox.PerformLayout();
			this.CreatedDateDateEdit.ResumeLayout(true);
			this.CreatedDateDateEdit.PerformLayout();
			this.AmountCalcFindBox.ResumeLayout(true);
			this.AmountCalcFindBox.PerformLayout();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.GLAccountFindBox.ResumeLayout(true);
			this.GLAccountFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		protected ZArchitecture.ZTextBox ConsolNumFindBox;
	}
}
