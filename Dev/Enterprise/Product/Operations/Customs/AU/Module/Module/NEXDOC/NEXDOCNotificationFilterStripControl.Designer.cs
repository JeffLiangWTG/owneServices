namespace Enterprise.Customs.AU.Module.NEXDOC
{
	partial class NEXDOCNotificationFilterStripControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNRexNumber = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNExporterReference = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNNotificationType = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNAcknowledgeStatus = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo QNReceivalDate = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNMessageStatus = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNSystemCreateUser = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNForwardingGroupID = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNReceivingExporterID = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNRexStatus = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo QNTransferringExporterID = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			QNRexNumber.Caption = "REX Number";
			QNRexNumber.ColumnName = "QN_RexNumber";
			QNExporterReference.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNExporterReference.Caption = "Exporter Reference";
			QNExporterReference.ColumnName = "QN_ExporterReference";
			QNExporterReference.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			QNNotificationType.Caption = "Notification Type";
			QNNotificationType.ColumnName = "QN_NotificationType";
			QNNotificationType.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNAcknowledgeStatus.Caption = "Acknowledge Status";
			QNAcknowledgeStatus.ColumnName = "QN_AcknowledgeStatus";
			QNAcknowledgeStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNReceivalDate.Caption = "Received Date";
			QNReceivalDate.ColumnName = "QN_ReceivedDate";
			QNReceivalDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNMessageStatus.Caption = "Message Status";
			QNMessageStatus.ColumnName = "QN_MessageStatus";
			QNMessageStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNSystemCreateUser.Caption = "Created By";
			QNSystemCreateUser.ColumnName = "QN_SystemCreateUser";
			QNSystemCreateUser.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNSystemCreateUser.IsVisible = false;
			QNForwardingGroupID.Caption = "Forwarding Group ID";
			QNForwardingGroupID.ColumnName = "QN_ForwardingGroupID";
			QNForwardingGroupID.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNReceivingExporterID.Caption = "Receiving Exporter ID";
			QNReceivingExporterID.ColumnName = "QN_ReceivingExporterID";
			QNReceivingExporterID.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNRexStatus.Caption = "REX Status";
			QNRexStatus.ColumnName = "QN_RexStatus";
			QNRexStatus.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			QNTransferringExporterID.Caption = "Transferring Exporter ID";
			QNTransferringExporterID.ColumnName = "QN_TransferringExporterID";
			QNTransferringExporterID.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.FilteredGrid.ColumnStyles.Add(QNRexNumber);
			this.FilteredGrid.ColumnStyles.Add(QNExporterReference);
			this.FilteredGrid.ColumnStyles.Add(QNNotificationType);
			this.FilteredGrid.ColumnStyles.Add(QNAcknowledgeStatus);
			this.FilteredGrid.ColumnStyles.Add(QNMessageStatus);
			this.FilteredGrid.ColumnStyles.Add(QNReceivalDate);
			this.FilteredGrid.ColumnStyles.Add(QNSystemCreateUser);
			this.FilteredGrid.ColumnStyles.Add(QNForwardingGroupID);
			this.FilteredGrid.ColumnStyles.Add(QNReceivingExporterID);
			this.FilteredGrid.ColumnStyles.Add(QNRexStatus);
			this.FilteredGrid.ColumnStyles.Add(QNTransferringExporterID);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 264, true);
			this.FilteredGrid.TabIndex = 5;
			// 
			// NEXDOCNotificationFilterControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Business.NEXDOCS.QuarantineNexDocNotification";
			this.Name = "NEXDOCSNotificationFilterStripControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
