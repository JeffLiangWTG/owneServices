#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class InvoicePrintingController
	{
		public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}
	}
}

#endif