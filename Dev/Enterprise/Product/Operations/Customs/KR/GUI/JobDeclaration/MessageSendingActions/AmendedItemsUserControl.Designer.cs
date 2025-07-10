namespace Enterprise.Customs.KR.GUI
{
	partial class AmendedItemsUserControl
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
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.AmendedItemsGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AmendedItemsGrid)).BeginInit();
            this.AmendedItemsGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject);
            // 
            // AmendedItemsGrid
            // 
            this.AmendedItemsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.AmendedItemsGrid, "AmendedItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)).SyncRoot)).AmendTypeDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)).SyncRoot)).ID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)).SyncRoot)).DataItemID)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)).SyncRoot)).DataItemDescription)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)).SyncRoot)).BeforeValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.AmendedItem)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclarationAmendmentMessageSendingObject)(null)).AmendedItems)).SyncRoot)).AfterValue)));
            this.AmendedItemsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "AmendTypeDescription";
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zMultiLineTextBoxColumnInfo1.ColumnName = "ID";
            zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
            zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zTextBoxColumnStyleInfo2.ColumnName = "DataItemID";
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo3.ColumnName = "DataItemDescription";
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
            zTextBoxColumnStyleInfo4.ColumnName = "BeforeValue";
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
            zTextBoxColumnStyleInfo5.ColumnName = "AfterValue";
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(167);
            this.AmendedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AmendedItemsGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
            this.AmendedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.AmendedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.AmendedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.AmendedItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.AmendedItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AmendedItemsGrid.GridId = "8e17f5ed-7fb5-45a9-b308-8c8428bdf023";
            this.AmendedItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AmendedItemsGrid.LayoutKey = "AmendedItemsGrid";
            this.AmendedItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.AmendedItemsGrid.Name = "AmendedItemsGrid";
            this.AmendedItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
            this.AmendedItemsGrid.TabIndex = 0;
            // 
            // AmendedItemsUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AmendedItemsGrid);
            this.Name = "AmendedItemsUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 120, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AmendedItemsGrid)).EndInit();
            this.AmendedItemsGrid.ResumeLayout(false);
            this.AmendedItemsGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
			this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid AmendedItemsGrid;
	}
}
