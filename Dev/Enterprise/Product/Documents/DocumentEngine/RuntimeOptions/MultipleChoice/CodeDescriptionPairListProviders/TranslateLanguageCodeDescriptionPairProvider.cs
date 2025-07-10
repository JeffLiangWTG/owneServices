using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class TranslateLanguageCodeDescriptionPairProvider : ICodeDescriptionPairListProvider
	{
		public ReadOnlyCodeDescriptionPairList GetCodeDescriptionPairList()
		{
			var result = new AvailableDocBuilderLanguageList(new BusinessObjectFactory());
			result.RemoveCode(Core.Constants.Languages.EnglishAmerican);
			result.RemoveCode(Core.Constants.Languages.EnglishBritish);
			return result;
		}
	}
}
