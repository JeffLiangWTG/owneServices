using System.Windows.Forms;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.NCTS.GUI;

partial class MultipleMrnMessageSendingDetailsUserControl
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
			this.UnloadedCargoConformsToDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnloadingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DateOfUnloadingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnloadingGroupBox.SuspendLayout();
			this.DateOfUnloadingDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectParent);
			// 
			// UnloadedCargoConformsToDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UnloadedCargoConformsToDeclarationCheckBox, "IsConformed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectParent)(null)).IsConformed)));
			this.UnloadedCargoConformsToDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(238, 23, true);
			this.UnloadedCargoConformsToDeclarationCheckBox.Name = "UnloadedCargoConformsToDeclarationCheckBox";
			this.UnloadedCargoConformsToDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 24, true);
			this.UnloadedCargoConformsToDeclarationCheckBox.TabIndex = 2;
			// 
			// UnloadingGroupBox
			// 
			this.UnloadingGroupBox.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("F92DE91E-2A30-4BC5-B587-4248E2D998DB", "Send all ticked MRN with:");
			this.UnloadingGroupBox.Controls.Add(this.UnloadedCargoConformsToDeclarationCheckBox);
			this.UnloadingGroupBox.Controls.Add(this.DateOfUnloadingDateEdit);
			this.UnloadingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.UnloadingGroupBox.Name = "UnloadingGroupBox";
			this.UnloadingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 58, true);
			this.UnloadingGroupBox.TabIndex = 2;
			this.UnloadingGroupBox.TabStop = false;
			// 
			// DateOfUnloadingDateEdit
			// 
			this.DateOfUnloadingDateEdit.AllowDrop = true;
			this.DateOfUnloadingDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.DateOfUnloadingDateEdit, "DateOfUnloading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CH.NCTS.Business.NctsHeaderArrivalMessageSendingObjectParent)(null)).DateOfUnloading)));
			this.DateOfUnloadingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 26, true);
			this.DateOfUnloadingDateEdit.Name = "DateOfUnloadingDateEdit";
			this.DateOfUnloadingDateEdit.TabIndex = 1;
			// 
			// MultipleMrnMessageSendingDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.CH.NCTS.GUI.Res.GetData("3A0BE245-87B4-4B39-9DF4-8B2C218667C5", "Send all ticked MRN with:");
			this.Controls.Add(this.UnloadingGroupBox);
			this.Name = "MultipleMrnMessageSendingDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 70, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnloadingGroupBox.ResumeLayout(false);
			this.UnloadingGroupBox.PerformLayout();
			this.DateOfUnloadingDateEdit.ResumeLayout(true);
			this.DateOfUnloadingDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZGroupBox UnloadingGroupBox;
	internal Enterprise.ZArchitecture.GUI.ZCheckBox UnloadedCargoConformsToDeclarationCheckBox;
	internal Enterprise.ZArchitecture.GUI.ZDateEdit DateOfUnloadingDateEdit;
}
