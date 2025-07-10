using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class PurchaseOrderGoodsReceivedNotesCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			ReadOnlyCodeDescriptionPairList result = (ReadOnlyCodeDescriptionPairList)ObjectFactory.Get<IAccounting>().GoodsReceivedStatusCodesList;
			return result;
		}
	}
}
