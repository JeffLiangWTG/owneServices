using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI
{
	sealed partial class ExitOfficeUserControl
	{
		void InitializeComponent()
		{
			this.ExitOfficeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitOfficeDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// ExitOfficeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExitOfficeTextBox, "CustomsEntryHeaders.EntryNumbersProvider.IvistoWrapper.Office");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumbersProvider.IvistoWrapper.Office)));
			this.ExitOfficeTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExitOfficeTextBox, false);
			this.ExitOfficeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExitOfficeTextBox.Name = "ExitOfficeTextBox";
			this.ExitOfficeTextBox.ReadOnly = true;
			this.ExitOfficeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.ExitOfficeTextBox.TabIndex = 0;
			// 
			// ExitOfficeDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExitOfficeDescriptionTextBox, "CustomsEntryHeaders.EntryNumbersProvider.IvistoWrapper.OfficeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumbersProvider.IvistoWrapper.OfficeDescription)));
			this.ExitOfficeDescriptionTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExitOfficeDescriptionTextBox, false);
			this.ExitOfficeDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 0, true);
			this.ExitOfficeDescriptionTextBox.Name = "ExitOfficeDescriptionTextBox";
			this.ExitOfficeDescriptionTextBox.ReadOnly = true;
			this.ExitOfficeDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.ExitOfficeDescriptionTextBox.TabIndex = 1;
			// 
			// ExitOfficeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExitOfficeDescriptionTextBox);
			this.Controls.Add(this.ExitOfficeTextBox);
			this.Name = "ExitOfficeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZTextBox ExitOfficeTextBox;
		internal ZTextBox ExitOfficeDescriptionTextBox;
	}
}
