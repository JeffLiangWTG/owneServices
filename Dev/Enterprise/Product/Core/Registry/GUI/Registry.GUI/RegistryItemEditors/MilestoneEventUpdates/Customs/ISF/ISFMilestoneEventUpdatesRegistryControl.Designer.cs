using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class ISFMilestoneEventUpdatesRegistryControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MilestoneEventUpdatesGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("132fb7a8-0dae-4e3a-af58-062a99c690d9", "Booking Party");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsBookingPartyUpdateAllowed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("96f6dd18-8c6b-4635-958e-5fab60e74e6d", "Buying Party");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsBuyingPartyUpdateAllowed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0566f308-ac73-469f-b7ad-58cf180a75d6", "Consolidator");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsConsolidatorUpdateAllowed";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("540a12bd-3d86-4c14-8236-481b38b85494", "Importer");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsImporterUpdateAllowed";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d163336e-3926-4ba4-aba4-84da3a94affc", "Manufacturer");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsManufacturerUpdateAllowed";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a7825fc2-120f-4035-9693-2213deec6c07", "Selling Party");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsSellingPartyUpdateAllowed";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("70c95476-202b-48e1-a2a5-e0fe47b2dd1b", "Sending Agent");
			zCheckBoxColumnStyleInfo7.ColumnName = "IsSendingAgentUpdateAllowed";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("652c09a6-3c07-4d84-aa24-2d6d0ae6441a", "Ship To Location");
			zCheckBoxColumnStyleInfo8.ColumnName = "IsShipToLocationUpdateAllowed";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ec8b2e73-93a2-401b-b863-050c73a47bd4", "Stuffing Location");
			zCheckBoxColumnStyleInfo9.ColumnName = "IsStuffingLocationUpdateAllowed";
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ISFMilestoneEventUpdates);
			// 
			// ISFMilestoneEventUpdatesRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Name = "ISFMilestoneEventUpdatesRegistryControl";
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

	}
}
