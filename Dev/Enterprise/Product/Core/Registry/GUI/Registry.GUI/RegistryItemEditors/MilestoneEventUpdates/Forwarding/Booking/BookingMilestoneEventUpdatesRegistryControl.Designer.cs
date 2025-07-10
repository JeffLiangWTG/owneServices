using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class BookingMilestoneEventUpdatesRegistryControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MilestoneEventUpdatesGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9441ad51-75d7-43a2-9f3b-1d817cb5e90b", "Consignee");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsConsigneeUpdateAllowed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bcdca3a3-9e53-45f2-9251-632a1194dbec", "Delivery");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsDeliveryAgentUpdateAllowed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("25fa8114-2baa-4d3a-b484-c3d33e61f563", "Export Broker");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsExportBrokerUpdateAllowed";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0f4d783a-fd11-48fc-9756-46edda73c67d", "Import Broker");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsImportBrokerUpdateAllowed";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c2d45ab1-ed23-498b-8a38-9c8c50daa9b3", "Local Client");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsLocalClientUpdateAllowed";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a7f22209-c388-48d0-b814-5e4344515901", "Receiving Agent");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsReceivingAgentUpdateAllowed";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("bfaad8f3-0765-4310-8fd1-e9a33bd828ec", "Sending Agent");
			zCheckBoxColumnStyleInfo7.ColumnName = "IsSendingAgentUpdateAllowed";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("30a68ed5-6025-4694-826e-baa62f23f127", "Shipper");
			zCheckBoxColumnStyleInfo8.ColumnName = "IsShipperUpdateAllowed";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.BookingMilestoneEventUpdates);
			// 
			// BookingMilestoneEventUpdatesRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Name = "BookingMilestoneEventUpdatesRegistryControl";
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

	}
}
