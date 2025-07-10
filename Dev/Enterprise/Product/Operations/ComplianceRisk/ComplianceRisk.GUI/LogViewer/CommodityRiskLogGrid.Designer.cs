using Enterprise.ZArchitecture;

namespace Enterprise.ComplianceRisk.GUI
{
	partial class CommodityRiskLogGrid
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
			Enterprise.ComplianceRisk.GUI.ComplianceRiskColumnStyleInfo complianceRiskColumnStyleInfo = new Enterprise.ComplianceRisk.GUI.ComplianceRiskColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoNomenclatureAlerts = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoTariffAlerts = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zCommoditySourceTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SnapshotCommoditiesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SnapshotCommoditiesGrid)).BeginInit();
			this.SnapshotCommoditiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLogCollection);
			// 
			// SnapshotCommoditiesGrid
			// 
			this.SnapshotCommoditiesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SnapshotCommoditiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).RiskStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).Conditions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).Source)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).CommoditySource)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).OriginOfGoods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.ComplianceRisk.Business.ComplianceCommodityRiskLog)(((System.Collections.IList)(((Enterprise.ComplianceRisk.Business.ComplianceRiskStatusChangeLog)(null)).CommodityRiskLogCollection)).SyncRoot)).DateAddedUtc)));
			this.SnapshotCommoditiesGrid.CaptionVisible = false;
			complianceRiskColumnStyleInfo.ColumnName = "RiskStatusDescription";
			complianceRiskColumnStyleInfo.DefaultCollectionIndex = 0;
			complianceRiskColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfoNomenclatureAlerts.ColumnName = "NomenclatureCondition";
			zTextBoxColumnStyleInfoNomenclatureAlerts.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfoNomenclatureAlerts.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfoTariffAlerts.ColumnName = "SpecificCondition";
			zTextBoxColumnStyleInfoTariffAlerts.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfoTariffAlerts.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.ColumnName = "Source";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo4.ColumnName = "OriginOfGoods";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zMultiLineTextBoxColumnInfo.ColumnName = "GoodsDescription";
			zMultiLineTextBoxColumnInfo.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo.DefaultCollectionIndex = 0;
			zMultiLineTextBoxColumnInfo.MinimumEditControlWidth = 200;
			zMultiLineTextBoxColumnInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCommoditySourceTextBoxColumnStyleInfo.ColumnName = "CommoditySource";
			zCommoditySourceTextBoxColumnStyleInfo.DefaultCollectionIndex = 0;
			zCommoditySourceTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDateEditColumnStyleInfo.ColumnName = "DateAddedUtc";
			zDateEditColumnStyleInfo.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			zDateEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(complianceRiskColumnStyleInfo);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoNomenclatureAlerts);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfoTariffAlerts);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zCommoditySourceTextBoxColumnStyleInfo);
			this.SnapshotCommoditiesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo);
			this.SnapshotCommoditiesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SnapshotCommoditiesGrid.GridId = "02172eed-24c0-4b5e-a2a2-e7ab4a4a5ab9";
			this.SnapshotCommoditiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SnapshotCommoditiesGrid.LayoutKey = "SnapshotCommoditiesGrid";
			this.SnapshotCommoditiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 38, true);
			this.SnapshotCommoditiesGrid.Name = "SnapshotCommoditiesGrid";
			this.SnapshotCommoditiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1411, 61, true);
			this.SnapshotCommoditiesGrid.TabIndex = 11;
			// 
			// CommodityRiskLogUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SnapshotCommoditiesGrid);
			this.Name = "CommodityRiskLogUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 487, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SnapshotCommoditiesGrid)).EndInit();
			this.SnapshotCommoditiesGrid.ResumeLayout(false);
			this.SnapshotCommoditiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZGrid SnapshotCommoditiesGrid;
	}
}
