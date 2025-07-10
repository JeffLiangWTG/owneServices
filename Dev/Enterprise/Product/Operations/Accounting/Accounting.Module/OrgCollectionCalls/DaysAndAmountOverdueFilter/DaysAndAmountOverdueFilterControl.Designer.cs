namespace Enterprise.Accounting.Module
{
	public partial class DaysAndAmountOverdueModuleFilterControl
	{
		private Enterprise.ZArchitecture.ZCalcEdit DaysOverdueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit AmountOverdueZCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZDropEdit AndOrDeciderZDropEdit;

		void InitializeComponent()
		{
			this.DaysOverdueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AmountOverdueZCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.AndOrDeciderZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DaysAndAmountOverdueModuleFilter);
			// 
			// DaysOverdueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.DaysOverdueCalcEdit, "DaysOverdue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DaysAndAmountOverdueModuleFilter)(null)).DaysOverdue)));
			this.DaysOverdueCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DaysAndAmountOverdueModuleFilterControl|6bdff2bd-2fe2-4419-8bda-3b332482816a", "Max Days Overdue");
			this.DaysOverdueCalcEdit.Decimals = 0;
			this.DaysOverdueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(307, 3, true);
			this.DaysOverdueCalcEdit.Name = "DaysOverdueCalcEdit";
			this.DaysOverdueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.DaysOverdueCalcEdit.TabIndex = 1;
			this.DaysOverdueCalcEdit.Text = "0";
			this.DaysOverdueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AmountOverdueZCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountOverdueZCalcEdit, "AmountOverdue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((DaysAndAmountOverdueModuleFilter)(null)).AmountOverdue)));
			this.AmountOverdueZCalcEdit.CaptionResourceString = Enterprise.Accounting.Module.Res.GetData("DaysAndAmountOverdueModuleFilterControl|1d3fd5d5-f715-4929-bd06-dbeb06f2c56f", "Total Overdue", "Total Amount Overdue");
			this.AmountOverdueZCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(530, 3, true);
			this.AmountOverdueZCalcEdit.Name = "AmountOverdueZCalcEdit";
			this.AmountOverdueZCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.AmountOverdueZCalcEdit.TabIndex = 4;
			this.AmountOverdueZCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AndOrDeciderZDropEdit
			// 
			this.BindingSource.SetBindingMember(this.AndOrDeciderZDropEdit, "AndOrDecider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DaysAndAmountOverdueModuleFilter)(null)).AndOrDecider)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DaysAndAmountOverdueModuleFilter)(null)).AndOrDeciderList)));
			this.AndOrDeciderZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 3, true);
			this.AndOrDeciderZDropEdit.Name = "AndOrDeciderZDropEdit";
			this.AndOrDeciderZDropEdit.PreBoundMaxLength = 2;
			this.AndOrDeciderZDropEdit.ShowDescriptionBox = false;
			this.AndOrDeciderZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.AndOrDeciderZDropEdit.TabIndex = 2;
			// 
			// DaysAndAmountOverdueModuleFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AndOrDeciderZDropEdit);
			this.Controls.Add(this.DaysOverdueCalcEdit);
			this.Controls.Add(this.AmountOverdueZCalcEdit);
			this.Name = "DaysAndAmountOverdueModuleFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(609, 26, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
