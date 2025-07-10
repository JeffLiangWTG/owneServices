#if DEBUG

using CargoWise.EntityFramework;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingVoucherController
	{
		public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}

		public IBusiness GetNewBusinessEntityInLocalFactory_ForTestOnly()
		{
			return GetNewBusinessEntityInLocalFactory();
		}

		public SecurityCheckpoint CheckPointForNew_ForTestOnly => CheckPointForNew;
	}
}

#endif