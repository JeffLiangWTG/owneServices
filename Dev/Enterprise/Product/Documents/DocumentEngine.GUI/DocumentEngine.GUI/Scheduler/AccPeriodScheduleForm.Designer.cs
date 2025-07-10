namespace Enterprise.DocumentEngine.GUI.Scheduler
{
	partial class AccPeriodScheduleForm
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
			this.DescriptionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DescriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.PeriodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PeriodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DescriptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.DescriptionGroupBox);
			this.MainPanel.Controls.Add(this.PeriodDropEdit);
			this.MainPanel.Controls.Add(this.PeriodCalcEdit);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 108, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 138, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule);
			// 
			// DescriptionGroupBox
			// 
			this.DescriptionGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccPeriodScheduleForm|84459e41-18c5-4d5e-b19a-15bef996bbcc", "Description");
			this.DescriptionGroupBox.Controls.Add(this.DescriptionLabel);
			this.DescriptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 48, true);
			this.DescriptionGroupBox.Name = "DescriptionGroupBox";
			this.DescriptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 54, true);
			this.DescriptionGroupBox.TabIndex = 3;
			this.DescriptionGroupBox.TabStop = false;
			// 
			// DescriptionLabel
			// 
			this.BindingSource.SetBindingMember(this.DescriptionLabel, "Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule)(null)).Description)));
			this.DescriptionLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DescriptionLabel, false);
			this.DescriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DescriptionLabel.Name = "DescriptionLabel";
			this.DescriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 35, true);
			this.DescriptionLabel.TabIndex = 0;
			this.DescriptionLabel.Text = "Description";
			this.DescriptionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// PeriodDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PeriodDropEdit, "PeriodScope");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule)(null)).PeriodScope)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule)(null)).Lookups.PeriodScopes)));
			this.PeriodDropEdit.BindToList = "Lookups+PeriodScopes";
			this.PeriodDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PeriodDropEdit.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccPeriodScheduleForm|918b4cc4-d991-4ee6-9163-3036622a1ebb", "Period");
			this.PeriodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 12, true);
			this.PeriodDropEdit.Name = "PeriodDropEdit";
			this.PeriodDropEdit.PreBoundMaxLength = 8;
			this.PeriodDropEdit.ShowDescriptionBox = false;
			this.PeriodDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.PeriodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.PeriodDropEdit.TabIndex = 1;
			// 
			// PeriodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PeriodCalcEdit, "PeriodCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule)(null)).PeriodCount)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.PeriodCalcEdit, false);
			this.PeriodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(202, 12, true);
			this.PeriodCalcEdit.Name = "PeriodCalcEdit";
			this.PeriodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
			this.PeriodCalcEdit.TabIndex = 2;
			this.PeriodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AccPeriodScheduleForm
			// 
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("AccPeriodScheduleForm|e1053825-1334-4eaa-97a5-ba5ad7887810", "Schedule");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(314, 162, true);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.Scheduler.Business.AccPeriodSchedule";
			this.Name = "AccPeriodScheduleForm";
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DescriptionGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox DescriptionGroupBox;
		private Enterprise.ZArchitecture.ZLabel DescriptionLabel;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PeriodDropEdit;
		private Enterprise.ZArchitecture.ZCalcEdit PeriodCalcEdit;
	}
}
