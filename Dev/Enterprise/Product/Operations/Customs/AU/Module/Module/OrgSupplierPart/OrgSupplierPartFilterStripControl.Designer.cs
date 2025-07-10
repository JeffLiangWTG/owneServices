using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.AU.Module
{
    partial class OrgSupplierPartFilterStripControl
    {
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.SuspendLayout();
			this.FilteredGrid.Size = new Size();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.Caption = "Export Classification Lookup Code";
			zTextBoxColumnStyleInfo1.ColumnName = "ExportClassificationLookup";
			zTextBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.Caption = "Import Classification Lookup Code";
			zTextBoxColumnStyleInfo2.ColumnName = "ImportClassificationLookup";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.Caption = "Treatment Code";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "ImportTreatmentCode";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Instrument Type";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "InstrumentType";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = "Inst. Code";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "InstrumentCode";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.Caption = "Add Info";
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "AddInfoLine";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.Caption = "Export Last Audit By";
			zTextBoxColumnStyleInfo7.ColumnName = "ExportLastAuditUser";
			zTextBoxColumnStyleInfo7.GroupName = Enterprise.Customs.AU.Module.Res.GetData("OrgSupplierPartFilter|9f7cb5f9-8ecd-4a5c-a619-da750248175f", "Export Audit");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.Caption = "Export Last Audit Date";
			zTextBoxColumnStyleInfo8.ColumnName = "ExportLastAuditDate";
			zTextBoxColumnStyleInfo8.GroupName = Enterprise.Customs.AU.Module.Res.GetData("OrgSupplierPartFilter|9f7cb5f9-8ecd-4a5c-a619-da750248175f", "Export Audit");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.Caption = "Import Last Audit By";
			zTextBoxColumnStyleInfo9.ColumnName = "ImportLastAuditUser";
			zTextBoxColumnStyleInfo9.GroupName = Enterprise.Customs.AU.Module.Res.GetData("OrgSupplierPartFilter|2f4b77f8-8061-4c3f-a20f-3b9f61171ae3", "Import Audit");
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo10.Caption = "Import Last Audit Date";
			zTextBoxColumnStyleInfo10.ColumnName = "ImportLastAuditDate";
			zTextBoxColumnStyleInfo10.GroupName = Enterprise.Customs.AU.Module.Res.GetData("OrgSupplierPartFilter|2f4b77f8-8061-4c3f-a20f-3b9f61171ae3", "Import Audit");
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.Caption = "Produce Type";
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "RFPProduceType";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo12.Caption = "Product";
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "RFPProduct";
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo13.Caption = "Supplementary Code";
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "RFPSupplementaryCode";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo14.Caption = "Pack Type";
			zTextBoxColumnStyleInfo14.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo14.ColumnName = "RFPPackType";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo15.Caption = "Preservation";
			zTextBoxColumnStyleInfo15.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo15.ColumnName = "RFPPreservation";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo16.Caption = "Cut Code";
			zTextBoxColumnStyleInfo16.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo16.ColumnName = "RFPCutCode";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Declaration.Business.AUOrgSupplierPart);
			// 
			// OrgSupplierPartFilterStripControl
			// 
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.AUOrgSupplierPart";
			this.Name = "OrgSupplierPartFilterStripControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
