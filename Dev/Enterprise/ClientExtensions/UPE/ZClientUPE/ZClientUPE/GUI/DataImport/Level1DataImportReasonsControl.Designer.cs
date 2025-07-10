namespace Enterprise.Client.UPE.GUI.DataImport
{
	partial class Level1DataImportReasonsControl
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
			this.ReasonsGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.ArrivalDateWarningTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.UnmatchedFileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.MAWBIsDuplicateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MAWBIsDuplicateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.duplicateHAWBsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.duplicateHAWBsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FlightNotITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.SurplusIndicatedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SurplusIndicatedLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReasonsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.DataImport.Level1DataImport);
			// 
			// ReasonsGroupBox
			// 
			this.ReasonsGroupBox.Controls.Add(this.ArrivalDateWarningTextBox);
			this.ReasonsGroupBox.Controls.Add(this.zLabel4);
			this.ReasonsGroupBox.Controls.Add(this.UnmatchedFileNameTextBox);
			this.ReasonsGroupBox.Controls.Add(this.zLabel5);
			this.ReasonsGroupBox.Controls.Add(this.MAWBIsDuplicateTextBox);
			this.ReasonsGroupBox.Controls.Add(this.MAWBIsDuplicateLabel);
			this.ReasonsGroupBox.Controls.Add(this.duplicateHAWBsTextBox);
			this.ReasonsGroupBox.Controls.Add(this.duplicateHAWBsLabel);
			this.ReasonsGroupBox.Controls.Add(this.FlightNotITextBox);
			this.ReasonsGroupBox.Controls.Add(this.zLabel3);
			this.ReasonsGroupBox.Controls.Add(this.SurplusIndicatedTextBox);
			this.ReasonsGroupBox.Controls.Add(this.SurplusIndicatedLabel);
			this.ReasonsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReasonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReasonsGroupBox.Name = "ReasonsGroupBox";
			this.ReasonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 304, true);
			this.ReasonsGroupBox.TabIndex = 2;
			this.ReasonsGroupBox.TabStop = false;
			this.ReasonsGroupBox.Text = "Reasons:";
			// 
			// ArrivalDateWarningTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalDateWarningTextBox, "ArrivalDateWarningNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).ArrivalDateWarningNote)));
			this.ArrivalDateWarningTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ArrivalDateWarningTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 245, true);
			this.ArrivalDateWarningTextBox.Multiline = true;
			this.ArrivalDateWarningTextBox.Name = "ArrivalDateWarningTextBox";
			this.ArrivalDateWarningTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.ArrivalDateWarningTextBox.TabIndex = 11;
			// 
			// zLabel4
			// 
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 245, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 26, true);
			this.zLabel4.TabIndex = 10;
			this.zLabel4.Text = "Arrival Date Is Outside The Allowed Range:";
			// 
			// UnmatchedFileNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnmatchedFileNameTextBox, "UnmatchedFilenameNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).UnmatchedFilenameNote)));
			this.UnmatchedFileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnmatchedFileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 200, true);
			this.UnmatchedFileNameTextBox.Multiline = true;
			this.UnmatchedFileNameTextBox.Name = "UnmatchedFileNameTextBox";
			this.UnmatchedFileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.UnmatchedFileNameTextBox.TabIndex = 9;
			// 
			// zLabel5
			// 
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 200, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 26, true);
			this.zLabel5.TabIndex = 8;
			this.zLabel5.Text = "FileName Does Not Match Port Of Discharge:";
			// 
			// MAWBIsDuplicateTextBox
			// 
			this.BindingSource.SetBindingMember(this.MAWBIsDuplicateTextBox, "MasterbillWarningNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).MasterbillWarningNote)));
			this.MAWBIsDuplicateTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MAWBIsDuplicateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 155, true);
			this.MAWBIsDuplicateTextBox.Multiline = true;
			this.MAWBIsDuplicateTextBox.Name = "MAWBIsDuplicateTextBox";
			this.MAWBIsDuplicateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.MAWBIsDuplicateTextBox.TabIndex = 7;
			// 
			// MAWBIsDuplicateLabel
			// 
			this.MAWBIsDuplicateLabel.AutoSize = true;
			this.MAWBIsDuplicateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MAWBIsDuplicateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 155, true);
			this.MAWBIsDuplicateLabel.Name = "MAWBIsDuplicateLabel";
			this.MAWBIsDuplicateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.MAWBIsDuplicateLabel.TabIndex = 6;
			this.MAWBIsDuplicateLabel.Text = "Mawb is Duplicate:";
			// 
			// duplicateHAWBsTextBox
			// 
			this.BindingSource.SetBindingMember(this.duplicateHAWBsTextBox, "DuplicateHAWBsNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).DuplicateHAWBsNote)));
			this.duplicateHAWBsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.duplicateHAWBsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 110, true);
			this.duplicateHAWBsTextBox.Multiline = true;
			this.duplicateHAWBsTextBox.Name = "duplicateHAWBsTextBox";
			this.duplicateHAWBsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.duplicateHAWBsTextBox.TabIndex = 5;
			// 
			// duplicateHAWBsLabel
			// 
			this.duplicateHAWBsLabel.AutoSize = true;
			this.duplicateHAWBsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.duplicateHAWBsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 110, true);
			this.duplicateHAWBsLabel.Name = "duplicateHAWBsLabel";
			this.duplicateHAWBsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(124, 13, true);
			this.duplicateHAWBsLabel.TabIndex = 4;
			this.duplicateHAWBsLabel.Text = "Duplicate HAWBs > 15%";
			// 
			// FlightNotITextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNotITextBox, "FlightNotInScheduleNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).FlightNotInScheduleNote)));
			this.FlightNotITextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FlightNotITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 65, true);
			this.FlightNotITextBox.Multiline = true;
			this.FlightNotITextBox.Name = "FlightNotITextBox";
			this.FlightNotITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.FlightNotITextBox.TabIndex = 3;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 65, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.zLabel3.TabIndex = 2;
			this.zLabel3.Text = "Flight Not In Schedule:";
			// 
			// SurplusIndicatedTextBox
			// 
			this.BindingSource.SetBindingMember(this.SurplusIndicatedTextBox, "SurplusIndicatedNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).SurplusIndicatedNote)));
			this.SurplusIndicatedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SurplusIndicatedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 20, true);
			this.SurplusIndicatedTextBox.Multiline = true;
			this.SurplusIndicatedTextBox.Name = "SurplusIndicatedTextBox";
			this.SurplusIndicatedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.SurplusIndicatedTextBox.TabIndex = 1;
			// 
			// SurplusIndicatedLabel
			// 
			this.SurplusIndicatedLabel.AutoSize = true;
			this.SurplusIndicatedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SurplusIndicatedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 20, true);
			this.SurplusIndicatedLabel.Name = "SurplusIndicatedLabel";
			this.SurplusIndicatedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.SurplusIndicatedLabel.TabIndex = 0;
			this.SurplusIndicatedLabel.Text = "Surplus Indicated:";
			// 
			// Level1DataImportReasonsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ReasonsGroupBox);
			this.Name = "Level1DataImportReasonsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 304, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReasonsGroupBox.ResumeLayout(false);
			this.ReasonsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KGroupBox ReasonsGroupBox;
		private ZArchitecture.ZTextBox ArrivalDateWarningTextBox;
		private ZArchitecture.ZLabel zLabel4;
		private ZArchitecture.ZTextBox UnmatchedFileNameTextBox;
		private ZArchitecture.ZLabel zLabel5;
		private ZArchitecture.ZTextBox MAWBIsDuplicateTextBox;
		private ZArchitecture.ZLabel MAWBIsDuplicateLabel;
		private ZArchitecture.ZTextBox duplicateHAWBsTextBox;
		private ZArchitecture.ZLabel duplicateHAWBsLabel;
		private ZArchitecture.ZTextBox FlightNotITextBox;
		private ZArchitecture.ZLabel zLabel3;
		private ZArchitecture.ZTextBox SurplusIndicatedTextBox;
		private ZArchitecture.ZLabel SurplusIndicatedLabel;
	}
}
