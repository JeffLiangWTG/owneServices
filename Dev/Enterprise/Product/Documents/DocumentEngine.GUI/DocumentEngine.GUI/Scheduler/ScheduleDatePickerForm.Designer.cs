namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	partial class ScheduleDatePickerForm
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
		public new void InitializeComponent()
		{
			this.NextScheduleDateDropEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.HelpLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(308);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask);
			// 
			// NextScheduleDateDropEdit
			// 
			this.NextScheduleDateDropEdit.AutoCompleteMonthThreshold = 1;
			this.NextScheduleDateDropEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.NextScheduleDateDropEdit, "CalcNextRunTimeLocal");
			this.NextScheduleDateDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleDatePickerForm|e128db91-3de9-41b2-a20d-2c5e2f45d8ae", "Schedule date");
			this.NextScheduleDateDropEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.NextScheduleDateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(139, 36, true);
			this.NextScheduleDateDropEdit.Name = "NextScheduleDateDropEdit";
			this.NextScheduleDateDropEdit.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleDatePickerForm|36c5c57b-e4e8-4b0c-ab83-0d8240dbdeaa", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(158, 66, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleDatePickerForm|3a95e862-b2ee-4ae9-b1e4-f34b73f49c9a", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 66, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// HelpLabel
			// 
			this.HelpLabel.AutoSize = true;
			this.HelpLabel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleDatePickerForm|815c2a8d-8bb2-41ec-8c3e-b39acf9525e0", "Please select the date and time that the report should run.");
			this.HelpLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.HelpLabel.Name = "HelpLabel";
			this.HelpLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.HelpLabel.TabIndex = 5;
			// 
			// ScheduleDatePickerForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 100, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ScheduleDatePickerForm|9055de22-9441-4d4a-a4b9-a17d57db124f", "Schedule");
			this.Controls.Add(this.HelpLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.NextScheduleDateDropEdit);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.Scheduler.Business.ReportScheduleTask";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "ScheduleDatePickerForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NextScheduleDateDropEdit, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.HelpLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDateEdit NextScheduleDateDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
		internal Enterprise.ZArchitecture.ZLabel HelpLabel;
	}
}
