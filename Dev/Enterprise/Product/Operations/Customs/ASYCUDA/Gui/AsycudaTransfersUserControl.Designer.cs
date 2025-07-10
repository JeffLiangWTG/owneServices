using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaTransfersUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.splitter = new CargoWise.Windows.UI.KSplitContainer();
			this.transfersGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transferHeadersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.transferDetailsUserControl = new Enterprise.Customs.ASYCUDA.GUI.AsycudaTransferDetailsUserControl();
			this.transferDetailsSpecificPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitter)).BeginInit();
			this.splitter.Panel1.SuspendLayout();
			this.splitter.Panel2.SuspendLayout();
			this.splitter.SuspendLayout();
			this.transfersGroupbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.transferHeadersGrid)).BeginInit();
			this.transferHeadersGrid.SuspendLayout();
			this.transferDetailsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader);
			// 
			// splitter
			// 
			this.splitter.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitter.Name = "splitter";
			this.splitter.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitter.Panel1
			// 
			this.splitter.Panel1.Controls.Add(this.transfersGroupbox);
			this.splitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 553, true);
			this.splitter.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(80);
			// 
			// splitter.Panel2
			// 
			this.splitter.Panel2.Controls.Add(this.transferDetailsUserControl);
			this.splitter.Panel2.Controls.Add(this.transferDetailsSpecificPanel);
			this.splitter.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
			this.splitter.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(180);
			this.splitter.TabIndex = 2;
			// 
			// transfersGroupbox
			// 
			this.transfersGroupbox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("42F8B808-9309-4003-9716-7B9827301D6F", "Transfers");
			this.transfersGroupbox.Controls.Add(this.transferHeadersGrid);
			this.transfersGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transfersGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.transfersGroupbox.Name = "transfersGroupbox";
			this.transfersGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 120, true);
			this.transfersGroupbox.TabIndex = 0;
			this.transfersGroupbox.TabStop = false;
			// 
			// transferHeadersGrid
			// 
			this.transferHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.transferHeadersGrid, "TransferHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_RL_NKDestinationPortCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_TransferType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).InBondCarrierOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_OA_Carrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_CarrierID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_OnwardCarrier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).DestinationWarehouseOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_OA_DestinationWarehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaTransferHeader)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)).SyncRoot)).ATF_DestinationWarehouseID)));
			this.transferHeadersGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "ATF_RL_NKDestinationPortCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "ATF_TransferType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(76);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "InBondCarrierOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("Enterprise.Customs.ASYCUDA.GUI.TransfersUserControl|In-Bond Carrier|GroupName", "In-Bond Carrier");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "ATF_OA_Carrier";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("Enterprise.Customs.ASYCUDA.GUI.TransfersUserControl|In-Bond Carrier|GroupName", "In-Bond Carrier");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ATF_CarrierID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "ATF_OnwardCarrier";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "DestinationWarehouseOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("Enterprise.Customs.ASYCUDA.GUI.TransfersUserControl|Bonded Premises|GroupName", "Bonded Premises");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "ATF_OA_DestinationWarehouse";
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("Enterprise.Customs.ASYCUDA.GUI.TransfersUserControl|Bonded Premises|GroupName", "Bonded Premises");
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "ATF_DestinationWarehouseID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.transferHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.transferHeadersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.transferHeadersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.transferHeadersGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.transferHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.transferHeadersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.transferHeadersGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.transferHeadersGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.transferHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.transferHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transferHeadersGrid.GridId = "FE0BECFA-3023-4E55-8939-D4194ED34EE0";
			this.transferHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.transferHeadersGrid.LayoutKey = "transferHeadersGrid";
			this.transferHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.transferHeadersGrid.Name = "transferHeadersGrid";
			this.transferHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 101, true);
			this.transferHeadersGrid.TabIndex = 0;
			// 
			// transferDetailsUserControl
			// 
			this.transferDetailsUserControl.AllowDrop = true;
			this.transferDetailsUserControl.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 0, true);
			this.BindingSource.SetBindingMember(this.transferDetailsUserControl, "TransferHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.AsycudaArrivalHeader)(null)).TransferHeaders)));
			this.transferDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.transferDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.transferDetailsUserControl.Name = "transferDetailsUserControl";
			this.transferDetailsUserControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.transferDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 395, true);
			this.transferDetailsUserControl.TabIndex = 0;
			// 
			// transferDetailsSpecificPanel
			// 
			this.transferDetailsSpecificPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.transferDetailsSpecificPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 395, true);
			this.transferDetailsSpecificPanel.Name = "transferDetailsSpecificPanel";
			this.transferDetailsSpecificPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 34, true);
			this.transferDetailsSpecificPanel.TabIndex = 1;
			// 
			// AsycudaTransfersUserControl
			// 
			this.Controls.Add(this.splitter);
			this.Name = "AsycudaTransfersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1014, 553, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitter.Panel1.ResumeLayout(false);
			this.splitter.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitter)).EndInit();
			this.splitter.ResumeLayout(false);
			this.splitter.PerformLayout();
			this.transfersGroupbox.ResumeLayout(false);
			this.transfersGroupbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.transferHeadersGrid)).EndInit();
			this.transferHeadersGrid.ResumeLayout(false);
			this.transferHeadersGrid.PerformLayout();
			this.transferDetailsUserControl.ResumeLayout(true);
			this.transferDetailsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox transfersGroupbox;
		private ZGrid transferHeadersGrid;
		private CargoWise.Windows.UI.KSplitContainer splitter;
		private AsycudaTransferDetailsUserControl transferDetailsUserControl;
		private ZPanel transferDetailsSpecificPanel;
	}
}
