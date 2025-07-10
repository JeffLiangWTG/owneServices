namespace Enterprise.Customs.EU.GUI
{
	partial class GuaranteeGroupBoxUserControl
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
			this.BondNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AmountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.OverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BondNumberCodeFindBox.SuspendLayout();
			this.AmountCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CommonGuarantee);
			// 
			// BondNumberCodeFindBox
			// 
			this.BondNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BondNumberCodeFindBox, "PW_BondNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CommonGuarantee)(null)).PW_BondNumber)));
			this.BondNumberCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("AAC13C38-1109-4D7B-A3BE-53565D3FAB66", "Guarantee Ref. No.");
			this.BondNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 25, true);
			this.BondNumberCodeFindBox.Name = "BondNumberCodeFindBox";
			this.BondNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.BondNumberCodeFindBox.ParentType = null;
			this.BondNumberCodeFindBox.ShowDescriptionBox = false;
			this.BondNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
			this.BondNumberCodeFindBox.TabIndex = 0;
			// 
			// AmountCalcDropEdit
			// 
			this.AmountCalcDropEdit.AllowDrop = true;
			this.AmountCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AmountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.CommonGuarantee)(null)).PW_BondAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CommonGuarantee)(null)).PW_RX_NKCurrency)));
			this.AmountCalcDropEdit.BindToAmount = "PW_BondAmount";
			this.AmountCalcDropEdit.BindToUnit = "PW_RX_NKCurrency";
			this.AmountCalcDropEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F80FFB6C-2FAD-4BBC-9D7E-BABD09880E01", "Liability Amount");
			this.AmountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 64, true);
			this.AmountCalcDropEdit.Name = "AmountCalcDropEdit";
			this.AmountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 15, true);
			this.AmountCalcDropEdit.TabIndex = 3;
			this.AmountCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// OverrideCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideCheckBox, "PW_Override");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.CommonGuarantee)(null)).PW_Override)));
			this.OverrideCheckBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("96791B12-BDAC-4D1F-956D-5785CA0FDA3B", "Override");
			this.OverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 105, true);
			this.OverrideCheckBox.Name = "OverrideCheckBox";
			this.OverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
			this.OverrideCheckBox.TabIndex = 0;
			// 
			// GuaranteeGroupBoxUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BondNumberCodeFindBox);
			this.Controls.Add(this.AmountCalcDropEdit);
			this.Controls.Add(this.OverrideCheckBox);
			this.Name = "GuaranteeGroupBoxUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 186, true);
			this.Tag = "";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BondNumberCodeFindBox.ResumeLayout(true);
			this.BondNumberCodeFindBox.PerformLayout();
			this.AmountCalcDropEdit.ResumeLayout(true);
			this.AmountCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ZArchitecture.GUI.ZCodeFindBox BondNumberCodeFindBox;
		internal ZArchitecture.GUI.ZCalcDropEdit AmountCalcDropEdit;
		internal ZArchitecture.GUI.ZCheckBox OverrideCheckBox;

		#endregion


	}
}
