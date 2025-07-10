using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enterprise.ZArchitecture.Business.Utilities
{
	public abstract class TextConverterBase
	{
		protected TextConverterBase()
		{
			wordConverters = new List<IWordConverter>();
			AddWordConverters(wordConverters);
		}

		readonly IList<IWordConverter> wordConverters;
		protected abstract void AddWordConverters(IList<IWordConverter> wordConverters);

		public string Convert(string text)
		{
			return Convert(text, null, null);
		}

		public string Convert(string text, IEnumerable<string> wordsToExclude, StringComparer stringComparer)
		{
			var result = new StringBuilder(text.Length);
			foreach (var word in text.Split(' '))
			{
				if (word.Length > 0 && (wordsToExclude == null || !wordsToExclude.Contains(word, stringComparer)))
				{
					var properCaseWord = word;
					foreach (var wordConverter in wordConverters)
					{
						properCaseWord = wordConverter.Convert(properCaseWord);
					}
					result.Append(properCaseWord + " ");
				}
				else
				{
					result.Append(word + " ");
				}
			}
			return result.Remove(result.Length - 1, 1).ToString();
		}
	}
}
