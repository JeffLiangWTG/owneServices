namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class CustomsProcessingUserControl
	{
		void InitializeComponent()
		{
			this.customsProcessingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.transmitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.customsProcessingGroupBox.SuspendLayout();
			this.transmitDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobDeclaration);
			// 
			// customsProcessingGroupBox
			// 
			this.customsProcessingGroupBox.Controls.Add(this.transmitDateDateEdit);
			this.customsProcessingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.customsProcessingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.customsProcessingGroupBox.Name = "customsProcessingGroupBox";
			this.customsProcessingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 49, true);
			this.customsProcessingGroupBox.TabIndex = 0;
			this.customsProcessingGroupBox.TabStop = false;
			this.customsProcessingGroupBox.Text = "Customs Processing";
			// 
			// transmitDateDateEdit
			// 
			this.transmitDateDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.transmitDateDateEdit, "JE_EDITransmitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobDeclaration)(null)).JE_EDITransmitDate)));
			this.transmitDateDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("CustomsProcessingUserControl|57308684-0352-4F42-977A-827C5170BC9D", "EDI Transmit Date");
			this.transmitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.transmitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.transmitDateDateEdit.Name = "transmitDateDateEdit";
			this.transmitDateDateEdit.TabIndex = 0;
			// 
			// CustomsProcessingUserControl
			// 
			this.Controls.Add(this.customsProcessingGroupBox);
			this.Name = "CustomsProcessingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.customsProcessingGroupBox.ResumeLayout(false);
			this.customsProcessingGroupBox.PerformLayout();
			this.transmitDateDateEdit.ResumeLayout(true);
			this.transmitDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZGroupBox customsProcessingGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit transmitDateDateEdit;
	}
}
