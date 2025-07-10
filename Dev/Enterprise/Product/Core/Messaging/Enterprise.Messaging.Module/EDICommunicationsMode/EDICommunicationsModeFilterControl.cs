using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.Module
{
	public class EDICommunicationsModeFilterControl : ZFilterStripControl
	{
		public EDICommunicationsModeFilterControl(EDICommunicationsModeCollection gridCollection, EDICommunicationsModeFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			var zDropEditColumnStyleInfo5 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo6 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo7 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo8 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo9 = new ZDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			var zOrganisationFindBoxColumnStyleInfo1 = new ZOrganisationFindBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			var zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			var zDropEditColumnStyleInfo10 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo11 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo12 = new ZDropEditColumnStyleInfo();
			var zDropEditColumnStyleInfo13 = new ZDropEditColumnStyleInfo();
			var zTextBoxColumnStyleInfo10 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			var zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			var zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			//
			zDropEditColumnStyleInfo5.ColumnName = "EK_Module";
			zDropEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo6.ColumnName = "EK_CommsDirection";
			zDropEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDropEditColumnStyleInfo7.ColumnName = "EK_FileFormat";
			zDropEditColumnStyleInfo7.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(63);
			zDropEditColumnStyleInfo8.ColumnName = "EK_MessagePurpose";
			zDropEditColumnStyleInfo8.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo8.IsVisible = false;
			zDropEditColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zDropEditColumnStyleInfo9.ColumnName = "EK_CommunicationsTransport";
			zDropEditColumnStyleInfo9.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo2.ColumnName = "EK_ServerAddressSubject";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "EK_Filename";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "EK_Destination";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(238);
			zTextBoxColumnStyleInfo5.ColumnName = "EK_LocalPartyVanID";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "EK_RelatedPartyVanID";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "EK_OH_MessageVAN";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo7.ColumnName = "EK_FtpLockingMethod";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Messaging.Module.Res.GetData("EDICommunicationsModeFilterControl|277c44f5-4ed8-479a-89fb-24cea04d66a6", "FTP");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "EK_PortNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Messaging.Module.Res.GetData("EDICommunicationsModeFilterControl|277c44f5-4ed8-479a-89fb-24cea04d66a6", "FTP");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo8.ColumnName = "EK_LoginName";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Messaging.Module.Res.GetData("EDICommunicationsModeFilterControl|277c44f5-4ed8-479a-89fb-24cea04d66a6", "FTP");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.ColumnName = "EK_Password";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Messaging.Module.Res.GetData("EDICommunicationsModeFilterControl|277c44f5-4ed8-479a-89fb-24cea04d66a6", "FTP");
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo10.ColumnName = "EK_TransportMode";
			zDropEditColumnStyleInfo10.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo10.IsVisible = false;
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.ColumnName = "EK_RecipientRole";
			zDropEditColumnStyleInfo11.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo11.IsVisible = false;
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.ColumnName = "EK_EventCode";
			zDropEditColumnStyleInfo12.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo12.IsVisible = false;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo13.ColumnName = "EK_EventReferenceConditionType";
			zDropEditColumnStyleInfo13.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo13.GroupName = Enterprise.Messaging.Module.Res.GetData("eb80aaa9-cf6c-4750-aa0b-5589ec7a9ce7", "Event Reference Condition");
			zDropEditColumnStyleInfo13.IsVisible = false;
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo10.ColumnName = "EK_EventReferenceConditionValue";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Messaging.Module.Res.GetData("eb80aaa9-cf6c-4750-aa0b-5589ec7a9ce7", "Event Reference Condition");
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Messaging.Module.Res.GetData("EDICommunicationsModeFilterControl|A3A6324B-DDAB-4CB7-8877-A29EC2423E2F", "Organization Code");
			zTextBoxColumnStyleInfo11.ColumnName = "EK_OH_Code";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Messaging.Module.Res.GetData("EDICommunicationsModeFilterControl|4BC7E2FB-BB21-4F1D-8CE8-BAA39CC18E40", "Organization Name");
			zTextBoxColumnStyleInfo12.ColumnName = "EK_OH_FullName";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);

			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Messaging.Module.Res.GetData("D07AD016-809E-4760-9424-A63500707256", "EDI Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CommunicationParty";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Messaging.EDICommunicationParty;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 331, true);
			this.grid.TabIndex = 10;
			// 
			// AddStripButton
			// 
			this.AddStripButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(591, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EDICommunicationsMode);
			// 
			// EDICommunicationsModeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "EDICommunicationsModeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(641, 334, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
