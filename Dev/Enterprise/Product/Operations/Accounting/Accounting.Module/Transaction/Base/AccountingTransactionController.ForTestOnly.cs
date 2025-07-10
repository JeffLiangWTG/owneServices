#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingTransactionController
	{
		public IZForm GetForm_ForTestOnly(IBusiness businessEntity)
		{
			return GetForm(businessEntity);
		}

		public Business.Base.Reversing.ReversingBase Reversing_ForTestOnly
		{
			get { return Reversing; }
			set { Reversing = value; }
		}

		public Business.Base.Reversing.MultipleReversingProviderForHeader MultipleReversingProvider_ForTestOnly
		{
			get { return MultipleReversingProvider; }
			set { MultipleReversingProvider = value; }
		}

		public bool DoBaseReversing_ForTestOnly(IBusiness transaction)
		{
			return DoBaseReversing(transaction);
		}

		public string GetIDForFormCache_ForTestOnly(IBusiness businessEntity)
		{
			return GetIDForFormCache(businessEntity);
		}

		public List<string> ReversingErrors_ForTestOnly
		{
			get { return ReversingErrors; }
			set { ReversingErrors = value; }
		}

		public IBusiness GetTopLevelBusinessObjectCached_ForTestOnly(IBusiness sourceEntity)
		{
			return GetTopLevelBusinessObjectCached(sourceEntity);
		}

		public IBusiness GetLoadedBusinessEntityInLocalFactoryCore2_ForTestOnly(IBusiness sourceEntity)
		{
			return GetLoadedBusinessEntityInLocalFactoryCore2(sourceEntity);
		}

		public ZGuid LatestSourceEntityIdentifier_ForTestOnly
		{
			get { return latestSourceEntityIdentifier; }
			set { latestSourceEntityIdentifier = value; }
		}

		public SecurityCheckpoint CheckPointForDelete_ForTestOnly => CheckPointForDelete;
	}
}

#endif
