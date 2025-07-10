namespace Enterprise.Accounting.GUI.JobInvoicing
{
	partial class DebtorsAcceptGroupChargesForm
	{
		public DebtorsAcceptGroupChargesForm()
		{
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.chargesForDebtor = new Enterprise.ZArchitecture.GUI.ZFilterGrid();
			this.autoratingOption = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.acceptButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.chargesForDebtor)).BeginInit();
			this.chargesForDebtor.SuspendLayout();
			this.autoratingOption.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 393, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1033, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob);
			// 
			// chargesForDebtor
			// 
			this.chargesForDebtor.AllowNavigation = false;
			this.chargesForDebtor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.chargesForDebtor, "ChargesForDebtor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).AcceptActionDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_AC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).CostCompanyChargeCodePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_Desc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_RX_NKSellCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_OSSellAmt)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).Creditor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_OH_SellAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_InvoiceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).SellRecognition)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_IsRevenuePosted)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_JobNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).JR_SellReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).DisplaySellInvoiceAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyCharge)(((System.Collections.IList)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).ChargesForDebtor)).SyncRoot)).DisplaySellInvoiceContact)));
			this.chargesForDebtor.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "AcceptActionDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(64);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JR_AC";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "CostCompanyChargeCodePK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "JR_Desc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.ColumnName = "JR_RX_NKSellCurrency";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "JR_OSSellAmt";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Creditor";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "JR_OH_SellAccount";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "JR_InvoiceType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zTextBoxColumnStyleInfo5.ColumnName = "SellRecognition";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(48);
			zCheckBoxColumnStyleInfo1.ColumnName = "JR_IsRevenuePosted";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(32);
			zTextBoxColumnStyleInfo6.ColumnName = "JR_JobNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "JR_SellReference";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "DisplaySellInvoiceAddress";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgAddresses;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo5.ColumnName = "DisplaySellInvoiceContact";
			zGuidFindBoxColumnStyleInfo5.IsVisible = false;
			zGuidFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.chargesForDebtor.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.chargesForDebtor.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.chargesForDebtor.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.chargesForDebtor.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.chargesForDebtor.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.chargesForDebtor.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.chargesForDebtor.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.chargesForDebtor.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.chargesForDebtor.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.chargesForDebtor.GridId = "6fdf8d63-dd2d-4d03-809e-3ceff4c1f068";
			this.chargesForDebtor.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.chargesForDebtor.LayoutKey = "groupCompanySellChargesGrid";
			this.chargesForDebtor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 6, true);
			this.chargesForDebtor.Name = "chargesForDebtor";
			this.chargesForDebtor.ReadOnly = true;
			this.chargesForDebtor.ShouldSetErrorsOnTabPage = false;
			this.chargesForDebtor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 355, true);
			this.chargesForDebtor.TabIndex = 1;
			// 
			// autoratingOption
			// 
			this.autoratingOption.AllowDrop = true;
			this.autoratingOption.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.autoratingOption, "AutoratingOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob)(null)).AutoratingOption)));
			this.autoratingOption.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ad12629f-cae5-4617-8268-020ffef4ddf3", "Autorating Options", "Sets whether Autorating will occur after accepting Group Company Costs");
			this.autoratingOption.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(677, 367, true);
			this.autoratingOption.Name = "autoratingOption";
			this.autoratingOption.PreBoundMaxLength = 3;
			this.autoratingOption.ShouldResizeByMaxLength = true;
			this.autoratingOption.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 20, true);
			this.autoratingOption.TabIndex = 3;
			// 
			// acceptButton
			// 
			this.acceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.acceptButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DebtorsAcceptGroupChargesForm|0d733439-c68d-4fc3-98b1-09202850dbfd", "Accept");
			this.acceptButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.acceptButton.IsCaptionOverridden = false;
			this.acceptButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(843, 365, true);
			this.acceptButton.Name = "acceptButton";
			this.acceptButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.acceptButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.acceptButton.TabIndex = 5;
			this.acceptButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.acceptButton.ToolTipCaption = null;
			this.acceptButton.UseVisualStyleBackColor = true;
			this.acceptButton.Click += new System.EventHandler(this.AcceptButton_Click);
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("DebtorsAcceptGroupChargesForm|b9cae165-f011-4c9c-a68b-bea128ceafbc", "Cancel");
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.IsCaptionOverridden = false;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(935, 365, true);
			this.closeButton.Name = "closeButton";
			this.closeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 23, true);
			this.closeButton.TabIndex = 6;
			this.closeButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.closeButton.ToolTipCaption = null;
			this.closeButton.UseVisualStyleBackColor = true;
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// DebtorsAcceptGroupChargesForm
			// 
			this.AcceptButton = this.acceptButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.closeButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1033, 417, true);
			this.Controls.Add(this.chargesForDebtor);
			this.Controls.Add(this.autoratingOption);
			this.Controls.Add(this.acceptButton);
			this.Controls.Add(this.closeButton);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.JobInvoicing.GroupCompanyChargesForJob);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 406, true);
			this.Name = "DebtorsAcceptGroupChargesForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.closeButton, 0);
			this.Controls.SetChildIndex(this.acceptButton, 0);
			this.Controls.SetChildIndex(this.autoratingOption, 0);
			this.Controls.SetChildIndex(this.chargesForDebtor, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.chargesForDebtor)).EndInit();
			this.chargesForDebtor.ResumeLayout(false);
			this.chargesForDebtor.PerformLayout();
			this.autoratingOption.ResumeLayout(true);
			this.autoratingOption.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.GUI.ZFilterGrid chargesForDebtor;
		ZArchitecture.GUI.ZDropEdit autoratingOption;
		ZArchitecture.GUI.ZButton acceptButton;
		ZArchitecture.GUI.ZButton closeButton;
	}
}
