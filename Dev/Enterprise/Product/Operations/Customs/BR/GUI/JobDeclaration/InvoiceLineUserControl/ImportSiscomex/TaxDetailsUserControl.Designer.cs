namespace Enterprise.Customs.BR.GUI
{
	partial class TaxDetailsUserControl
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
			this.DutyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RegimeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TemporaryAdmissionReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyLegalBaseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DutyTaxRegimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReductionDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FTADutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutyRateIsOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RatePreferenceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReducedDutyRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReductionMarginRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FTAMarginRateValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DutyVigentRateValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PisCofinsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CofinsRateIsOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PisRateIsOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.PisCofinsTaxRegimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CofinsVigentRateValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PisVigentRateValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PisCofinsLegalBaseDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IPIGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IPIRateIsOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IPILegalBasisGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IPITaxBenefitLegalActYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IPITaxBenefitLegalActNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IPITaxBenefitLegalActIssuingBodyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IPITaxBenefitLegalActTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IPITaxRegimeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IPIVigentRateValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ComplementaryNoteCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AdditionalTariffsGridLayout = new Enterprise.Customs.BR.GUI.AdditionalTariffsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DutyGroupBox.SuspendLayout();
			this.RegimeGroupBox.SuspendLayout();
			this.TemporaryAdmissionReasonDropEdit.SuspendLayout();
			this.DutyLegalBaseDropEdit.SuspendLayout();
			this.DutyTaxRegimeDropEdit.SuspendLayout();
			this.RateGroupBox.SuspendLayout();
			this.RatePreferenceDropEdit.SuspendLayout();
			this.PisCofinsGroupBox.SuspendLayout();
			this.PisCofinsTaxRegimeDropEdit.SuspendLayout();
			this.PisCofinsLegalBaseDropEdit.SuspendLayout();
			this.IPIGroupBox.SuspendLayout();
			this.IPILegalBasisGroupBox.SuspendLayout();
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.SuspendLayout();
			this.IPITaxBenefitLegalActTypeDropEdit.SuspendLayout();
			this.IPITaxRegimeDropEdit.SuspendLayout();
			this.ComplementaryNoteCodeFindBox.SuspendLayout();
			this.AdditionalTariffsGridLayout.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobComInvoiceLine);
			//
			// DutyGroupBox
			// 
			this.DutyGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("89e20cf3-497f-43c7-aad9-5ed0be939ee8", "Duty");
			this.DutyGroupBox.Controls.Add(this.RegimeGroupBox);
			this.DutyGroupBox.Controls.Add(this.RateGroupBox);
			this.DutyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.DutyGroupBox.Name = "DutyGroupBox";
			this.DutyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 289, true);
			this.DutyGroupBox.TabIndex = 1;
			this.DutyGroupBox.TabStop = false;
			//
			// RegimeGroupBox
			//
			this.RegimeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RegimeGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("E3C637E4-B5DF-4AFB-A2D8-4D81B37CFD89", "Regime");
			this.RegimeGroupBox.Controls.Add(this.TemporaryAdmissionReasonDropEdit);
			this.RegimeGroupBox.Controls.Add(this.DutyLegalBaseDropEdit);
			this.RegimeGroupBox.Controls.Add(this.DutyTaxRegimeDropEdit);
			this.RegimeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 14, true);
			this.RegimeGroupBox.Name = "RegimeGroupBox";
			this.RegimeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 108, true);
			this.RegimeGroupBox.TabIndex = 0;
			this.RegimeGroupBox.TabStop = false;
			//
			// TemporaryAdmissionReasonDropEdit
			//
			this.TemporaryAdmissionReasonDropEdit.AllowDrop = true;
			this.TemporaryAdmissionReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TemporaryAdmissionReasonDropEdit, "JI_TemporaryAdmissionReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_TemporaryAdmissionReason)));
			this.TemporaryAdmissionReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 71, true);
			this.TemporaryAdmissionReasonDropEdit.Name = "TemporaryAdmissionReasonDropEdit";
			this.TemporaryAdmissionReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 38, true);
			this.TemporaryAdmissionReasonDropEdit.TabIndex = 2;
			//
			// DutyLegalBaseDropEdit
			//
			this.DutyLegalBaseDropEdit.AllowDrop = true;
			this.DutyLegalBaseDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DutyLegalBaseDropEdit, "DutyLegalBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DutyLegalBase)));
			this.DutyLegalBaseDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("71419169-5e5c-41db-97cf-f7d7a3045a4b", "Legal Base");
			this.DutyLegalBaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 45, true);
			this.DutyLegalBaseDropEdit.Name = "DutyLegalBaseDropEdit";
			this.DutyLegalBaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 38, true);
			this.DutyLegalBaseDropEdit.TabIndex = 1;
			//
			// DutyTaxRegimeDropEdit
			//
			this.DutyTaxRegimeDropEdit.AllowDrop = true;
			this.DutyTaxRegimeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DutyTaxRegimeDropEdit, "DutyTaxRegime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DutyTaxRegime)));
			this.DutyTaxRegimeDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("40ef4c56-c30c-448f-9ac7-14b181683da1", "Tax Regime");
			this.DutyTaxRegimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 19, true);
			this.DutyTaxRegimeDropEdit.Name = "DutyTaxRegimeDropEdit";
			this.DutyTaxRegimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 38, true);
			this.DutyTaxRegimeDropEdit.TabIndex = 0;
			//
			// RateGroupBox
			//
			this.RateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RateGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0B78ABF7-4835-4C31-9466-D8DFC190BF46", "Rate");
			this.RateGroupBox.Controls.Add(this.ReductionDutyRateCalcEdit);
			this.RateGroupBox.Controls.Add(this.FTADutyRateCalcEdit);
			this.RateGroupBox.Controls.Add(this.DutyRateIsOverriddenCheckBox);
			this.RateGroupBox.Controls.Add(this.RatePreferenceDropEdit);
			this.RateGroupBox.Controls.Add(this.ReducedDutyRateCalcEdit);
			this.RateGroupBox.Controls.Add(this.ReductionMarginRateCalcEdit);
			this.RateGroupBox.Controls.Add(this.FTAMarginRateValueCalcEdit);
			this.RateGroupBox.Controls.Add(this.DutyVigentRateValueCalcEdit);
			this.RateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 128, true);
			this.RateGroupBox.Name = "RateGroupBox";
			this.RateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 154, true);
			this.RateGroupBox.TabIndex = 1;
			this.RateGroupBox.TabStop = false;
			//
			// ReductionDutyRateCalcEdit
			//
			this.BindingSource.SetBindingMember(this.ReductionDutyRateCalcEdit, "ReductionDutyRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ReductionDutyRateValue)));
			this.ReductionDutyRateCalcEdit.DecimalPlaces = 2;
			this.ReductionDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 98, true);
			this.ReductionDutyRateCalcEdit.Name = "ReductionDutyRateCalcEdit";
			this.ReductionDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 38, true);
			this.ReductionDutyRateCalcEdit.TabIndex = 7;
			this.ReductionDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReductionDutyRateCalcEdit.TrackDisposedAccess = true;
			//
			// FTADutyRateCalcEdit
			//
			this.FTADutyRateCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FTADutyRateCalcEdit, "FTADutyRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).FTADutyRateValue)));
			this.FTADutyRateCalcEdit.DecimalPlaces = 2;
			this.FTADutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 72, true);
			this.FTADutyRateCalcEdit.Name = "FTADutyRateCalcEdit";
			this.FTADutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.FTADutyRateCalcEdit.TabIndex = 6;
			this.FTADutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.FTADutyRateCalcEdit.TrackDisposedAccess = true;
			//
			// DutyRateIsOverriddenCheckBox
			//
			this.BindingSource.SetBindingMember(this.DutyRateIsOverriddenCheckBox, "DutyRateIsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DutyRateIsOverridden)));
			this.DutyRateIsOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(273, 19, true);
			this.DutyRateIsOverriddenCheckBox.Name = "DutyRateIsOverriddenCheckBox";
			this.DutyRateIsOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 21, true);
			this.DutyRateIsOverriddenCheckBox.TabIndex = 1;
			this.DutyRateIsOverriddenCheckBox.UseVisualStyleBackColor = true;
			//
			// RatePreferenceDropEdit
			//
			this.RatePreferenceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RatePreferenceDropEdit, "JI_PrimaryPreference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_PrimaryPreference)));
			this.RatePreferenceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 20, true);
			this.RatePreferenceDropEdit.Name = "RatePreferenceDropEdit";
			this.RatePreferenceDropEdit.ShowDescriptionBox = false;
			this.RatePreferenceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.RatePreferenceDropEdit.TabIndex = 0;
			//
			// ReducedDutyRateCalcEdit
			//
			this.ReducedDutyRateCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReducedDutyRateCalcEdit, "ReducedDutyRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ReducedDutyRateValue)));
			this.ReducedDutyRateCalcEdit.DecimalPlaces = 2;
			this.ReducedDutyRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 124, true);
			this.ReducedDutyRateCalcEdit.Name = "ReducedDutyRateCalcEdit";
			this.ReducedDutyRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 20, true);
			this.ReducedDutyRateCalcEdit.TabIndex = 5;
			this.ReducedDutyRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReducedDutyRateCalcEdit.TrackDisposedAccess = true;
			//
			// ReductionMarginRateCalcEdit
			//
			this.ReductionMarginRateCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReductionMarginRateCalcEdit, "ReductionMarginRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).ReductionMarginRateValue)));
			this.ReductionMarginRateCalcEdit.DecimalPlaces = 2;
			this.ReductionMarginRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 98, true);
			this.ReductionMarginRateCalcEdit.Name = "ReductionMarginRateCalcEdit";
			this.ReductionMarginRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.ReductionMarginRateCalcEdit.TabIndex = 4;
			this.ReductionMarginRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReductionMarginRateCalcEdit.TrackDisposedAccess = true;
			//
			// FTAMarginRateValueCalcEdit
			//
			this.FTAMarginRateValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FTAMarginRateValueCalcEdit, "FTAMarginRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).FTAMarginRateValue)));
			this.FTAMarginRateValueCalcEdit.DecimalPlaces = 2;
			this.FTAMarginRateValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 72, true);
			this.FTAMarginRateValueCalcEdit.Name = "FTAMarginRateValueCalcEdit";
			this.FTAMarginRateValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.FTAMarginRateValueCalcEdit.TabIndex = 3;
			this.FTAMarginRateValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.FTAMarginRateValueCalcEdit.TrackDisposedAccess = true;
			//
			// DutyVigentRateValueCalcEdit
			//
			this.DutyVigentRateValueCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DutyVigentRateValueCalcEdit, "DutyVigentRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).DutyVigentRateValue)));
			this.DutyVigentRateValueCalcEdit.DecimalPlaces = 2;
			this.DutyVigentRateValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 46, true);
			this.DutyVigentRateValueCalcEdit.Name = "DutyVigentRateValueCalcEdit";
			this.DutyVigentRateValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 20, true);
			this.DutyVigentRateValueCalcEdit.TabIndex = 2;
			this.DutyVigentRateValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.DutyVigentRateValueCalcEdit.TrackDisposedAccess = true;
			//
			// PisCofinsGroupBox
			//
			this.PisCofinsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.PisCofinsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("9721db07-d58e-4267-8dcc-45f50ee052cc", "PIS/COFINS");
			this.PisCofinsGroupBox.Controls.Add(this.CofinsRateIsOverriddenCheckBox);
			this.PisCofinsGroupBox.Controls.Add(this.PisRateIsOverriddenCheckBox);
			this.PisCofinsGroupBox.Controls.Add(this.PisCofinsTaxRegimeDropEdit);
			this.PisCofinsGroupBox.Controls.Add(this.CofinsVigentRateValueCalcEdit);
			this.PisCofinsGroupBox.Controls.Add(this.PisVigentRateValueCalcEdit);
			this.PisCofinsGroupBox.Controls.Add(this.PisCofinsLegalBaseDropEdit);
			this.PisCofinsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 156, true);
			this.PisCofinsGroupBox.Name = "PisCofinsGroupBox";
			this.PisCofinsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 133, true);
			this.PisCofinsGroupBox.TabIndex = 4;
			this.PisCofinsGroupBox.TabStop = false;
			//
			// CofinsRateIsOverriddenCheckBox
			//
			this.BindingSource.SetBindingMember(this.CofinsRateIsOverriddenCheckBox, "CofinsRateIsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).CofinsRateIsOverridden)));
			this.CofinsRateIsOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 99, true);
			this.CofinsRateIsOverriddenCheckBox.Name = "CofinsRateIsOverriddenCheckBox";
			this.CofinsRateIsOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.CofinsRateIsOverriddenCheckBox.TabIndex = 5;
			this.CofinsRateIsOverriddenCheckBox.UseVisualStyleBackColor = true;
			//
			// PisRateIsOverriddenCheckBox
			//
			this.BindingSource.SetBindingMember(this.PisRateIsOverriddenCheckBox, "PisRateIsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).PisRateIsOverridden)));
			this.PisRateIsOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 72, true);
			this.PisRateIsOverriddenCheckBox.Name = "PisRateIsOverriddenCheckBox";
			this.PisRateIsOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.PisRateIsOverriddenCheckBox.TabIndex = 4;
			this.PisRateIsOverriddenCheckBox.UseVisualStyleBackColor = true;
			//
			// PisCofinsTaxRegimeDropEdit
			//
			this.PisCofinsTaxRegimeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PisCofinsTaxRegimeDropEdit, "PisCofinsTaxRegime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).PisCofinsTaxRegime)));
			this.PisCofinsTaxRegimeDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("40ef4c56-c30c-448f-9ac7-14b181683da1", "Tax Regime");
			this.PisCofinsTaxRegimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 22, true);
			this.PisCofinsTaxRegimeDropEdit.Name = "PisCofinsTaxRegimeDropEdit";
			this.PisCofinsTaxRegimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.PisCofinsTaxRegimeDropEdit.TabIndex = 0;
			//
			// CofinsVigentRateValueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CofinsVigentRateValueCalcEdit, "CofinsVigentRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).CofinsVigentRateValue)));
			this.CofinsVigentRateValueCalcEdit.DecimalPlaces = 2;
			this.CofinsVigentRateValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 100, true);
			this.CofinsVigentRateValueCalcEdit.Name = "CofinsVigentRateValueCalcEdit";
			this.CofinsVigentRateValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CofinsVigentRateValueCalcEdit.TabIndex = 3;
			this.CofinsVigentRateValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CofinsVigentRateValueCalcEdit.TrackDisposedAccess = true;
			//
			// PisVigentRateValueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.PisVigentRateValueCalcEdit, "PisVigentRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).PisVigentRateValue)));
			this.PisVigentRateValueCalcEdit.DecimalPlaces = 2;
			this.PisVigentRateValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 74, true);
			this.PisVigentRateValueCalcEdit.Name = "PisVigentRateValueCalcEdit";
			this.PisVigentRateValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.PisVigentRateValueCalcEdit.TabIndex = 2;
			this.PisVigentRateValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PisVigentRateValueCalcEdit.TrackDisposedAccess = true;
			//
			// PisCofinsLegalBaseDropEdit
			//
			this.PisCofinsLegalBaseDropEdit.AllowDrop = true;
			this.PisCofinsLegalBaseDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PisCofinsLegalBaseDropEdit, "PisCofinsLegalBase");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).PisCofinsLegalBase)));
			this.PisCofinsLegalBaseDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("71419169-5e5c-41db-97cf-f7d7a3045a4b", "Legal Base");
			this.PisCofinsLegalBaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 48, true);
			this.PisCofinsLegalBaseDropEdit.Name = "PisCofinsLegalBaseDropEdit";
			this.PisCofinsLegalBaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
			this.PisCofinsLegalBaseDropEdit.TabIndex = 1;
			//
			// IPIGroupBox
			//
			this.IPIGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.IPIGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("0bb97aaf-c914-487d-8573-608bcca7fd20", "IPI");
			this.IPIGroupBox.Controls.Add(this.IPIRateIsOverriddenCheckBox);
			this.IPIGroupBox.Controls.Add(this.IPILegalBasisGroupBox);
			this.IPIGroupBox.Controls.Add(this.IPITaxRegimeDropEdit);
			this.IPIGroupBox.Controls.Add(this.IPIVigentRateValueCalcEdit);
			this.IPIGroupBox.Controls.Add(this.ComplementaryNoteCodeFindBox);
			this.IPIGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(373, 1, true);
			this.IPIGroupBox.Name = "IPIGroupBox";
			this.IPIGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 149, true);
			this.IPIGroupBox.TabIndex = 2;
			this.IPIGroupBox.TabStop = false;
			//
			// IPIRateIsOverriddenCheckBox
			//
			this.BindingSource.SetBindingMember(this.IPIRateIsOverriddenCheckBox, "IPIRateIsOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPIRateIsOverridden)));
			this.IPIRateIsOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 37, true);
			this.IPIRateIsOverriddenCheckBox.Name = "IPIRateIsOverriddenCheckBox";
			this.IPIRateIsOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IPIRateIsOverriddenCheckBox.TabIndex = 5;
			this.IPIRateIsOverriddenCheckBox.UseVisualStyleBackColor = true;
			//
			// IPILegalBasisGroupBox
			//
			this.IPILegalBasisGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.IPILegalBasisGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("18402e10-7a84-4879-acb1-269282d7c38a", "Legal Basis of the Taxation Regime");
			this.IPILegalBasisGroupBox.Controls.Add(this.IPITaxBenefitLegalActYearTextBox);
			this.IPILegalBasisGroupBox.Controls.Add(this.IPITaxBenefitLegalActNumberTextBox);
			this.IPILegalBasisGroupBox.Controls.Add(this.IPITaxBenefitLegalActIssuingBodyDropEdit);
			this.IPILegalBasisGroupBox.Controls.Add(this.IPITaxBenefitLegalActTypeDropEdit);
			this.IPILegalBasisGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 94, true);
			this.IPILegalBasisGroupBox.Name = "IPILegalBasisGroupBox";
			this.IPILegalBasisGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(576, 47, true);
			this.IPILegalBasisGroupBox.TabIndex = 1;
			this.IPILegalBasisGroupBox.TabStop = false;
			//
			// IPITaxBenefitLegalActYearTextBox
			//
			this.BindingSource.SetBindingMember(this.IPITaxBenefitLegalActYearTextBox, "IPITaxBenefitLegalActYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPITaxBenefitLegalActYear)));
			this.IPITaxBenefitLegalActYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 20, true);
			this.IPITaxBenefitLegalActYearTextBox.Name = "IPITaxBenefitLegalActYearTextBox";
			this.IPITaxBenefitLegalActYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 20, true);
			this.IPITaxBenefitLegalActYearTextBox.TabIndex = 3;
			//
			// IPITaxBenefitLegalActNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.IPITaxBenefitLegalActNumberTextBox, "IPITaxBenefitLegalActNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPITaxBenefitLegalActNumber)));
			this.IPITaxBenefitLegalActNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(383, 20, true);
			this.IPITaxBenefitLegalActNumberTextBox.Name = "IPITaxBenefitLegalActNumberTextBox";
			this.IPITaxBenefitLegalActNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 20, true);
			this.IPITaxBenefitLegalActNumberTextBox.TabIndex = 2;
			//
			// IPITaxBenefitLegalActIssuingBodyDropEdit
			//
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IPITaxBenefitLegalActIssuingBodyDropEdit, "IPITaxBenefitLegalActIssuingBody");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPITaxBenefitLegalActIssuingBody)));
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 20, true);
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.Name = "IPITaxBenefitLegalActIssuingBodyDropEdit";
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.PreBoundMaxLength = 10;
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.ShowDescriptionBox = false;
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.TabIndex = 1;
			//
			// IPITaxBenefitLegalActTypeDropEdit
			//
			this.IPITaxBenefitLegalActTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IPITaxBenefitLegalActTypeDropEdit, "IPITaxBenefitLegalActType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPITaxBenefitLegalActType)));
			this.IPITaxBenefitLegalActTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 20, true);
			this.IPITaxBenefitLegalActTypeDropEdit.Name = "IPITaxBenefitLegalActTypeDropEdit";
			this.IPITaxBenefitLegalActTypeDropEdit.PreBoundMaxLength = 5;
			this.IPITaxBenefitLegalActTypeDropEdit.ShowDescriptionBox = false;
			this.IPITaxBenefitLegalActTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.IPITaxBenefitLegalActTypeDropEdit.TabIndex = 0;
			//
			// IPITaxRegimeDropEdit
			//
			this.IPITaxRegimeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IPITaxRegimeDropEdit, "IPITaxRegime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPITaxRegime)));
			this.IPITaxRegimeDropEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("40ef4c56-c30c-448f-9ac7-14b181683da1", "Tax Regime");
			this.IPITaxRegimeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 14, true);
			this.IPITaxRegimeDropEdit.Name = "IPITaxRegimeDropEdit";
			this.IPITaxRegimeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.IPITaxRegimeDropEdit.TabIndex = 0;
			//
			// IPIVigentRateValueCalcEdit
			//
			this.BindingSource.SetBindingMember(this.IPIVigentRateValueCalcEdit, "IPIVigentRateValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).IPIVigentRateValue)));
			this.IPIVigentRateValueCalcEdit.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("a86bd9e0-8191-40c5-a648-3d375c415509", "Ad Valorem Rate (%)");
			this.IPIVigentRateValueCalcEdit.DecimalPlaces = 2;
			this.IPIVigentRateValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 37, true);
			this.IPIVigentRateValueCalcEdit.Name = "IPIVigentRateValueCalcEdit";
			this.IPIVigentRateValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.IPIVigentRateValueCalcEdit.TabIndex = 0;
			this.IPIVigentRateValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.IPIVigentRateValueCalcEdit.TrackDisposedAccess = true;
			//
			// ComplementaryNoteCodeFindBox
			//
			this.ComplementaryNoteCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplementaryNoteCodeFindBox, "JI_ComplementaryNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).JI_ComplementaryNote)));
			this.ComplementaryNoteCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 61, true);
			this.ComplementaryNoteCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff;
			this.ComplementaryNoteCodeFindBox.Name = "ComplementaryNoteCodeFindBox";
			this.ComplementaryNoteCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ComplementaryNoteCodeFindBox.ParentType = null;
			this.ComplementaryNoteCodeFindBox.ShowDescriptionBox = false;
			this.ComplementaryNoteCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ComplementaryNoteCodeFindBox.TabIndex = 4;
			//
			// AdditionalTariffsGridLayout
			//
			this.AdditionalTariffsGridLayout.AllowDrop = true;
			this.AdditionalTariffsGridLayout.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalTariffsGridLayout.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.AdditionalTariffsGridLayout, "AdditionalTariffs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.BR.Business.AdditionalTariffCollection)(((Enterprise.Customs.BR.Business.JobComInvoiceLine)(null)).AdditionalTariffs)));
			this.AdditionalTariffsGridLayout.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 296, true);
			this.AdditionalTariffsGridLayout.Name = "AdditionalTariffsGridLayout";
			this.AdditionalTariffsGridLayout.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(954, 142, true);
			this.AdditionalTariffsGridLayout.TabIndex = 0;
			//
			// TaxDetailsUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalTariffsGridLayout);
			this.Controls.Add(this.IPIGroupBox);
			this.Controls.Add(this.PisCofinsGroupBox);
			this.Controls.Add(this.DutyGroupBox);
			this.Name = "TaxDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(964, 441, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DutyGroupBox.ResumeLayout(false);
			this.DutyGroupBox.PerformLayout();
			this.RegimeGroupBox.ResumeLayout(false);
			this.RegimeGroupBox.PerformLayout();
			this.TemporaryAdmissionReasonDropEdit.ResumeLayout(true);
			this.TemporaryAdmissionReasonDropEdit.PerformLayout();
			this.DutyLegalBaseDropEdit.ResumeLayout(true);
			this.DutyLegalBaseDropEdit.PerformLayout();
			this.DutyTaxRegimeDropEdit.ResumeLayout(true);
			this.DutyTaxRegimeDropEdit.PerformLayout();
			this.RateGroupBox.ResumeLayout(false);
			this.RateGroupBox.PerformLayout();
			this.RatePreferenceDropEdit.ResumeLayout(true);
			this.RatePreferenceDropEdit.PerformLayout();
			this.PisCofinsGroupBox.ResumeLayout(false);
			this.PisCofinsGroupBox.PerformLayout();
			this.PisCofinsTaxRegimeDropEdit.ResumeLayout(true);
			this.PisCofinsTaxRegimeDropEdit.PerformLayout();
			this.PisCofinsLegalBaseDropEdit.ResumeLayout(true);
			this.PisCofinsLegalBaseDropEdit.PerformLayout();
			this.IPIGroupBox.ResumeLayout(false);
			this.IPIGroupBox.PerformLayout();
			this.IPILegalBasisGroupBox.ResumeLayout(false);
			this.IPILegalBasisGroupBox.PerformLayout();
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.ResumeLayout(true);
			this.IPITaxBenefitLegalActIssuingBodyDropEdit.PerformLayout();
			this.IPITaxBenefitLegalActTypeDropEdit.ResumeLayout(true);
			this.IPITaxBenefitLegalActTypeDropEdit.PerformLayout();
			this.IPITaxRegimeDropEdit.ResumeLayout(true);
			this.IPITaxRegimeDropEdit.PerformLayout();
			this.ComplementaryNoteCodeFindBox.ResumeLayout(true);
			this.ComplementaryNoteCodeFindBox.PerformLayout();
			this.AdditionalTariffsGridLayout.ResumeLayout(true);
			this.AdditionalTariffsGridLayout.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox DutyGroupBox;
		internal ZArchitecture.GUI.ZGroupBox PisCofinsGroupBox;
		internal ZArchitecture.ZCalcEdit PisVigentRateValueCalcEdit;
		internal ZArchitecture.ZCalcEdit CofinsVigentRateValueCalcEdit;
		internal ZArchitecture.GUI.ZGroupBox IPIGroupBox;
		internal ZArchitecture.ZCalcEdit IPIVigentRateValueCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit DutyLegalBaseDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DutyTaxRegimeDropEdit;
		internal AdditionalTariffsUserControl AdditionalTariffsGridLayout;
		internal ZArchitecture.GUI.ZGroupBox IPILegalBasisGroupBox;
		internal ZArchitecture.GUI.ZDropEdit IPITaxBenefitLegalActTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit IPITaxBenefitLegalActIssuingBodyDropEdit;
		internal ZArchitecture.ZTextBox IPITaxBenefitLegalActYearTextBox;
		internal ZArchitecture.ZTextBox IPITaxBenefitLegalActNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit IPITaxRegimeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit TemporaryAdmissionReasonDropEdit;
		internal ZArchitecture.GUI.ZGroupBox RegimeGroupBox;
		internal ZArchitecture.GUI.ZDropEdit PisCofinsTaxRegimeDropEdit;
		internal ZArchitecture.GUI.ZGroupBox RateGroupBox;
		internal ZArchitecture.ZCalcEdit ReducedDutyRateCalcEdit;
		internal ZArchitecture.ZCalcEdit ReductionMarginRateCalcEdit;
		internal ZArchitecture.ZCalcEdit FTAMarginRateValueCalcEdit;
		internal ZArchitecture.ZCalcEdit DutyVigentRateValueCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit RatePreferenceDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox ComplementaryNoteCodeFindBox;
		internal ZArchitecture.GUI.ZCheckBox DutyRateIsOverriddenCheckBox;
		internal ZArchitecture.GUI.ZDropEdit PisCofinsLegalBaseDropEdit;
		internal ZArchitecture.GUI.ZCheckBox CofinsRateIsOverriddenCheckBox;
		internal ZArchitecture.GUI.ZCheckBox PisRateIsOverriddenCheckBox;
		internal ZArchitecture.GUI.ZCheckBox IPIRateIsOverriddenCheckBox;
		internal ZArchitecture.ZCalcEdit FTADutyRateCalcEdit;
		internal ZArchitecture.ZCalcEdit ReductionDutyRateCalcEdit;
	}
}
