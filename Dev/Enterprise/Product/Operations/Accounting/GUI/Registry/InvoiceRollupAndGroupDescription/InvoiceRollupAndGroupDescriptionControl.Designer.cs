namespace Enterprise.Accounting.Registry.GUI
{
	public partial class InvoiceRollupAndGroupDescriptionControl
	{

		#region Component Designer Generated Code

		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.InvoiceRollupAndGroupDescriptionGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceRollupAndGroupDescriptionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.InvoiceRollupAndGroupDescription);
			// 
			// InvoiceRollupAndGroupDescriptionGrid
			// 
			this.InvoiceRollupAndGroupDescriptionGrid.AllowCopyToNewRowMenuItem = false;
			this.InvoiceRollupAndGroupDescriptionGrid.AllowNavigation = false;
			this.InvoiceRollupAndGroupDescriptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvoiceRollupAndGroupDescriptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.InvoiceRollupAndGroupDescription)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceRollupAndGroupDescription)(null)).Style)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceRollupAndGroupDescription)(null)).Group)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.InvoiceRollupAndGroupDescription)(null)).EnglishDescription)));
			this.InvoiceRollupAndGroupDescriptionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("cdcc8f85-0050-4d47-a6fe-2ee2de7c0665", "Style");
			zTextBoxColumnStyleInfo1.ColumnName = "Style";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zTextBoxColumnStyleInfo2.Caption = "";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("5e764306-3a70-4e97-83f3-23c52f768dbb", "Group");
			zTextBoxColumnStyleInfo2.ColumnName = "Group";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			zTextBoxColumnStyleInfo3.Caption = "";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3b266a28-5773-4aeb-8e83-6633bc08e83e", "Roll-up/Group Description");
			zTextBoxColumnStyleInfo3.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.InvoiceRollupAndGroupDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.InvoiceRollupAndGroupDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.InvoiceRollupAndGroupDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.InvoiceRollupAndGroupDescriptionGrid.GridId = "76a2ca71-0ad5-4839-b200-e37a55b4ab5b";
			this.InvoiceRollupAndGroupDescriptionGrid.CopySelectedRowsAllowed = false;
			this.InvoiceRollupAndGroupDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InvoiceRollupAndGroupDescriptionGrid.LayoutKey = "InvoiceRollupAndGroupDescriptionGrid";
			this.InvoiceRollupAndGroupDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceRollupAndGroupDescriptionGrid.Name = "InvoiceRollupAndGroupDescriptionGrid";
			this.InvoiceRollupAndGroupDescriptionGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InvoiceRollupAndGroupDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 236, true);
			this.InvoiceRollupAndGroupDescriptionGrid.TabIndex = 0;
			this.InvoiceRollupAndGroupDescriptionGrid.RowsDeleting += new System.EventHandler<ZArchitecture.RowsDeletingEventArgs>(this.InvoiceRollupAndGroupDescriptionGrid_RowDeleting);
			// 
			// InvoiceRollupAndGroupDescriptionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.InvoiceRollupAndGroupDescriptionGrid);
			this.Name = "InvoiceRollupAndGroupDescriptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InvoiceRollupAndGroupDescriptionGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		protected internal ZArchitecture.ZGrid InvoiceRollupAndGroupDescriptionGrid;
	}
}
