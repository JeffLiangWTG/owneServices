using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class G5V1TemporaryStorageDetailsUserControl
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
			this.components = new System.ComponentModel.Container();
			this.LRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MRNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CircuitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcceptanceDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ClearanceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DsdtSdFormatHasUrlUserControl = new DsdtSdFormatUserControl(false, true);
			this.DsdtSdFormatNoUrlUserControl = new DsdtSdFormatUserControl(false, false);
			this.DsdtMrnBindingMemberUserControl = new DsdtSdFormatUserControl(true, true);
			this.DsdtMrnNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LAMEEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LAMEEntryDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader);
			// 
			// LRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.LRNTextBox, "LRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).LRN)));
			this.LRNTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("9735181B-A6A5-4203-BCAB-3B1D5078F14E", "LRN");
			this.LRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.LRNTextBox.Name = "LRNTextBox";
			this.LRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.LRNTextBox.TabIndex = 24;
			// 
			// MRNTextBox
			// 
			this.BindingSource.SetBindingMember(this.MRNTextBox, "MRN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).MRN)));
			this.MRNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 48, true);
			this.MRNTextBox.Name = "MRNTextBox";
			this.MRNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.MRNTextBox.TabIndex = 24;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).CustomsStatus)));
			this.CustomsStatusDropEdit.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("7EEDD809-C4D8-4200-817E-400C62494ACA", "Customs Status");
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 76, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CustomsStatusDropEdit.TabIndex = 35;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "AMA_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).AMA_MessageStatus)));
			this.MessageStatusDropEdit.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("DFDD4E80-227B-4A74-93DF-4BDC47ABF368", "Message Status");
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 104, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.MessageStatusDropEdit.TabIndex = 36;
			// 
			// CircuitTextBox
			// 
			this.BindingSource.SetBindingMember(this.CircuitTextBox, "EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).EntryStatus)));
			this.CircuitTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("8185DA02-61B2-47FB-B4E8-22B32E91CCB6", "Circuit");
			this.CircuitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 132, true);
			this.CircuitTextBox.Name = "CircuitTextBox";
			this.CircuitTextBox.ReadOnly = true;
			this.CircuitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CircuitTextBox.TabIndex = 1;
			this.CircuitTextBox.TabStop = false;
			// 
			// AcceptanceDateDateEdit
			//
			this.AcceptanceDateDateEdit.AllowDrop = true;
			this.AcceptanceDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.AcceptanceDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AcceptanceDateDateEdit, "AcceptanceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).AcceptanceDate)));
			this.AcceptanceDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 113, true);
			this.AcceptanceDateDateEdit.Name = "AcceptanceDateDateEdit";
			this.AcceptanceDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AcceptanceDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.AcceptanceDateDateEdit.TabIndex = 0;
			this.AcceptanceDateDateEdit.TabStop = false;
			// 
			// ClearanceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClearanceNumberTextBox, "ClearanceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).ClearanceNumber)));
			this.ClearanceNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("0C1352AF-748A-4EDF-A69D-2173388F61F6", "Clearance Number");
			this.ClearanceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 81, true);
			this.ClearanceNumberTextBox.Name = "ClearanceNumberTextBox";
			this.ClearanceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ClearanceNumberTextBox.TabIndex = 2;
			this.ClearanceNumberTextBox.TabStop = false;
			// 
			// DsdtMrnNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DsdtMrnNumberTextBox, "DsdtMrnNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).DsdtMrnNumber)));
			this.DsdtMrnNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("3211AAE6-0204-4F6A-BAD3-CA9A512E7A51", englishCaption: "DSDT MRN", englishMediumCaption: "DSDT MRN", englishShortCaption: "DSDT MRN", englishFullDescription: "DSDT MRN (Summary Declaration)");
			this.DsdtMrnNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 81, true);
			this.DsdtMrnNumberTextBox.Name = "DsdtMrnNumberTextBox";
			this.DsdtMrnNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DsdtMrnNumberTextBox.TabIndex = 2;
			this.DsdtMrnNumberTextBox.TabStop = false;
			// 
			// DsdtSdFormatHasUrlUserControl
			// 
			this.DsdtSdFormatHasUrlUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DsdtSdFormatHasUrlUserControl, ".");
			this.DsdtSdFormatHasUrlUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 51, true);
			this.DsdtSdFormatHasUrlUserControl.Name = "DsdtSdFormatHasUrlUserControl";
			this.DsdtSdFormatHasUrlUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DsdtSdFormatHasUrlUserControl.TabIndex = 0;
			// 
			// DsdtSdFormatNoUrlUserControl
			// 
			this.DsdtSdFormatNoUrlUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DsdtSdFormatNoUrlUserControl, ".");
			this.DsdtSdFormatNoUrlUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 51, true);
			this.DsdtSdFormatNoUrlUserControl.Name = "DsdtSdFormatNoUrlUserControl";
			this.DsdtSdFormatNoUrlUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DsdtSdFormatNoUrlUserControl.TabIndex = 0;
			// 
			// DsdtMrnBindingMemberUserControl
			// 
			this.DsdtMrnBindingMemberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DsdtMrnBindingMemberUserControl, ".");
			this.DsdtMrnBindingMemberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 51, true);
			this.DsdtMrnBindingMemberUserControl.Name = "DsdtMrnBindingMemberUserControl";
			this.DsdtMrnBindingMemberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.DsdtMrnBindingMemberUserControl.TabIndex = 0;
			// 
			// LAMEEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LAMEEntryNumberTextBox, "EntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).EntryNumber)));
			this.LAMEEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("4EDCFE9E-DD58-4E21-926D-FC0FACF9D827", "Entry Number");
			this.LAMEEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 20, true);
			this.LAMEEntryNumberTextBox.Name = "LAMEEntryNumberTextBox";
			this.LAMEEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.LAMEEntryNumberTextBox.TabIndex = 24;
			// 
			// LAMEEntryDateDateEdit
			//
			this.LAMEEntryDateDateEdit.AllowDrop = true;
			this.LAMEEntryDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.LAMEEntryDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LAMEEntryDateDateEdit, "EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.ES.Business.CusTempStorage.TemporaryStorageHeader)(null)).EntryDate)));
			this.LAMEEntryDateDateEdit.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("12AF5841-0017-4C08-BA35-32A7E3F99864", "Entry Date");
			this.LAMEEntryDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 113, true);
			this.LAMEEntryDateDateEdit.Name = "LAMEEntryDateDateEdit";
			this.LAMEEntryDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LAMEEntryDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.LAMEEntryDateDateEdit.TabIndex = 0;
			this.LAMEEntryDateDateEdit.TabStop = false;
			// 
			// G5V1TemporaryStorageDetailUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LRNTextBox);
			this.Controls.Add(this.MRNTextBox);
			this.Controls.Add(this.CustomsStatusDropEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.CircuitTextBox);
			this.Controls.Add(this.AcceptanceDateDateEdit);
			this.Controls.Add(this.ClearanceNumberTextBox);
			this.Controls.Add(this.DsdtMrnNumberTextBox);
			this.Controls.Add(this.DsdtSdFormatHasUrlUserControl);
			this.Controls.Add(this.DsdtSdFormatNoUrlUserControl);
			this.Controls.Add(this.DsdtMrnBindingMemberUserControl);
			this.Controls.Add(this.LAMEEntryNumberTextBox);
			this.Controls.Add(this.LAMEEntryDateDateEdit);
			this.Name = "G5V1TemporaryStorageDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 186, true);
			this.Tag = "";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox LRNTextBox;
		internal ZArchitecture.ZTextBox MRNTextBox;
		internal ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.ZTextBox CircuitTextBox;
		internal ZArchitecture.GUI.ZDateEdit AcceptanceDateDateEdit;
		internal ZArchitecture.ZTextBox ClearanceNumberTextBox;
		internal ZArchitecture.ZTextBox DsdtMrnNumberTextBox;
		internal ZUserControl DsdtMrnBindingMemberUserControl;
		internal ZUserControl DsdtSdFormatNoUrlUserControl;
		internal ZUserControl DsdtSdFormatHasUrlUserControl;
		internal ZArchitecture.ZTextBox LAMEEntryNumberTextBox;
		internal ZArchitecture.GUI.ZDateEdit LAMEEntryDateDateEdit;

	}
}
