namespace Enterprise.Customs.IL.GUI
{
	partial class SupportingDocumentMetaDataGridControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.gridSupportingDocumentMetaData = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridSupportingDocumentMetaData)).BeginInit();
			this.gridSupportingDocumentMetaData.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.SupportingDocument);
			// 
			// gridSupportingDocumentMetaData
			// 
			this.gridSupportingDocumentMetaData.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridSupportingDocumentMetaData, "SupportingDocumentMetadataItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).SupportingDocumentMetadataItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocumentMetaData)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).SupportingDocumentMetadataItems)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocumentMetaData)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).SupportingDocumentMetadataItems)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IL.Business.SupportingDocumentMetaData)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).SupportingDocumentMetadataItems)).SyncRoot)).Mandatory)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocumentMetaData)(((System.Collections.IList)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).SupportingDocumentMetadataItems)).SyncRoot)).CY_Data)));
			this.gridSupportingDocumentMetaData.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("F9445AC1-DDC1-4457-8D6E-566467EA3E3B", "Description");
			zCheckBoxColumnStyleInfo1.ColumnName = "Mandatory";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("D56CE3C1-0CC4-497F-9E07-B6A5113CB5AA", "Mandatory");
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridSupportingDocumentMetaData.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.gridSupportingDocumentMetaData.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.gridSupportingDocumentMetaData.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.gridSupportingDocumentMetaData.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridSupportingDocumentMetaData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridSupportingDocumentMetaData.GridId = "4A21D60E-5071-46FA-B491-A91743FF6FD8";
			this.gridSupportingDocumentMetaData.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridSupportingDocumentMetaData.LayoutKey = "gridSupportingDocumentMetaData";
			this.gridSupportingDocumentMetaData.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gridSupportingDocumentMetaData.Name = "gridSupportingDocumentMetaData";
			this.gridSupportingDocumentMetaData.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 166, true);
			this.gridSupportingDocumentMetaData.TabIndex = 0;
			// 
			// SupportingDocumentMetaDataGridControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.gridSupportingDocumentMetaData);
			this.Name = "SupportingDocumentMetaDataGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridSupportingDocumentMetaData)).EndInit();
			this.gridSupportingDocumentMetaData.ResumeLayout(false);
			this.gridSupportingDocumentMetaData.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.ZGrid gridSupportingDocumentMetaData;

		#endregion
	}
}
