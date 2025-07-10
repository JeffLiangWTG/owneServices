using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Manifest.GUI
{
	partial class AsycudaAdditionalInfoUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo StatementTypeDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo StatementCodeMultiControlColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo ContentMultiControlColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.additionalInfosPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.additionalInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.additionalInfosPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalInfosGrid)).BeginInit();
			this.additionalInfosGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// AdditionalInfosPanel
			// 
			this.additionalInfosPanel.Controls.Add(this.additionalInfosGrid);
			this.additionalInfosPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.additionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.additionalInfosPanel.Name = "additionalInfosPanel";
			this.additionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.additionalInfosPanel.TabIndex = 0;
			// 
			// AdditionalInfosGrid
			// 
			this.additionalInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.additionalInfosGrid, "AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).ReferenceNumberFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).AdditionalInfos)).SyncRoot)).DescriptionFieldType)));

			this.additionalInfosGrid.CaptionVisible = false;
			StatementTypeDropEditColumnStyleInfo.ColumnName = "CSI_Code";
			StatementTypeDropEditColumnStyleInfo.DefaultCollectionIndex = 0;
			StatementTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			StatementCodeMultiControlColumnStyleInfo.ColumnName = "CSI_ReferenceNumber";
			StatementCodeMultiControlColumnStyleInfo.DefaultCollectionIndex = 0;
			StatementCodeMultiControlColumnStyleInfo.FieldTypeColumnName = "ReferenceNumberFieldType";
			StatementCodeMultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			ContentMultiControlColumnStyleInfo.ColumnName = "CSI_Description";
			ContentMultiControlColumnStyleInfo.DefaultCollectionIndex = 0;
			ContentMultiControlColumnStyleInfo.FieldTypeColumnName = "DescriptionFieldType";
			ContentMultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.additionalInfosGrid.ColumnStyles.Add(StatementTypeDropEditColumnStyleInfo);
			this.additionalInfosGrid.ColumnStyles.Add(StatementCodeMultiControlColumnStyleInfo);
			this.additionalInfosGrid.ColumnStyles.Add(ContentMultiControlColumnStyleInfo);
			this.additionalInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;

			this.additionalInfosGrid.GridId = "ffe13d21-dacb-4019-82cd-bb6a872d2dd9";
			this.additionalInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.additionalInfosGrid.LayoutKey = "additionalInfosGrid";
			this.additionalInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.additionalInfosGrid.Name = "additionalInfosGrid";
			this.additionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 80, true);
			this.additionalInfosGrid.TabIndex = 0;
			// 
			// AsycudaBillAdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.additionalInfosPanel);
			this.Name = "AsycudaBillAdditionalInfoUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.additionalInfosPanel.ResumeLayout(false);
			this.additionalInfosPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.additionalInfosGrid)).EndInit();
			this.additionalInfosGrid.ResumeLayout(false);
			this.additionalInfosGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel additionalInfosPanel;
		private ZArchitecture.ZGrid additionalInfosGrid;
	}
}
