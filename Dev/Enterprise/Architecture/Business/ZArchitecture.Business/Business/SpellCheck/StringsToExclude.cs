using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.ZArchitecture.Business.SpellCheck
{
	public class StringsToExclude : ICollection<string>
	{
		public StringsToExclude(ICollection<string> wordDictionary)
		{
			this.wordDictionary = wordDictionary;
			this.wordsToIgnore = wordDictionary;
		}

		readonly ICollection<string> wordDictionary;
		ICollection<string> wordsToIgnore;

		public void UpdateWordsToIgnore(IEnumerable<string> words)
		{
			ICollection<string> SplitWord(string input)
				=> !string.IsNullOrEmpty(input) ?
					input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) :
					Array.Empty<string>();

			if (!(words?.Any() ?? false))
			{
				wordsToIgnore = wordDictionary;
			}
			else if (wordDictionary == null)
			{
				wordsToIgnore = words.SelectMany(SplitWord).Distinct().ToList();
			}
			else
			{
				wordsToIgnore = words.SelectMany(SplitWord).Concat(wordDictionary).Distinct().ToList();
			}
		}

		#region ICollection

		int ICollection<string>.Count => wordsToIgnore?.Count ?? 0;
		bool ICollection<string>.IsReadOnly => wordsToIgnore?.IsReadOnly ?? false;
		void ICollection<string>.Add(string word) => wordsToIgnore?.Add(word);
		bool ICollection<string>.Remove(string item) => wordsToIgnore?.Remove(item) ?? false;
		void ICollection<string>.Clear() => wordsToIgnore?.Clear();
		bool ICollection<string>.Contains(string item) => wordsToIgnore?.Contains(item) ?? false;
		void ICollection<string>.CopyTo(string[] array, int arrayIndex) => wordsToIgnore?.CopyTo(array, arrayIndex);
		IEnumerator<string> IEnumerable<string>.GetEnumerator() => wordsToIgnore?.GetEnumerator();

		public IEnumerator GetEnumerator() => wordsToIgnore?.GetEnumerator();

		#endregion
	}
}
