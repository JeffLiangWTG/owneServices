namespace Enterprise.Customs.EU.GUI
{
	public partial class InvoiceLineCopyDocumentsUserControl
	{
		private void InitializeComponent()
		{
			this.InvoiceNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InvoiceNumberDropEditGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceNumberDropEdit.SuspendLayout();
			this.InvoiceNumberDropEditGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CopyDocumentsSelectionHeader);
			// 
			// InvoiceNumberDropEdit
			// 
			this.InvoiceNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InvoiceNumberDropEdit, "InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CopyDocumentsSelectionHeader)(null)).InvoiceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.CopyDocumentsSelectionHeader)(null)).Lookups.Invoices)));
			this.InvoiceNumberDropEdit.BindToList = "Lookups.Invoices";
			this.InvoiceNumberDropEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceNumberDropEdit.Name = "InvoiceNumberDropEdit";
			this.InvoiceNumberDropEdit.PreBoundMaxLength = 35;
			this.InvoiceNumberDropEdit.ShouldResizeByMaxLength = false;
			this.InvoiceNumberDropEdit.ShowDescriptionBox = false;
			this.InvoiceNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 21, true);
			this.InvoiceNumberDropEdit.TabIndex = 1;
			// 
			// InvoiceNumberDropEditGroupBox
			// 
			this.InvoiceNumberDropEditGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3fb736ef-a8da-48c2-94e1-27adcebc0ce2", "Copy to lines of Invoice:");
			this.InvoiceNumberDropEditGroupBox.Controls.Add(this.InvoiceNumberDropEdit);
			this.InvoiceNumberDropEditGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 30, true);
			this.InvoiceNumberDropEditGroupBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 40, true);
			this.InvoiceNumberDropEditGroupBox.Name = "InvoiceNumberDropEditGroupBox";
			this.InvoiceNumberDropEditGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 40, true);
			this.InvoiceNumberDropEditGroupBox.TabIndex = 2;
			this.InvoiceNumberDropEditGroupBox.TabStop = false;
			// 
			// InvoiceLineCopyDocumentsUserControl
			// 
			this.Controls.Add(this.InvoiceNumberDropEditGroupBox);
			this.Name = "InvoiceLineCopyDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 314, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceNumberDropEdit.ResumeLayout(true);
			this.InvoiceNumberDropEdit.PerformLayout();
			this.InvoiceNumberDropEditGroupBox.ResumeLayout(false);
			this.InvoiceNumberDropEditGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit InvoiceNumberDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox InvoiceNumberDropEditGroupBox;

	}
}
