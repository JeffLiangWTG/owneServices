using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.SpellCheck
{
	public class WordDictionary : AutoWordDictionary, ICollection<string>
	{
		public WordDictionary(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public void AddWord(string word)
		{
			word = word?.Trim();

			if (string.IsNullOrEmpty(word))
			{
				throw new ArgumentNullException(nameof(word));
			}
			else if (word.Contains(","))
			{
				throw new ArgumentException("word cannot contain comma", nameof(word));
			}

			if (WordsSet.Add(word))
			{
				base.DIC_WordsList = string.IsNullOrEmpty(DIC_WordsList) ? word : string.Concat(DIC_WordsList, ",", word);
			}
		}

		public bool RemoveWord(string word)
		{
			if (!WordsSet.Remove(word))
			{
				return false;
			}

			base.DIC_WordsList = string.Join(",", WordsSet);
			return true;
		}

		public static WordDictionary GetOrAdd(BusinessObjectFactory factory, string staffCode)
		{
			var query = new ZQuery(WordDictionarySchema.DIC_GS_NKStaff, staffCode);
			var dictionary = factory.LoadTop1<WordDictionary>(query);
			if (dictionary == null)
			{
				dictionary = factory.New<WordDictionary>();
				dictionary.DIC_GS_NKStaff = staffCode;
			}

			return dictionary;
		}

		public void RevertWordsChanges()
		{
			base.DIC_WordsList = (ZString)DIC_WordsListInfo.OriginalValue;
			wordsCache = null;
		}

		[BusinessObjectTestExclude]
		public override ZString DIC_WordsList
		{
			get => base.DIC_WordsList;
			set => throw new NotSupportedException("Use the AddWord and RemoveWord methods");
		}

		public override bool IsSavedByFactory => IsInDatabase || Words.Any();

		public IEnumerable<string> Words => WordsSet;

		ISet<string> WordsSet => wordsCache ?? (wordsCache = CreateWordsSet());
		ISet<string> wordsCache;

		ISet<string> CreateWordsSet()
		{
			var set = new HashSet<string>(DIC_WordsList.Split(',').Select(s => s.Trim().ToString()));
			set.Remove(string.Empty);
			return set;
		}

		protected override void ReloadCore()
		{
			wordsCache = null;
			base.ReloadCore();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			DIC_GS_NKStaff = "XZY";

			AddWord("SomethingToEnsureIsSavedByFactoryIsTrue");
		}
#endif

		#region ICollection

		int ICollection<string>.Count => WordsSet.Count;
		bool ICollection<string>.IsReadOnly => false;

		void ICollection<string>.Add(string word) => AddWord(word);
		bool ICollection<string>.Remove(string item) => RemoveWord(item);

		void ICollection<string>.Clear() => Words.ToList().ForEach(w => RemoveWord(w));
		bool ICollection<string>.Contains(string item) => WordsSet.Contains(item);
		void ICollection<string>.CopyTo(string[] array, int arrayIndex) => WordsSet.CopyTo(array, arrayIndex);
		IEnumerator<string> IEnumerable<string>.GetEnumerator() => WordsSet.GetEnumerator();
		public IEnumerator GetEnumerator() => WordsSet.GetEnumerator();

		#endregion
	}
}
