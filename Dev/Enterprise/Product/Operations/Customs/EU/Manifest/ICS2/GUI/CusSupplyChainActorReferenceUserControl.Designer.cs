using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class CusSupplyChainActorReferenceUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.supplyChainActorReferenceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.supplyChainActorReferenceGrid)).BeginInit();
			this.supplyChainActorReferenceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader);
			// 
			// supplyChainActorReferenceGrid
			// 
			this.supplyChainActorReferenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.supplyChainActorReferenceGrid, "CusSupplyChainActorReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.supplyChainActorReferenceGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.ColumnName = "OwnerOrgPK";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zAddressDropEditColumnStyleInfo1.ColumnName = "CFR_OA_Owner";
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.supplyChainActorReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.supplyChainActorReferenceGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.supplyChainActorReferenceGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.supplyChainActorReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.supplyChainActorReferenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supplyChainActorReferenceGrid.GridId = "a831d8d9-1040-47dc-abb3-6f98b64af920";
			this.supplyChainActorReferenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.supplyChainActorReferenceGrid.LayoutKey = "supplyChainActorReferenceGrid";
			this.supplyChainActorReferenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supplyChainActorReferenceGrid.Name = "supplyChainActorReferenceGrid";
			this.supplyChainActorReferenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 80, true);
			this.supplyChainActorReferenceGrid.TabIndex = 0;
			// 
			// CusSupplyChainActorReferenceUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.supplyChainActorReferenceGrid);
			this.Name = "CusSupplyChainActorReferenceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.supplyChainActorReferenceGrid)).EndInit();
			this.supplyChainActorReferenceGrid.ResumeLayout(false);
			this.supplyChainActorReferenceGrid.PerformLayout();
			this.ResumeLayout(false);

		}
		ZGrid supplyChainActorReferenceGrid;

		#endregion
	}
}
