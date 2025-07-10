namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	partial class CustomSupportIncidentUserControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.IncidentGrid = new ZArchitecture.GUI.ZDisplayGrid();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.IncidentGrid)).BeginInit();
			this.IncidentGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// IncidentGrid
			// 
			this.IncidentGrid.AllowBeginDrag = false;
			this.IncidentGrid.AllowDragDropWithChanges = false;
			this.IncidentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.IncidentGrid, ".");
			zCheckBoxColumnStyleInfo1.Caption = " ";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsChecked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(25);
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("f16f1fb8-0131-441c-95be-fc75df2cd108", "Organization");
			zTextBoxColumnStyleInfo1.ColumnName = "ClientCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("114e1084-178d-4ae5-854a-96f58afcd836", "Contact");
			zTextBoxColumnStyleInfo2.ColumnName = "ContactName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("643c7a48-bfc4-47b2-bc23-0c625e76eb2c", "Incident Number");
			zTextBoxColumnStyleInfo3.ColumnName = "IncidentNumber";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("c5732365-16ef-4d42-80f6-b5892ad0b102", "Incident Summary");
			zTextBoxColumnStyleInfo4.ColumnName = "Description";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("5fe80192-8544-40ed-a7c1-f1104b04e076", "Product");
			zTextBoxColumnStyleInfo5.ColumnName = "Product";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("a58819e0-24ab-4808-993a-9d5281e870f3", "Product Area");
			zTextBoxColumnStyleInfo6.ColumnName = "ProductArea";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("f032e24b-76f8-4778-bb74-3b23d662f4d1", "Criticality");
			zTextBoxColumnStyleInfo7.ColumnName = "Priority";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			this.IncidentGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.IncidentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.IncidentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.IncidentGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.IncidentGrid.TabIndex = 3;
			this.IncidentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IncidentGrid.ReadOnly = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CustomSupportIncident);
			// 
			// CustomSupportIncidentUserControl
			// 
			this.Name = "CustomSupportIncidentUserControl";
			this.Controls.Add(IncidentGrid);
			((System.ComponentModel.ISupportInitialize)(this.IncidentGrid)).EndInit();
			this.IncidentGrid.ResumeLayout(false);
			this.IncidentGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected ZArchitecture.GUI.ZDisplayGrid IncidentGrid;

		#endregion
	}
}
