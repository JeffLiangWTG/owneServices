using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class CashFlowCategoryCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			ReadOnlyCodeDescriptionPairList result = (ReadOnlyCodeDescriptionPairList)ObjectFactory.Get<IAccounting>().CashFlowCategoryCodeDescriptionList;
			return result;
		}
	}
}
