
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRRelatedTransactionList : CodeDescriptionPairList
	{
		public CMRRelatedTransactionList()
		{
			Add(CMRRelatedTransaction.Default);
			Add(CMRRelatedTransaction.Yes);
			Add(CMRRelatedTransaction.No);
		}
	}
}
