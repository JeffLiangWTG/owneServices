using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class ApprovalAndReImportUserControl
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
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            this.ApprovalDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ApprovalDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
            this.NonApprovalDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.NonApprovalDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
            this.ReImportGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.ReImportGrid = new Enterprise.ZArchitecture.ZGrid();
            this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
            this.kSplitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ApprovalDocumentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ApprovalDocumentGrid)).BeginInit();
            this.ApprovalDocumentGrid.SuspendLayout();
            this.NonApprovalDocumentGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NonApprovalDocumentGrid)).BeginInit();
            this.NonApprovalDocumentGrid.SuspendLayout();
            this.ReImportGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReImportGrid)).BeginInit();
            this.ReImportGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
            this.kSplitContainer1.Panel1.SuspendLayout();
            this.kSplitContainer1.Panel2.SuspendLayout();
            this.kSplitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).BeginInit();
            this.kSplitContainer2.Panel1.SuspendLayout();
            this.kSplitContainer2.Panel2.SuspendLayout();
            this.kSplitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // ApprovalDocumentGroupBox
            // 
            this.ApprovalDocumentGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("505F22D2-FB78-435A-AA37-2398862C5F6C", "Approval Documents");
            this.ApprovalDocumentGroupBox.Controls.Add(this.ApprovalDocumentGrid);
            this.ApprovalDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ApprovalDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ApprovalDocumentGroupBox.Name = "ApprovalDocumentGroupBox";
            this.ApprovalDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 136, true);
            this.ApprovalDocumentGroupBox.TabIndex = 0;
            this.ApprovalDocumentGroupBox.TabStop = false;
            // 
            // ApprovalDocumentGrid
            // 
            this.ApprovalDocumentGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ApprovalDocumentGrid, "FilteredInvoiceLines.GAApprovalDataCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_LineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Procedure)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_DateOfIssue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_SubType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.GAApproval)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).GAApprovalDataCollection)).SyncRoot)).CSI_ReferenceNumber2)));
            this.ApprovalDocumentGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
            zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo1.IsReadOnly = true;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Procedure";
            zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDateEditColumnStyleInfo1.ColumnName = "CSI_DateOfIssue";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo2.ColumnName = "CSI_SubType";
            zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
            zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber2";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.ApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.ApprovalDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ApprovalDocumentGrid.GridId = "58383bef-4805-4174-8ff0-29842a4319e0";
            this.ApprovalDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ApprovalDocumentGrid.LayoutKey = "ApprovalDocumentGrid";
            this.ApprovalDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.ApprovalDocumentGrid.Name = "ApprovalDocumentGrid";
            this.ApprovalDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 119, true);
            this.ApprovalDocumentGrid.TabIndex = 0;
            // 
            // NonApprovalDocumentGroupBox
            // 
            this.NonApprovalDocumentGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("FAD91BEA-1B78-49E6-A750-9F3EA236B25D", "Non-Approval Documents");
            this.NonApprovalDocumentGroupBox.Controls.Add(this.NonApprovalDocumentGrid);
            this.NonApprovalDocumentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NonApprovalDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.NonApprovalDocumentGroupBox.Name = "NonApprovalDocumentGroupBox";
            this.NonApprovalDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 132, true);
            this.NonApprovalDocumentGroupBox.TabIndex = 1;
            this.NonApprovalDocumentGroupBox.TabStop = false;
            // 
            // NonApprovalDocumentGrid
            // 
            this.NonApprovalDocumentGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.NonApprovalDocumentGrid, "FilteredInvoiceLines.NonGADetailCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_LineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_Procedure)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)).SyncRoot)).NonGAReasonType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)).SyncRoot)).CSI_Description)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.NonGADetail)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).NonGADetailCollection)).SyncRoot)).ImportNonGAMandatoryDocument)));
            this.NonApprovalDocumentGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "CSI_LineNo";
            zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo2.IsReadOnly = true;
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zCodeFindBoxColumnStyleInfo2.ColumnName = "CSI_Procedure";
            zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo3.ColumnName = "CSI_Code";
            zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCodeFindBoxColumnStyleInfo3.ColumnName = "NonGAReasonType";
            zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo4.ColumnName = "CSI_Description";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
            zTextBoxColumnStyleInfo5.ColumnName = "ImportNonGAMandatoryDocument";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.IsReadOnly = true;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.NonApprovalDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.NonApprovalDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NonApprovalDocumentGrid.GridId = "72082bc0-0de0-48ce-9e00-b7f3a8ee176a";
            this.NonApprovalDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.NonApprovalDocumentGrid.LayoutKey = "NonApprovalDocumentGrid";
            this.NonApprovalDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.NonApprovalDocumentGrid.Name = "NonApprovalDocumentGrid";
            this.NonApprovalDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 115, true);
            this.NonApprovalDocumentGrid.TabIndex = 0;
            // 
            // ReImportGroupBox
            // 
            this.ReImportGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("B7E58810-34E0-4FFF-B6A9-8398A9DDEB3F", "Re-Import");
            this.ReImportGroupBox.Controls.Add(this.ReImportGrid);
            this.ReImportGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReImportGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ReImportGroupBox.Name = "ReImportGroupBox";
            this.ReImportGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 132, true);
            this.ReImportGroupBox.TabIndex = 1;
            this.ReImportGroupBox.TabStop = false;
            // 
            // ReImportGrid
            // 
            this.ReImportGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.ReImportGrid, "FilteredInvoiceLines.PreviousExpDecLineCollection");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.PreviousExpDecLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)).SyncRoot)).CSI_LineNo)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.PreviousExpDecLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)).SyncRoot)).CSI_ReferenceNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.PreviousExpDecLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)).SyncRoot)).EntryLineNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.PreviousExpDecLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)).SyncRoot)).CSI_ItemNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.PreviousExpDecLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)).SyncRoot)).CSI_Quantity)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.PreviousExpDecLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousExpDecLineCollection)).SyncRoot)).CSI_UnitOfQuantity)));
            this.ReImportGrid.CaptionVisible = false;
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "CSI_LineNo";
            zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo3.IsReadOnly = true;
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
            zTextBoxColumnStyleInfo6.ColumnName = "CSI_ReferenceNumber";
            zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo4.ColumnName = "EntryLineNumber";
            zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo5.ColumnName = "CSI_ItemNumber";
            zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo6.ColumnName = "CSI_Quantity";
            zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
            zCalcEditColumnStyleInfo6.GroupName = Enterprise.Customs.KR.GUI.Res.GetData("43b9a8da-5a20-44de-a78b-0a7acfb34e9f", "Re Import Amount");
            zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDropEditColumnStyleInfo4.ColumnName = "CSI_UnitOfQuantity";
            zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
            zDropEditColumnStyleInfo4.GroupName = Enterprise.Customs.KR.GUI.Res.GetData("43b9a8da-5a20-44de-a78b-0a7acfb34e9f", "Re Import Amount");
            zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
            this.ReImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.ReImportGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.ReImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
            this.ReImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
            this.ReImportGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
            this.ReImportGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
            this.ReImportGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReImportGrid.GridId = "04f25cbf-5429-4925-9dd1-64ab8b5105aa";
            this.ReImportGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.ReImportGrid.LayoutKey = "ReImportGrid";
            this.ReImportGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
            this.ReImportGrid.Name = "ReImportGrid";
            this.ReImportGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 115, true);
            this.ReImportGrid.TabIndex = 0;
            // 
            // kSplitContainer1
            // 
            this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.kSplitContainer1.Name = "kSplitContainer1";
            // 
            // kSplitContainer1.Panel1
            // 
            this.kSplitContainer1.Panel1.Controls.Add(this.NonApprovalDocumentGroupBox);
            // 
            // kSplitContainer1.Panel2
            // 
            this.kSplitContainer1.Panel2.Controls.Add(this.ReImportGroupBox);
            this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 132, true);
            this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(419);
            this.kSplitContainer1.SplitterWidth = 7;
            this.kSplitContainer1.TabIndex = 2;
            // 
            // kSplitContainer2
            // 
            this.kSplitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kSplitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.kSplitContainer2.Name = "kSplitContainer2";
            this.kSplitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // kSplitContainer2.Panel1
            // 
            this.kSplitContainer2.Panel1.Controls.Add(this.ApprovalDocumentGroupBox);
            // 
            // kSplitContainer2.Panel2
            // 
            this.kSplitContainer2.Panel2.Controls.Add(this.kSplitContainer1);
            this.kSplitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 273, true);
            this.kSplitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(136);
            this.kSplitContainer2.SplitterWidth = 7;
            this.kSplitContainer2.TabIndex = 3;
            // 
            // ApprovalAndReImportUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.kSplitContainer2);
            this.Name = "ApprovalAndReImportUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 273, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ApprovalDocumentGroupBox.ResumeLayout(false);
            this.ApprovalDocumentGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ApprovalDocumentGrid)).EndInit();
            this.ApprovalDocumentGrid.ResumeLayout(false);
            this.ApprovalDocumentGrid.PerformLayout();
            this.NonApprovalDocumentGroupBox.ResumeLayout(false);
            this.NonApprovalDocumentGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NonApprovalDocumentGrid)).EndInit();
            this.NonApprovalDocumentGrid.ResumeLayout(false);
            this.NonApprovalDocumentGrid.PerformLayout();
            this.ReImportGroupBox.ResumeLayout(false);
            this.ReImportGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ReImportGrid)).EndInit();
            this.ReImportGrid.ResumeLayout(false);
            this.ReImportGrid.PerformLayout();
            this.kSplitContainer1.Panel1.ResumeLayout(false);
            this.kSplitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
            this.kSplitContainer1.ResumeLayout(false);
            this.kSplitContainer1.PerformLayout();
            this.kSplitContainer2.Panel1.ResumeLayout(false);
            this.kSplitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kSplitContainer2)).EndInit();
            this.kSplitContainer2.ResumeLayout(false);
            this.kSplitContainer2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZGroupBox ApprovalDocumentGroupBox;
		public ZArchitecture.ZGrid ApprovalDocumentGrid;
		public ZGroupBox NonApprovalDocumentGroupBox;
		public ZArchitecture.ZGrid NonApprovalDocumentGrid;
		public ZGroupBox ReImportGroupBox;
		public ZArchitecture.ZGrid ReImportGrid;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer2;
	}
}
