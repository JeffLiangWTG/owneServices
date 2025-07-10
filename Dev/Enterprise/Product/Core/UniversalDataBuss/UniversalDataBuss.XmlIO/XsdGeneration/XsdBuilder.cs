using System;
using System.Collections.Generic;
using System.Globalization;

namespace Enterprise.UniversalDataBuss.XmlIO.XsdGeneration
{
	internal class XsdBuilder
	{
		public XsdBuilder()
		{
			this.internalList = new List<string>();
		}

		readonly List<string> internalList;
		int indentCount;
		bool lastLineIndented;

		public void AddBlankLine()
		{
			lastLineIndented = false;
			internalList.Add(string.Empty);
		}

		public void AddBlankLineIfNotFirstInIndentedSection()
		{
			if (!lastLineIndented)
			{
				AddBlankLine();
			}
		}

		public void Add(string lineValue)
		{
			if (string.IsNullOrEmpty(lineValue))
			{
				AddBlankLine();
				return;
			}

			bool isEndElementLine = lineValue.StartsWith("</", StringComparison.Ordinal);
			if (isEndElementLine)
			{
				indentCount--;
			}

			internalList.Add("".PadRight(indentCount * 2, ' ') + lineValue);

			lastLineIndented = false;
			if (!isEndElementLine && !lineValue.EndsWith("/>", StringComparison.Ordinal))
			{
				indentCount++;
				lastLineIndented = true;
			}
		}

		public void Add(string value, params object[] parameters)
		{
			Add(string.Format(CultureInfo.InvariantCulture, value, parameters));
		}

		public string WriteOutXsd()
		{
			return string.Join("\r\n", internalList.ToArray());
		}
	}
}
