using System.Collections.Generic;
using System.Text;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public static class SumARegistrationNumberFormatter
	{
		public static ZString Format(ZString regNumber)
		{
			return regNumber.LeftOrNull(2) == "AT" ? MultiInsertCharIntoString(regNumber.KeepAlphanumericCharacters(), '/', 1, 2, 4, 10, 12, 16) : regNumber;
		}

		static ZString MultiInsertCharIntoString(ZString str, char insertChar, params int[] positions)
		{
			var result = ZString.Empty;
			if (!str.IsEmpty)
			{
				var sb = new StringBuilder(str.Length + (positions.Length));
				var posLookup = new HashSet<int>(positions);
				for (var i = 0; i < str.Length; i++)
				{
					sb.Append(str[i]);
					if (posLookup.Contains(i) && i < str.Length - 1)
					{
						sb.Append(insertChar);
					}
				}
				result = sb.ToString();
			}
			return result;
		}
	}
}
