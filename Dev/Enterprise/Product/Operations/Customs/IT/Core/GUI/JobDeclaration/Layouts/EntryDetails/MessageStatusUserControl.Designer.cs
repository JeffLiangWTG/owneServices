using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI
{
	sealed partial class MessageStatusUserControl
	{
		void InitializeComponent()
		{
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessageStatusDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// MessageStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "CustomsEntryHeaders.CH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_Status)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageStatusTextBox, false);
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.ReadOnly = true;
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MessageStatusTextBox.TabIndex = 0;
			// 
			// MessageStatusDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageStatusDescriptionTextBox, "CustomsEntryHeaders.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).MessageStatusDescription)));
			this.MessageStatusDescriptionTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageStatusDescriptionTextBox, false);
			this.MessageStatusDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(42, 0, true);
			this.MessageStatusDescriptionTextBox.Name = "MessageStatusDescriptionTextBox";
			this.MessageStatusDescriptionTextBox.ReadOnly = true;
			this.MessageStatusDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.MessageStatusDescriptionTextBox.TabIndex = 1;
			// 
			// MessageStatusUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageStatusDescriptionTextBox);
			this.Controls.Add(this.MessageStatusTextBox);
			this.Name = "MessageStatusUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZTextBox MessageStatusTextBox;
		internal ZTextBox MessageStatusDescriptionTextBox;
	}
}
