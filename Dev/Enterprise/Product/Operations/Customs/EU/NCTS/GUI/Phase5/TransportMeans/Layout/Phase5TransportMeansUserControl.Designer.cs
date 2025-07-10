namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5TransportMeansUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TransportAtDepartureTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransportAtDepartureIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportAtDepartureNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportAtDepartureTypeDropEdit.SuspendLayout();
			this.TransportAtDepartureNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusInBondEvent);
			// 
			// TransportAtDepartureTypeDropEdit
			// 
			this.TransportAtDepartureTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportAtDepartureTypeDropEdit, "BN_TransportAtDepartureType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.TransportAtDepartureTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 28, true);
			this.TransportAtDepartureTypeDropEdit.Name = "TransportAtDepartureTypeDropEdit";
			this.TransportAtDepartureTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.TransportAtDepartureTypeDropEdit.TabIndex = 1;
			// 
			// ReferenceNumberUCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportAtDepartureIDTextBox, "BN_TransportAtDepartureID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.TransportAtDepartureIDTextBox.CaptionResourceString = null;
			this.TransportAtDepartureIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TransportAtDepartureIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 54, true);
			this.TransportAtDepartureIDTextBox.Name = "TransportAtDepartureIDTextBox";
			this.TransportAtDepartureIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.TransportAtDepartureIDTextBox.TabIndex = 4;
			// 
			// TypeCodeFindBox
			// 
			this.TransportAtDepartureNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportAtDepartureNationalityCodeFindBox, "BN_RN_NKTransportAtDepartureIDNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.TransportAtDepartureNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 80, true);
			this.TransportAtDepartureNationalityCodeFindBox.Name = "TransportAtDepartureNationalityCodeFindBox";
			this.TransportAtDepartureNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportAtDepartureNationalityCodeFindBox.ParentType = null;
			this.TransportAtDepartureNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TransportAtDepartureNationalityCodeFindBox.TabIndex = 5;
			// 
			// Phase5TransportMeansUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransportAtDepartureNationalityCodeFindBox);
			this.Controls.Add(this.TransportAtDepartureIDTextBox);
			this.Controls.Add(this.TransportAtDepartureTypeDropEdit);
			this.Name = "Phase5TransportMeansUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(742, 348, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportAtDepartureTypeDropEdit.ResumeLayout(true);
			this.TransportAtDepartureTypeDropEdit.PerformLayout();
			this.TransportAtDepartureNationalityCodeFindBox.ResumeLayout(true);
			this.TransportAtDepartureNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit TransportAtDepartureTypeDropEdit;
		internal ZArchitecture.ZTextBox TransportAtDepartureIDTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox TransportAtDepartureNationalityCodeFindBox;
	}
}
