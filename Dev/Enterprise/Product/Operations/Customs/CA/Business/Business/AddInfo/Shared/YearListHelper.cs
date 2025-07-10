using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class YearListHelper
	{
		public static CodeDescriptionPairList GetYearList(ZInt firstYearToShow)
		{
			var yearList = new CodeDescriptionPairList();

			var currentYear = firstYearToShow;
			while (currentYear >= 1885)
			{
				var yearString = currentYear.ToString();
				yearList.AddPair(yearString, yearString);
				currentYear--;
			}

			return yearList;
		}
	}
}
