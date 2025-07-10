namespace Enterprise.Accounting.Registry.GUI
{
	public partial class TransactionTypePrefixControl
	{

		#region Component Designer Generated Code

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TransactionTypePrefixGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionTypePrefixGrid)).BeginInit();
			this.TransactionTypePrefixGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.TransactionTypePrefixCollection);
			// 
			// TransactionTypePrefixGrid
			// 
			this.TransactionTypePrefixGrid.AllowCopyToNewRowMenuItem = false;
			this.TransactionTypePrefixGrid.AllowNavigation = false;
			this.TransactionTypePrefixGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TransactionTypePrefixGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.TransactionTypePrefix)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.TransactionTypePrefix)(null)).Ledger)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.TransactionTypePrefix)(null)).TransactionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.TransactionTypePrefix)(null)).Prefix)));
			this.TransactionTypePrefixGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b94d19ca-897b-4589-90e9-42270bd2f843", "Ledger");
			zDropEditColumnStyleInfo1.ColumnName = "Ledger";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("878b7927-004b-45da-afd2-d2763ddd803d", "Transaction Type");
			zDropEditColumnStyleInfo2.ColumnName = "TransactionType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ff082902-ae5c-4e14-b91f-d9348a0cc59f", "Prefix");
			zTextBoxColumnStyleInfo1.ColumnName = "Prefix";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.TransactionTypePrefixGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TransactionTypePrefixGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.TransactionTypePrefixGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TransactionTypePrefixGrid.CopySelectedRowsAllowed = false;
			this.TransactionTypePrefixGrid.GridId = "f80a96ca-6bb2-475f-b740-30ca1a8624a9";
			this.TransactionTypePrefixGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TransactionTypePrefixGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TransactionTypePrefixGrid.LayoutKey = "TransactionTypePrefixGrid";
			this.TransactionTypePrefixGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransactionTypePrefixGrid.Name = "TransactionTypePrefixGrid";
			this.TransactionTypePrefixGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 236, true);
			this.TransactionTypePrefixGrid.TabIndex = 0;
			// 
			// TransactionTypePrefixControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransactionTypePrefixGrid);
			this.Name = "TransactionTypePrefixControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TransactionTypePrefixGrid)).EndInit();
			this.TransactionTypePrefixGrid.ResumeLayout(false);
			this.TransactionTypePrefixGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZGrid TransactionTypePrefixGrid;
	}
}
