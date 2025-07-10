using CargoWise.Application;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class GLPresentationCategoryGroupCodeDescriptionPairProvider : GLPresentationCategoryCodeDescriptionPairProvider
	{
		protected override ReadOnlyCodeDescriptionPairList GetValidCodes()
		{
			return (ReadOnlyCodeDescriptionPairList)ObjectFactory.Get<IAccounting>().GLPresentationJournalCategoriesGroupList;
		}
	}
}
