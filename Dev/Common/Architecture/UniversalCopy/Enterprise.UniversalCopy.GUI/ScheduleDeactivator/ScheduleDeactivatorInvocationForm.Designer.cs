
namespace Enterprise.UniversalCopy.GUI
{
	partial class ScheduleDeactivatorInvocationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.formDescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.SchedulesDisplayTextbox = new Enterprise.ZArchitecture.ZTextBox();
			this.buttonStripPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 257, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.UniversalCopy.Business.ScheduleDeactivatorViewModel);
			// 
			// formDescriptionLabel
			// 
			this.formDescriptionLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.formDescriptionLabel, "FormCaption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.ScheduleDeactivatorViewModel)(null)).FormCaption)));
			this.formDescriptionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Largest | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.formDescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 13, true);
			this.formDescriptionLabel.Name = "formDescriptionLabel";
			this.formDescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 20, true);
			this.formDescriptionLabel.TabIndex = 4;
			// 
			// SchedulesDisplayTextbox
			// 
			this.BindingSource.SetBindingMember(this.SchedulesDisplayTextbox, "SchedulesText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.UniversalCopy.Business.ScheduleDeactivatorViewModel)(null)).SchedulesText)));
			this.SchedulesDisplayTextbox.CaptionResourceString = null;
			this.SchedulesDisplayTextbox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.SchedulesDisplayTextbox, false);
			this.SchedulesDisplayTextbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 63, true);
			this.SchedulesDisplayTextbox.Multiline = true;
			this.SchedulesDisplayTextbox.Name = "SchedulesDisplayTextbox";
			this.SchedulesDisplayTextbox.ReadOnly = true;
			this.SchedulesDisplayTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.SchedulesDisplayTextbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 99, true);
			this.SchedulesDisplayTextbox.TabIndex = 5;
			this.SchedulesDisplayTextbox.TabStop = false;
			// 
			// buttonStripPanel
			// 
			this.buttonStripPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 171, true);
			this.buttonStripPanel.Name = "buttonStripPanel";
			this.buttonStripPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 80, true);
			this.buttonStripPanel.TabIndex = 6;
			// 
			// ScheduleDeactivatorInvocationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.UniversalCopy.GUI.Res.GetData("6c7a45b4-ad67-4e77-ab0e-0f155dd5c45d", "Deactivation of related copy schedules");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 274, true);
			this.Controls.Add(this.buttonStripPanel);
			this.Controls.Add(this.SchedulesDisplayTextbox);
			this.Controls.Add(this.formDescriptionLabel);
			this.DataSourceType = typeof(Enterprise.UniversalCopy.Business.ScheduleDeactivatorViewModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ScheduleDeactivatorInvocationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.formDescriptionLabel, 0);
			this.Controls.SetChildIndex(this.SchedulesDisplayTextbox, 0);
			this.Controls.SetChildIndex(this.buttonStripPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZLabel formDescriptionLabel;
		private ZArchitecture.ZTextBox SchedulesDisplayTextbox;
		private ZArchitecture.GUI.ZPanel buttonStripPanel;
	}
}
