using System;
using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EUExportSupplierHeaderUserControl : EUCustomsSupplierHeaderUserControl
	{
		public EUExportSupplierHeaderUserControl()
		{
			InitializeComponent();
			InvoiceHeadersBoundGrid.ColumnLayoutContext = nameof(Customs.GUI.DeclarationType.Export);
			JobComInvoiceHeadersBoundGrid.InnerGrid.GridId = "GridLayoutp2kAlYGfl+VNYTeGMw3VqA==";
		}

		protected override bool ShouldShowVatGstColumnInChargesGrid => false;

		protected override Type GetAdditionalInfosUserControlType() => IsUCC6
			? typeof(AdditionalInfosUserControlWithGrid)
			: typeof(AdditionalInfosUserControl);

		bool IsUCC6 => CurrentDataItem?.Configuration.IsUCC6(CurrentDataItem) ?? false;
	}
}
