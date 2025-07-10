namespace Enterprise.Accounting.Registry.GUI
{
	public partial class PaymentReceiptTypeReferenceNumberControl
	{
		#region Component Designer Generated Code

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PaymentReceiptTypeReferenceNumberGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PaymentReceiptTypeReferenceNumberGrid)).BeginInit();
			this.PaymentReceiptTypeReferenceNumberGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.PaymentReceiptTypeReferenceNumber);
			// 
			// PaymentReceiptTypeReferenceNumberGrid
			// 
			this.PaymentReceiptTypeReferenceNumberGrid.AllowCopyToNewRowMenuItem = false;
			this.PaymentReceiptTypeReferenceNumberGrid.AllowNavigation = false;
			this.PaymentReceiptTypeReferenceNumberGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PaymentReceiptTypeReferenceNumberGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.PaymentReceiptTypeReferenceNumber)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PaymentReceiptTypeReferenceNumber)(null)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.PaymentReceiptTypeReferenceNumber)(null)).ReferenceNumber)));
			this.PaymentReceiptTypeReferenceNumberGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ca9244d7-ed92-4554-98dd-43010cdb0a31", "Payment/Receipt Type");
			zTextBoxColumnStyleInfo1.ColumnName = "Type";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("0b0826c7-a14f-4b6e-972d-12b13b7f92f8", "Reference Number");
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.PaymentReceiptTypeReferenceNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PaymentReceiptTypeReferenceNumberGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PaymentReceiptTypeReferenceNumberGrid.CopySelectedRowsAllowed = false;
			this.PaymentReceiptTypeReferenceNumberGrid.GridId = "e314bef2-048b-4bd0-8301-e41bf974abcd";
			this.PaymentReceiptTypeReferenceNumberGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PaymentReceiptTypeReferenceNumberGrid.LayoutKey = "PaymentReceiptTypeReferenceNumberGrid";
			this.PaymentReceiptTypeReferenceNumberGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PaymentReceiptTypeReferenceNumberGrid.Name = "PaymentReceiptTypeReferenceNumberGrid";
			this.PaymentReceiptTypeReferenceNumberGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.PaymentReceiptTypeReferenceNumberGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 236, true);
			this.PaymentReceiptTypeReferenceNumberGrid.TabIndex = 0;
			this.PaymentReceiptTypeReferenceNumberGrid.RowsDeleting += new System.EventHandler<ZArchitecture.RowsDeletingEventArgs>(this.PaymentReceiptTypeReferenceNumberGrid_RowDeleting);
			// 
			// PaymentReceiptTypeReferenceNumberControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PaymentReceiptTypeReferenceNumberGrid);
			this.Name = "PaymentReceiptTypeReferenceNumberControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PaymentReceiptTypeReferenceNumberGrid)).EndInit();
			this.PaymentReceiptTypeReferenceNumberGrid.ResumeLayout(false);
			this.PaymentReceiptTypeReferenceNumberGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected internal ZArchitecture.ZGrid PaymentReceiptTypeReferenceNumberGrid;
	}
}
