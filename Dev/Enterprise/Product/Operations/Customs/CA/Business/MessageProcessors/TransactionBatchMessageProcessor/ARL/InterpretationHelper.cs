using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	static class InterpretationHelper
	{
		public static ZString FormatAmount(ZDecimal input, bool blankWhenZero = false)
		{
			return input == 0 && blankWhenZero ? ZString.Empty :
				(input >= 0 ? ZString.Format("{0:N}", Math.Abs(input)) : ZString.Format("({0:N})", Math.Abs(input)));
		}

		public static ZString FormatDate(ZDateTime input)
		{
			return input.IsEmpty ? string.Empty : input.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
		}
	}
}
