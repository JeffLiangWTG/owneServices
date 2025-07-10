using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class TradeChainPartnerSendingMessageForm
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
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TCPGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TCPGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TCPGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TCPGrid)).BeginInit();
			this.TCPGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 312, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.MessageManagers.TradeChainPartnerMessageManager);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(770, 283, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|E3B150E8-4675-407E-9BB5-4DE1A3601866", "Send");
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(851, 283, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 4;
			this.CancelButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|32430B27-C325-42C1-99DB-499CA9189CEB", "Cancel");
			this.CancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// TCPGroupBox
			// 
			this.TCPGroupBox.Controls.Add(this.TCPGrid);
			this.TCPGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.TCPGroupBox.Name = "TCPGroupBox";
			this.TCPGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 265, true);
			this.TCPGroupBox.TabIndex = 1;
			this.TCPGroupBox.TabStop = false;
			this.TCPGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|A4D541C0-DD5F-42B2-9FE5-C199F1F706D0", "Trade Chain Partners");
			// 
			// TCPGrid
			// 
			this.TCPGrid.AllowNavigation = false;
			this.TCPGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.TCPGrid, "TCPCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.MessageManagers.TradeChainPartnerMessageManager)(null)).TCPCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).CA_Org)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).TradeChainPartner.AddInfoLookups.ImportersList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).CA_Address)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).TradeChainPartner.CAOrgAddress.Lookups.SelectedOrganisationAddresses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).CA_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).CA_CSAIDType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).CA_CSAID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.TradeChainPartnerSendingObject)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).Parent)).SyncRoot)).CA_Action)));
			this.TCPGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "SendOption";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|DDE537B1-3299-445E-9BEB-1860C6C77618", "Send ?");
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CA_Org";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|9F2B85A9-18F2-42E0-8270-F44AAE80A390", "Organization");
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "TradeChainPartner+AddInfoLookups+ImportersList";
			zOrganisationFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(113);
			zGuidDropEditColumnStyleInfo1.ColumnName = "CA_Address";
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|A62748CC-F797-42CB-A58E-B506FDE2B013", "Address");
			zGuidDropEditColumnStyleInfo1.BindToList = "TradeChainPartner+CAOrgAddress.Lookups+SelectedOrganisationAddresses";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
			zDropEditColumnStyleInfo1.ColumnName = "CA_Type";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|FC057DF6-6CBD-4CE6-AC76-4C145A756858", "Type");
			zDropEditColumnStyleInfo1.BindToList = "TradeChainPartner+AddInfoLookups+TradeChainPartnersTypeList";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo2.ColumnName = "CA_CSAIDType";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|329D5F83-4681-4513-82CA-FBAF2AE447CD", "CSA ID Type");
			zDropEditColumnStyleInfo2.BindToList = "TradeChainPartner+AddInfoLookups+CSAIDTypeList";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.ColumnName = "CA_CSAID";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|990DE566-1745-4F03-9DCE-50DA4B49D18F", "CSA ID");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo4.ColumnName = "CA_Action";
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("TradeChainPartnerSendingMessageForm|00EC3CE2-9951-4FB4-8683-3548B025DC8E", "Action");
			zDropEditColumnStyleInfo4.BindToList = "TradeChainPartner+AddInfoLookups+CSAActionTypeList";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.TCPGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TCPGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.TCPGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.TCPGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TCPGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TCPGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TCPGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.TCPGrid.GridId = "ABDC4853-B90E-4F14-A1D9-3E3EE3276217";
			this.TCPGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TCPGrid.LayoutKey = "TCPGrid";
			this.TCPGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TCPGrid.Name = "TCPGrid";
			this.TCPGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(905, 243, true);
			this.TCPGrid.TabIndex = 2;
			// 
			// TradeChainPartnerSendingMessageForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 336, true);
			this.Controls.Add(this.TCPGroupBox);
			this.Controls.Add(this.SendButton);
			this.Controls.Add(this.CancelButton);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			this.Name = "TradeChainPartnerSendingMessageForm";
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.TCPGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TCPGroupBox.ResumeLayout(false);
			this.TCPGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TCPGrid)).EndInit();
			this.TCPGrid.ResumeLayout(false);
			this.TCPGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private new ZButton CancelButton;
		public ZButton SendButton;
		private ZGroupBox TCPGroupBox;
		private ZGrid TCPGrid;
	}
}
