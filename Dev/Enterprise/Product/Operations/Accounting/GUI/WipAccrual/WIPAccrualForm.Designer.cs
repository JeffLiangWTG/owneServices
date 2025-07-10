namespace Enterprise.Accounting.GUI.WipAccrual
{
	public abstract partial class WIPAccrualForm
	{

		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ChargeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.AccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.JobFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CreatingUserTextBox = new ZArchitecture.ZTextBox();
			this.CreatedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AmountCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			this.PostDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReverseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.GLAccountFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.PostButton = new Enterprise.Core.Forms.ZPostOrCancelButton();
			this.CloseButton = new Enterprise.Core.Forms.ZPostOrCancelButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 296, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 22, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(257);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual);
			// 
			// ChargeCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.ChargeCodeFindBox, "AL_AC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).ChargeCodeCollection)));
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 133, true);
			this.ChargeCodeFindBox.Name = "ChargeCodeFindBox";
			this.ChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ChargeCodeFindBox.TabIndex = 6;
			// 
			// AccountFindBox
			// 
			this.BindingSource.SetBindingMember(this.AccountFindBox, "AL_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_OH)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).OrganisationsCollection)));
			this.AccountFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|2a305505-a3aa-4c44-bfeb-5c8986ae1105", "Account");
			this.AccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 109, true);
			this.AccountFindBox.Name = "AccountFindBox";
			this.AccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.AccountFindBox.TabIndex = 5;
			// 
			// DepartmentFindBox
			// 
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "AL_GE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).DepartmentCollection)));
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 85, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.DepartmentFindBox.TabIndex = 4;
			// 
			// BranchFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchFindBox, "AL_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_GB)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 61, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.BranchFindBox.TabIndex = 3;
			// 
			// JobFindBox
			// 
			this.JobFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|bc1db4c6-e560-4e20-86cb-a08827acfd1c", "Job Number");
			this.JobFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 37, true);
			this.JobFindBox.Name = "JobFindBox";
			this.JobFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.JobFindBox.TabIndex = 2;
			// 
			// CreatingUserTextBox
			// 
			this.BindingSource.SetBindingMember(this.CreatingUserTextBox, "AL_Calc_CreatingUserName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_Calc_CreatingUserName)));
			this.CreatingUserTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|f779bc80-3bc9-401a-a635-1d4b8bb58616", "Creating User");
			this.CreatingUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 206, true);
			this.CreatingUserTextBox.Name = "CreatingUserTextBox";
			this.CreatingUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.CreatingUserTextBox.TabIndex = 9;
			// 
			// CreatedDateDateEdit
			// 
			this.CreatedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.CreatedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.CreatedDateDateEdit, "AL_Calc_CreatedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_Calc_CreatedDate)));
			this.CreatedDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|37cfaf92-e169-439f-a3f5-be7740a85a3b", "Created Date");
			this.CreatedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 206, true);
			this.CreatedDateDateEdit.Name = "CreatedDateDateEdit";
			this.CreatedDateDateEdit.TabIndex = 10;
			// 
			// AmountCalcFindBox
			// 
			this.AmountCalcFindBox.BindToAmount = "AL_OSExTaxAmount";
			this.AmountCalcFindBox.BindToDecimalPlaces = "AL_Calc_LocalRXDecimals";
			this.AmountCalcFindBox.BindToUnit = "AL_RX_NKTransactionCurrency";
			this.AmountCalcFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|50608192-760f-4a33-9dd2-211d84a576e0", "Amount");
			this.AmountCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.AmountCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 182, true);
			this.AmountCalcFindBox.Name = "AmountCalcFindBox";
			this.AmountCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.AmountCalcFindBox.TabIndex = 8;
			// 
			// PostDateDateEdit
			// 
			this.PostDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.PostDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PostDateDateEdit, "AL_PostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_PostDate)));
			this.PostDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|72a62d40-70b1-4192-99da-3627f8b88ea9", "Post To Date");
			this.PostDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 13, true);
			this.PostDateDateEdit.Name = "PostDateDateEdit";
			this.PostDateDateEdit.TabIndex = 0;
			// 
			// ReverseDateEdit
			// 
			this.ReverseDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReverseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReverseDateEdit, "AL_ReverseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_ReverseDate)));
			this.ReverseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 13, true);
			this.ReverseDateEdit.Name = "ReverseDateEdit";
			this.ReverseDateEdit.TabIndex = 1;
			this.ReverseDateEdit.Visible = false;
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.LogsTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 263, true);
			this.TabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.DetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|639612f7-4f16-45ff-8654-ea8ff0e6ee75", "Transaction");
			this.DetailsTabPage.Controls.Add(this.GLAccountFindBox);
			this.DetailsTabPage.Controls.Add(this.PostDateDateEdit);
			this.DetailsTabPage.Controls.Add(this.ReverseDateEdit);
			this.DetailsTabPage.Controls.Add(this.JobFindBox);
			this.DetailsTabPage.Controls.Add(this.AmountCalcFindBox);
			this.DetailsTabPage.Controls.Add(this.BranchFindBox);
			this.DetailsTabPage.Controls.Add(this.CreatedDateDateEdit);
			this.DetailsTabPage.Controls.Add(this.DepartmentFindBox);
			this.DetailsTabPage.Controls.Add(this.CreatingUserTextBox);
			this.DetailsTabPage.Controls.Add(this.ChargeCodeFindBox);
			this.DetailsTabPage.Controls.Add(this.AccountFindBox);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 236, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// GLAccountFindBox
			// 
			this.BindingSource.SetBindingMember(this.GLAccountFindBox, "AL_AG");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual)(null)).AL_AG)));
			this.GLAccountFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 157, true);
			this.GLAccountFindBox.Name = "GLAccountFindBox";
			this.GLAccountFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.GLAccountFindBox.TabIndex = 7;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LogsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0fbe7dc1-4f35-4c89-98cf-64628b35000e", "Logs");
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 236, true);
			this.LogsTabPage.TabIndex = 1;
			// 
			// PostButton
			// 
			this.PostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|3c4717a4-1ff2-4927-8863-a79fa3ead5b1", "&Post");
			this.PostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(365, 269, true);
			this.PostButton.Name = "PostButton";
			this.PostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.PostButton.TabIndex = 1;
			this.PostButton.Text = "&Post";
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|898a3fae-8cec-4b3c-8450-c8583f88f061", "&Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 269, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Text = "&Cancel";
			// 
			// WIPAccrualForm
			// 

			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("WIPAccrualForm|094cf333-343b-4bda-9313-b4a19c14b215", "WIP Accrual Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(528, 318, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.PostButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.WIPAccrual.BaseWIPAccrual";
			this.IsPostOnly = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 327, true);
			this.Name = "WIPAccrualForm";
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.PostButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox ChargeCodeFindBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox DepartmentFindBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox JobFindBox;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox AccountFindBox;
		protected ZArchitecture.ZTextBox CreatingUserTextBox;
		protected Enterprise.ZArchitecture.GUI.ZDateEdit CreatedDateDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZCalcFindBox AmountCalcFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit PostDateDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit ReverseDateEdit;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl TabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage LogsTabPage;
		protected Enterprise.Core.Forms.ZPostOrCancelButton PostButton;
		protected Enterprise.Core.Forms.ZPostOrCancelButton CloseButton;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox GLAccountFindBox;
	}
}
