using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.UPE.GUI
{
	public partial class UPSUserControl : BaseCustomsEntryUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid RelatedUPECusHAWBsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.ZCalcEdit QuarantineFeeCalcEdit;
		private Enterprise.ZArchitecture.ZLabel SpecialAttendanceChargeLabel;
		private Enterprise.ZArchitecture.ZLabel zLabel13;
		private Enterprise.ZArchitecture.ZCalcEdit QuarantineProcessingFeeCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZGroupBox RefundGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox RefundEnquiryCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox AuditGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsAuditCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZButton ProcessRefundButton;
		private Enterprise.ZArchitecture.GUI.ZCheckBox zCheckBox1;
		private ZGroupBox OtherGroupBox;
		private ZCalcEdit JobsForImporterCalcEdit;
		private ZLabel TotalJobsForImporterLabel;
		private ZGroupBox ShipperMatchingErrorGroupBox;
		internal ZCheckBox ShipperMatchingErrorCheckBox;
		private ZLabel zLabelControlNumber;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RelatedUPECusHAWBsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QuarantineFeeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SpecialAttendanceChargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel13 = new Enterprise.ZArchitecture.ZLabel();
			this.QuarantineProcessingFeeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabelControlNumber = new Enterprise.ZArchitecture.ZLabel();
			this.zCheckBox1 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProcessRefundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RefundEnquiryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AuditGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsAuditCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OtherGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.JobsForImporterCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalJobsForImporterLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShipperMatchingErrorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ShipperMatchingErrorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedUPECusHAWBsGrid)).BeginInit();
			this.RelatedUPECusHAWBsGrid.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.RefundGroupBox.SuspendLayout();
			this.AuditGroupBox.SuspendLayout();
			this.OtherGroupBox.SuspendLayout();
			this.ShipperMatchingErrorGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.UPEJobDeclaration);
			// 
			// RelatedUPECusHAWBsGrid
			// 
			this.RelatedUPECusHAWBsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.RelatedUPECusHAWBsGrid, "RelatedCusHAWBs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).CS_HAWB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).WayBillShort)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).BillingTerms)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).CS_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).DutyType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).IsHoldForCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).HoldForCollectDepot)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).Lookups.HoldForCollectDepotList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).HFCContactName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPECusHAWB)(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).RelatedCusHAWBs)).SyncRoot)).HFCContactPhoneNumber)));
			this.RelatedUPECusHAWBsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Tracking Number";
			zTextBoxColumnStyleInfo1.ColumnName = "CS_HAWB";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(135);
			zTextBoxColumnStyleInfo2.Caption = "Waybill Short No";
			zTextBoxColumnStyleInfo2.ColumnName = "WayBillShort";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo3.Caption = "Billing Terms";
			zTextBoxColumnStyleInfo3.ColumnName = "BillingTerms";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Customs Status";
			zTextBoxColumnStyleInfo4.ColumnName = "CS_CustomsStatus";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zTextBoxColumnStyleInfo5.Caption = "Duty Type";
			zTextBoxColumnStyleInfo5.ColumnName = "DutyType";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "Is Hold For Collection";
			zCheckBoxColumnStyleInfo1.ColumnName = "IsHoldForCollection";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.BindToList = "Lookups+HoldForCollectDepotList";
			zDropEditColumnStyleInfo1.Caption = "Hold For Collect Depot";
			zDropEditColumnStyleInfo1.ColumnName = "HoldForCollectDepot";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.Caption = "Contact";
			zTextBoxColumnStyleInfo6.ColumnName = "HFCContactName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "Phone";
			zTextBoxColumnStyleInfo7.ColumnName = "HFCContactPhoneNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.RelatedUPECusHAWBsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.RelatedUPECusHAWBsGrid.CopySelectedRowsAllowed = true;
			this.RelatedUPECusHAWBsGrid.Dock = System.Windows.Forms.DockStyle.Top;
			this.RelatedUPECusHAWBsGrid.GridId = "59682d66-6e6f-4efe-981d-918fa1310e59";
			this.RelatedUPECusHAWBsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RelatedUPECusHAWBsGrid.LayoutKey = "RelatedUPECusHAWBsGrid";
			this.RelatedUPECusHAWBsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelatedUPECusHAWBsGrid.Name = "RelatedUPECusHAWBsGrid";
			this.RelatedUPECusHAWBsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.RelatedUPECusHAWBsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 176, true);
			this.RelatedUPECusHAWBsGrid.TabIndex = 0;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.QuarantineFeeCalcEdit);
			this.zGroupBox1.Controls.Add(this.SpecialAttendanceChargeLabel);
			this.zGroupBox1.Controls.Add(this.zLabel13);
			this.zGroupBox1.Controls.Add(this.QuarantineProcessingFeeCalcEdit);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 184, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 80, true);
			this.zGroupBox1.TabIndex = 1;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "UPS Charges";
			// 
			// QuarantineFeeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuarantineFeeCalcEdit, "QuarantineFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).QuarantineFee)));
			this.QuarantineFeeCalcEdit.DecimalPlaces = 2;
			this.QuarantineFeeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 17, true);
			this.QuarantineFeeCalcEdit.Name = "QuarantineFeeCalcEdit";
			this.QuarantineFeeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.QuarantineFeeCalcEdit.TabIndex = 1;
			this.QuarantineFeeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuarantineFeeCalcEdit, false);
			// 
			// SpecialAttendanceChargeLabel
			// 
			this.SpecialAttendanceChargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.SpecialAttendanceChargeLabel.Name = "SpecialAttendanceChargeLabel";
			this.SpecialAttendanceChargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.SpecialAttendanceChargeLabel.TabIndex = 0;
			this.SpecialAttendanceChargeLabel.Text = "Quarantine Fee";
			// 
			// zLabel13
			// 
			this.zLabel13.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 41, true);
			this.zLabel13.Name = "zLabel13";
			this.zLabel13.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 23, true);
			this.zLabel13.TabIndex = 2;
			this.zLabel13.Text = "UPS Inspection Fee";
			// 
			// QuarantineProcessingFeeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QuarantineProcessingFeeCalcEdit, "QuarantineProcessingFee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).QuarantineProcessingFee)));
			this.QuarantineProcessingFeeCalcEdit.DecimalPlaces = 2;
			this.QuarantineProcessingFeeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 41, true);
			this.QuarantineProcessingFeeCalcEdit.Name = "QuarantineProcessingFeeCalcEdit";
			this.QuarantineProcessingFeeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.QuarantineProcessingFeeCalcEdit.TabIndex = 3;
			this.QuarantineProcessingFeeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QuarantineProcessingFeeCalcEdit, false);
			// 
			// RefundGroupBox
			// 
			this.RefundGroupBox.Controls.Add(this.zLabelControlNumber);
			this.RefundGroupBox.Controls.Add(this.zCheckBox1);
			this.RefundGroupBox.Controls.Add(this.ProcessRefundButton);
			this.RefundGroupBox.Controls.Add(this.RefundEnquiryCheckBox);
			this.RefundGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 184, true);
			this.RefundGroupBox.Name = "RefundGroupBox";
			this.RefundGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 80, true);
			this.RefundGroupBox.TabIndex = 2;
			this.RefundGroupBox.TabStop = false;
			this.RefundGroupBox.Text = "Refund Processing";
			// 
			// zLabelControlNumber
			// 
			this.BindingSource.SetBindingMember(this.zLabelControlNumber, "Refund+T10_ControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).Refund.T10_ControlNumber)));
			this.zLabelControlNumber.ForeColor = System.Drawing.Color.Blue;
			this.zLabelControlNumber.IsFontBold = true;
			this.zLabelControlNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 19, true);
			this.zLabelControlNumber.Name = "zLabelControlNumber";
			this.zLabelControlNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 18, true);
			this.zLabelControlNumber.TabIndex = 3;
			// 
			// zCheckBox1
			// 
			this.BindingSource.SetBindingMember(this.zCheckBox1, "IsRefundProcessed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).IsRefundProcessed)));
			this.zCheckBox1.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.zCheckBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zCheckBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 47, true);
			this.zCheckBox1.Name = "zCheckBox1";
			this.zCheckBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 17, true);
			this.zCheckBox1.TabIndex = 1;
			this.zCheckBox1.Text = "Refund Processed";
			this.zCheckBox1.UseVisualStyleBackColor = true;
			// 
			// ProcessRefundButton
			// 
			this.ProcessRefundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(141, 43, true);
			this.ProcessRefundButton.Name = "ProcessRefundButton";
			this.ProcessRefundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 23, true);
			this.ProcessRefundButton.TabIndex = 2;
			this.ProcessRefundButton.Text = "Process Refund";
			this.ProcessRefundButton.UseVisualStyleBackColor = true;
			this.ProcessRefundButton.Click += new System.EventHandler(this.ProcessRefundButton_Click);
			// 
			// RefundEnquiryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.RefundEnquiryCheckBox, "IsRefundEnquiryTemp");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).IsRefundEnquiryTemp)));
			this.RefundEnquiryCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.RefundEnquiryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RefundEnquiryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.RefundEnquiryCheckBox.Name = "RefundEnquiryCheckBox";
			this.RefundEnquiryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 18, true);
			this.RefundEnquiryCheckBox.TabIndex = 0;
			this.RefundEnquiryCheckBox.Text = "Refund Enquiry";
			this.RefundEnquiryCheckBox.UseVisualStyleBackColor = true;
			// 
			// AuditGroupBox
			// 
			this.AuditGroupBox.Controls.Add(this.IsAuditCheckBox);
			this.AuditGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(526, 184, true);
			this.AuditGroupBox.Name = "AuditGroupBox";
			this.AuditGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(126, 80, true);
			this.AuditGroupBox.TabIndex = 3;
			this.AuditGroupBox.TabStop = false;
			this.AuditGroupBox.Text = "Audit";
			// 
			// IsAuditCheckBox
			// 
			this.IsAuditCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsAuditCheckBox, "IsAudit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).IsAudit)));
			this.IsAuditCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsAuditCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsAuditCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.IsAuditCheckBox.Name = "IsAuditCheckBox";
			this.IsAuditCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 16, true);
			this.IsAuditCheckBox.TabIndex = 0;
			this.IsAuditCheckBox.Text = "Is Audit";
			this.IsAuditCheckBox.UseVisualStyleBackColor = true;
			// 
			// OtherGroupBox
			// 
			this.OtherGroupBox.Controls.Add(this.JobsForImporterCalcEdit);
			this.OtherGroupBox.Controls.Add(this.TotalJobsForImporterLabel);
			this.OtherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(658, 184, true);
			this.OtherGroupBox.Name = "OtherGroupBox";
			this.OtherGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 80, true);
			this.OtherGroupBox.TabIndex = 4;
			this.OtherGroupBox.TabStop = false;
			this.OtherGroupBox.Text = "Other";
			// 
			// JobsForImporterCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.JobsForImporterCalcEdit, "TotalJobsImportedForImporter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.UPEJobDeclaration)(null)).TotalJobsImportedForImporter)));
			this.JobsForImporterCalcEdit.DecimalPlaces = 0;
			this.JobsForImporterCalcEdit.Decimals = 0;
			this.JobsForImporterCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 17, true);
			this.JobsForImporterCalcEdit.Name = "JobsForImporterCalcEdit";
			this.JobsForImporterCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 17, true);
			this.JobsForImporterCalcEdit.TabIndex = 1;
			this.JobsForImporterCalcEdit.Text = "0";
			this.JobsForImporterCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.JobsForImporterCalcEdit, false);
			// 
			// TotalJobsForImporterLabel
			// 
			this.TotalJobsForImporterLabel.AutoSize = true;
			this.TotalJobsForImporterLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 21, true);
			this.TotalJobsForImporterLabel.Name = "TotalJobsForImporterLabel";
			this.TotalJobsForImporterLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 13, true);
			this.TotalJobsForImporterLabel.TabIndex = 0;
			this.TotalJobsForImporterLabel.Text = "Jobs For Importer:";
			// 
			// ShipperMatchingErrorGroupBox
			// 
			this.ShipperMatchingErrorGroupBox.Controls.Add(this.ShipperMatchingErrorCheckBox);
			this.ShipperMatchingErrorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 270, true);
			this.ShipperMatchingErrorGroupBox.Name = "ShipperMatchingErrorGroupBox";
			this.ShipperMatchingErrorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 49, true);
			this.ShipperMatchingErrorGroupBox.TabIndex = 5;
			this.ShipperMatchingErrorGroupBox.TabStop = false;
			this.ShipperMatchingErrorGroupBox.Text = "Shipper Matching Error";
			// 
			// ShipperMatchingErrorCheckBox
			// 
			this.ShipperMatchingErrorCheckBox.AutoSize = true;
			this.ShipperMatchingErrorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ShipperMatchingErrorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShipperMatchingErrorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 19, true);
			this.ShipperMatchingErrorCheckBox.Name = "ShipperMatchingErrorCheckBox";
			this.ShipperMatchingErrorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 16, true);
			this.ShipperMatchingErrorCheckBox.TabIndex = 0;
			this.ShipperMatchingErrorCheckBox.Text = "Is Shipper Matching Error";
			this.ShipperMatchingErrorCheckBox.UseVisualStyleBackColor = true;
			this.ShipperMatchingErrorCheckBox.Click += new System.EventHandler(this.ShipperMatchingError_Clicked);
			// 
			// UPSUserControl
			// 
			this.Controls.Add(this.ShipperMatchingErrorGroupBox);
			this.Controls.Add(this.OtherGroupBox);
			this.Controls.Add(this.AuditGroupBox);
			this.Controls.Add(this.RefundGroupBox);
			this.Controls.Add(this.zGroupBox1);
			this.Controls.Add(this.RelatedUPECusHAWBsGrid);
			this.Name = "UPSUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 435, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RelatedUPECusHAWBsGrid)).EndInit();
			this.RelatedUPECusHAWBsGrid.ResumeLayout(false);
			this.RelatedUPECusHAWBsGrid.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.RefundGroupBox.ResumeLayout(false);
			this.RefundGroupBox.PerformLayout();
			this.AuditGroupBox.ResumeLayout(false);
			this.AuditGroupBox.PerformLayout();
			this.OtherGroupBox.ResumeLayout(false);
			this.OtherGroupBox.PerformLayout();
			this.ShipperMatchingErrorGroupBox.ResumeLayout(false);
			this.ShipperMatchingErrorGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
