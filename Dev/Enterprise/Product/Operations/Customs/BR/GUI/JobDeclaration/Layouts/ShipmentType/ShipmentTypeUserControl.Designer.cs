namespace Enterprise.Customs.BR.GUI
{
	partial class ShipmentTypeUserControl
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
			this.SpecialTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsMultimodalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OperationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DispatchModalityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarantTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BRTransportModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SpecialTransportDropEdit.SuspendLayout();
			this.OperationTypeDropEdit.SuspendLayout();
			this.DispatchModalityDropEdit.SuspendLayout();
			this.DeclarantTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// DeclarantTypeDropEdit
			// 
			this.DeclarantTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarantTypeDropEdit, "DeclarantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).DeclarantType)));
			this.DeclarantTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 34, true);
			this.DeclarantTypeDropEdit.Name = "DeclarantTypeDropEdit";
			this.DeclarantTypeDropEdit.PreBoundMaxLength = 1;
			this.DeclarantTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.DeclarantTypeDropEdit.TabIndex = 20;
			// 
			// OperationTypeDropEdit
			// 
			this.OperationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OperationTypeDropEdit, "OperationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).OperationType)));
			this.OperationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 60, true);
			this.OperationTypeDropEdit.Name = "OperationTypeDropEdit";
			this.OperationTypeDropEdit.PreBoundMaxLength = 1;
			this.OperationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.OperationTypeDropEdit.TabIndex = 21;
			// 
			// SpecialTransportDropEdit
			// 
			this.SpecialTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecialTransportDropEdit, "JE_SpecialTransport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_SpecialTransport)));
			this.SpecialTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 86, true);
			this.SpecialTransportDropEdit.Name = "SpecialTransportDropEdit";
			this.SpecialTransportDropEdit.PreBoundMaxLength = 3;
			this.SpecialTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.SpecialTransportDropEdit.TabIndex = 22;
			// 
			// IsMultimodalCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsMultimodalCheckBox, "JE_IsMultimodal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_IsMultimodal)));
			this.IsMultimodalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 112, true);
			this.IsMultimodalCheckBox.Name = "IsMultimodalCheckBox";
			this.IsMultimodalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 17, true);
			this.IsMultimodalCheckBox.TabIndex = 23;
			this.IsMultimodalCheckBox.UseVisualStyleBackColor = true;
			// 
			// DispatchModalityDropEdit
			// 
			this.DispatchModalityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DispatchModalityDropEdit, "JE_DispatchModality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).JE_DispatchModality)));
			this.DispatchModalityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 92, true);
			this.DispatchModalityDropEdit.Name = "DispatchModalityDropEdit";
			this.DispatchModalityDropEdit.PreBoundMaxLength = 1;
			this.DispatchModalityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.DispatchModalityDropEdit.TabIndex = 22;
			// 
			// BRTransportModeDropEdit
			// 
			this.BRTransportModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BRTransportModeDropEdit, "BRTransportMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).BRTransportMode)));
			this.BRTransportModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 138, true);
			this.BRTransportModeDropEdit.Name = "BRTransportModeDropEdit";
			this.BRTransportModeDropEdit.PreBoundMaxLength = 3;
			this.BRTransportModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 16, true);
			this.BRTransportModeDropEdit.TabIndex = 22;
			// 
			// ShipmentTypeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BRTransportModeDropEdit);
			this.Controls.Add(this.DeclarantTypeDropEdit);
			this.Controls.Add(this.DispatchModalityDropEdit);
			this.Controls.Add(this.OperationTypeDropEdit);
			this.Controls.Add(this.IsMultimodalCheckBox);
			this.Controls.Add(this.SpecialTransportDropEdit);
			this.Name = "ShipmentTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarantTypeDropEdit.ResumeLayout(true);
			this.DeclarantTypeDropEdit.PerformLayout();
			this.OperationTypeDropEdit.ResumeLayout(true);
			this.OperationTypeDropEdit.PerformLayout();
			this.SpecialTransportDropEdit.ResumeLayout(true);
			this.SpecialTransportDropEdit.PerformLayout();
			this.DispatchModalityDropEdit.ResumeLayout(true);
			this.DispatchModalityDropEdit.PerformLayout();
			this.BRTransportModeDropEdit.ResumeLayout(true);
			this.BRTransportModeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDropEdit DeclarantTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit OperationTypeDropEdit;
		public ZArchitecture.GUI.ZDropEdit SpecialTransportDropEdit;
		public ZArchitecture.GUI.ZCheckBox IsMultimodalCheckBox;
		public ZArchitecture.GUI.ZDropEdit DispatchModalityDropEdit;
		public ZArchitecture.GUI.ZDropEdit BRTransportModeDropEdit;
	}
}
