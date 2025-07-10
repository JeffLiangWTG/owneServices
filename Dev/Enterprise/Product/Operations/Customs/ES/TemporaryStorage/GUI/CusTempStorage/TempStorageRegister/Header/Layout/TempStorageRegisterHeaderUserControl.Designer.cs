namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class TempStorageRegisterHeaderUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TempStorageRegisterUserControl));
			this.DDTNumberUserControl = new Enterprise.Customs.ES.TemporaryStorage.GUI.DDTNumberUserControl();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PresentationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.PreviousReferenceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PreviousReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InternalReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DDTNumberUserControl.SuspendLayout();
			this.ArrivalDateEdit.SuspendLayout();
			this.PresentationDateEdit.SuspendLayout();
			this.PreviousReferenceTypeDropEdit.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader);
			// 
			// DDTNumberUserControl
			// 
			this.DDTNumberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DDTNumberUserControl, ".");
			this.DDTNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 19, true);
			this.DDTNumberUserControl.Name = "DDTNumberUserControl";
			this.DDTNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.DDTNumberUserControl.TabIndex = 0;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "SRH_ArrivalDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_ArrivalDate)));
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 19, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 1;
			// 
			// PresentationDateEdit
			// 
			this.PresentationDateEdit.AllowDrop = true;
			this.PresentationDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.PresentationDateEdit, "SRH_PresentationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_PresentationDate)));
			this.PresentationDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.PresentationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(790, 19, true);
			this.PresentationDateEdit.Name = "PresentationDateEdit";
			this.PresentationDateEdit.TabIndex = 2;
			// 
			// PreviousReferenceTypeDropEdit
			// 
			this.PreviousReferenceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PreviousReferenceTypeDropEdit, "SRH_PreviousReferenceType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_PreviousReferenceType)));
			this.PreviousReferenceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 45, true);
			this.PreviousReferenceTypeDropEdit.Name = "PreviousReferenceTypeDropEdit";
			this.PreviousReferenceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PreviousReferenceTypeDropEdit.TabIndex = 4;
			// 
			// PreviousReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousReferenceNumberTextBox, "SRH_PreviousReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_PreviousReference)));
			this.PreviousReferenceNumberTextBox.CaptionResourceString = ((CargoWiseOne.ResourceStrings.ResourceStringData)(resources.GetObject("PreviousReferenceNumberTextBox.CaptionResourceString")));
			this.PreviousReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 71, true);
			this.PreviousReferenceNumberTextBox.Name = "PreviousReferenceNumberTextBox";
			this.PreviousReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PreviousReferenceNumberTextBox.TabIndex = 6;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "SRH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_Status)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 71, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StatusDropEdit.TabIndex = 5;
			// 
			// InternalReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalReferenceTextBox, "SRH_InternalReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegHeader)(null)).SRH_InternalReference)));
			this.InternalReferenceTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("F9015631-435A-4C8D-BBCF-1A05272AA95E", "Job Reference");
			this.InternalReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 45, true);
			this.InternalReferenceTextBox.Name = "InternalReferenceTextBox";
			this.InternalReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.InternalReferenceTextBox.TabIndex = 3;
			// 
			// TempStorageRegisterHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InternalReferenceTextBox);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.PreviousReferenceTypeDropEdit);
			this.Controls.Add(this.PresentationDateEdit);
			this.Controls.Add(this.ArrivalDateEdit);
			this.Controls.Add(this.PreviousReferenceNumberTextBox);
			this.Controls.Add(this.DDTNumberUserControl);
			this.Name = "TempStorageRegisterHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 186, true);
			this.Tag = "";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DDTNumberUserControl.ResumeLayout(true);
			this.DDTNumberUserControl.PerformLayout();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.PresentationDateEdit.ResumeLayout(true);
			this.PresentationDateEdit.PerformLayout();
			this.PreviousReferenceTypeDropEdit.ResumeLayout(true);
			this.PreviousReferenceTypeDropEdit.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ZArchitecture.ZTextBox InternalReferenceTextBox;
		internal ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PreviousReferenceTypeDropEdit;
		internal ZArchitecture.GUI.ZDateEdit PresentationDateEdit;
		internal ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		internal ZArchitecture.ZTextBox PreviousReferenceNumberTextBox;
		internal DDTNumberUserControl DDTNumberUserControl;

		#endregion


	}
}
