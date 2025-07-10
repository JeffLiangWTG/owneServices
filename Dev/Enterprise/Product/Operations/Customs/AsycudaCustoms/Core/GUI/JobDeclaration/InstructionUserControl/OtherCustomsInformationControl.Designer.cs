namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class OtherCustomsInformationControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.CustomsOfficeOfDestinationOrExitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LocalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsOfficeOfDestinationOrExitDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction);
			// 
			// CustomsOfficeOfDestinationOrExitDropEdit
			// 
			this.CustomsOfficeOfDestinationOrExitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeOfDestinationOrExitDropEdit, "ASY_PortOfExit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).ASY_PortOfExit)));
			this.CustomsOfficeOfDestinationOrExitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 24, true);
			this.CustomsOfficeOfDestinationOrExitDropEdit.Name = "CustomsOfficeOfDestinationOrExitDropEdit";
			this.CustomsOfficeOfDestinationOrExitDropEdit.PreBoundMaxLength = 4;
			this.CustomsOfficeOfDestinationOrExitDropEdit.ShouldResizeByMaxLength = true;
			this.CustomsOfficeOfDestinationOrExitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CustomsOfficeOfDestinationOrExitDropEdit.TabIndex = 2;
			// 
			// LocalReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.LocalReferenceNumberTextBox, "ASY_LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AsycudaCustoms.Business.CusEntryInstruction)(null)).ASY_LocalReferenceNumber)));
			this.LocalReferenceNumberTextBox.CaptionResourceString = null;
			this.LocalReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 0, true);
			this.LocalReferenceNumberTextBox.Name = "LocalReferenceNumberTextBox";
			this.LocalReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.LocalReferenceNumberTextBox.TabIndex = 1;
			// 
			// OtherCustomsInformationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsOfficeOfDestinationOrExitDropEdit);
			this.Controls.Add(this.LocalReferenceNumberTextBox);
			this.Name = "OtherCustomsInformationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(604, 65, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsOfficeOfDestinationOrExitDropEdit.ResumeLayout(true);
			this.CustomsOfficeOfDestinationOrExitDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.GUI.ZDropEdit CustomsOfficeOfDestinationOrExitDropEdit;
		private ZArchitecture.ZTextBox LocalReferenceNumberTextBox;
	}
}
