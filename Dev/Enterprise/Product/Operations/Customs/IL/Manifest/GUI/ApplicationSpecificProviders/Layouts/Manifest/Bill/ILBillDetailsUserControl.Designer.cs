namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class ILBillDetailsUserControl
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

		private void InitializeComponent()
		{
            this.DischargePortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DischargePortCodeFindBox.SuspendLayout();
			this.ConditionDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
            // 
            // DischargePortCodeFindBox
            // 
            this.DischargePortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DischargePortCodeFindBox, "ABL_RL_NKPortOfDischarge");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).ABL_RL_NKPortOfDischarge)));
            this.DischargePortCodeFindBox.CaptionResourceString = Enterprise.Customs.IL.Manifest.GUI.Res.GetData("8B5ADDC1-AFF1-4F8D-844F-C5C71A4F403A", "Discharge Port");
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.DischargePortCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
            this.DischargePortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DischargePortCodeFindBox.Name = "DischargePortCodeFindBox";
            this.DischargePortCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DischargePortCodeFindBox.ParentType = null;
            this.DischargePortCodeFindBox.PreBoundMaxLength = 5;
            this.DischargePortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.DischargePortCodeFindBox.TabIndex = 1;
            // 
			// IncotermDropEdit
			// 
			this.ConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConditionDropEdit, "ABL_Condition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).ABL_Condition)));
			this.ConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 166, true);
			this.ConditionDropEdit.Name = "ConditionDropEdit";
			this.ConditionDropEdit.PreBoundMaxLength = 3;
			this.ConditionDropEdit.ShouldResizeByMaxLength = true;
			this.ConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.ConditionDropEdit.TabIndex = 1;
			// 
            // ILBillPartiesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.DischargePortCodeFindBox);
			this.Controls.Add(this.ConditionDropEdit);
            this.Name = "ILBillPartiesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 100, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConditionDropEdit.ResumeLayout(true);
			this.ConditionDropEdit.PerformLayout();
            this.DischargePortCodeFindBox.ResumeLayout(true);
            this.DischargePortCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox DischargePortCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ConditionDropEdit;
	}
}
