using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class ConsolCostDefaultApportionmentMethodControl
	{


		#region Designer generated code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.ConsolCostDefaultApportionmentMethodGrid = new ZArchitecture.ZGrid();
			this.ConsolCostDefaultApportionmentMethodBox = new ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ConsolCostDefaultApportionmentMethodGrid)).BeginInit();
			this.ConsolCostDefaultApportionmentMethodGrid.SuspendLayout();
			this.ConsolCostDefaultApportionmentMethodBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ConsolCostDefaultApportionmentMethodConfiguration);
			// 
			// ConsolCostDefaultApportionmentMethodGrid
			// 
			this.ConsolCostDefaultApportionmentMethodGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConsolCostDefaultApportionmentMethodGrid, "ConsolCostDefaultApportionmentMethodCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ConsolCostDefaultApportionmentMethod)(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)).SyncRoot)).Apportionment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ConsolCostDefaultApportionmentMethod)(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)).SyncRoot)).Module)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ConsolCostDefaultApportionmentMethod)(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)).SyncRoot)).Direction)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ConsolCostDefaultApportionmentMethod)(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)).SyncRoot)).TransportMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ConsolCostDefaultApportionmentMethod)(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)).SyncRoot)).ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.ConsolCostDefaultApportionmentMethod)(((System.Collections.IList)(((Business.ConsolCostDefaultApportionmentMethodConfiguration)(null)).ConsolCostDefaultApportionmentMethodCollection)).SyncRoot)).ConsolType)));
			this.ConsolCostDefaultApportionmentMethodGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolCostDefaultApportionmentMethodControl|46a45808-9023-4f2a-ba6a-0c2e9c22d3bb", "Consol Type");
			zDropEditColumnStyleInfo1.ColumnName = "ConsolType";
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolCostDefaultApportionmentMethodControl|d52588f3-702a-404b-a2cc-66419fe1f90c", "Transport Mode");
			zDropEditColumnStyleInfo2.ColumnName = "TransportMode";
			zDropEditColumnStyleInfo3.Caption = null;
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolCostDefaultApportionmentMethodControl|C81003DD-8905-438E-BCB7-163F83DAE733", "Direction");
			zDropEditColumnStyleInfo3.ColumnName = "Direction";
			zDropEditColumnStyleInfo4.Caption = null;
			zDropEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolCostDefaultApportionmentMethodControl|9cde6c82-3801-4507-8875-4f40306c9c97", "Container Mode");
			zDropEditColumnStyleInfo4.ColumnName = "ContainerMode";
			zDropEditColumnStyleInfo5.Caption = null;
			zDropEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolCostDefaultApportionmentMethodControl|bebdecab-34d3-4fc9-b71e-db952c3e6572", "Apportionment");
			zDropEditColumnStyleInfo5.ColumnName = "Apportionment";
			zDropEditColumnStyleInfo6.Caption = null;
			zDropEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ConsolCostDefaultApportionmentMethodControl|61635d57-1707-4679-a0da-0bd3d82123ff", "Module");
			zDropEditColumnStyleInfo6.ColumnName = "Module";
			this.ConsolCostDefaultApportionmentMethodGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ConsolCostDefaultApportionmentMethodGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ConsolCostDefaultApportionmentMethodGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.ConsolCostDefaultApportionmentMethodGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.ConsolCostDefaultApportionmentMethodGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.ConsolCostDefaultApportionmentMethodGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.ConsolCostDefaultApportionmentMethodGrid.CopySelectedRowsAllowed = true;
			this.ConsolCostDefaultApportionmentMethodGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCostDefaultApportionmentMethodGrid.GridId = "61d246fa-baa8-4384-826a-d24f2dd75849";
			this.ConsolCostDefaultApportionmentMethodGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolCostDefaultApportionmentMethodGrid.LayoutKey = "ConsolCostDefaultApportionmentMethodGrid";
			this.ConsolCostDefaultApportionmentMethodGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ConsolCostDefaultApportionmentMethodGrid.Name = "ConsolCostDefaultApportionmentMethodGrid";
			this.ConsolCostDefaultApportionmentMethodGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(436, 276, true);
			this.ConsolCostDefaultApportionmentMethodGrid.TabIndex = 6;
			// 
			// DefaultConsolCostGroupBox
			// 
			this.ConsolCostDefaultApportionmentMethodBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2aaedbda-5ad6-4b91-88a4-d71d4e1202bb", "Default Consol Cost Apportionment Configuration");
			this.ConsolCostDefaultApportionmentMethodBox.Controls.Add(this.ConsolCostDefaultApportionmentMethodGrid);
			this.ConsolCostDefaultApportionmentMethodBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolCostDefaultApportionmentMethodBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConsolCostDefaultApportionmentMethodBox.Name = "DefaultConsolCostGroupBox";
			this.ConsolCostDefaultApportionmentMethodBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			this.ConsolCostDefaultApportionmentMethodBox.TabIndex = 7;
			this.ConsolCostDefaultApportionmentMethodBox.TabStop = false;
			// 
			// DefaultConsolCostApportionmentControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ConsolCostDefaultApportionmentMethodBox);
			this.Name = "ConsolCostDefaultApportionmentMethodControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 295, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ConsolCostDefaultApportionmentMethodGrid)).EndInit();
			this.ConsolCostDefaultApportionmentMethodGrid.ResumeLayout(false);
			this.ConsolCostDefaultApportionmentMethodBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

	}
}