namespace Enterprise.Client.UPE.GUI.DataImport
{
	partial class Level1DataImportReasonsForManifestControl
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
			this.FlightNotITextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
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
			this.ReasonsGroupBox.Controls.Add(this.FlightNotITextBox);
			this.ReasonsGroupBox.Controls.Add(this.zLabel3);
			this.ReasonsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReasonsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReasonsGroupBox.Name = "ReasonsGroupBox";
			this.ReasonsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 299, true);
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
			this.ArrivalDateWarningTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 129, true);
			this.ArrivalDateWarningTextBox.Multiline = true;
			this.ArrivalDateWarningTextBox.Name = "ArrivalDateWarningTextBox";
			this.ArrivalDateWarningTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.ArrivalDateWarningTextBox.TabIndex = 2;
			// 
			// zLabel4
			// 
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 129, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 26, true);
			this.zLabel4.TabIndex = 5;
			this.zLabel4.Text = "Arrival Date Is Outside The Allowed Range:";
			// 
			// UnmatchedFileNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UnmatchedFileNameTextBox, "UnmatchedFilenameNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).UnmatchedFilenameNote)));
			this.UnmatchedFileNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UnmatchedFileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 71, true);
			this.UnmatchedFileNameTextBox.Multiline = true;
			this.UnmatchedFileNameTextBox.Name = "UnmatchedFileNameTextBox";
			this.UnmatchedFileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.UnmatchedFileNameTextBox.TabIndex = 1;
			// 
			// zLabel5
			// 
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 71, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 26, true);
			this.zLabel5.TabIndex = 4;
			this.zLabel5.Text = "FileName Does Not Match Port Of Discharge:";
			// 
			// FlightNotITextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNotITextBox, "FlightNotInScheduleNote");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.DataImport.Level1DataImport)(null)).FlightNotInScheduleNote)));
			this.FlightNotITextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FlightNotITextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 17, true);
			this.FlightNotITextBox.Multiline = true;
			this.FlightNotITextBox.Name = "FlightNotITextBox";
			this.FlightNotITextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(506, 40, true);
			this.FlightNotITextBox.TabIndex = 0;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 19, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.zLabel3.TabIndex = 3;
			this.zLabel3.Text = "Flight Not In Schedule:";
			// 
			// Level1DataImportReasonsForManifestControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ReasonsGroupBox);
			this.Name = "Level1DataImportReasonsForManifestControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 299, true);
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
		private ZArchitecture.ZTextBox FlightNotITextBox;
		private ZArchitecture.ZLabel zLabel3;
	}
}
