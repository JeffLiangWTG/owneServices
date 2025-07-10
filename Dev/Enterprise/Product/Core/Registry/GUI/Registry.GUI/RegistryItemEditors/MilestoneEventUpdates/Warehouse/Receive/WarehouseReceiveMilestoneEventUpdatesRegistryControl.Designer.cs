using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class WarehouseReceiveMilestoneEventUpdatesRegistryControl
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
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MilestoneEventUpdatesGrid
			// 
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("79fc8676-817f-4cb9-b592-c57b117bfed3", "Client");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsClientUpdateAllowed";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("25e6777c-b47e-4db7-a1b4-bf056900fa85", "Supplier");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsSupplierUpdateAllowed";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0ddb2d1c-a9c2-4e08-a710-eadbe967b086", "Transport");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsTransportUpdateAllowed";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MilestoneEventUpdatesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.WarehouseReceiveMilestoneEventUpdates);
			// 
			// WarehouseReceiveMilestoneEventUpdatesRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.Name = "WarehouseReceiveMilestoneEventUpdatesRegistryControl";
			((System.ComponentModel.ISupportInitialize)(this.MilestoneEventUpdatesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

	}
}
