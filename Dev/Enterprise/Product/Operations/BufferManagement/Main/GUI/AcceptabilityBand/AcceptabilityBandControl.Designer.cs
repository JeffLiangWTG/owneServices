namespace Enterprise.BufferManagement.GUI
{
	partial class AcceptabilityBandControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BMComponentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FiltersControl = new Enterprise.BufferManagement.GUI.AcceptabilityBandFiltersControl();
			this.FencepostValuesControl = new Enterprise.BufferManagement.GUI.AcceptabilityBandFencepostValuesControl();
			this.ActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BMComponentFindBox.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.FiltersControl.SuspendLayout();
			this.FencepostValuesControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand);
			// 
			// NameTextBox
			// 
			this.NameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NameTextBox, "BAB_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_Name)));
			this.NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 14, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 20, true);
			this.NameTextBox.TabIndex = 0;
			// 
			// BMComponentFindBox
			// 
			this.BMComponentFindBox.AllowDrop = true;
			this.BMComponentFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.BMComponentFindBox, "BAB_FC_Component");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_FC_Component)));
			this.BMComponentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 40, true);
			this.BMComponentFindBox.Name = "BMComponentFindBox";
			this.BMComponentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(616, 20, true);
			this.BMComponentFindBox.TabIndex = 1;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.TypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "BAB_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_Type)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 66, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 20, true);
			this.TypeDropEdit.TabIndex = 5;
			// 
			// FiltersControl
			// 
			this.FiltersControl.AllowDrop = true;
			this.FiltersControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FiltersControl, ".");
			this.FiltersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 170, true);
			this.FiltersControl.Name = "FiltersControl";
			this.FiltersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 467, true);
			this.FiltersControl.TabIndex = 16;
			// 
			// FencepostValuesControl
			// 
			this.FencepostValuesControl.AllowDrop = true;
			this.FencepostValuesControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.FencepostValuesControl, ".");
			this.FencepostValuesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 92, true);
			this.FencepostValuesControl.Name = "FencepostValuesControl";
			this.FencepostValuesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 72, true);
			this.FencepostValuesControl.TabIndex = 15;
			// 
			// ActiveCheckBox
			// 
			this.ActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ActiveCheckBox, "BAB_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_IsActive)));
			this.ActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 68, true);
			this.ActiveCheckBox.Name = "ActiveCheckBox";
			this.ActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.ActiveCheckBox.TabIndex = 17;
			this.ActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// AcceptabilityBandControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ActiveCheckBox);
			this.Controls.Add(this.FencepostValuesControl);
			this.Controls.Add(this.FiltersControl);
			this.Controls.Add(this.TypeDropEdit);
			this.Controls.Add(this.BMComponentFindBox);
			this.Controls.Add(this.NameTextBox);
			this.Name = "AcceptabilityBandControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(719, 640, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BMComponentFindBox.ResumeLayout(true);
			this.BMComponentFindBox.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.FiltersControl.ResumeLayout(true);
			this.FiltersControl.PerformLayout();
			this.FencepostValuesControl.ResumeLayout(true);
			this.FencepostValuesControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox NameTextBox;
		private ZArchitecture.GUI.ZGuidFindBox BMComponentFindBox;
		private ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		private AcceptabilityBandFiltersControl FiltersControl;
		private AcceptabilityBandFencepostValuesControl FencepostValuesControl;
		private ZArchitecture.GUI.ZCheckBox ActiveCheckBox;
	}
}
