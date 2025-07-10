using System.Collections.Generic;

namespace Enterprise.ZArchitecture.Business.Utilities
{
	public class ProperCaseConverter : TextConverterBase
	{
		protected override void AddWordConverters(IList<IWordConverter> wordConverters)
		{
			wordConverters.Add(new ConvertToTitleCase());
			wordConverters.Add(new CapitaliseApostropheSuffix(4));
			wordConverters.Add(new DecapitaliseFrenchApostrophePrefix());
			wordConverters.Add(new DecapitaliseNumberSuffix());
		}
	}
}
