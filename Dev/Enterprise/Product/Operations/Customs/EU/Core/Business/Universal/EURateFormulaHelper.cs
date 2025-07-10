using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business
{
	public static class EURateFormulaHelper
	{
		public static bool ContainsMursingPattern(ZString rateFormula)
		{
			var regex = new Regex(@"^.*#(ADFM|ADFMR|ADSZ|ADSZR|EA|EAR)\([0-9]\)#.*$", RegexOptions.IgnoreCase);
			return regex.IsMatch(rateFormula);
		}
	}
}
