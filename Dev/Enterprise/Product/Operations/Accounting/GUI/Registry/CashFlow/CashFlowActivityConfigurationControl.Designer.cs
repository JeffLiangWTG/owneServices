using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class CashFlowActivityConfigurationControl
	{


		#region Component Designer generated code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CashFlowActivityConfigurationGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CashFlowActivityConfigurationGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CashFlowActivityConfigurationCollection);
			// 
			// CashFlowActivityConfigurationGrid
			// 
			this.CashFlowActivityConfigurationGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CashFlowActivityConfigurationGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CashFlowActivityConfiguration)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowActivityConfiguration)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowActivityConfiguration)(null)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowActivityConfiguration)(null)).ActivityType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CashFlowActivityConfiguration)(null)).ActivityDescription)));
			this.CashFlowActivityConfigurationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowActivityConfigurationControl|9adc2852-2d5e-43ab-9dc6-b1dcb187da9d", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowActivityConfigurationControl|79dcce81-4f5b-4980-88ca-447f796c8327", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowActivityConfigurationControl|76f2c5ef-e51c-4198-8c89-e9c59abb4c1e", "Activity Type");
			zDropEditColumnStyleInfo1.ColumnName = "ActivityType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CashFlowActivityConfigurationControl|207620de-33d8-42f8-ba72-b3c78bbec9b2", "Activity Description");
			zTextBoxColumnStyleInfo3.ColumnName = "ActivityDescription";
			this.CashFlowActivityConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CashFlowActivityConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CashFlowActivityConfigurationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CashFlowActivityConfigurationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CashFlowActivityConfigurationGrid.CopySelectedRowsAllowed = true;
			this.CashFlowActivityConfigurationGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CashFlowActivityConfigurationGrid.GridId = "1bfabd40-4eb7-4d26-8262-6b93422e619f";
			this.CashFlowActivityConfigurationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CashFlowActivityConfigurationGrid.LayoutKey = "CashFlowActivityConfigurationGrid";
			this.CashFlowActivityConfigurationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CashFlowActivityConfigurationGrid.Name = "CashFlowActivityConfigurationGrid";
			this.CashFlowActivityConfigurationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			this.CashFlowActivityConfigurationGrid.TabIndex = 0;
			// 
			// CashFlowActivityConfigurationControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.CashFlowActivityConfigurationGrid);
			this.Name = "CashFlowActivityConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(748, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CashFlowActivityConfigurationGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}