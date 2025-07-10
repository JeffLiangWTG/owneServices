using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI
{
	sealed partial class ExitStatusUserControl
	{
		void InitializeComponent()
		{
			this.ExitStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// ExitStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExitStatusTextBox, "CustomsEntryHeaders.EntryNumbersProvider.IvistoWrapper.Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumbersProvider.IvistoWrapper.Status)));
			this.ExitStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExitStatusTextBox, false);
			this.ExitStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExitStatusTextBox.Name = "ExitStatusTextBox";
			this.ExitStatusTextBox.ReadOnly = true;
			this.ExitStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ExitStatusTextBox.TabIndex = 0;
			// 
			// ExitStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExitStatusDescriptionTextBox, "CustomsEntryHeaders.EntryNumbersProvider.IvistoWrapper.StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumbersProvider.IvistoWrapper.StatusDescription)));
			this.ExitStatusDescriptionTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExitStatusDescriptionTextBox, false);
			this.ExitStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 0, true);
			this.ExitStatusDescriptionTextBox.Name = "ExitStatusDescriptionTextBox";
			this.ExitStatusDescriptionTextBox.ReadOnly = true;
			this.ExitStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.ExitStatusDescriptionTextBox.TabIndex = 1;
			// 
			// ExitStatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExitStatusDescriptionTextBox);
			this.Controls.Add(this.ExitStatusTextBox);
			this.Name = "ExitStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZTextBox ExitStatusTextBox;
		internal ZTextBox ExitStatusDescriptionTextBox;
	}
}
