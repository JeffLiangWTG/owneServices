using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class DeclarationMilestoneEventUpdatesRegistryControl
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
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MilestoneEventUpdatesGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("db4eb72d-d204-4bf5-a6f0-f7548deda5a6", "Carrier");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsCarrierUpdateAllowed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("f18a109b-be6d-4e14-a32b-3b225e73f18d", "External Broker");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsExternalBrokerUpdateAllowed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("de46b9a4-7d7c-4c8f-ad2f-93c7ddc232a5", "Forwarder");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsForwarderUpdateAllowed";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("80ce50cd-0284-470d-92ca-3cf72e869213", "Importer");
			zCheckBoxColumnStyleInfo4.ColumnName = "IsImporterUpdateAllowed";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b7cc4de3-b606-4688-9fda-f6709842e2cd", "Supplier");
			zCheckBoxColumnStyleInfo5.ColumnName = "IsSupplierUpdateAllowed";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("025b2d75-5852-4f1b-b610-ad7fe79700ca", "Ultimate Consignee");
			zCheckBoxColumnStyleInfo6.ColumnName = "IsUltimateConsigneeUpdateAllowed";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DeclarationMilestoneEventUpdates);
			// 
			// DeclarationMilestoneEventUpdatesRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Name = "DeclarationMilestoneEventUpdatesRegistryControl";
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

	}
}
