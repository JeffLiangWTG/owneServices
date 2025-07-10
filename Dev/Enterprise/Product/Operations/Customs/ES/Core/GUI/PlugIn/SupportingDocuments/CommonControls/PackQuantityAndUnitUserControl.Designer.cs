namespace Enterprise.Customs.ES.GUI
{
	public partial class PackQuantityAndUnitUserControl
	{
		private void InitializeComponent()
		{
			this.SupDocPackQuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfPackQuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnitOfPackQuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// SupDocPackQuantityCalcEdit
			// 
			this.SupDocPackQuantityCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.SupDocPackQuantityCalcEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_PackQty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_PackQty)));
			this.SupDocPackQuantityCalcEdit.DecimalPlaces = 5;
			this.SupDocPackQuantityCalcEdit.Decimals = 5;
			this.SupDocPackQuantityCalcEdit.Extra1LabelText = "EUAddInfoSupportingDocumentSchema.xml";
			this.SupDocPackQuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupDocPackQuantityCalcEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.SupDocPackQuantityCalcEdit.Name = "SupDocPackQuantityCalcEdit";
			this.SupDocPackQuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.SupDocPackQuantityCalcEdit.TabIndex = 0;
			this.SupDocPackQuantityCalcEdit.Text = "0.00000";
			this.SupDocPackQuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitOfPackQuantityDropEdit
			// 
			this.UnitOfPackQuantityDropEdit.AllowDrop = true;
			this.UnitOfPackQuantityDropEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UnitOfPackQuantityDropEdit, "FilteredInvoiceLines.SupportingDocuments.CSI_PackType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Business.Declaration.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).SupportingDocuments)).SyncRoot)).CSI_PackType)));
			this.UnitOfPackQuantityDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.UnitOfPackQuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 0, true);
			this.UnitOfPackQuantityDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.UnitOfPackQuantityDropEdit.Name = "UnitOfPackQuantityDropEdit";
			this.UnitOfPackQuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.UnitOfPackQuantityDropEdit.TabIndex = 1;
			// 
			// PackQuantityAndUnitUserControl
			// 
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnitOfPackQuantityDropEdit);
			this.Controls.Add(this.SupDocPackQuantityCalcEdit);
			this.Name = "PackQuantityAndUnitUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UnitOfPackQuantityDropEdit.ResumeLayout(true);
			this.UnitOfPackQuantityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZCalcEdit SupDocPackQuantityCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit UnitOfPackQuantityDropEdit;
	}
}
