namespace Enterprise.Customs.KR.GUI
{
	partial class IMPOtherGovernmentAgencyInfoUserControl
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
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.GovernmentAgencyInfoBoundGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ApprovalDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.NonApprovalDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.NonApprovalDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GovernmentAgencyInfoBoundGrid)).BeginInit();
            this.GovernmentAgencyInfoBoundGrid.SuspendLayout();
            this.ApprovalDocumentGroupBox.SuspendLayout();
            this.NonApprovalDocumentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NonApprovalDocumentGrid)).BeginInit();
            this.NonApprovalDocumentGrid.SuspendLayout();
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
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Procedure)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_SubType)));
            this.GovernmentAgencyInfoBoundGrid.CaptionVisible = false;
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Procedure";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2c949320-68fd-4282-92e7-cf94d63653ec", "Use Code");
            zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.GovernmentAgencyInfoBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.GovernmentAgencyInfoBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GovernmentAgencyInfoBoundGrid.GridId = "f2083a5c-3334-4829-804c-307610fae8a4";
            this.GovernmentAgencyInfoBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.GovernmentAgencyInfoBoundGrid.LayoutKey = "GovernmentAgencyInfoBoundGrid";
            this.GovernmentAgencyInfoBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.GovernmentAgencyInfoBoundGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.GovernmentAgencyInfoBoundGrid.Name = "GovernmentAgencyInfoBoundGrid";
            this.GovernmentAgencyInfoBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 136, true);
            this.GovernmentAgencyInfoBoundGrid.TabIndex = 0;
            // 
            // ApprovalDocumentGroupBox
            // 
            this.ApprovalDocumentGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5fee80c3-87b8-431f-b253-c2aa2253f3fd", "Approval Documents");
            this.ApprovalDocumentGroupBox.Controls.Add(this.GovernmentAgencyInfoBoundGrid);
            this.ApprovalDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.ApprovalDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ApprovalDocumentGroupBox.Name = "ApprovalDocumentGroupBox";
            this.ApprovalDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 153, true);
            this.ApprovalDocumentGroupBox.TabIndex = 1;
            this.ApprovalDocumentGroupBox.TabStop = false;
            // 
            // NonApprovalDocumentGroupBox
            // 
            this.NonApprovalDocumentGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d4c214b0-9e82-4fa6-ac40-b88fd1f33669", "Non-Approval Documents");
            this.NonApprovalDocumentGroupBox.Controls.Add(this.NonApprovalDocumentGrid);
            this.NonApprovalDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NonApprovalDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
            this.NonApprovalDocumentGroupBox.Name = "NonApprovalDocumentGroupBox";
            this.NonApprovalDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 166, true);
            this.NonApprovalDocumentGroupBox.TabIndex = 2;
            this.NonApprovalDocumentGroupBox.TabStop = false;
            // 
            // NonApprovalDocumentGrid
            // 
            this.NonApprovalDocumentGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.NonApprovalDocumentGrid, "PivotsForBinding.NonGADetailCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).NonGADetailCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_Procedure)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).NonGADetailCollection)).SyncRoot)).NonGAReasonType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).NonGADetailCollection)).SyncRoot)).ImportNonGAMandatoryDocument)));
            this.NonApprovalDocumentGrid.CaptionVisible = false;
            zCodeFindBoxColumnStyleInfo2.ColumnName = "CSI_Procedure";
            zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo3.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
            zCodeFindBoxColumnStyleInfo3.ColumnName = "NonGAReasonType";
            zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
            zTextBoxColumnStyleInfo3.ColumnName = "ImportNonGAMandatoryDocument";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.NonApprovalDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NonApprovalDocumentGrid.GridId = "adcc0e03-8c92-4517-929b-ea26f586081a";
            this.NonApprovalDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.NonApprovalDocumentGrid.LayoutKey = "NonApprovalDocumentGrid";
            this.NonApprovalDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.NonApprovalDocumentGrid.Name = "NonApprovalDocumentGrid";
            this.NonApprovalDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(702, 149, true);
            this.NonApprovalDocumentGrid.TabIndex = 0;
            // 
            // IMPOtherGovernmentAgencyInfoUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.NonApprovalDocumentGroupBox);
            this.Controls.Add(this.ApprovalDocumentGroupBox);
            this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.Name = "IMPOtherGovernmentAgencyInfoUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(706, 319, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GovernmentAgencyInfoBoundGrid)).EndInit();
            this.GovernmentAgencyInfoBoundGrid.ResumeLayout(false);
            this.GovernmentAgencyInfoBoundGrid.PerformLayout();
            this.ApprovalDocumentGroupBox.ResumeLayout(false);
            this.ApprovalDocumentGroupBox.PerformLayout();
            this.NonApprovalDocumentGroupBox.ResumeLayout(false);
            this.NonApprovalDocumentGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NonApprovalDocumentGrid)).EndInit();
            this.NonApprovalDocumentGrid.ResumeLayout(false);
            this.NonApprovalDocumentGrid.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZGrid GovernmentAgencyInfoBoundGrid;
		#endregion

		private ZArchitecture.GUI.ZGroupBox ApprovalDocumentGroupBox;
		private ZArchitecture.GUI.ZGroupBox NonApprovalDocumentGroupBox;
		private ZArchitecture.ZGrid NonApprovalDocumentGrid;
	}
}
