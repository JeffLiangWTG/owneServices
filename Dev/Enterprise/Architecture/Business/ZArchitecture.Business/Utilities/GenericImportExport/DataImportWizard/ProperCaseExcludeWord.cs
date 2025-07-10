using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DataMapping
{
	public class ProperCaseExcludeWord : AutoProperCaseExcludeWord
	{
		public ProperCaseExcludeWord(BusinessObjectFactory factory)
			: this(factory, "")
		{
		}

		public ProperCaseExcludeWord(BusinessObjectFactory factory, string word)
			: base(factory)
		{
			Word = word;
		}
	}
}
