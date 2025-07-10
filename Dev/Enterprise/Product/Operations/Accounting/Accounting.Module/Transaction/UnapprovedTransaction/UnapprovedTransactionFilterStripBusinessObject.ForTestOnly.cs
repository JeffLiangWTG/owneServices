#if DEBUG

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedTransactionFilterStripBusinessObject
	{
		public CodeDescriptionPairList TransactionTypeList_ForTestOnly => TransactionTypeList;
	}
}

#endif
