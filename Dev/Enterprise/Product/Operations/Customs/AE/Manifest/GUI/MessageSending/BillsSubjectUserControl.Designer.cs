namespace Enterprise.Customs.AE.Manifest.GUI
{
	partial class BillsSubjectUserControl
	{
		void InitializeComponent()
		{
			this.SubjectTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SubjectGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SubjectGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AE.Manifest.Business.MessageChooserItem);
			// 
			// SubjectTextBox
			// 
			this.BindingSource.SetBindingMember(this.SubjectTextBox, "Subject");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AE.Manifest.Business.MessageChooserItem)(null)).Subject)));
			this.SubjectTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.SubjectTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.SubjectTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.SubjectTextBox.Multiline = true;
			this.SubjectTextBox.Name = "SubjectTextBox";
			this.SubjectTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.SubjectTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 127, true);
			this.SubjectTextBox.TabIndex = 0;
			// 
			// SubjectGroupBox
			// 
			this.SubjectGroupBox.CaptionResourceString = Enterprise.Customs.AE.Manifest.GUI.Res.GetData("5cf65efe-52ae-4984-97d0-dfd5b65c51b0", "Subject");
			this.SubjectGroupBox.Controls.Add(this.SubjectTextBox);
			this.SubjectGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.SubjectGroupBox.Name = "SubjectGroupBox";
			this.SubjectGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 142, true);
			this.SubjectGroupBox.TabIndex = 1;
			this.SubjectGroupBox.TabStop = false;
			// 
			// BillsSubjectUserControl
			// 
			this.Controls.Add(this.SubjectGroupBox);
			this.Name = "BillsSubjectUserControl";
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SubjectGroupBox.ResumeLayout(false);
			this.SubjectGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZArchitecture.ZTextBox SubjectTextBox;
		private ZArchitecture.GUI.ZGroupBox SubjectGroupBox;
	}
}
