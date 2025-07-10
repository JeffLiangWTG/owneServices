using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class AccQueryClaimUserControl
	{

		#region Component Designer generated code
		protected ZGroupBox DetailsGroupBox;
		protected ZTextBox DetailsTextBox;
		protected ZGroupBox AccountDetailsGroupBox;
		protected ZGroupBox ClaimDetailsGroupBox;
		protected ZGuidDropEdit ContactGuidDropEdit;
		protected ZGuidFindBox OrganisationGuidFindBox;
		protected ZGuidFindBox BranchGuidFindBox;
		protected ZDateEdit AY_QueryClaimNextFollowUpDateEdit;
		protected ZDropEdit StatusDropEdit;
		protected ZTextBox ShortDescriptionTextBox;
		protected ZDropEdit TypeDropEdit;
		protected ZDropEdit ReasonDropEdit;
		protected ZCalcEdit AmountCalcEdit;
		protected ZGuidFindBox InvoiceGuidFindBox;
		protected ZButton AddToClaimLogButton;
		protected ZButton ApproveClaimButton;
		protected ZButton RejectClaimButton;
		protected ZGroupBox ClaimStatusGroupBox;
		protected ZGroupBox IntercompanyClaimDetailsGroupBox;
		protected ZGuidFindBox TransactionBranchGuidFindBox;
		protected ZGuidFindBox TransactionBranchOrgProxyFindBox;
		protected ZTextBox CurrencyTextBox;
		private ZCodeFindBox CreatorCodeFindBox;
		private ZCodeFindBox StaffCodeFindBox;
		private KFlowLayoutPanel kFlowLayoutPanel1;
		protected ZDropEdit HoldOptionEdit;
		private ZTextBox mawbTextBox;
		private readonly System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.DetailsGroupBox = new ZGroupBox();
			this.AddToClaimLogButton = new ZButton();
			this.DetailsTextBox = new ZTextBox();
			this.AccountDetailsGroupBox = new ZGroupBox();
			this.mawbTextBox = new ZTextBox();
			this.ContactGuidDropEdit = new ZGuidDropEdit();
			this.AmountCalcEdit = new ZCalcEdit();
			this.OrganisationGuidFindBox = new ZGuidFindBox();
			this.InvoiceGuidFindBox = new ZGuidFindBox();
			this.CurrencyTextBox = new ZTextBox();
			this.ShortDescriptionTextBox = new ZTextBox();
			this.BranchGuidFindBox = new ZGuidFindBox();
			this.ClaimDetailsGroupBox = new ZGroupBox();
			this.HoldOptionEdit = new ZDropEdit();
			this.CreatorCodeFindBox = new ZCodeFindBox();
			this.TypeDropEdit = new ZDropEdit();
			this.ReasonDropEdit = new ZDropEdit();
			this.AY_QueryClaimNextFollowUpDateEdit = new ZDateEdit();
			this.StatusDropEdit = new ZDropEdit();
			this.ApproveClaimButton = new ZButton();
			this.RejectClaimButton = new ZButton();
			this.ClaimStatusGroupBox = new ZGroupBox();
			this.StaffCodeFindBox = new ZCodeFindBox();
			this.IntercompanyClaimDetailsGroupBox = new ZGroupBox();
			this.kFlowLayoutPanel1 = new KFlowLayoutPanel();
			this.TransactionBranchOrgProxyFindBox = new ZGuidFindBox();
			this.TransactionBranchGuidFindBox = new ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.AccountDetailsGroupBox.SuspendLayout();
			this.ContactGuidDropEdit.SuspendLayout();
			this.OrganisationGuidFindBox.SuspendLayout();
			this.InvoiceGuidFindBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.ClaimDetailsGroupBox.SuspendLayout();
			this.HoldOptionEdit.SuspendLayout();
			this.CreatorCodeFindBox.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.ReasonDropEdit.SuspendLayout();
			this.AY_QueryClaimNextFollowUpDateEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.ClaimStatusGroupBox.SuspendLayout();
			this.StaffCodeFindBox.SuspendLayout();
			this.IntercompanyClaimDetailsGroupBox.SuspendLayout();
			this.kFlowLayoutPanel1.SuspendLayout();
			this.TransactionBranchOrgProxyFindBox.SuspendLayout();
			this.TransactionBranchGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccQueryClaim);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|98edfae9-2512-45a2-bd8e-36d91b055832b", "Claim Log");
			this.DetailsGroupBox.Controls.Add(this.AddToClaimLogButton);
			this.DetailsGroupBox.Controls.Add(this.DetailsTextBox);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 8, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 285, true);
			this.DetailsGroupBox.TabIndex = 3;
			this.DetailsGroupBox.TabStop = false;
			// 
			// AddToClaimLogButton
			// 
			this.AddToClaimLogButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AddToClaimLogButton.AutoSize = true;
			this.AddToClaimLogButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|3e237b77-e1d7-465a-aac6-83422da613b3", "Add To Claim Log");
			this.AddToClaimLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 257, true);
			this.AddToClaimLogButton.Name = "AddToClaimLogButton";
			this.AddToClaimLogButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddToClaimLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 23, true);
			this.AddToClaimLogButton.TabIndex = 1;
			this.AddToClaimLogButton.ToolTipCaption = null;
			this.AddToClaimLogButton.UseVisualStyleBackColor = true;
			this.AddToClaimLogButton.Click += new EventHandler(this.AddToClaimLogButton_Click);
			// 
			// DetailsTextBox
			// 
			this.DetailsTextBox.Anchor = ((AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DetailsTextBox, "Details");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaim)(null)).Details)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DetailsTextBox, false);
			this.DetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.DetailsTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 110, true);
			this.DetailsTextBox.Multiline = true;
			this.DetailsTextBox.Name = "DetailsTextBox";
			this.DetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 235, true);
			this.DetailsTextBox.TabIndex = 0;
			// 
			// AccountDetailsGroupBox
			// 
			this.AccountDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|a8c8ba75-d5a0-4705-b77b-a4985ca6d575", "Account Details");
			this.AccountDetailsGroupBox.Controls.Add(this.mawbTextBox);
			this.AccountDetailsGroupBox.Controls.Add(this.ContactGuidDropEdit);
			this.AccountDetailsGroupBox.Controls.Add(this.AmountCalcEdit);
			this.AccountDetailsGroupBox.Controls.Add(this.OrganisationGuidFindBox);
			this.AccountDetailsGroupBox.Controls.Add(this.InvoiceGuidFindBox);
			this.AccountDetailsGroupBox.Controls.Add(this.CurrencyTextBox);
			this.AccountDetailsGroupBox.Controls.Add(this.ShortDescriptionTextBox);
			this.AccountDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.AccountDetailsGroupBox.Name = "AccountDetailsGroupBox";
			this.AccountDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 187, true);
			this.AccountDetailsGroupBox.TabIndex = 0;
			this.AccountDetailsGroupBox.TabStop = false;
			// 
			// mawbTextBox
			// 
			this.BindingSource.SetBindingMember(this.mawbTextBox, "AY_MasterBillNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaim)(null)).AY_MasterBillNumber)));
			this.mawbTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 38, true);
			this.mawbTextBox.Name = "mawbTextBox";
			this.mawbTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.mawbTextBox.TabIndex = 2;
			// 
			// ContactGuidDropEdit
			// 
			this.ContactGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContactGuidDropEdit, "AY_OC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccQueryClaim)(null)).AY_OC)));
			this.ContactGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 60, true);
			this.ContactGuidDropEdit.Name = "ContactGuidDropEdit";
			this.ContactGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.ContactGuidDropEdit.TabIndex = 3;
			// 
			// AmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountCalcEdit, "AY_QueryClaimAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccQueryClaim)(null)).AY_QueryClaimAmount)));
			this.AmountCalcEdit.DecimalPlaces = 2;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 82, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.AmountCalcEdit.TabIndex = 4;
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OrganisationGuidFindBox
			// 
			this.OrganisationGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OrganisationGuidFindBox, "AY_OH_Debtor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccQueryClaim)(null)).AY_OH_Debtor)));
			this.OrganisationGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.OrganisationGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 16, true);
			this.OrganisationGuidFindBox.Name = "OrganisationGuidFindBox";
			this.OrganisationGuidFindBox.ShouldResize = true;
			this.OrganisationGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 17, true);
			this.OrganisationGuidFindBox.TabIndex = 0;
			// 
			// InvoiceGuidFindBox
			// 
			this.InvoiceGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceGuidFindBox, "AY_AH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccQueryClaim)(null)).AY_AH)));
			this.InvoiceGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|3df63f32-348d-474f-9600-aa1fb23a51ee", "Invoice Number");
			this.InvoiceGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.InvoiceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 38, true);
			this.InvoiceGuidFindBox.Name = "InvoiceGuidFindBox";
			this.InvoiceGuidFindBox.ShouldResize = true;
			this.InvoiceGuidFindBox.ShowDescriptionBox = false;
			this.InvoiceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.InvoiceGuidFindBox.TabIndex = 1;
			// 
			// CurrencyTextBox
			// 
			this.BindingSource.SetBindingMember(this.CurrencyTextBox, "AY_RX_TransactionCurrencyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaim)(null)).AY_RX_TransactionCurrencyCode)));
			this.CurrencyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(207, 82, true);
			this.CurrencyTextBox.Name = "CurrencyTextBox";
			this.CurrencyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.CurrencyTextBox.TabIndex = 5;
			// 
			// ShortDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ShortDescriptionTextBox, "AY_ShortDescriptionOfClaim");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaim)(null)).AY_ShortDescriptionOfClaim)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.ShortDescriptionTextBox, 0);
			this.ShortDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 104, true);
			this.ShortDescriptionTextBox.Multiline = true;
			this.ShortDescriptionTextBox.Name = "ShortDescriptionTextBox";
			this.ShortDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(242, 74, true);
			this.ShortDescriptionTextBox.TabIndex = 6;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "AY_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccQueryClaim)(null)).AY_GB)));
			this.BranchGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 59, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.ShouldResize = true;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 17, true);
			this.BranchGuidFindBox.TabIndex = 2;
			// 
			// ClaimDetailsGroupBox
			// 
			this.ClaimDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|1b85cc83-bc46-4463-9e26-e492afe6997f", "Claim Details");
			this.ClaimDetailsGroupBox.Controls.Add(this.HoldOptionEdit);
			this.ClaimDetailsGroupBox.Controls.Add(this.CreatorCodeFindBox);
			this.ClaimDetailsGroupBox.Controls.Add(this.TypeDropEdit);
			this.ClaimDetailsGroupBox.Controls.Add(this.ReasonDropEdit);
			this.ClaimDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 201, true);
			this.ClaimDetailsGroupBox.Name = "ClaimDetailsGroupBox";
			this.ClaimDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 118, true);
			this.ClaimDetailsGroupBox.TabIndex = 1;
			this.ClaimDetailsGroupBox.TabStop = false;
			// 
			// HoldOptionEdit
			// 
			this.HoldOptionEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HoldOptionEdit, "AY_HoldOption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccQueryClaim)(null)).AY_HoldOption)));
			this.HoldOptionEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 87, true);
			this.HoldOptionEdit.Name = "HoldOptionEdit";
			this.HoldOptionEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.HoldOptionEdit.TabIndex = 3;
			// 
			// CreatorCodeFindBox
			// 
			this.CreatorCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreatorCodeFindBox, "AY_GS_NKCreator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaim)(null)).AY_GS_NKCreator)));
			this.CreatorCodeFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|22f4e3e6-8150-4c15-8650-576b39184e74", "Claim Creator", "Staff member who created this claim.");
			this.CreatorCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 61, true);
			this.CreatorCodeFindBox.Name = "CreatorCodeFindBox";
			this.CreatorCodeFindBox.ShouldResize = true;
			this.CreatorCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.CreatorCodeFindBox.TabIndex = 2;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.TypeDropEdit.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "AY_QueryClaimType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccQueryClaim)(null)).AY_QueryClaimType)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 15, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.TypeDropEdit.TabIndex = 0;
			// 
			// ReasonDropEdit
			// 
			this.ReasonDropEdit.AllowDrop = true;
			this.ReasonDropEdit.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReasonDropEdit, "AY_QueryClaimReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccQueryClaim)(null)).AY_QueryClaimReasonCode)));
			this.ReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 38, true);
			this.ReasonDropEdit.Name = "ReasonDropEdit";
			this.ReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 17, true);
			this.ReasonDropEdit.TabIndex = 1;
			// 
			// AY_QueryClaimNextFollowUpDateEdit
			// 
			this.AY_QueryClaimNextFollowUpDateEdit.AllowDrop = true;
			this.AY_QueryClaimNextFollowUpDateEdit.AutoCompleteMonthThreshold = 1;
			this.AY_QueryClaimNextFollowUpDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AY_QueryClaimNextFollowUpDateEdit, "AY_QueryClaimNextFollowUp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccQueryClaim)(null)).AY_QueryClaimNextFollowUp)));
			this.AY_QueryClaimNextFollowUpDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|4c73d51a-a2a3-4486-8fc4-566695740f23", "Next Follow Up");
			this.AY_QueryClaimNextFollowUpDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 81, true);
			this.AY_QueryClaimNextFollowUpDateEdit.Name = "AY_QueryClaimNextFollowUpDateEdit";
			this.AY_QueryClaimNextFollowUpDateEdit.TabIndex = 3;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "AY_QueryClaimStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccQueryClaim)(null)).AY_QueryClaimStatus)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 15, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 17, true);
			this.StatusDropEdit.TabIndex = 0;
			// 
			// ApproveClaimButton
			// 
			this.ApproveClaimButton.AutoSize = true;
			this.ApproveClaimButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|9e892404-2ad4-4085-a5e4-22c7c5e8c7ab", "Approve Claim");
			this.ApproveClaimButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 2, true);
			this.ApproveClaimButton.Name = "ApproveClaimButton";
			this.ApproveClaimButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApproveClaimButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 23, true);
			this.ApproveClaimButton.TabIndex = 3;
			this.ApproveClaimButton.ToolTipCaption = null;
			this.ApproveClaimButton.UseVisualStyleBackColor = true;
			this.ApproveClaimButton.Click += new EventHandler(this.ApproveClaimButton_Click);
			// 
			// RejectClaimButton
			// 
			this.RejectClaimButton.AutoSize = true;
			this.RejectClaimButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|4db90435-7d2a-4620-91c2-39386acef322", "Reject Claim");
			this.RejectClaimButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(237, 2, true);
			this.RejectClaimButton.Name = "RejectClaimButton";
			this.RejectClaimButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RejectClaimButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 23, true);
			this.RejectClaimButton.TabIndex = 2;
			this.RejectClaimButton.ToolTipCaption = null;
			this.RejectClaimButton.UseVisualStyleBackColor = true;
			this.RejectClaimButton.Click += new EventHandler(this.RejectClaimButton_Click);
			// 
			// ClaimStatusGroupBox
			// 
			this.ClaimStatusGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|03b289b1-0280-4269-b152-cbd5be75be53", "Claim Status");
			this.ClaimStatusGroupBox.Controls.Add(this.StaffCodeFindBox);
			this.ClaimStatusGroupBox.Controls.Add(this.BranchGuidFindBox);
			this.ClaimStatusGroupBox.Controls.Add(this.AY_QueryClaimNextFollowUpDateEdit);
			this.ClaimStatusGroupBox.Controls.Add(this.StatusDropEdit);
			this.ClaimStatusGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 325, true);
			this.ClaimStatusGroupBox.Name = "ClaimStatusGroupBox";
			this.ClaimStatusGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 109, true);
			this.ClaimStatusGroupBox.TabIndex = 2;
			this.ClaimStatusGroupBox.TabStop = false;
			// 
			// StaffCodeFindBox
			// 
			this.StaffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StaffCodeFindBox, "AY_GS_NKStaffAssignedTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((AccQueryClaim)(null)).AY_GS_NKStaffAssignedTo)));
			this.StaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 37, true);
			this.StaffCodeFindBox.Name = "StaffCodeFindBox";
			this.StaffCodeFindBox.ShouldResize = true;
			this.StaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 17, true);
			this.StaffCodeFindBox.TabIndex = 1;
			// 
			// IntercompanyClaimDetailsGroupBox
			// 
			this.IntercompanyClaimDetailsGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|7868b17e-dd2f-44e8-870b-05c7ba87c8b4", "Intercompany Claim Details");
			this.IntercompanyClaimDetailsGroupBox.Controls.Add(this.kFlowLayoutPanel1);
			this.IntercompanyClaimDetailsGroupBox.Controls.Add(this.TransactionBranchOrgProxyFindBox);
			this.IntercompanyClaimDetailsGroupBox.Controls.Add(this.TransactionBranchGuidFindBox);
			this.IntercompanyClaimDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 299, true);
			this.IntercompanyClaimDetailsGroupBox.Name = "IntercompanyClaimDetailsGroupBox";
			this.IntercompanyClaimDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 99, true);
			this.IntercompanyClaimDetailsGroupBox.TabIndex = 4;
			this.IntercompanyClaimDetailsGroupBox.TabStop = false;
			// 
			// kFlowLayoutPanel1
			// 
			this.kFlowLayoutPanel1.Controls.Add(this.ApproveClaimButton);
			this.kFlowLayoutPanel1.Controls.Add(this.RejectClaimButton);
			this.kFlowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			this.kFlowLayoutPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 65, true);
			this.kFlowLayoutPanel1.Name = "kFlowLayoutPanel1";
			this.kFlowLayoutPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(418, 30, true);
			this.kFlowLayoutPanel1.TabIndex = 4;
			// 
			// TransactionBranchOrgProxyFindBox
			// 
			this.TransactionBranchOrgProxyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionBranchOrgProxyFindBox, "AY_OH_TransactionBranchOrgProxy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccQueryClaim)(null)).AY_OH_TransactionBranchOrgProxy)));
			this.TransactionBranchOrgProxyFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|e404ce9e-7043-4538-ba6d-b92d46e92379", "Organization");
			this.TransactionBranchOrgProxyFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.TransactionBranchOrgProxyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 42, true);
			this.TransactionBranchOrgProxyFindBox.Name = "TransactionBranchOrgProxyFindBox";
			this.TransactionBranchOrgProxyFindBox.ShouldResize = true;
			this.TransactionBranchOrgProxyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.TransactionBranchOrgProxyFindBox.TabIndex = 1;
			// 
			// TransactionBranchGuidFindBox
			// 
			this.TransactionBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransactionBranchGuidFindBox, "AY_GB_TransactionBranch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((AccQueryClaim)(null)).AY_GB_TransactionBranch)));
			this.TransactionBranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("AccQueryClaimUserControl|d0fe6bae-4aec-4026-aeb9-4ef7ed1d14f5", "Branch");
			this.TransactionBranchGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.TransactionBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 19, true);
			this.TransactionBranchGuidFindBox.Name = "TransactionBranchGuidFindBox";
			this.TransactionBranchGuidFindBox.ShouldResize = true;
			this.TransactionBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.TransactionBranchGuidFindBox.TabIndex = 0;
			// 
			// AccQueryClaimUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ClaimStatusGroupBox);
			this.Controls.Add(this.IntercompanyClaimDetailsGroupBox);
			this.Controls.Add(this.ClaimDetailsGroupBox);
			this.Controls.Add(this.AccountDetailsGroupBox);
			this.Controls.Add(this.DetailsGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 449, true);
			this.Name = "AccQueryClaimUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.AccountDetailsGroupBox.ResumeLayout(false);
			this.AccountDetailsGroupBox.PerformLayout();
			this.ContactGuidDropEdit.ResumeLayout(true);
			this.ContactGuidDropEdit.PerformLayout();
			this.OrganisationGuidFindBox.ResumeLayout(true);
			this.OrganisationGuidFindBox.PerformLayout();
			this.InvoiceGuidFindBox.ResumeLayout(true);
			this.InvoiceGuidFindBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.ClaimDetailsGroupBox.ResumeLayout(false);
			this.ClaimDetailsGroupBox.PerformLayout();
			this.HoldOptionEdit.ResumeLayout(true);
			this.HoldOptionEdit.PerformLayout();
			this.CreatorCodeFindBox.ResumeLayout(true);
			this.CreatorCodeFindBox.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.ReasonDropEdit.ResumeLayout(true);
			this.ReasonDropEdit.PerformLayout();
			this.AY_QueryClaimNextFollowUpDateEdit.ResumeLayout(true);
			this.AY_QueryClaimNextFollowUpDateEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ClaimStatusGroupBox.ResumeLayout(false);
			this.ClaimStatusGroupBox.PerformLayout();
			this.StaffCodeFindBox.ResumeLayout(true);
			this.StaffCodeFindBox.PerformLayout();
			this.IntercompanyClaimDetailsGroupBox.ResumeLayout(false);
			this.IntercompanyClaimDetailsGroupBox.PerformLayout();
			this.kFlowLayoutPanel1.ResumeLayout(false);
			this.kFlowLayoutPanel1.PerformLayout();
			this.TransactionBranchOrgProxyFindBox.ResumeLayout(true);
			this.TransactionBranchOrgProxyFindBox.PerformLayout();
			this.TransactionBranchGuidFindBox.ResumeLayout(true);
			this.TransactionBranchGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
