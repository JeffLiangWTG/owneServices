using System.Collections.Generic;
using System.Text;

namespace Enterprise.ZArchitecture.Core
{
	public static class ListFormatter
	{
		static void AppendElement(StringBuilder builder, string element)
		{
			builder.Append("'");
			builder.Append(element);
			builder.Append("'");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "non-translatable text")]
		public static string GetCommaSeparatedText<T>(IEnumerable<T> list)
		{
			StringBuilder result = new StringBuilder();

			List<T> listWrapper = new List<T>(list);
			AppendElement(result, listWrapper[0].ToString());

			for (int i = 1; i < listWrapper.Count; i++)
			{
				if (i == (listWrapper.Count - 1))
				{
					result.Append(" and ");
				}
				else
				{
					result.Append(", ");
				}
				AppendElement(result, listWrapper[i].ToString());
			}

			return result.ToString();
		}
	}
}
