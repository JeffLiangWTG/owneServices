#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoicingBaseConsolCostImporter
	{
		public ZQuery CostsFilter_ForTestOnly => CostsFilter;
	}
}

#endif
