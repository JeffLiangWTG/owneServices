namespace Enterprise.Customs.KR.GUI
{
	partial class EXPOtherGovernmentAgencyInfoUserControl
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
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.GovernmentAgencyInfoBoundGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GovernmentAgencyInfoBoundGrid)).BeginInit();
            this.GovernmentAgencyInfoBoundGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.OrgSupplierPart);
            // 
            // GovernmentAgencyInfoBoundGrid
            // 
            this.GovernmentAgencyInfoBoundGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.GovernmentAgencyInfoBoundGrid, "PivotsForBinding.GAApprovalDataCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_SubType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Procedure)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).NonGAReasonType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_AdditionalDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).ExportNonGAMandatoryDocument)));
            this.GovernmentAgencyInfoBoundGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zDropEditColumnStyleInfo2.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Procedure";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zCodeFindBoxColumnStyleInfo2.ColumnName = "NonGAReasonType";
            zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_AdditionalDescription";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
            zTextBoxColumnStyleInfo3.ColumnName = "ExportNonGAMandatoryDocument";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.GovernmentAgencyInfoBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GovernmentAgencyInfoBoundGrid.GridId = "86fa3b38-b779-4212-8063-860e1b5a93da";
            this.GovernmentAgencyInfoBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GovernmentAgencyInfoBoundGrid.LayoutKey = "GovernmentAgencyInfoBoundGrid";
            this.GovernmentAgencyInfoBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GovernmentAgencyInfoBoundGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.GovernmentAgencyInfoBoundGrid.Name = "GovernmentAgencyInfoBoundGrid";
            this.GovernmentAgencyInfoBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 319, true);
            this.GovernmentAgencyInfoBoundGrid.TabIndex = 0;
            // 
            // EXPOtherGovernmentAgencyInfoUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.GovernmentAgencyInfoBoundGrid);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.Name = "EXPOtherGovernmentAgencyInfoUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 319, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GovernmentAgencyInfoBoundGrid)).EndInit();
            this.GovernmentAgencyInfoBoundGrid.ResumeLayout(false);
            this.GovernmentAgencyInfoBoundGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZGrid GovernmentAgencyInfoBoundGrid;
		#endregion
	}
}
