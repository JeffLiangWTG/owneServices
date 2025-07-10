using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.GUI.Matching
{
	partial class SettlementOrganisationsForm
	{
		public SettlementOrganisationsForm(SettlementOrganisation settlementOrgBizO)
			: base(settlementOrgBizO)
		{
			InitializeComponent();
			fSettlementOrganisation = settlementOrgBizO;
			HookEvents();
			ZFormPostingButtonsStrategy.SetupPosting(this, null, CloseButton, null);
			zGrid1.ReadOnly = !fSettlementOrganisation.IsPrimaryOrgValidAndNonEmpty;
		}

		void HookEvents()
		{
			fSettlementOrganisation.PrimaryOrganizationInfo.ValueChanged += PrimaryOrganizationInfo_ValueChanged;
		}

		void PrimaryOrganizationInfo_ValueChanged(object sender, EventArgs e)
		{
			zGrid1.ReadOnly = !fSettlementOrganisation.IsPrimaryOrgValidAndNonEmpty;
		}

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
		private new void InitializeComponent()
		{
            Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.IncludeAllARTransactions = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.IncludeAllAPTransactionsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.zPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
            this.zGrid1.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 294, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 19, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation);
            // 
            // zPanel1
            // 
            this.zPanel1.Controls.Add(this.IncludeAllARTransactions);
            this.zPanel1.Controls.Add(this.IncludeAllAPTransactionsCheckBox);
            this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zPanel1.Name = "zPanel1";
            this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 38, true);
            this.zPanel1.TabIndex = 1;
            // 
            // IncludeAllARTransactions
            // 
            this.BindingSource.SetBindingMember(this.IncludeAllARTransactions, "MatchingFilterBizO.IncludeAllAR");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation)(null)).MatchingFilterBizO.IncludeAllAR)));
            this.IncludeAllARTransactions.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("SettlementOrganisationsForm|1d7a07a5-e61b-4e08-a83c-75fbc90b08e7", "Include all AR", "Include all AR", "");
            this.IncludeAllARTransactions.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 8, true);
            this.IncludeAllARTransactions.Name = "IncludeAllARTransactions";
            this.IncludeAllARTransactions.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 21, true);
            this.IncludeAllARTransactions.TabIndex = 2;
			// 
			// IncludeAllAPTransactionsCheckBox
			// 
			this.IncludeAllAPTransactionsCheckBox.BackColor = System.Drawing.SystemColors.Info;
            this.BindingSource.SetBindingMember(this.IncludeAllAPTransactionsCheckBox, "MatchingFilterBizO.IncludeAllAP");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation)(null)).MatchingFilterBizO.IncludeAllAP)));
            this.IncludeAllAPTransactionsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("SettlementOrganisationsForm|08ec4812-5d8c-4066-be2e-0b7a661c87e5", "Include all AP", "Include all AP", "");
            this.IncludeAllAPTransactionsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 9, true);
            this.IncludeAllAPTransactionsCheckBox.Name = "IncludeAllAPTransactionsCheckBox";
            this.IncludeAllAPTransactionsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 19, true);
            this.IncludeAllAPTransactionsCheckBox.TabIndex = 3;
            this.IncludeAllAPTransactionsCheckBox.UseVisualStyleBackColor = false;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.IsCaptionOverridden = true;
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(220, 267, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
            this.CloseButton.TabIndex = 5;
            this.CloseButton.ToolTipCaption = null;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			this.CloseButton.Text = Res.GetString("SettlementOrganisationsForm|1b615a27-ba19-4d80-bf4e-ae98d4bb5073", "Close");
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
            this.zGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.zGrid1, "MatchingFilterBizO+SettlementOrgInfos");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation)(null)).MatchingFilterBizO.SettlementOrgInfos)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.Base.Filters.OrgLedgerFilter)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation)(null)).MatchingFilterBizO.SettlementOrgInfos)).SyncRoot)).Organization)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Filters.OrgLedgerFilter)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation)(null)).MatchingFilterBizO.SettlementOrgInfos)).SyncRoot)).APLedger)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.Base.Filters.OrgLedgerFilter)(((System.Collections.IList)(((Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation)(null)).MatchingFilterBizO.SettlementOrgInfos)).SyncRoot)).ARLedger)));
            this.zGrid1.CaptionVisible = false;
            zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("SettlementOrganisationsForm|67bc5376-5530-44d2-a598-766a6b6676ee", "Org.", "Organization");
            zGuidFindBoxColumnStyleInfo2.ColumnName = "Organization";
            zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
            zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("SettlementOrganisationsForm|12b173a1-12a1-4849-a4e4-a4cf6d708672", "AP");
            zCheckBoxColumnStyleInfo3.ColumnName = "APLedger";
            zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("SettlementOrganisationsForm|14a04126-afde-4480-977e-66a2685de1b8", "AR");
            zCheckBoxColumnStyleInfo4.ColumnName = "ARLedger";
            zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            this.zGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
            this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
            this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
            this.zGrid1.GridId = "16436337-b222-4b7e-a72c-a11452c931c0";
            this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGrid1.LayoutKey = "zGrid1";
            this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
            this.zGrid1.Name = "zGrid1";
            this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 218, true);
            this.zGrid1.TabIndex = 4;
			// 
			// SettlementOrganisationsForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 314, true);
            this.Controls.Add(this.zGrid1);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.zPanel1);
            this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
            this.DataSourceType = typeof(Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation);
            this.DataSourceTypeName = "Enterprise.Accounting.Business.Base.Matching.SettlementOrganisation";
            this.Name = "SettlementOrganisationsForm";
			this.Text = Res.GetString("1ac9ec84-1bff-4b44-af4e-3c17b154eb67", "Settlement Organizations");
			this.Controls.SetChildIndex(this.zPanel1, 0);
            this.Controls.SetChildIndex(this.CloseButton, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.zGrid1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.zPanel1.ResumeLayout(false);
            this.zPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
            this.zGrid1.ResumeLayout(false);
            this.zGrid1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		SettlementOrganisation fSettlementOrganisation;
        private ZArchitecture.GUI.ZPanel zPanel1;
        private ZArchitecture.GUI.ZCheckBox IncludeAllAPTransactionsCheckBox;
        private ZArchitecture.GUI.ZCheckBox IncludeAllARTransactions;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
        private ZGrid zGrid1;
    }
}
