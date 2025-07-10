#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class GenericChargeCollectionBuilder
	{
		public ZQuery GenerateFilter_ForTestOnly()
		{
			return GenerateFilter();
		}
	}
}

#endif
