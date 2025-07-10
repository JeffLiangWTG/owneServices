using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class WebPrintNudgeSuspendingUserControl
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
		void InitializeComponent()
		{
			this.OptionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.MinutesGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.HoursGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.MaxErrorsInMinutesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IntervalMinutesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SuspendMinutesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxErrorsInHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IntervalHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SuspendHoursCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.GUI.WebPrintNudgeWrapper);
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.Controls.Add(this.MinutesGroupBox);
			this.OptionGroupBox.Controls.Add(this.HoursGroupBox);
			this.OptionGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Name = "optionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 230, true);
			this.OptionGroupBox.TabIndex = 1;
			this.OptionGroupBox.TabStop = false;
			this.OptionGroupBox.Text = Enterprise.Registry.GUI.Res.GetString("RegistryForm|830DC62D-C2C3-4C1A-91D4-99BEB5A184F2", "Nudge suspending setting");
			// 
			// MinutesGroupBox
			//
			this.MinutesGroupBox.Controls.Add(this.MaxErrorsInMinutesCalcEdit);
			this.MinutesGroupBox.Controls.Add(this.IntervalMinutesCalcEdit);
			this.MinutesGroupBox.Controls.Add(this.SuspendMinutesCalcEdit);
			this.MinutesGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.MinutesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 20, true);
			this.MinutesGroupBox.Name = "MinutesGroupBox";
			this.MinutesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.MinutesGroupBox.TabIndex = 1;
			this.MinutesGroupBox.TabStop = false;
			this.MinutesGroupBox.Text = Enterprise.Registry.GUI.Res.GetString("RegistryForm|05DAC5DD-DF78-4231-BFE9-41443850FEB5", "Minutes");
			// 
			// HoursGroupBox
			//
			this.HoursGroupBox.Controls.Add(this.MaxErrorsInHoursCalcEdit);
			this.HoursGroupBox.Controls.Add(this.IntervalHoursCalcEdit);
			this.HoursGroupBox.Controls.Add(this.SuspendHoursCalcEdit);
			this.HoursGroupBox.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Bold);
			this.HoursGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 120, true);
			this.HoursGroupBox.Name = "HoursGroupBox";
			this.HoursGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
			this.HoursGroupBox.TabIndex = 1;
			this.HoursGroupBox.TabStop = false;
			this.HoursGroupBox.Text = Enterprise.Registry.GUI.Res.GetString("RegistryForm|21E45E66-3E77-499A-BA72-E191185E93F3", "Hours");
			//
			// MaxErrorsInMinutesCalcEdit
			//
			this.BindingSource.SetBindingMember(this.MaxErrorsInMinutesCalcEdit, "MaxErrorsInMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.MaxErrorsInMinutesCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("2A60E325-C61B-4D00-89FA-BCD92B11E5E6", "Maximum error count:");
			this.MaxErrorsInMinutesCalcEdit.DecimalPlaces = 0;
			this.MaxErrorsInMinutesCalcEdit.IsCalculatorEnabled = false;
			this.MaxErrorsInMinutesCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.MaxErrorsInMinutesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.MaxErrorsInMinutesCalcEdit.Name = "MaxErrorsInMinutesCalcEdit";
			this.MaxErrorsInMinutesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.MaxErrorsInMinutesCalcEdit.TabIndex = 2;
			this.MaxErrorsInMinutesCalcEdit.TabStop = false;
			this.MaxErrorsInMinutesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MaxErrorsInMinutesCalcEdit.AllowNegative = false;
			//
			// SuspendInMinutesCalcEdit
			//
			this.BindingSource.SetBindingMember(this.IntervalMinutesCalcEdit, "IntervalMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.IntervalMinutesCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("EA5F9F44-A83D-44CD-A2B3-9DBC1BC28DFA", "Interval minutes:");
			this.IntervalMinutesCalcEdit.DecimalPlaces = 0;
			this.IntervalMinutesCalcEdit.IsCalculatorEnabled = false;
			this.IntervalMinutesCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.IntervalMinutesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 40, true);
			this.IntervalMinutesCalcEdit.Name = "IntervalMinutesCalcEdit";
			this.IntervalMinutesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.IntervalMinutesCalcEdit.TabIndex = 3;
			this.IntervalMinutesCalcEdit.TabStop = false;
			this.IntervalMinutesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.IntervalMinutesCalcEdit.AllowNegative = false;
			//
			// MinuteTimeLimitsCalcEdit
			//
			this.BindingSource.SetBindingMember(this.SuspendMinutesCalcEdit, "SuspendMinutes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.SuspendMinutesCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("59CF0A7D-20AE-4636-AEA5-7A6C48929E22", "Suspend minutes:");
			this.SuspendMinutesCalcEdit.DecimalPlaces = 0;
			this.SuspendMinutesCalcEdit.IsCalculatorEnabled = false;
			this.SuspendMinutesCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.SuspendMinutesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 60, true);
			this.SuspendMinutesCalcEdit.Name = "SuspendMinutesCalcEdit";
			this.SuspendMinutesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.SuspendMinutesCalcEdit.TabIndex = 4;
			this.SuspendMinutesCalcEdit.TabStop = false;
			this.SuspendMinutesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SuspendMinutesCalcEdit.AllowNegative = false;
			//
			// MaxErrorsInHoursCalcEdit
			//
			this.BindingSource.SetBindingMember(this.MaxErrorsInHoursCalcEdit, "MaxErrorsInHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.MaxErrorsInHoursCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4E6FCFFA-31BE-493F-A573-392EF5129704", "Maximum error count:");
			this.MaxErrorsInHoursCalcEdit.DecimalPlaces = 0;
			this.MaxErrorsInHoursCalcEdit.IsCalculatorEnabled = false;
			this.MaxErrorsInHoursCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.MaxErrorsInHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.MaxErrorsInHoursCalcEdit.Name = "MaxErrorsInHoursCalcEdit";
			this.MaxErrorsInHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.MaxErrorsInHoursCalcEdit.TabIndex = 2;
			this.MaxErrorsInHoursCalcEdit.TabStop = false;
			this.MaxErrorsInHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MaxErrorsInHoursCalcEdit.AllowNegative = false;
			//
			// SuspendInMinutesCalcEdit
			//
			this.BindingSource.SetBindingMember(this.IntervalHoursCalcEdit, "IntervalHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.IntervalHoursCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("11F3D3E1-CBC5-4FB0-98CE-73B353C96E79", "Interval hours:");
			this.IntervalHoursCalcEdit.DecimalPlaces = 0;
			this.IntervalHoursCalcEdit.IsCalculatorEnabled = false;
			this.IntervalHoursCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.IntervalHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 40, true);
			this.IntervalHoursCalcEdit.Name = "IntervalHoursCalcEdit";
			this.IntervalHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.IntervalHoursCalcEdit.TabIndex = 3;
			this.IntervalHoursCalcEdit.TabStop = false;
			this.IntervalHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.IntervalHoursCalcEdit.AllowNegative = false;
			//
			// MinuteTimeLimitsCalcEdit
			//
			this.BindingSource.SetBindingMember(this.SuspendHoursCalcEdit, "SuspendHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.SuspendHoursCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("165D33EC-0F45-41C9-8360-6EE646667B2C", "Suspend hours:");
			this.SuspendHoursCalcEdit.DecimalPlaces = 0;
			this.SuspendHoursCalcEdit.IsCalculatorEnabled = false;
			this.SuspendHoursCalcEdit.Font = new System.Drawing.Font(OFont.NormalFontName, 8F, System.Drawing.FontStyle.Regular);
			this.SuspendHoursCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 60, true);
			this.SuspendHoursCalcEdit.Name = "SuspendHoursCalcEdit";
			this.SuspendHoursCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.SuspendHoursCalcEdit.TabIndex = 4;
			this.SuspendHoursCalcEdit.TabStop = false;
			this.SuspendHoursCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.SuspendHoursCalcEdit.AllowNegative = false;
			// 
			// WebPrintNudgeDirectIPAddressUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OptionGroupBox);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 230, true);
			this.Name = "WebPrintNudgeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 230, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private CargoWise.Windows.UI.KGroupBox OptionGroupBox;
		private CargoWise.Windows.UI.KGroupBox MinutesGroupBox;
		private CargoWise.Windows.UI.KGroupBox HoursGroupBox;
		private ZArchitecture.ZCalcEdit MaxErrorsInMinutesCalcEdit;
		private ZArchitecture.ZCalcEdit IntervalMinutesCalcEdit;
		private ZArchitecture.ZCalcEdit SuspendMinutesCalcEdit;

		private ZArchitecture.ZCalcEdit MaxErrorsInHoursCalcEdit;
		private ZArchitecture.ZCalcEdit IntervalHoursCalcEdit;
		private ZArchitecture.ZCalcEdit SuspendHoursCalcEdit;
	}
}
