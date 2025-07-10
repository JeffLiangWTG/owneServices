#if DEBUG

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class InvoicingBaseBulkConsolCostImportForm
	{
		public ZArchitecture.ZGrid ConsolCostsGrid_ForTestOnly
		{
			get { return ConsolCostsGrid; }
			set { ConsolCostsGrid = value; }
		}

		public ZArchitecture.GUI.ZButton DeselectAllButton_ForTestOnly
		{
			get { return DeselectAllButton; }
			set { DeselectAllButton = value; }
		}

		public ZArchitecture.GUI.ZButton SelectAllButton_ForTestOnly
		{
			get { return SelectAllButton; }
			set { SelectAllButton = value; }
		}
	}
}

#endif
