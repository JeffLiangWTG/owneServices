using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class PeriodClosureConfigurationControl
	{


		#region Designer generated code

		void InitializeComponent()
		{
			this.GroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.IntervalTypeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.SubLedgerCalcEdit = new ZArchitecture.ZCalcEdit();
			this.GeneralLedgerCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AdjustmentLedgerCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBox.SuspendLayout();
			this.IntervalTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.PeriodClosureConfiguration);
			// 
			// GroupBox
			// 
			this.GroupBox.Controls.Add(this.IntervalTypeDropEdit);
			this.GroupBox.Controls.Add(this.SubLedgerCalcEdit);
			this.GroupBox.Controls.Add(this.GeneralLedgerCalcEdit);
			this.GroupBox.Controls.Add(this.AdjustmentLedgerCalcEdit);
			this.GroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.GroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodClosureConfigurationControl|77930761-9138-48c1-87f4-c274b73bf242", "Auto Period Closure Configuration");
			this.GroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBox.Name = "GroupBox";
			this.GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 147, true);
			this.GroupBox.TabIndex = 1;
			this.GroupBox.TabStop = false;
			// 
			// IntervalTypeDropEdit
			// 
			this.IntervalTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntervalTypeDropEdit, "IntervalType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Business.PeriodClosureConfiguration)(null)).IntervalType)));
			this.IntervalTypeDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodClosureConfigurationControl|514fbafe-fd6f-4cd8-8570-fb7dc9b1219c", "Interval Type");
			this.IntervalTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 17, true);
			this.IntervalTypeDropEdit.Name = "IntervalTypeDropEdit";
			this.IntervalTypeDropEdit.ShouldResizeByMaxLength = true;
			this.IntervalTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(142, 18, true);
			this.IntervalTypeDropEdit.TabIndex = 1;
			// 
			// SubLedgerCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.SubLedgerCalcEdit, "SubLedgerInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.PeriodClosureConfiguration)(null)).SubLedgerInterval)));
			this.SubLedgerCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodClosureConfigurationControl|ed0bf818-e527-472d-a948-7dfc973f2e01", "Sub Ledger");
			this.SubLedgerCalcEdit.DecimalPlaces = 0;
			this.SubLedgerCalcEdit.Decimals = 0;
			this.SubLedgerCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 50, true);
			this.SubLedgerCalcEdit.Name = "SubLedgerCalcEdit";
			this.SubLedgerCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.SubLedgerCalcEdit.TabIndex = 2;
			this.SubLedgerCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// GeneralLedgerCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GeneralLedgerCalcEdit, "GeneralLedgerInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.PeriodClosureConfiguration)(null)).GeneralLedgerInterval)));
			this.GeneralLedgerCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodClosureConfigurationControl|ffa0d479-d761-4d48-9575-ac8a9026e59b", "General Ledger");
			this.GeneralLedgerCalcEdit.DecimalPlaces = 0;
			this.GeneralLedgerCalcEdit.Decimals = 0;
			this.GeneralLedgerCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 83, true);
			this.GeneralLedgerCalcEdit.Name = "GeneralLedgerCalcEdit";
			this.GeneralLedgerCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.GeneralLedgerCalcEdit.TabIndex = 3;
			this.GeneralLedgerCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AdjustmentLedgerCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AdjustmentLedgerCalcEdit, "AdjustmentLedgerInterval");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Business.PeriodClosureConfiguration)(null)).AdjustmentLedgerInterval)));
			this.AdjustmentLedgerCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodClosureConfigurationControl|8ba67afa-817d-4966-9c7a-d9ffe8315ed2", "Adjustment Ledger");
			this.AdjustmentLedgerCalcEdit.DecimalPlaces = 0;
			this.AdjustmentLedgerCalcEdit.Decimals = 0;
			this.AdjustmentLedgerCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 117, true);
			this.AdjustmentLedgerCalcEdit.Name = "AdjustmentLedgerCalcEdit";
			this.AdjustmentLedgerCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.AdjustmentLedgerCalcEdit.TabIndex = 4;
			this.AdjustmentLedgerCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PeriodClosureConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GroupBox);
			this.Name = "PeriodClosureConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBox.ResumeLayout(false);
			this.GroupBox.PerformLayout();
			this.IntervalTypeDropEdit.ResumeLayout(true);
			this.IntervalTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZArchitecture.GUI.ZGroupBox GroupBox;
		ZArchitecture.GUI.ZDropEdit IntervalTypeDropEdit;
		ZArchitecture.ZCalcEdit SubLedgerCalcEdit;
		ZArchitecture.ZCalcEdit GeneralLedgerCalcEdit;
		ZArchitecture.ZCalcEdit AdjustmentLedgerCalcEdit;

		#endregion

	}
}