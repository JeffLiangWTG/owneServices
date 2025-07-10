namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class TransportBorderUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.BorderTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BorderTransportTypeOfIdDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BorderTransportIdAndNationalityUserControl = new Enterprise.Customs.EU.NCTS.GUI.BorderTransportIdAndNationalityUserControl();
			this.BorderConveyanceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BorderOfficeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalTransportBorderUserControl = new Enterprise.Customs.EU.NCTS.GUI.AdditionalTransportBorderUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BorderTransportModeDropEdit.SuspendLayout();
			this.BorderTransportTypeOfIdDropEdit.SuspendLayout();
			this.BorderTransportIdAndNationalityUserControl.SuspendLayout();
			this.BorderOfficeDropEdit.SuspendLayout();
			this.AdditionalTransportBorderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader);
			// 
			// BorderTransportModeDropEdit
			// 
			this.BorderTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportModeDropEdit, "BM_ExportTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_ExportTransportMode)));
			this.BorderTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 17, true);
			this.BorderTransportModeDropEdit.Name = "BorderTransportModeDropEdit";
			this.BorderTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BorderTransportModeDropEdit.TabIndex = 0;
			// 
			// BorderTransportTypeOfIdDropEdit
			// 
			this.BorderTransportTypeOfIdDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportTypeOfIdDropEdit, "BM_ActiveBorderIdentificationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_ActiveBorderIdentificationType)));
			this.BorderTransportTypeOfIdDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 43, true);
			this.BorderTransportTypeOfIdDropEdit.Name = "BorderTransportTypeOfIdDropEdit";
			this.BorderTransportTypeOfIdDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.BorderTransportTypeOfIdDropEdit.TabIndex = 1;
			// 
			// BorderTransportIdAndNationalityUserControl
			// 
			this.BorderTransportIdAndNationalityUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportIdAndNationalityUserControl, ".");
			this.BorderTransportIdAndNationalityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 78, true);
			this.BorderTransportIdAndNationalityUserControl.Name = "BorderTransportIdAndNationalityUserControl";
			this.BorderTransportIdAndNationalityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 23, true);
			this.BorderTransportIdAndNationalityUserControl.TabIndex = 2;
			// 
			// BorderConveyanceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BorderConveyanceNumberTextBox, "BM_ConveyanceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_ConveyanceNumber)));
			this.BorderConveyanceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BorderConveyanceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 125, true);
			this.BorderConveyanceNumberTextBox.Name = "BorderConveyanceNumberTextBox";
			this.BorderConveyanceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.BorderConveyanceNumberTextBox.TabIndex = 9;
			// 
			// BorderOfficeDropEdit
			// 
			this.BorderOfficeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderOfficeDropEdit, "BM_CustomsOfficeAtBorder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsDepartureMovementHeader)(null)).BM_CustomsOfficeAtBorder)));
			this.BorderOfficeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 150, true);
			this.BorderOfficeDropEdit.Name = "BorderOfficeDropEdit";
			this.BorderOfficeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.BorderOfficeDropEdit.TabIndex = 10;
			// 
			// AdditionalTransportBorderUserControl
			// 
			this.AdditionalTransportBorderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalTransportBorderUserControl, ".");
			this.AdditionalTransportBorderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 206, true);
			this.AdditionalTransportBorderUserControl.Name = "AdditionalTransportBorderUserControl";
			this.AdditionalTransportBorderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(277, 33, true);
			this.AdditionalTransportBorderUserControl.TabIndex = 12;
			// 
			// TransportBorderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BorderTransportTypeOfIdDropEdit);
			this.Controls.Add(this.BorderTransportModeDropEdit);
			this.Controls.Add(this.BorderTransportIdAndNationalityUserControl);
			this.Controls.Add(this.BorderConveyanceNumberTextBox);
			this.Controls.Add(this.BorderOfficeDropEdit);
			this.Controls.Add(this.AdditionalTransportBorderUserControl);
			this.Name = "TransportBorderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(853, 319, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BorderTransportModeDropEdit.ResumeLayout(true);
			this.BorderTransportModeDropEdit.PerformLayout();
			this.BorderTransportTypeOfIdDropEdit.ResumeLayout(true);
			this.BorderTransportTypeOfIdDropEdit.PerformLayout();
			this.BorderTransportIdAndNationalityUserControl.ResumeLayout(true);
			this.BorderTransportIdAndNationalityUserControl.PerformLayout();
			this.BorderOfficeDropEdit.ResumeLayout(true);
			this.BorderOfficeDropEdit.PerformLayout();
			this.AdditionalTransportBorderUserControl.ResumeLayout(true);
			this.AdditionalTransportBorderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit BorderTransportModeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit BorderTransportTypeOfIdDropEdit;
		internal BorderTransportIdAndNationalityUserControl BorderTransportIdAndNationalityUserControl;
		internal ZArchitecture.ZTextBox BorderConveyanceNumberTextBox;
		internal ZArchitecture.GUI.ZDropEdit BorderOfficeDropEdit;
		internal AdditionalTransportBorderUserControl AdditionalTransportBorderUserControl;
	}
}
