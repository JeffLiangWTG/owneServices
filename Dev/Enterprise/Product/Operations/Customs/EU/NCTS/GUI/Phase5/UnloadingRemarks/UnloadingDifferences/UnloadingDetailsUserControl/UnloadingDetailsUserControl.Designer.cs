namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class UnloadingDetailsUserControl
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
			this.UnloadingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.UnloadingConformCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.StateOfSealsCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.UnloadingCompletedCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.UnloadingRemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OtherThingsToReportTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadingDateDateEdit.SuspendLayout();
			this.UnloadingConformCheckBox.SuspendLayout();
			this.StateOfSealsCheckBox.SuspendLayout();
			this.UnloadingCompletedCheckBox.SuspendLayout();
			this.UnloadingRemarksTextBox.SuspendLayout();
			this.OtherThingsToReportTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// UnloadingDate
			// 
			this.UnloadingDateDateEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingDateDateEdit, "ArrivalMovementHeader.BM_UnloadingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).BM_UnloadingDate)));
			this.UnloadingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 20, true);
			this.UnloadingDateDateEdit.Name = "UnloadingDateDateEdit";
			this.UnloadingDateDateEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UnloadingDateDateEdit.TabIndex = 0;
			this.UnloadingDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			// 
			// UnloadingConform
			// 
			this.UnloadingConformCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingConformCheckBox, "ArrivalMovementHeader.BM_NoChangesToReport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).BM_NoChangesToReport)));
			this.UnloadingConformCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 48, true);
			this.UnloadingConformCheckBox.Name = "UnloadingConformCheckBox";
			this.UnloadingConformCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UnloadingConformCheckBox.TabIndex = 1;			
			// 
			// StateOfSeals
			// 
			this.StateOfSealsCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StateOfSealsCheckBox, "ArrivalMovementHeader.BM_StateOfSealsBoolean");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).BM_StateOfSealsBoolean)));
			this.StateOfSealsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 76, true);
			this.StateOfSealsCheckBox.Name = "StateOfSealsCheckBox";
			this.StateOfSealsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StateOfSealsCheckBox.TabIndex = 2;			
			// 
			// UnloadingCompleted
			// 
			this.UnloadingCompletedCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadingCompletedCheckBox, "ArrivalMovementHeader.BM_UnloadingCompleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).BM_UnloadingCompleted)));
			this.UnloadingCompletedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 104, true);
			this.UnloadingCompletedCheckBox.Name = "UnloadingCompletedCheckBox";
			this.UnloadingCompletedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UnloadingCompletedCheckBox.TabIndex = 3;			
			// 
			// UnloadingRemarks
			// 
			this.UnloadingRemarksTextBox.AllowDrop = true;
			this.UnloadingRemarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BindingSource.SetBindingMember(this.UnloadingRemarksTextBox, "ArrivalMovementHeader.BM_UnloadingRemarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).BM_UnloadingRemarks)));
			this.UnloadingRemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 132, true);
			this.UnloadingRemarksTextBox.Name = "UnloadingRemarksTextBox";
			this.UnloadingRemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UnloadingRemarksTextBox.TabIndex = 4;			
			this.UnloadingRemarksTextBox.Multiline = true;
			this.UnloadingRemarksTextBox.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// OtherThingsToReport
			// 
			this.OtherThingsToReportTextBox.AllowDrop = true;
			this.OtherThingsToReportTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BindingSource.SetBindingMember(this.OtherThingsToReportTextBox, "ArrivalMovementHeader.OtherThingsToReport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalMovementHeader)(null)).OtherThingsToReport)));
			this.OtherThingsToReportTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 160, true);
			this.OtherThingsToReportTextBox.Name = "OtherThingsToReportTextBox";
			this.OtherThingsToReportTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OtherThingsToReportTextBox.TabIndex = 5;			
			this.OtherThingsToReportTextBox.Multiline = true;
			this.OtherThingsToReportTextBox.Height = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			// 
			// UnloadingDetailsUserControl
			//
			this.Controls.Add(this.UnloadingDateDateEdit);
			this.Controls.Add(this.UnloadingConformCheckBox);
			this.Controls.Add(this.StateOfSealsCheckBox);
			this.Controls.Add(this.UnloadingCompletedCheckBox);
			this.Controls.Add(this.UnloadingRemarksTextBox);
			this.Controls.Add(this.OtherThingsToReportTextBox);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadingDateDateEdit);
			this.Controls.SetChildIndex(this.UnloadingDateDateEdit, 0);
			this.Controls.Add(this.UnloadingConformCheckBox);
			this.Controls.SetChildIndex(this.UnloadingConformCheckBox, 0);
			this.Controls.Add(this.StateOfSealsCheckBox);
			this.Controls.SetChildIndex(this.StateOfSealsCheckBox, 0);
			this.Controls.Add(this.UnloadingCompletedCheckBox);
			this.Controls.SetChildIndex(this.UnloadingCompletedCheckBox, 0);
			this.Controls.Add(this.OtherThingsToReportTextBox);
			this.Controls.SetChildIndex(this.OtherThingsToReportTextBox, 0);
			this.Name = "UnloadingDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadingDateDateEdit.ResumeLayout(false);
			this.UnloadingDateDateEdit.PerformLayout();
			this.UnloadingConformCheckBox.ResumeLayout(false);
			this.UnloadingConformCheckBox.PerformLayout();
			this.StateOfSealsCheckBox.ResumeLayout(false);
			this.StateOfSealsCheckBox.PerformLayout();
			this.UnloadingCompletedCheckBox.ResumeLayout(false);
			this.UnloadingCompletedCheckBox.PerformLayout();
			this.UnloadingRemarksTextBox.ResumeLayout(false);
			this.UnloadingRemarksTextBox.PerformLayout();
			this.OtherThingsToReportTextBox.ResumeLayout(false);
			this.OtherThingsToReportTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZDateEdit UnloadingDateDateEdit;
		internal ZArchitecture.GUI.ZCheckBox UnloadingConformCheckBox;
		internal ZArchitecture.GUI.ZCheckBox StateOfSealsCheckBox;
		internal ZArchitecture.GUI.ZCheckBox UnloadingCompletedCheckBox;
		internal ZArchitecture.ZTextBox UnloadingRemarksTextBox;
		internal ZArchitecture.ZTextBox OtherThingsToReportTextBox;

		#endregion
	}
}
