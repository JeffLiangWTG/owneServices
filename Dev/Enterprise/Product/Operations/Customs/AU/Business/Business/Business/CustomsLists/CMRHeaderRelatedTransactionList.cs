using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business;

public class CMRHeaderRelatedTransactionList : CodeDescriptionPairList
{
	public CMRHeaderRelatedTransactionList()
	{
		Add(CMRRelatedTransaction.Yes);
		Add(CMRRelatedTransaction.No);
	}
}
