namespace Enterprise.Customs.KR.GUI
{
	partial class RefundDetailsUserControl
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
			this.PaidLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RefundLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PenaltyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DutyAmountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidDutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LiquorTaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidLiquorTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SpecialConsumptionTaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidSpecialConsumptionTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TransportTaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidTransportTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EducationTaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidEducationTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AgricultureTaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidAgricultureTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VATLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ValueForVATLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VATExemptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPenaltyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidTotalPenaltyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyForLateDeclarationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PenaltyForMissedDeclarationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PenaltyForLatePaymentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NonDutyTaxRevenueLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LatePaymentPenaltyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PaidLatePaymentPenaltyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalTaxLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TotalPaidAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundDutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundLiquorTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundSpecialConsumptionTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundTransportTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundEducationTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundAgricultureTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RefundVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ValueForVATCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VATExemptionCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyForLateDeclarationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyForMissedDeclarationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyForLatePaymentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NonDutyTaxRevenueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalRefundAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundDutyAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundLiquorTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundTransportTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundEducationTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PenaltyToRefundVATAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalPenaltyToRefundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.TotalLateRefundAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.Refund2Label = new Enterprise.ZArchitecture.ZLabel();
			this.EmptyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.Empty2Label = new Enterprise.ZArchitecture.ZLabel();
			this.Empty3Label = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.CusReconDeclaration);
			// 
			// PaidLabel
			// 
			this.PaidLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0a7e257e-e6b8-4906-a326-6824c2162d44", "Paid");
			this.PaidLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PaidLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 17, true);
			this.PaidLabel.Name = "PaidLabel";
			this.PaidLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 19, true);
			this.PaidLabel.TabIndex = 0;
			this.PaidLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.PaidLabel.UseMnemonic = false;
			// 
			// RefundLabel
			// 
			this.RefundLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("dd7738e2-478f-46cd-b6cb-036e79c478a0", "Refund Amount");
			this.RefundLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RefundLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 18, true);
			this.RefundLabel.Name = "RefundLabel";
			this.RefundLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.RefundLabel.TabIndex = 1;
			this.RefundLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.RefundLabel.UseMnemonic = false;
			// 
			// PenaltyLabel
			// 
			this.PenaltyLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("51becacd-6c07-4dcb-8643-3ca9c205dbe5", "Penalty to Refund");
			this.PenaltyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PenaltyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 16, true);
			this.PenaltyLabel.Name = "PenaltyLabel";
			this.PenaltyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 19, true);
			this.PenaltyLabel.TabIndex = 2;
			this.PenaltyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.PenaltyLabel.UseMnemonic = false;
			// 
			// DutyAmountLabel
			// 
			this.DutyAmountLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("532f8b2a-a916-4416-bab3-da6e62518749", "Duty Amount");
			this.DutyAmountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DutyAmountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 65, true);
			this.DutyAmountLabel.Name = "DutyAmountLabel";
			this.DutyAmountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.DutyAmountLabel.TabIndex = 3;
			this.DutyAmountLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.DutyAmountLabel.UseMnemonic = false;
			// 
			// PaidDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidDutyAmountCalcEdit, "CusReconEntryLines.PaidDutyAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidDutyAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidDutyAmountCalcEdit, false);
			this.PaidDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 68, true);
			this.PaidDutyAmountCalcEdit.Name = "PaidDutyAmountCalcEdit";
			this.PaidDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidDutyAmountCalcEdit.TabIndex = 4;
			this.PaidDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidDutyAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// LiquorTaxLabel
			// 
			this.LiquorTaxLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("6672a13b-271a-4244-8231-5c916b4bfa4e", "Liquor Tax");
			this.LiquorTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LiquorTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 92, true);
			this.LiquorTaxLabel.Name = "LiquorTaxLabel";
			this.LiquorTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 22, true);
			this.LiquorTaxLabel.TabIndex = 5;
			this.LiquorTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.LiquorTaxLabel.UseMnemonic = false;
			// 
			// PaidLiquorTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidLiquorTaxAmountCalcEdit, "CusReconEntryLines.PaidLiquorTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidLiquorTaxAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidLiquorTaxAmountCalcEdit, false);
			this.PaidLiquorTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 94, true);
			this.PaidLiquorTaxAmountCalcEdit.Name = "PaidLiquorTaxAmountCalcEdit";
			this.PaidLiquorTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidLiquorTaxAmountCalcEdit.TabIndex = 6;
			this.PaidLiquorTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidLiquorTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// SpecialConsumptionTaxLabel
			// 
			this.SpecialConsumptionTaxLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4636d8c3-bd09-4949-bd07-15c1f6cbe14b", "Special Consumption Tax");
			this.SpecialConsumptionTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SpecialConsumptionTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 122, true);
			this.SpecialConsumptionTaxLabel.Name = "SpecialConsumptionTaxLabel";
			this.SpecialConsumptionTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
			this.SpecialConsumptionTaxLabel.TabIndex = 7;
			this.SpecialConsumptionTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.SpecialConsumptionTaxLabel.UseMnemonic = false;
			// 
			// PaidSpecialConsumptionTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidSpecialConsumptionTaxAmountCalcEdit, "CusReconEntryLines.PaidSpecialConsumptionTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidSpecialConsumptionTaxAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidSpecialConsumptionTaxAmountCalcEdit, false);
			this.PaidSpecialConsumptionTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 122, true);
			this.PaidSpecialConsumptionTaxAmountCalcEdit.Name = "PaidSpecialConsumptionTaxAmountCalcEdit";
			this.PaidSpecialConsumptionTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidSpecialConsumptionTaxAmountCalcEdit.TabIndex = 8;
			this.PaidSpecialConsumptionTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidSpecialConsumptionTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TransportTaxLabel
			// 
			this.TransportTaxLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("95ec2373-27b3-4baf-a5f1-1af6960ee7fe", "Transportation Tax");
			this.TransportTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TransportTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 152, true);
			this.TransportTaxLabel.Name = "TransportTaxLabel";
			this.TransportTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.TransportTaxLabel.TabIndex = 9;
			this.TransportTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.TransportTaxLabel.UseMnemonic = false;
			// 
			// PaidTransportTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidTransportTaxAmountCalcEdit, "CusReconEntryLines.PaidTransportTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidTransportTaxAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidTransportTaxAmountCalcEdit, false);
			this.PaidTransportTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 152, true);
			this.PaidTransportTaxAmountCalcEdit.Name = "PaidTransportTaxAmountCalcEdit";
			this.PaidTransportTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidTransportTaxAmountCalcEdit.TabIndex = 10;
			this.PaidTransportTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidTransportTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// EducationTaxLabel
			// 
			this.EducationTaxLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d9b12fc6-9c89-4a62-a9ed-036ec9d96920", "Education Tax");
			this.EducationTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EducationTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 179, true);
			this.EducationTaxLabel.Name = "EducationTaxLabel";
			this.EducationTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.EducationTaxLabel.TabIndex = 11;
			this.EducationTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.EducationTaxLabel.UseMnemonic = false;
			// 
			// PaidEducationTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidEducationTaxAmountCalcEdit, "CusReconEntryLines.PaidEducationTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidEducationTaxAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidEducationTaxAmountCalcEdit, false);
			this.PaidEducationTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 179, true);
			this.PaidEducationTaxAmountCalcEdit.Name = "PaidEducationTaxAmountCalcEdit";
			this.PaidEducationTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidEducationTaxAmountCalcEdit.TabIndex = 12;
			this.PaidEducationTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidEducationTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// AgricultureTaxLabel
			// 
			this.AgricultureTaxLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("912639e8-adb5-49e7-b614-ac98143ed37b", "Agriculture Tax");
			this.AgricultureTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AgricultureTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 208, true);
			this.AgricultureTaxLabel.Name = "AgricultureTaxLabel";
			this.AgricultureTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.AgricultureTaxLabel.TabIndex = 13;
			this.AgricultureTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AgricultureTaxLabel.UseMnemonic = false;
			// 
			// PaidAgricultureTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidAgricultureTaxAmountCalcEdit, "CusReconEntryLines.PaidAgricultureTaxAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidAgricultureTaxAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidAgricultureTaxAmountCalcEdit, false);
			this.PaidAgricultureTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 206, true);
			this.PaidAgricultureTaxAmountCalcEdit.Name = "PaidAgricultureTaxAmountCalcEdit";
			this.PaidAgricultureTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidAgricultureTaxAmountCalcEdit.TabIndex = 14;
			this.PaidAgricultureTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidAgricultureTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// VATLabel
			// 
			this.VATLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("73a420f1-e559-461a-a909-2859cc2bad54", "VAT");
			this.VATLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VATLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 233, true);
			this.VATLabel.Name = "VATLabel";
			this.VATLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.VATLabel.TabIndex = 15;
			this.VATLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.VATLabel.UseMnemonic = false;
			// 
			// ValueForVATLabel
			// 
			this.ValueForVATLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2e31000b-95c7-472e-98ac-1ad48aa4729b", "Value For VAT");
			this.ValueForVATLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ValueForVATLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 43, true);
			this.ValueForVATLabel.Name = "ValueForVATLabel";
			this.ValueForVATLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.ValueForVATLabel.TabIndex = 16;
			this.ValueForVATLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ValueForVATLabel.UseMnemonic = false;
			// 
			// VATExemptionLabel
			// 
			this.VATExemptionLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("57493e3f-d7e0-4d2e-90cd-89d72c0def5d", "VAT Exemption Value");
			this.VATExemptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VATExemptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 68, true);
			this.VATExemptionLabel.Name = "VATExemptionLabel";
			this.VATExemptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.VATExemptionLabel.TabIndex = 17;
			this.VATExemptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.VATExemptionLabel.UseMnemonic = false;
			// 
			// PaidVATAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidVATAmountCalcEdit, "CusReconEntryLines.PaidVATAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidVATAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidVATAmountCalcEdit, false);
			this.PaidVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 233, true);
			this.PaidVATAmountCalcEdit.Name = "PaidVATAmountCalcEdit";
			this.PaidVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidVATAmountCalcEdit.TabIndex = 18;
			this.PaidVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidVATAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalPenaltyLabel
			// 
			this.TotalPenaltyLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("e11bedac-c47d-4084-aef9-52abf408f50c", "Total Interest and Penalty");
			this.TotalPenaltyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalPenaltyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 262, true);
			this.TotalPenaltyLabel.Name = "TotalPenaltyLabel";
			this.TotalPenaltyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.TotalPenaltyLabel.TabIndex = 19;
			this.TotalPenaltyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.TotalPenaltyLabel.UseMnemonic = false;
			// 
			// PaidTotalPenaltyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidTotalPenaltyCalcEdit, "CusReconEntryLines.PaidTotalPenalty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidTotalPenalty)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidTotalPenaltyCalcEdit, false);
			this.PaidTotalPenaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 262, true);
			this.PaidTotalPenaltyCalcEdit.Name = "PaidTotalPenaltyCalcEdit";
			this.PaidTotalPenaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidTotalPenaltyCalcEdit.TabIndex = 20;
			this.PaidTotalPenaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidTotalPenaltyCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyForLateDeclarationLabel
			// 
			this.PenaltyForLateDeclarationLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("71123dff-c24e-4500-aad3-8ab314db78ef", "Penalty For Late Declaration");
			this.PenaltyForLateDeclarationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PenaltyForLateDeclarationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 102, true);
			this.PenaltyForLateDeclarationLabel.Name = "PenaltyForLateDeclarationLabel";
			this.PenaltyForLateDeclarationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.PenaltyForLateDeclarationLabel.TabIndex = 21;
			this.PenaltyForLateDeclarationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PenaltyForLateDeclarationLabel.UseMnemonic = false;
			// 
			// PenaltyForMissedDeclarationLabel
			// 
			this.PenaltyForMissedDeclarationLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("265cfb0f-4c25-47bd-a815-98df7ab2fc75", "Penalty For Missed Declaration");
			this.PenaltyForMissedDeclarationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PenaltyForMissedDeclarationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 127, true);
			this.PenaltyForMissedDeclarationLabel.Name = "PenaltyForMissedDeclarationLabel";
			this.PenaltyForMissedDeclarationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.PenaltyForMissedDeclarationLabel.TabIndex = 22;
			this.PenaltyForMissedDeclarationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PenaltyForMissedDeclarationLabel.UseMnemonic = false;
			// 
			// PenaltyForLatePaymentLabel
			// 
			this.PenaltyForLatePaymentLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("51f94974-c062-45bb-8660-a9db85c21f94", "Penalty For Late Payment");
			this.PenaltyForLatePaymentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PenaltyForLatePaymentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 150, true);
			this.PenaltyForLatePaymentLabel.Name = "PenaltyForLatePaymentLabel";
			this.PenaltyForLatePaymentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.PenaltyForLatePaymentLabel.TabIndex = 23;
			this.PenaltyForLatePaymentLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PenaltyForLatePaymentLabel.UseMnemonic = false;
			// 
			// NonDutyTaxRevenueLabel
			// 
			this.NonDutyTaxRevenueLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("1566e1c5-4ae4-4b04-a8de-78e3cea6ceb1", "Non-Duty Tax Revenue");
			this.NonDutyTaxRevenueLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.NonDutyTaxRevenueLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(712, 171, true);
			this.NonDutyTaxRevenueLabel.Name = "NonDutyTaxRevenueLabel";
			this.NonDutyTaxRevenueLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.NonDutyTaxRevenueLabel.TabIndex = 24;
			this.NonDutyTaxRevenueLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.NonDutyTaxRevenueLabel.UseMnemonic = false;
			// 
			// LatePaymentPenaltyLabel
			// 
			this.LatePaymentPenaltyLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2f9a3b19-77ec-454a-a16e-085ef2646a0d", "Total Late & Missed Dec Penalty");
			this.LatePaymentPenaltyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LatePaymentPenaltyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 295, true);
			this.LatePaymentPenaltyLabel.Name = "LatePaymentPenaltyLabel";
			this.LatePaymentPenaltyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.LatePaymentPenaltyLabel.TabIndex = 25;
			this.LatePaymentPenaltyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.LatePaymentPenaltyLabel.UseMnemonic = false;
			// 
			// PaidLatePaymentPenaltyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaidLatePaymentPenaltyCalcEdit, "CusReconEntryLines.PaidLatePaymentPenalty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PaidLatePaymentPenalty)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PaidLatePaymentPenaltyCalcEdit, false);
			this.PaidLatePaymentPenaltyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 294, true);
			this.PaidLatePaymentPenaltyCalcEdit.Name = "PaidLatePaymentPenaltyCalcEdit";
			this.PaidLatePaymentPenaltyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PaidLatePaymentPenaltyCalcEdit.TabIndex = 26;
			this.PaidLatePaymentPenaltyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PaidLatePaymentPenaltyCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalTaxLabel
			// 
			this.TotalTaxLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5db3690a-c93d-4440-a6be-31c2636e1350", "Total Tax");
			this.TotalTaxLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalTaxLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 43, true);
			this.TotalTaxLabel.Name = "TotalTaxLabel";
			this.TotalTaxLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 15, true);
			this.TotalTaxLabel.TabIndex = 27;
			this.TotalTaxLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.TotalTaxLabel.UseMnemonic = false;
			// 
			// TotalPaidAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPaidAmountCalcEdit, "CusReconEntryLines.TotalPaid");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).TotalPaid)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalPaidAmountCalcEdit, false);
			this.TotalPaidAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(227, 43, true);
			this.TotalPaidAmountCalcEdit.Name = "TotalPaidAmountCalcEdit";
			this.TotalPaidAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TotalPaidAmountCalcEdit.TabIndex = 28;
			this.TotalPaidAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPaidAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundDutyAmountCalcEdit, "CusReconEntryLines.DutyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).DutyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundDutyAmountCalcEdit, false);
			this.RefundDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 68, true);
			this.RefundDutyAmountCalcEdit.Name = "RefundDutyAmountCalcEdit";
			this.RefundDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundDutyAmountCalcEdit.TabIndex = 29;
			this.RefundDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundDutyAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundLiquorTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundLiquorTaxAmountCalcEdit, "CusReconEntryLines.LQTToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).LQTToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundLiquorTaxAmountCalcEdit, false);
			this.RefundLiquorTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 96, true);
			this.RefundLiquorTaxAmountCalcEdit.Name = "RefundLiquorTaxAmountCalcEdit";
			this.RefundLiquorTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundLiquorTaxAmountCalcEdit.TabIndex = 30;
			this.RefundLiquorTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundLiquorTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundSpecialConsumptionTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundSpecialConsumptionTaxAmountCalcEdit, "CusReconEntryLines.SCTToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).SCTToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundSpecialConsumptionTaxAmountCalcEdit, false);
			this.RefundSpecialConsumptionTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 122, true);
			this.RefundSpecialConsumptionTaxAmountCalcEdit.Name = "RefundSpecialConsumptionTaxAmountCalcEdit";
			this.RefundSpecialConsumptionTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundSpecialConsumptionTaxAmountCalcEdit.TabIndex = 31;
			this.RefundSpecialConsumptionTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundSpecialConsumptionTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundTransportTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundTransportTaxAmountCalcEdit, "CusReconEntryLines.TRTToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).TRTToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundTransportTaxAmountCalcEdit, false);
			this.RefundTransportTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 152, true);
			this.RefundTransportTaxAmountCalcEdit.Name = "RefundTransportTaxAmountCalcEdit";
			this.RefundTransportTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundTransportTaxAmountCalcEdit.TabIndex = 32;
			this.RefundTransportTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundTransportTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundEducationTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundEducationTaxAmountCalcEdit, "CusReconEntryLines.EDTToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).EDTToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundEducationTaxAmountCalcEdit, false);
			this.RefundEducationTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 179, true);
			this.RefundEducationTaxAmountCalcEdit.Name = "RefundEducationTaxAmountCalcEdit";
			this.RefundEducationTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundEducationTaxAmountCalcEdit.TabIndex = 33;
			this.RefundEducationTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundEducationTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundAgricultureTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundAgricultureTaxAmountCalcEdit, "CusReconEntryLines.AGTToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).AGTToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundAgricultureTaxAmountCalcEdit, false);
			this.RefundAgricultureTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 205, true);
			this.RefundAgricultureTaxAmountCalcEdit.Name = "RefundAgricultureTaxAmountCalcEdit";
			this.RefundAgricultureTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundAgricultureTaxAmountCalcEdit.TabIndex = 34;
			this.RefundAgricultureTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundAgricultureTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// RefundVATAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RefundVATAmountCalcEdit, "CusReconEntryLines.VATToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).VATToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefundVATAmountCalcEdit, false);
			this.RefundVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 230, true);
			this.RefundVATAmountCalcEdit.Name = "RefundVATAmountCalcEdit";
			this.RefundVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.RefundVATAmountCalcEdit.TabIndex = 35;
			this.RefundVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.RefundVATAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// ValueForVATCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValueForVATCalcEdit, "CusReconEntryLines.ValueForVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).ValueForVAT)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ValueForVATCalcEdit, false);
			this.ValueForVATCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 46, true);
			this.ValueForVATCalcEdit.Name = "ValueForVATCalcEdit";
			this.ValueForVATCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.ValueForVATCalcEdit.TabIndex = 36;
			this.ValueForVATCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ValueForVATCalcEdit.TrackDisposedAccess = true;
			// 
			// VATExemptionCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VATExemptionCalcEdit, "CusReconEntryLines.VATExemptionValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).VATExemptionValue)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.VATExemptionCalcEdit, false);
			this.VATExemptionCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 71, true);
			this.VATExemptionCalcEdit.Name = "VATExemptionCalcEdit";
			this.VATExemptionCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.VATExemptionCalcEdit.TabIndex = 37;
			this.VATExemptionCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.VATExemptionCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyForLateDeclarationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyForLateDeclarationCalcEdit, "CusReconEntryLines.PenaltyLateDecToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PenaltyLateDecToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyForLateDeclarationCalcEdit, false);
			this.PenaltyForLateDeclarationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 102, true);
			this.PenaltyForLateDeclarationCalcEdit.Name = "PenaltyForLateDeclarationCalcEdit";
			this.PenaltyForLateDeclarationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyForLateDeclarationCalcEdit.TabIndex = 38;
			this.PenaltyForLateDeclarationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyForLateDeclarationCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyForMissedDeclarationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyForMissedDeclarationCalcEdit, "CusReconEntryLines.PenaltyMissedDecToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PenaltyMissedDecToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyForMissedDeclarationCalcEdit, false);
			this.PenaltyForMissedDeclarationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 127, true);
			this.PenaltyForMissedDeclarationCalcEdit.Name = "PenaltyForMissedDeclarationCalcEdit";
			this.PenaltyForMissedDeclarationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyForMissedDeclarationCalcEdit.TabIndex = 39;
			this.PenaltyForMissedDeclarationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyForMissedDeclarationCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyForLatePaymentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyForLatePaymentCalcEdit, "CusReconEntryLines.PenaltyLatePaymentToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).PenaltyLatePaymentToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyForLatePaymentCalcEdit, false);
			this.PenaltyForLatePaymentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 150, true);
			this.PenaltyForLatePaymentCalcEdit.Name = "PenaltyForLatePaymentCalcEdit";
			this.PenaltyForLatePaymentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyForLatePaymentCalcEdit.TabIndex = 40;
			this.PenaltyForLatePaymentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyForLatePaymentCalcEdit.TrackDisposedAccess = true;
			// 
			// NonDutyTaxRevenueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NonDutyTaxRevenueCalcEdit, "CusReconEntryLines.NonDutyTaxRevenueToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).NonDutyTaxRevenueToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.NonDutyTaxRevenueCalcEdit, false);
			this.NonDutyTaxRevenueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 174, true);
			this.NonDutyTaxRevenueCalcEdit.Name = "NonDutyTaxRevenueCalcEdit";
			this.NonDutyTaxRevenueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.NonDutyTaxRevenueCalcEdit.TabIndex = 41;
			this.NonDutyTaxRevenueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NonDutyTaxRevenueCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalRefundAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalRefundAmountCalcEdit, "CusReconEntryLines.TotalRefundAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).TotalRefundAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalRefundAmountCalcEdit, false);
			this.TotalRefundAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 43, true);
			this.TotalRefundAmountCalcEdit.Name = "TotalRefundAmountCalcEdit";
			this.TotalRefundAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TotalRefundAmountCalcEdit.TabIndex = 42;
			this.TotalRefundAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalRefundAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundDutyAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundDutyAmountCalcEdit, "CusReconEntryLines.DutyPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).DutyPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundDutyAmountCalcEdit, false);
			this.PenaltyToRefundDutyAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 68, true);
			this.PenaltyToRefundDutyAmountCalcEdit.Name = "PenaltyToRefundDutyAmountCalcEdit";
			this.PenaltyToRefundDutyAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundDutyAmountCalcEdit.TabIndex = 43;
			this.PenaltyToRefundDutyAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundDutyAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundLiquorTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundLiquorTaxAmountCalcEdit, "CusReconEntryLines.LQTPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).LQTPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundLiquorTaxAmountCalcEdit, false);
			this.PenaltyToRefundLiquorTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 96, true);
			this.PenaltyToRefundLiquorTaxAmountCalcEdit.Name = "PenaltyToRefundLiquorTaxAmountCalcEdit";
			this.PenaltyToRefundLiquorTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundLiquorTaxAmountCalcEdit.TabIndex = 44;
			this.PenaltyToRefundLiquorTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundLiquorTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit, "CusReconEntryLines.SCTPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).SCTPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit, false);
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 122, true);
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit.Name = "PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit";
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit.TabIndex = 45;
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundTransportTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundTransportTaxAmountCalcEdit, "CusReconEntryLines.TRTPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).TRTPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundTransportTaxAmountCalcEdit, false);
			this.PenaltyToRefundTransportTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 149, true);
			this.PenaltyToRefundTransportTaxAmountCalcEdit.Name = "PenaltyToRefundTransportTaxAmountCalcEdit";
			this.PenaltyToRefundTransportTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundTransportTaxAmountCalcEdit.TabIndex = 46;
			this.PenaltyToRefundTransportTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundTransportTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundEducationTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundEducationTaxAmountCalcEdit, "CusReconEntryLines.EDTPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).EDTPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundEducationTaxAmountCalcEdit, false);
			this.PenaltyToRefundEducationTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 176, true);
			this.PenaltyToRefundEducationTaxAmountCalcEdit.Name = "PenaltyToRefundEducationTaxAmountCalcEdit";
			this.PenaltyToRefundEducationTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundEducationTaxAmountCalcEdit.TabIndex = 47;
			this.PenaltyToRefundEducationTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundEducationTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundAgricultureTaxAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundAgricultureTaxAmountCalcEdit, "CusReconEntryLines.AGTPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).AGTPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundAgricultureTaxAmountCalcEdit, false);
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 205, true);
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit.Name = "PenaltyToRefundAgricultureTaxAmountCalcEdit";
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit.TabIndex = 48;
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundAgricultureTaxAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// PenaltyToRefundVATAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PenaltyToRefundVATAmountCalcEdit, "CusReconEntryLines.VATPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).VATPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PenaltyToRefundVATAmountCalcEdit, false);
			this.PenaltyToRefundVATAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 230, true);
			this.PenaltyToRefundVATAmountCalcEdit.Name = "PenaltyToRefundVATAmountCalcEdit";
			this.PenaltyToRefundVATAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.PenaltyToRefundVATAmountCalcEdit.TabIndex = 49;
			this.PenaltyToRefundVATAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PenaltyToRefundVATAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalPenaltyToRefundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalPenaltyToRefundCalcEdit, "CusReconEntryLines.TotalPenaltyToRefund");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).TotalPenaltyToRefund)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalPenaltyToRefundCalcEdit, false);
			this.TotalPenaltyToRefundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 262, true);
			this.TotalPenaltyToRefundCalcEdit.Name = "TotalPenaltyToRefundCalcEdit";
			this.TotalPenaltyToRefundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TotalPenaltyToRefundCalcEdit.TabIndex = 50;
			this.TotalPenaltyToRefundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalPenaltyToRefundCalcEdit.TrackDisposedAccess = true;
			// 
			// TotalLateRefundAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TotalLateRefundAmountCalcEdit, "CusReconEntryLines.TotalLateRefundAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.CusReconEntryLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.CusReconDeclaration)(null)).CusReconEntryLines)).SyncRoot)).TotalLateRefundAmount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TotalLateRefundAmountCalcEdit, false);
			this.TotalLateRefundAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 294, true);
			this.TotalLateRefundAmountCalcEdit.Name = "TotalLateRefundAmountCalcEdit";
			this.TotalLateRefundAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.TotalLateRefundAmountCalcEdit.TabIndex = 51;
			this.TotalLateRefundAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TotalLateRefundAmountCalcEdit.TrackDisposedAccess = true;
			// 
			// Refund2Label
			// 
			this.Refund2Label.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("dd7738e2-478f-46cd-b6cb-036e79c478a0", "Refund Amount");
			this.Refund2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Refund2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(897, 17, true);
			this.Refund2Label.Name = "Refund2Label";
			this.Refund2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 17, true);
			this.Refund2Label.TabIndex = 52;
			this.Refund2Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Refund2Label.UseMnemonic = false;
			// 
			// EmptyLabel
			// 
			this.EmptyLabel.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("a72b0266-561b-40ba-a2fe-80bb1ee483d0", " ");
			this.EmptyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.EmptyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 15, true);
			this.EmptyLabel.Name = "EmptyLabel";
			this.EmptyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 19, true);
			this.EmptyLabel.TabIndex = 53;
			this.EmptyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.EmptyLabel.UseMnemonic = false;
			// 
			// Empty2Label
			// 
			this.Empty2Label.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2e0f4ad1-d8db-4720-a14e-4c2e446b6f55", " ");
			this.Empty2Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Empty2Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 263, true);
			this.Empty2Label.Name = "Empty2Label";
			this.Empty2Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 19, true);
			this.Empty2Label.TabIndex = 54;
			this.Empty2Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Empty2Label.UseMnemonic = false;
			// 
			// Empty3Label
			// 
			this.Empty3Label.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("d84c521f-6d96-49a3-bd1c-2ee5cbe3347e", " ");
			this.Empty3Label.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.Empty3Label.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(542, 39, true);
			this.Empty3Label.Name = "Empty3Label";
			this.Empty3Label.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 19, true);
			this.Empty3Label.TabIndex = 55;
			this.Empty3Label.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.Empty3Label.UseMnemonic = false;
			// 
			// RefundDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.Empty3Label);
			this.Controls.Add(this.Empty2Label);
			this.Controls.Add(this.EmptyLabel);
			this.Controls.Add(this.Refund2Label);
			this.Controls.Add(this.TotalLateRefundAmountCalcEdit);
			this.Controls.Add(this.TotalPenaltyToRefundCalcEdit);
			this.Controls.Add(this.PenaltyToRefundVATAmountCalcEdit);
			this.Controls.Add(this.PenaltyToRefundAgricultureTaxAmountCalcEdit);
			this.Controls.Add(this.PenaltyToRefundEducationTaxAmountCalcEdit);
			this.Controls.Add(this.PenaltyToRefundTransportTaxAmountCalcEdit);
			this.Controls.Add(this.PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit);
			this.Controls.Add(this.PenaltyToRefundLiquorTaxAmountCalcEdit);
			this.Controls.Add(this.PenaltyToRefundDutyAmountCalcEdit);
			this.Controls.Add(this.TotalRefundAmountCalcEdit);
			this.Controls.Add(this.NonDutyTaxRevenueCalcEdit);
			this.Controls.Add(this.PenaltyForLatePaymentCalcEdit);
			this.Controls.Add(this.PenaltyForMissedDeclarationCalcEdit);
			this.Controls.Add(this.PenaltyForLateDeclarationCalcEdit);
			this.Controls.Add(this.VATExemptionCalcEdit);
			this.Controls.Add(this.ValueForVATCalcEdit);
			this.Controls.Add(this.RefundVATAmountCalcEdit);
			this.Controls.Add(this.RefundAgricultureTaxAmountCalcEdit);
			this.Controls.Add(this.RefundEducationTaxAmountCalcEdit);
			this.Controls.Add(this.RefundTransportTaxAmountCalcEdit);
			this.Controls.Add(this.RefundSpecialConsumptionTaxAmountCalcEdit);
			this.Controls.Add(this.RefundLiquorTaxAmountCalcEdit);
			this.Controls.Add(this.RefundDutyAmountCalcEdit);
			this.Controls.Add(this.TotalPaidAmountCalcEdit);
			this.Controls.Add(this.TotalTaxLabel);
			this.Controls.Add(this.PaidLatePaymentPenaltyCalcEdit);
			this.Controls.Add(this.LatePaymentPenaltyLabel);
			this.Controls.Add(this.NonDutyTaxRevenueLabel);
			this.Controls.Add(this.PenaltyForLatePaymentLabel);
			this.Controls.Add(this.PenaltyForMissedDeclarationLabel);
			this.Controls.Add(this.PenaltyForLateDeclarationLabel);
			this.Controls.Add(this.PaidTotalPenaltyCalcEdit);
			this.Controls.Add(this.TotalPenaltyLabel);
			this.Controls.Add(this.PaidVATAmountCalcEdit);
			this.Controls.Add(this.VATExemptionLabel);
			this.Controls.Add(this.ValueForVATLabel);
			this.Controls.Add(this.VATLabel);
			this.Controls.Add(this.PaidAgricultureTaxAmountCalcEdit);
			this.Controls.Add(this.AgricultureTaxLabel);
			this.Controls.Add(this.PaidEducationTaxAmountCalcEdit);
			this.Controls.Add(this.EducationTaxLabel);
			this.Controls.Add(this.PaidTransportTaxAmountCalcEdit);
			this.Controls.Add(this.TransportTaxLabel);
			this.Controls.Add(this.PaidSpecialConsumptionTaxAmountCalcEdit);
			this.Controls.Add(this.SpecialConsumptionTaxLabel);
			this.Controls.Add(this.PaidLiquorTaxAmountCalcEdit);
			this.Controls.Add(this.LiquorTaxLabel);
			this.Controls.Add(this.PaidDutyAmountCalcEdit);
			this.Controls.Add(this.DutyAmountLabel);
			this.Controls.Add(this.PenaltyLabel);
			this.Controls.Add(this.RefundLabel);
			this.Controls.Add(this.PaidLabel);
			this.Name = "RefundDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 355, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZLabel RefundLabel;
		internal ZArchitecture.ZLabel PenaltyLabel;
		internal ZArchitecture.ZLabel DutyAmountLabel;
		internal ZArchitecture.ZCalcEdit PaidDutyAmountCalcEdit;
		internal ZArchitecture.ZLabel LiquorTaxLabel;
		internal ZArchitecture.ZCalcEdit PaidLiquorTaxAmountCalcEdit;
		internal ZArchitecture.ZLabel SpecialConsumptionTaxLabel;
		internal ZArchitecture.ZCalcEdit PaidSpecialConsumptionTaxAmountCalcEdit;
		internal ZArchitecture.ZLabel TransportTaxLabel;
		internal ZArchitecture.ZCalcEdit PaidTransportTaxAmountCalcEdit;
		internal ZArchitecture.ZLabel EducationTaxLabel;
		internal ZArchitecture.ZCalcEdit PaidEducationTaxAmountCalcEdit;
		internal ZArchitecture.ZLabel AgricultureTaxLabel;
		internal ZArchitecture.ZCalcEdit PaidAgricultureTaxAmountCalcEdit;
		internal ZArchitecture.ZLabel VATLabel;
		internal ZArchitecture.ZLabel ValueForVATLabel;
		internal ZArchitecture.ZLabel VATExemptionLabel;
		internal ZArchitecture.ZCalcEdit PaidVATAmountCalcEdit;
		internal ZArchitecture.ZLabel TotalPenaltyLabel;
		internal ZArchitecture.ZCalcEdit PaidTotalPenaltyCalcEdit;
		internal ZArchitecture.ZLabel PenaltyForLateDeclarationLabel;
		internal ZArchitecture.ZLabel PenaltyForMissedDeclarationLabel;
		internal ZArchitecture.ZLabel PenaltyForLatePaymentLabel;
		internal ZArchitecture.ZLabel NonDutyTaxRevenueLabel;
		internal ZArchitecture.ZLabel LatePaymentPenaltyLabel;
		internal ZArchitecture.ZCalcEdit PaidLatePaymentPenaltyCalcEdit;
		internal ZArchitecture.ZLabel TotalTaxLabel;
		internal ZArchitecture.ZCalcEdit TotalPaidAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundDutyAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundLiquorTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundSpecialConsumptionTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundTransportTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundEducationTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundAgricultureTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit RefundVATAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit ValueForVATCalcEdit;
		internal ZArchitecture.ZCalcEdit VATExemptionCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyForLateDeclarationCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyForMissedDeclarationCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyForLatePaymentCalcEdit;
		internal ZArchitecture.ZCalcEdit NonDutyTaxRevenueCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalRefundAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundDutyAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundLiquorTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundSpecialConsumptionTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundTransportTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundEducationTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundAgricultureTaxAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit PenaltyToRefundVATAmountCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalPenaltyToRefundCalcEdit;
		internal ZArchitecture.ZCalcEdit TotalLateRefundAmountCalcEdit;
		internal ZArchitecture.ZLabel Refund2Label;
		internal ZArchitecture.ZLabel PaidLabel;
		internal ZArchitecture.ZLabel EmptyLabel;
		internal ZArchitecture.ZLabel Empty2Label;
		internal ZArchitecture.ZLabel Empty3Label;
	}
}
