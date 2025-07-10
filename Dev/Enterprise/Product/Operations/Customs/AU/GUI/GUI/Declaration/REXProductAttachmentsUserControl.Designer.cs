using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class REXProductAttachmentsUserControl
	{
		ZGroupBox zGroupBoxAttachments;
		ZArchitecture.ZGrid zGridEDocs;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.zGroupBoxAttachments = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zGridEDocs = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zGroupBoxAttachments.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridEDocs)).BeginInit();
			this.zGridEDocs.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent);
			// 
			// zGroupBoxAttachments
			// 
			this.zGroupBoxAttachments.Controls.Add(this.zGridEDocs);
			this.zGroupBoxAttachments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBoxAttachments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBoxAttachments.Name = "zGroupBoxAttachments";
			this.zGroupBoxAttachments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 251, true);
			this.zGroupBoxAttachments.TabIndex = 0;
			this.zGroupBoxAttachments.TabStop = false;
			this.zGroupBoxAttachments.Text = "Attachments";
			// 
			// zGridEDocs
			// 
			this.zGridEDocs.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGridEDocs, "EDocPivotCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent)(null)).EDocPivotCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent)(null)).EDocPivotCollection)).SyncRoot)).CSD_StorageDocReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent)(null)).EDocPivotCollection)).SyncRoot)).CSD_DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusStorageDocPivot)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent)(null)).EDocPivotCollection)).SyncRoot)).CSD_Description)));
			this.zGridEDocs.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "CSD_StorageDocReference";
			zGuidDropEditColumnStyleInfo1.IsMandatory = true;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zDropEditColumnStyleInfo1.ColumnName = "CSD_DocType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiLineTextBoxColumnInfo1.ColumnName = "CSD_Description";
			zMultiLineTextBoxColumnInfo1.IsMandatory = true;
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.zGridEDocs.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.zGridEDocs.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGridEDocs.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.zGridEDocs.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGridEDocs.GridId = "cbb3b8cf-743f-41a8-9152-468cbb25a61f";
			this.zGridEDocs.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGridEDocs.LayoutKey = "zGrid";
			this.zGridEDocs.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zGridEDocs.Name = "zGridEDocs";
			this.zGridEDocs.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(775, 232, true);
			this.zGridEDocs.TabIndex = 0;
			// 
			// RFPProductAttachmentsUserControl
			// 
			this.Controls.Add(this.zGroupBoxAttachments);
			this.Name = "RFPProductAttachmentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 251, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBoxAttachments.ResumeLayout(false);
			this.zGroupBoxAttachments.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGridEDocs)).EndInit();
			this.zGridEDocs.ResumeLayout(false);
			this.zGridEDocs.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
