using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	partial class RFPAnalysisUserControl
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
			this.SkinsAndHidesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_SaltingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DairyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_IMA1ProductDesciptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_IMA1QuotaYearTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_IMA1SerialNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_PercentOfMilkFatCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QL_PercentOfMilkProteinCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.HorticultureGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.QL_GrowerNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FishGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FishCatchEndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.FishCatchStartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QL_DrainedWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QL_FishWaterIndicatorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OtherGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManufacturerAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.EggGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FarmTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FarmCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SkinsAndHidesGroupBox.SuspendLayout();
			this.QL_SaltingDateDateEdit.SuspendLayout();
			this.DairyGroupBox.SuspendLayout();
			this.HorticultureGroupBox.SuspendLayout();
			this.FishGroupBox.SuspendLayout();
			this.FishCatchEndDateEdit.SuspendLayout();
			this.FishCatchStartDateEdit.SuspendLayout();
			this.QL_DrainedWeightCalcDropEdit.SuspendLayout();
			this.QL_FishWaterIndicatorDropEdit.SuspendLayout();
			this.OtherGroupBox.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.EggGroupBox.SuspendLayout();
			this.FarmTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine);
			// 
			// SkinsAndHidesGroupBox
			// 
			this.SkinsAndHidesGroupBox.Controls.Add(this.QL_SaltingDateDateEdit);
			this.SkinsAndHidesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 174, true);
			this.SkinsAndHidesGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 50, true);
			this.SkinsAndHidesGroupBox.Name = "SkinsAndHidesGroupBox";
			this.SkinsAndHidesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 41, true);
			this.SkinsAndHidesGroupBox.TabIndex = 5;
			this.SkinsAndHidesGroupBox.TabStop = false;
			this.SkinsAndHidesGroupBox.Text = "Skins And Hides";
			// 
			// QL_SaltingDateDateEdit
			// 
			this.QL_SaltingDateDateEdit.AllowDrop = true;
			this.QL_SaltingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.QL_SaltingDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.QL_SaltingDateDateEdit, "QuarantineExDocLine.QL_SaltingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_SaltingDate)));
			this.QL_SaltingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 16, true);
			this.QL_SaltingDateDateEdit.Name = "QL_SaltingDateDateEdit";
			this.QL_SaltingDateDateEdit.TabIndex = 1;
			// 
			// DairyGroupBox
			// 
			this.DairyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.DairyGroupBox.Controls.Add(this.QL_IMA1ProductDesciptionTextBox);
			this.DairyGroupBox.Controls.Add(this.QL_IMA1QuotaYearTextBox);
			this.DairyGroupBox.Controls.Add(this.QL_IMA1SerialNumberTextBox);
			this.DairyGroupBox.Controls.Add(this.QL_TotalWeightOfMilkFatInMixturesCalcEdit);
			this.DairyGroupBox.Controls.Add(this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit);
			this.DairyGroupBox.Controls.Add(this.QL_PercentOfMilkFatCalcEdit);
			this.DairyGroupBox.Controls.Add(this.QL_PercentOfMilkProteinCalcEdit);
			this.DairyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 6, true);
			this.DairyGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 131, true);
			this.DairyGroupBox.Name = "DairyGroupBox";
			this.DairyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 131, true);
			this.DairyGroupBox.TabIndex = 3;
			this.DairyGroupBox.TabStop = false;
			this.DairyGroupBox.Text = "Dairy";
			// 
			// QL_IMA1ProductDesciptionTextBox
			// 
			this.QL_IMA1ProductDesciptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_IMA1ProductDesciptionTextBox, "QuarantineExDocLine.QL_IMA1ProductDesciption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IMA1ProductDesciption)));
			this.QL_IMA1ProductDesciptionTextBox.CaptionResourceString = null;
			this.QL_IMA1ProductDesciptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 105, true);
			this.QL_IMA1ProductDesciptionTextBox.Name = "QL_IMA1ProductDesciptionTextBox";
			this.QL_IMA1ProductDesciptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 17, true);
			this.QL_IMA1ProductDesciptionTextBox.TabIndex = 13;
			// 
			// QL_IMA1QuotaYearTextBox
			// 
			this.QL_IMA1QuotaYearTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_IMA1QuotaYearTextBox, "QuarantineExDocLine.QL_IMA1QuotaYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IMA1QuotaYear)));
			this.QL_IMA1QuotaYearTextBox.CaptionResourceString = null;
			this.QL_IMA1QuotaYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 82, true);
			this.QL_IMA1QuotaYearTextBox.Name = "QL_IMA1QuotaYearTextBox";
			this.QL_IMA1QuotaYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 17, true);
			this.QL_IMA1QuotaYearTextBox.TabIndex = 11;
			// 
			// QL_IMA1SerialNumberTextBox
			// 
			this.QL_IMA1SerialNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_IMA1SerialNumberTextBox, "QuarantineExDocLine.QL_IMA1SerialNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_IMA1SerialNumber)));
			this.QL_IMA1SerialNumberTextBox.CaptionResourceString = null;
			this.QL_IMA1SerialNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 59, true);
			this.QL_IMA1SerialNumberTextBox.Name = "QL_IMA1SerialNumberTextBox";
			this.QL_IMA1SerialNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 17, true);
			this.QL_IMA1SerialNumberTextBox.TabIndex = 9;
			// 
			// QL_TotalWeightOfMilkFatInMixturesCalcEdit
			// 
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_TotalWeightOfMilkFatInMixturesCalcEdit, "QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures)));
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.CaptionResourceString = null;
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.DecimalPlaces = 2;
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 36, true);
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.Name = "QL_TotalWeightOfMilkFatInMixturesCalcEdit";
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 17, true);
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.TabIndex = 7;
			this.QL_TotalWeightOfMilkFatInMixturesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_TotalWeightOfMilkProteinInMixturesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit, "QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures)));
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.CaptionResourceString = null;
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.DecimalPlaces = 2;
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 36, true);
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.Name = "QL_TotalWeightOfMilkProteinInMixturesCalcEdit";
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.TabIndex = 5;
			this.QL_TotalWeightOfMilkProteinInMixturesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_PercentOfMilkFatCalcEdit
			// 
			this.QL_PercentOfMilkFatCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_PercentOfMilkFatCalcEdit, "QuarantineExDocLine.QL_PercentOfMilkFat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_PercentOfMilkFat)));
			this.QL_PercentOfMilkFatCalcEdit.CaptionResourceString = null;
			this.QL_PercentOfMilkFatCalcEdit.DecimalPlaces = 2;
			this.QL_PercentOfMilkFatCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(351, 13, true);
			this.QL_PercentOfMilkFatCalcEdit.Name = "QL_PercentOfMilkFatCalcEdit";
			this.QL_PercentOfMilkFatCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(21, 17, true);
			this.QL_PercentOfMilkFatCalcEdit.TabIndex = 3;
			this.QL_PercentOfMilkFatCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QL_PercentOfMilkProteinCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.QL_PercentOfMilkProteinCalcEdit, "QuarantineExDocLine.QL_PercentOfMilkProtein");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_PercentOfMilkProtein)));
			this.QL_PercentOfMilkProteinCalcEdit.CaptionResourceString = null;
			this.QL_PercentOfMilkProteinCalcEdit.DecimalPlaces = 2;
			this.QL_PercentOfMilkProteinCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 13, true);
			this.QL_PercentOfMilkProteinCalcEdit.Name = "QL_PercentOfMilkProteinCalcEdit";
			this.QL_PercentOfMilkProteinCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(62, 17, true);
			this.QL_PercentOfMilkProteinCalcEdit.TabIndex = 1;
			this.QL_PercentOfMilkProteinCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// HorticultureGroupBox
			// 
			this.HorticultureGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.HorticultureGroupBox.Controls.Add(this.QL_GrowerNumberTextBox);
			this.HorticultureGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(407, 137, true);
			this.HorticultureGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 50, true);
			this.HorticultureGroupBox.Name = "HorticultureGroupBox";
			this.HorticultureGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 36, true);
			this.HorticultureGroupBox.TabIndex = 4;
			this.HorticultureGroupBox.TabStop = false;
			this.HorticultureGroupBox.Text = "Horticulture";
			// 
			// QL_GrowerNumberTextBox
			// 
			this.QL_GrowerNumberTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.QL_GrowerNumberTextBox, "QuarantineExDocLine.QL_GrowerNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_GrowerNumber)));
			this.QL_GrowerNumberTextBox.CaptionResourceString = null;
			this.QL_GrowerNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 11, true);
			this.QL_GrowerNumberTextBox.Name = "QL_GrowerNumberTextBox";
			this.QL_GrowerNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 17, true);
			this.QL_GrowerNumberTextBox.TabIndex = 1;
			// 
			// FishGroupBox
			// 
			this.FishGroupBox.Controls.Add(this.FishCatchEndDateEdit);
			this.FishGroupBox.Controls.Add(this.FishCatchStartDateEdit);
			this.FishGroupBox.Controls.Add(this.QL_DrainedWeightCalcDropEdit);
			this.FishGroupBox.Controls.Add(this.QL_FishWaterIndicatorDropEdit);
			this.FishGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 9, true);
			this.FishGroupBox.Name = "FishGroupBox";
			this.FishGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 106, true);
			this.FishGroupBox.TabIndex = 1;
			this.FishGroupBox.TabStop = false;
			this.FishGroupBox.Text = "Fish";
			// 
			// FishCatchEndDateEdit
			// 
			this.FishCatchEndDateEdit.AllowDrop = true;
			this.FishCatchEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.FishCatchEndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FishCatchEndDateEdit, "QuarantineExDocLine.QL_CatchEndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_CatchEndDate)));
			this.FishCatchEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 79, true);
			this.FishCatchEndDateEdit.Name = "FishCatchEndDateEdit";
			this.FishCatchEndDateEdit.TabIndex = 4;
			// 
			// FishCatchStartDateEdit
			// 
			this.FishCatchStartDateEdit.AllowDrop = true;
			this.FishCatchStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.FishCatchStartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FishCatchStartDateEdit, "QuarantineExDocLine.QL_CatchStartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_CatchStartDate)));
			this.FishCatchStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 57, true);
			this.FishCatchStartDateEdit.Name = "FishCatchStartDateEdit";
			this.FishCatchStartDateEdit.TabIndex = 3;
			// 
			// QL_DrainedWeightCalcDropEdit
			// 
			this.QL_DrainedWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_DrainedWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_DrainedWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_DrainedWeightUnit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.Lookups.MetricWeight)));
			this.QL_DrainedWeightCalcDropEdit.BindToAmount = "QuarantineExDocLine.QL_DrainedWeight";
			this.QL_DrainedWeightCalcDropEdit.BindToList = "QuarantineExDocLine.Lookups+MetricWeight";
			this.QL_DrainedWeightCalcDropEdit.BindToUnit = "QuarantineExDocLine.QL_DrainedWeightUnit";
			this.QL_DrainedWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 13, true);
			this.QL_DrainedWeightCalcDropEdit.Name = "QL_DrainedWeightCalcDropEdit";
			this.QL_DrainedWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.QL_DrainedWeightCalcDropEdit.TabIndex = 1;
			// 
			// QL_FishWaterIndicatorDropEdit
			// 
			this.QL_FishWaterIndicatorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QL_FishWaterIndicatorDropEdit, "QuarantineExDocLine.QL_FishWaterIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_FishWaterIndicator)));
			this.QL_FishWaterIndicatorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 35, true);
			this.QL_FishWaterIndicatorDropEdit.Name = "QL_FishWaterIndicatorDropEdit";
			this.QL_FishWaterIndicatorDropEdit.ShouldResizeByMaxLength = true;
			this.QL_FishWaterIndicatorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 17, true);
			this.QL_FishWaterIndicatorDropEdit.TabIndex = 2;
			// 
			// OtherGroupBox
			// 
			this.OtherGroupBox.Controls.Add(this.ManufacturerAddressControl);
			this.OtherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 123, true);
			this.OtherGroupBox.Name = "OtherGroupBox";
			this.OtherGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 50, true);
			this.OtherGroupBox.TabIndex = 2;
			this.OtherGroupBox.TabStop = false;
			this.OtherGroupBox.Text = "Other Goods";
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManufacturerAddressControl, "JI_OA_ManufacturerAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).JI_OA_ManufacturerAddress)));
			this.ManufacturerAddressControl.BindToOrgList = "Lookups.SuppliersList";
			this.ManufacturerAddressControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("AUQuarantineInvoiceLineUserControl|DBF42379-498D-4E41-B2CE-5667DDF6B8E1", "Manufacturer");
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 20, true);
			this.ManufacturerAddressControl.Name = "ManufacturerAddressControl";
			this.ManufacturerAddressControl.PopupCaption = "";
			this.ManufacturerAddressControl.ReadOnly = false;
			this.ManufacturerAddressControl.ShowAddress = false;
			this.ManufacturerAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 17, true);
			this.ManufacturerAddressControl.TabIndex = 0;
			// 
			// EggGroupBox
			// 
			this.EggGroupBox.Controls.Add(this.FarmTypeDropEdit);
			this.EggGroupBox.Controls.Add(this.FarmCodeTextBox);
			this.EggGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 176, true);
			this.EggGroupBox.Name = "EggGroupBox";
			this.EggGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 64, true);
			this.EggGroupBox.TabIndex = 6;
			this.EggGroupBox.TabStop = false;
			this.EggGroupBox.Text = "Egg";
			// 
			// FarmTypeDropEdit
			// 
			this.FarmTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FarmTypeDropEdit, "QuarantineExDocLine.QL_FarmType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_FarmType)));
			this.FarmTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 42, true);
			this.FarmTypeDropEdit.Name = "FarmTypeDropEdit";
			this.FarmTypeDropEdit.ShouldResizeByMaxLength = false;
			this.FarmTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 15, true);
			this.FarmTypeDropEdit.TabIndex = 1;
			// 
			// FarmCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FarmCodeTextBox, "QuarantineExDocLine.QL_FarmCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceLine)(null)).QuarantineExDocLine.QL_FarmCode)));
			this.FarmCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(119, 20, true);
			this.FarmCodeTextBox.Name = "FarmCodeTextBox";
			this.FarmCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 15, true);
			this.FarmCodeTextBox.TabIndex = 0;
			// 
			// RFPAnalysisUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.EggGroupBox);
			this.Controls.Add(this.SkinsAndHidesGroupBox);
			this.Controls.Add(this.DairyGroupBox);
			this.Controls.Add(this.HorticultureGroupBox);
			this.Controls.Add(this.FishGroupBox);
			this.Controls.Add(this.OtherGroupBox);
			this.Name = "RFPAnalysisUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(750, 246, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SkinsAndHidesGroupBox.ResumeLayout(false);
			this.SkinsAndHidesGroupBox.PerformLayout();
			this.QL_SaltingDateDateEdit.ResumeLayout(true);
			this.QL_SaltingDateDateEdit.PerformLayout();
			this.DairyGroupBox.ResumeLayout(false);
			this.DairyGroupBox.PerformLayout();
			this.HorticultureGroupBox.ResumeLayout(false);
			this.HorticultureGroupBox.PerformLayout();
			this.FishGroupBox.ResumeLayout(false);
			this.FishGroupBox.PerformLayout();
			this.FishCatchEndDateEdit.ResumeLayout(true);
			this.FishCatchEndDateEdit.PerformLayout();
			this.FishCatchStartDateEdit.ResumeLayout(true);
			this.FishCatchStartDateEdit.PerformLayout();
			this.QL_DrainedWeightCalcDropEdit.ResumeLayout(true);
			this.QL_DrainedWeightCalcDropEdit.PerformLayout();
			this.QL_FishWaterIndicatorDropEdit.ResumeLayout(true);
			this.QL_FishWaterIndicatorDropEdit.PerformLayout();
			this.OtherGroupBox.ResumeLayout(false);
			this.OtherGroupBox.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.EggGroupBox.ResumeLayout(false);
			this.EggGroupBox.PerformLayout();
			this.FarmTypeDropEdit.ResumeLayout(true);
			this.FarmTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBox SkinsAndHidesGroupBox;
		private ZGroupBox DairyGroupBox;
		private ZGroupBox HorticultureGroupBox;
		private ZGroupBox FishGroupBox;
		private ZGroupBox OtherGroupBox;
		private ZDateEdit QL_SaltingDateDateEdit;
		private ZTextBox QL_IMA1ProductDesciptionTextBox;
		private ZTextBox QL_IMA1QuotaYearTextBox;
		private ZTextBox QL_IMA1SerialNumberTextBox;
		private ZCalcEdit QL_TotalWeightOfMilkFatInMixturesCalcEdit;
		private ZCalcEdit QL_TotalWeightOfMilkProteinInMixturesCalcEdit;
		private ZCalcEdit QL_PercentOfMilkFatCalcEdit;
		private ZCalcEdit QL_PercentOfMilkProteinCalcEdit;
		private ZTextBox QL_GrowerNumberTextBox;
		private ZDateEdit FishCatchEndDateEdit;
		private ZDateEdit FishCatchStartDateEdit;
		private ZCalcDropEdit QL_DrainedWeightCalcDropEdit;
		private ZDropEdit QL_FishWaterIndicatorDropEdit;
		private ZAddressControl ManufacturerAddressControl;
		private ZGroupBox EggGroupBox;
		private ZTextBox FarmCodeTextBox;
		private ZDropEdit FarmTypeDropEdit;
	}
}
