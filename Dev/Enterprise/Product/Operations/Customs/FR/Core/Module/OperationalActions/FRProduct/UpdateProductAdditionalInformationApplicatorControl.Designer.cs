namespace Enterprise.Customs.FR.Module
{
	partial class UpdateProductAdditionalInformationApplicatorControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.ProductPendingUpdateDataGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductPendingUpdateDataGrid)).BeginInit();
			this.ProductPendingUpdateDataGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.OperationalActions.UpdateProductAdditionalInformationApplicator);
			// 
			// ProductPendingUpdateDataGrid
			// 
			this.ProductPendingUpdateDataGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductPendingUpdateDataGrid, "ProductPendingUpdateDataForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.UpdateProductAdditionalInformationApplicator)(null)).ProductPendingUpdateDataForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.OperationalActions.ProductPendingUpdateDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.UpdateProductAdditionalInformationApplicator)(null)).ProductPendingUpdateDataForBinding)).SyncRoot)).ProductPk)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.ProductPendingUpdateDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.UpdateProductAdditionalInformationApplicator)(null)).ProductPendingUpdateDataForBinding)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.ProductPendingUpdateDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.UpdateProductAdditionalInformationApplicator)(null)).ProductPendingUpdateDataForBinding)).SyncRoot)).CustomsType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.FR.Business.OperationalActions.ProductPendingUpdateDataObject)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.OperationalActions.UpdateProductAdditionalInformationApplicator)(null)).ProductPendingUpdateDataForBinding)).SyncRoot)).OrganizationPk)));
			this.ProductPendingUpdateDataGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ProductPk";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.SupplierPart;
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.ColumnName = "CustomsType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OrganizationPk";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ProductPendingUpdateDataGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ProductPendingUpdateDataGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ProductPendingUpdateDataGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ProductPendingUpdateDataGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.ProductPendingUpdateDataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductPendingUpdateDataGrid.GridId = "B431AA08-2F22-4963-8A84-A6DE3D13C22C";
			this.ProductPendingUpdateDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductPendingUpdateDataGrid.LayoutKey = "ProductPendingUpdateDataGrid";
			this.ProductPendingUpdateDataGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ProductPendingUpdateDataGrid.Name = "ProductPendingUpdateDataGrid";
			this.ProductPendingUpdateDataGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 461, true);
			this.ProductPendingUpdateDataGrid.TabIndex = 0;
			// 
			// UpdateProductAdditionalInformationApplicatorControl
			// 
			this.Controls.Add(this.ProductPendingUpdateDataGrid);
			this.Name = "UpdateProductAdditionalInformationApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1142, 461, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ProductPendingUpdateDataGrid)).EndInit();
			this.ProductPendingUpdateDataGrid.ResumeLayout(false);
			this.ProductPendingUpdateDataGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.ZGrid ProductPendingUpdateDataGrid;

		#endregion
	}
}
