
namespace Enterprise.Customs.EU.GUI
{
	partial class CommonEntryDetailsUserControl
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
			this.NoPacksCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.CustomsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.DutyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.VatCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EntryLinesCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubmittedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.MRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AcceptanceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TotalsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.CustomsQuantityCalcDropEdit.SuspendLayout();
			this.SubmittedDateDateEdit.SuspendLayout();
			this.ReleaseDateDateEdit.SuspendLayout();
			this.EntryStatusDropEdit.SuspendLayout();
			this.AcceptanceDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// NoPacksCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NoPacksCalcEdit, "CustomsEntryHeaders.PackagesCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).PackagesCount)));
			this.NoPacksCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("FF4856E8-B540-46F8-8E7F-0F6AC18F6DF1", "No. Packs");
			this.NoPacksCalcEdit.DecimalPlaces = 2;
			this.NoPacksCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 19, true);
			this.NoPacksCalcEdit.Name = "NoPacksCalcEdit";
			this.NoPacksCalcEdit.ReadOnly = true;
			this.NoPacksCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.NoPacksCalcEdit.TabIndex = 0;
			this.NoPacksCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalGrossWeightInKG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalGrossWeightUQ)));
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4CB3D713-70D1-4210-9616-B6FA08D5B373", "Gross Weight");
			this.GrossWeightCalcDropEdit.BindToAmount = "CustomsEntryHeaders.TotalGrossWeightInKG";
			this.GrossWeightCalcDropEdit.BindToUnit = "CustomsEntryHeaders.TotalGrossWeightUQ";
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 42, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.GrossWeightCalcDropEdit.TabIndex = 1;
			this.GrossWeightCalcDropEdit.Text = "0.00000";
			// 
			// NetWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalNetWeightInKG)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalNetWeightUQ)));
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F884CA37-B08C-4043-8F6D-EB61F6330F75", "Net Weight");
			this.NetWeightCalcDropEdit.BindToAmount = "CustomsEntryHeaders.TotalNetWeightInKG";
			this.NetWeightCalcDropEdit.BindToUnit = "CustomsEntryHeaders.TotalNetWeightUQ";
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 65, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 3;
			this.NetWeightCalcDropEdit.Text = "0.00000";
			// 
			// CustomsQuantityCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CustomsQuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalCustomsQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).TotalCustomsUQ)));
			this.CustomsQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("439E19AE-B788-4FD3-BF68-C77091580F52", "Customs Quantity");
			this.CustomsQuantityCalcDropEdit.BindToAmount = "CustomsEntryHeaders.TotalCustomsQuantity";
			this.CustomsQuantityCalcDropEdit.BindToUnit = "CustomsEntryHeaders.TotalCustomsUQ";
			this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 88, true);
			this.CustomsQuantityCalcDropEdit.Name = "CustomsQuantityCalcDropEdit";
			this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CustomsQuantityCalcDropEdit.TabIndex = 5;
			this.CustomsQuantityCalcDropEdit.Text = "0.00000";
			// 
			// DutyCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DutyCalcEdit, "CustomsEntryHeaders.Duty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).Duty)));
			this.DutyCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A228572B-2949-45FD-AC55-22AFD3E07E47", "Duty");
			this.DutyCalcEdit.DecimalPlaces = 2;
			this.DutyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 157, true);
			this.DutyCalcEdit.Name = "DutyCalcEdit";
			this.DutyCalcEdit.ReadOnly = true;
			this.DutyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.DutyCalcEdit.TabIndex = 10;
			this.DutyCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VatCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.VatCalcEdit, "CustomsEntryHeaders.VAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).VAT)));
			this.VatCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("CEE772FC-6240-4875-A88D-7DE4AB6C160E", "VAT");
			this.VatCalcEdit.DecimalPlaces = 2;
			this.VatCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 180, true);
			this.VatCalcEdit.Name = "VatCalcEdit";
			this.VatCalcEdit.ReadOnly = true;
			this.VatCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.VatCalcEdit.TabIndex = 11;
			this.VatCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EntryLinesCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.EntryLinesCountCalcEdit, "CustomsEntryHeaders.MergedLinesCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MergedLinesCount)));
			this.EntryLinesCountCalcEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("126760A5-5786-491A-B428-89045DEE56D7", "Total Entry Lines");
			this.EntryLinesCountCalcEdit.DecimalPlaces = 0;
			this.EntryLinesCountCalcEdit.Decimals = 0;
			this.EntryLinesCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 203, true);
			this.EntryLinesCountCalcEdit.Name = "EntryLinesCountCalcEdit";
			this.EntryLinesCountCalcEdit.ReadOnly = true;
			this.EntryLinesCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.EntryLinesCountCalcEdit.TabIndex = 12;
			this.EntryLinesCountCalcEdit.Text = "0";
			this.EntryLinesCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CustomsEntryHeaders.CH_BGMReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_BGMReference)));
			this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A636E535-3AB7-47B3-8E11-619418BF0F89", "Reference No.");
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 226, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.ReadOnly = true;
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 0;
			// 
			// SubmittedDateDateEdit
			// 
			this.SubmittedDateDateEdit.AllowDrop = true;
			this.SubmittedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.SubmittedDateDateEdit, "CustomsEntryHeaders.CH_EntrySubmittedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntrySubmittedDate)));
			this.SubmittedDateDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4015AA00-3F60-47D7-8475-3560036F3217", "Submitted Date");
			this.SubmittedDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SubmittedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 249, true);
			this.SubmittedDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.SubmittedDateDateEdit.Name = "SubmittedDateDateEdit";
			this.SubmittedDateDateEdit.TabIndex = 2;
			// 
			// MRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.MRNTextBox, "CustomsEntryHeaders.MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MovementReferenceNumber)));
			this.MRNTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1E9D6BDF-F0E7-4224-B5C2-B6CCFC8670AC", "MRN");
			this.MRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 272, true);
			this.MRNTextBox.Name = "MRNTextBox";
			this.MRNTextBox.ReadOnly = true;
			this.MRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.MRNTextBox.TabIndex = 3;
			// 
			// ReleaseDateDateEdit
			// 
			this.ReleaseDateDateEdit.AllowDrop = true;
			this.ReleaseDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateDateEdit, "CustomsEntryHeaders.CH_EntryReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryReleaseDate)));
			this.ReleaseDateDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("B3540F05-834B-450C-ABD8-B0186540D9AB", "Release Date");
			this.ReleaseDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 295, true);
			this.ReleaseDateDateEdit.Name = "ReleaseDateDateEdit";
			this.ReleaseDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ReleaseDateDateEdit.TabIndex = 5;
			// 
			// EntryStatusDropEdit
			// 
			this.EntryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CustomsEntryHeaders.CH_EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			this.EntryStatusDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("D7380DA6-2660-42E8-9D15-4819BB3AAE62", "Entry Status");
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 318, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.PreBoundMaxLength = 2;
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.EntryStatusDropEdit.TabIndex = 8;
			// 
			// AcceptanceDateDateEdit
			// 
			this.AcceptanceDateDateEdit.AllowDrop = true;
			this.AcceptanceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptanceDateDateEdit, "CustomsEntryHeaders.MovementReferenceNumberIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MovementReferenceNumberIssueDate)));
			this.AcceptanceDateDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("54D56EB2-8C0A-47E0-8F9A-8B36C44EC5C5", "Acceptance Date");
			this.AcceptanceDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AcceptanceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 349, true);
			this.AcceptanceDateDateEdit.Name = "AcceptanceDateDateEdit";
			this.AcceptanceDateDateEdit.TabIndex = 13;
			// 
			// TotalsLabel
			// 
			this.TotalsLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("0631B289-02B5-4C33-B4DD-CDC7161279B8", "==== Totals ====");
			this.TotalsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TotalsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 397, true);
			this.TotalsLabel.Name = "TotalsLabel";
			this.TotalsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.TotalsLabel.TabIndex = 14;
			this.TotalsLabel.UseMnemonic = false;
			// 
			// CustomsLabel
			// 
			this.CustomsLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("548F3A3C-7508-4F05-8919-5DBE15D7D15C", "==== Customs ====");
			this.CustomsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 439, true);
			this.CustomsLabel.Name = "CustomsLabel";
			this.CustomsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.CustomsLabel.TabIndex = 15;
			this.CustomsLabel.UseMnemonic = false;
			// 
			// CommonEntryDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsLabel);
			this.Controls.Add(this.TotalsLabel);
			this.Controls.Add(this.AcceptanceDateDateEdit);
			this.Controls.Add(this.NoPacksCalcEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.CustomsQuantityCalcDropEdit);
			this.Controls.Add(this.DutyCalcEdit);
			this.Controls.Add(this.VatCalcEdit);
			this.Controls.Add(this.EntryLinesCountCalcEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.SubmittedDateDateEdit);
			this.Controls.Add(this.MRNTextBox);
			this.Controls.Add(this.ReleaseDateDateEdit);
			this.Controls.Add(this.EntryStatusDropEdit);
			this.Name = "CommonEntryDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1035, 628, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
			this.CustomsQuantityCalcDropEdit.PerformLayout();
			this.SubmittedDateDateEdit.ResumeLayout(true);
			this.SubmittedDateDateEdit.PerformLayout();
			this.ReleaseDateDateEdit.ResumeLayout(true);
			this.ReleaseDateDateEdit.PerformLayout();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			this.AcceptanceDateDateEdit.ResumeLayout(true);
			this.AcceptanceDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit NoPacksCalcEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit CustomsQuantityCalcDropEdit;
		internal ZArchitecture.ZCalcEdit DutyCalcEdit;
		internal ZArchitecture.ZCalcEdit VatCalcEdit;
		internal ZArchitecture.ZCalcEdit EntryLinesCountCalcEdit;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZDateEdit SubmittedDateDateEdit;
		internal ZArchitecture.ZTextBox MRNTextBox;
		internal ZArchitecture.GUI.ZDateEdit ReleaseDateDateEdit;
		internal ZArchitecture.GUI.ZDropEdit EntryStatusDropEdit;
		internal ZArchitecture.GUI.ZDateEdit AcceptanceDateDateEdit;
		internal ZArchitecture.ZLabel TotalsLabel;
		internal ZArchitecture.ZLabel CustomsLabel;
	}
}
