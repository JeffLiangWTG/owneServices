using System;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUImportSupplierHeaderUserControl : EUCustomsSupplierHeaderUserControl
	{
		public EUImportSupplierHeaderUserControl()
		{
			InitializeComponent();
			AddImportInvoiceHeaderDescriptionsUserControl();
			this.InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Import);
			this.HeaderDescriptionsTabPage.TabVisible = false;
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutRh5OdMrTXWw1W+7iTHilOQ==";
		}

		void AddImportInvoiceHeaderDescriptionsUserControl()
		{
			fImportInvoiceHeaderDescriptionsUserControl = GetImportInvoiceHeaderDescriptionsUserControl();
			this.HeaderDescriptionsTabPage.Controls.Add(this.fImportInvoiceHeaderDescriptionsUserControl);
			this.fImportInvoiceHeaderDescriptionsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.fImportInvoiceHeaderDescriptionsUserControl, ".");
			this.fImportInvoiceHeaderDescriptionsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.fImportInvoiceHeaderDescriptionsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.fImportInvoiceHeaderDescriptionsUserControl.Name = "HeaderDescriptionsUserControl";
			this.fImportInvoiceHeaderDescriptionsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(963, 263, true);
			this.fImportInvoiceHeaderDescriptionsUserControl.TabIndex = 0;
		}

		ImportInvoiceHeaderDescriptionsUserControl fImportInvoiceHeaderDescriptionsUserControl;

		protected virtual ImportInvoiceHeaderDescriptionsUserControl GetImportInvoiceHeaderDescriptionsUserControl() => new ImportInvoiceHeaderDescriptionsUserControl();

		protected override Type GetAdditionalInfosUserControlType() => IsUCC6
			? typeof(AdditionalInfosUserControlWithGrid)
			: typeof(AdditionalInfosUserControl);

		bool IsUCC6 => CurrentDataItem?.Configuration.IsUCC6(CurrentDataItem) ?? false;
	}
}
