#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Transaction.Base
{
	public partial class TransactionControllerWithLoginCompanyCheck
	{
		public bool ShouldCheckLoginCompanyMatch_ForTestOnly => ShouldCheckLoginCompanyMatch;

		public IBusiness GetLoadedBusinessEntityInLocalFactory_ForTestOnly(IBusiness sourceEntity)
		{
			return GetLoadedBusinessEntityInLocalFactory(sourceEntity);
		}

		public IZForm ShowCopyForm_ForTestOnly(BusinessObject inMemorySourceEntity, Func<IBusiness, IBusiness> returnsNewBusinessEntity)
		{
			return ShowCopyForm(inMemorySourceEntity, new CopyOfBusinessObject(returnsNewBusinessEntity));
		}
	}
}

#endif
