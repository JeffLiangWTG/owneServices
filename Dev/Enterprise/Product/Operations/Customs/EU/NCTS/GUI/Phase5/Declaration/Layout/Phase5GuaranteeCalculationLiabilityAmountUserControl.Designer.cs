namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5GuaranteeCalculationLiabilityAmountUserControl
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
			this.LiabilityPercentageIntEdit = new Enterprise.ZArchitecture.GUI.ZIntEdit();
			this.LiabilityAmountCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.TotalValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LiabilityAmountTotalValueCalculationMethodUserControl = new Enterprise.Customs.EU.NCTS.GUI.LiabilityAmountTotalValueCalculationMethodUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LiabilityAmountCalcDropEdit.SuspendLayout();
			this.TotalValueCalcDropEdit.SuspendLayout();
			this.LiabilityAmountTotalValueCalculationMethodUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj);
			// 
			// LiabilityPercentageIntEdit
			// 
			this.BindingSource.SetBindingMember(this.LiabilityPercentageIntEdit, "LiabilityPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).LiabilityPercentage)));
			this.LiabilityPercentageIntEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 76, true);
			this.LiabilityPercentageIntEdit.Name = "LiabilityPercentageIntEdit";
			this.LiabilityPercentageIntEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 20, true);
			this.LiabilityPercentageIntEdit.TabIndex = 1;
			// 
			// LiabilityAmountCalcDropEdit
			// 
			this.LiabilityAmountCalcDropEdit.AllowDrop = true;
			this.LiabilityAmountCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LiabilityAmountCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).LiabilityAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).Currency)));
			this.LiabilityAmountCalcDropEdit.BindToAmount = "LiabilityAmount";
			this.LiabilityAmountCalcDropEdit.BindToUnit = "Currency";
			this.LiabilityAmountCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 102, true);
			this.LiabilityAmountCalcDropEdit.Name = "LiabilityAmountCalcDropEdit";
			this.LiabilityAmountCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.LiabilityAmountCalcDropEdit.TabIndex = 3;
			this.LiabilityAmountCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// TotalValueCalcDropEdit
			// 
			this.TotalValueCalcDropEdit.AllowDrop = true;
			this.TotalValueCalcDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TotalValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).TotalValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).Currency)));
			this.TotalValueCalcDropEdit.BindToAmount = "TotalValue";
			this.TotalValueCalcDropEdit.BindToUnit = "Currency";
			this.TotalValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 50, true);
			this.TotalValueCalcDropEdit.Name = "TotalValueCalcDropEdit";
			this.TotalValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(353, 20, true);
			this.TotalValueCalcDropEdit.TabIndex = 0;
			this.TotalValueCalcDropEdit.UnitPreBoundMaxLength = 3;
			// 
			// LiabilityAmountTotalValueCalculationMethodUserControl
			// 
			this.LiabilityAmountTotalValueCalculationMethodUserControl.AllowDrop = true;
			this.LiabilityAmountTotalValueCalculationMethodUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LiabilityAmountTotalValueCalculationMethodUserControl, ".");
			this.LiabilityAmountTotalValueCalculationMethodUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 3, true);
			this.LiabilityAmountTotalValueCalculationMethodUserControl.Name = "LiabilityAmountTotalValueCalculationMethodUserControl";
			this.LiabilityAmountTotalValueCalculationMethodUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 22, true);
			this.LiabilityAmountTotalValueCalculationMethodUserControl.TabIndex = 4;
			// 
			// Phase5GuaranteeCalculationLiabilityAmountUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LiabilityAmountTotalValueCalculationMethodUserControl);
			this.Controls.Add(this.LiabilityAmountCalcDropEdit);
			this.Controls.Add(this.LiabilityPercentageIntEdit);
			this.Controls.Add(this.TotalValueCalcDropEdit);
			this.Name = "Phase5GuaranteeCalculationLiabilityAmountUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 337, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LiabilityAmountCalcDropEdit.ResumeLayout(true);
			this.LiabilityAmountCalcDropEdit.PerformLayout();
			this.TotalValueCalcDropEdit.ResumeLayout(true);
			this.TotalValueCalcDropEdit.PerformLayout();
			this.LiabilityAmountTotalValueCalculationMethodUserControl.ResumeLayout(true);
			this.LiabilityAmountTotalValueCalculationMethodUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.GUI.ZIntEdit LiabilityPercentageIntEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit LiabilityAmountCalcDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit TotalValueCalcDropEdit;
		internal LiabilityAmountTotalValueCalculationMethodUserControl LiabilityAmountTotalValueCalculationMethodUserControl;
	}
}
