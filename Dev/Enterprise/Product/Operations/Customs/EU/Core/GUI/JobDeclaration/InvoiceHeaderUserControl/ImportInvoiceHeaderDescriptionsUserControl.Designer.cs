namespace Enterprise.Customs.EU.GUI
{
	partial class ImportInvoiceHeaderDescriptionsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.HeaderDescriptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderDescriptionsGrid)).BeginInit();
			this.HeaderDescriptionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// HeaderDescriptionsGrid
			// 
			this.HeaderDescriptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HeaderDescriptionsGrid, "Invoices.HeaderDescriptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).HeaderDescriptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.InvoiceHeaderDescription)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).HeaderDescriptions)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.InvoiceHeaderDescription)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).Invoices)).SyncRoot)).HeaderDescriptions)).SyncRoot)).CY_Data)));
			this.HeaderDescriptionsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("58ccce19-d2db-4d26-844a-b63eebdea3e1", "Code");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4BB02245-5AC5-44A7-8A03-53B2D6191904", "Description");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.HeaderDescriptionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.HeaderDescriptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HeaderDescriptionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HeaderDescriptionsGrid.GridId = "03712002-512b-451b-b3cb-a1122d6f4dbd";
			this.HeaderDescriptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HeaderDescriptionsGrid.LayoutKey = "HeaderDescriptionsGrid";
			this.HeaderDescriptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HeaderDescriptionsGrid.Name = "HeaderDescriptionsGrid";
			this.HeaderDescriptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 216, true);
			this.HeaderDescriptionsGrid.TabIndex = 0;
			// 
			// ImportInvoiceHeaderDescriptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HeaderDescriptionsGrid);
			this.Name = "ImportInvoiceHeaderDescriptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 216, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HeaderDescriptionsGrid)).EndInit();
			this.HeaderDescriptionsGrid.ResumeLayout(false);
			this.HeaderDescriptionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.ZGrid HeaderDescriptionsGrid;
	}
}
