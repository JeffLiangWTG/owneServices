using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.SpellCheck.Testing
{
	[TestedType(typeof(WordDictionary))]
	sealed class WordDictionaryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNewDictionaryHasEmptyWordsList()
		{
			AssertEquals("New dictionary should be empty", 0, WordDictionary.GetOrAdd(Factory, "XXX").Words.Count());
		}

		public void TestRevertChanges()
		{
			var dictionary = WordDictionary.GetOrAdd(Factory, "XXX");

			dictionary.AddWord("G'Day");
			dictionary.AddWord("m80");
			dictionary.AddWord("drongo");

			Factory.Save();

			dictionary.RemoveWord("drongo");
			dictionary.AddWord("wongo");

			Assert("PRE: The word should be removed", !dictionary.Contains("drongo"));
			Assert("PRE: The added word should be added", dictionary.Contains("wongo"));

			dictionary.RevertWordsChanges();

			Assert("The removed word should be returned", dictionary.Contains("drongo"));
			Assert("The added word should be removed", !dictionary.Contains("wongo"));
		}

		public void TestAddWord_AddsToCommaseparatedList()
		{
			var dictionary = WordDictionary.GetOrAdd(Factory, "XXX");
			AssertEquals("PRE: Dict is empty", ZString.Empty, dictionary.DIC_WordsList);

			dictionary.AddWord("G'Day");

			AssertEquals("After adding a word it should appear in the list", "G'Day", dictionary.DIC_WordsList);

			dictionary.AddWord("m80");
			dictionary.AddWord("drongo");

			AssertEquals("Added words should be comma separated", "G'Day,m80,drongo", dictionary.DIC_WordsList);
		}

		public void TestRemoveWord_RemovesFromCommaseparatedList()
		{
			var dictionary = WordDictionary.GetOrAdd(Factory, "ASS");

			dictionary.AddWord("G'Day");
			dictionary.AddWord("m80");
			dictionary.AddWord("drongo");

			AssertEquals("PRE: Dict has words", "G'Day,m80,drongo", dictionary.DIC_WordsList);

			dictionary.RemoveWord("m80");

			AssertEquals("Removed words should no longer be in the WordsList", "G'Day,drongo", dictionary.DIC_WordsList);
		}

		public void TestHandlesUnicode()
		{
			var staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff.GS_Code = "ASS";
			var dictionary = WordDictionary.GetOrAdd(Factory, "ASS");

			dictionary.AddWord("G'Day");
			dictionary.AddWord("ööáéíüñm");
			dictionary.AddWord("drongo");

			AssertEquals("PRE: Dict has words", "G'Day,ööáéíüñm,drongo", dictionary.DIC_WordsList);

			Factory.Save();

			var reloaded = WordDictionary.GetOrAdd(new BusinessObjectFactory(), "ASS");
			AssertEquals("Unicode characters should be unmodified", "G'Day,ööáéíüñm,drongo", reloaded.DIC_WordsList);
		}

		public void TestAddWordsForBadWords()
		{
			var dictionary = WordDictionary.GetOrAdd(Factory, "XXX");
			AssertExceptionThrown<ArgumentException>("Words are stored as a comma separated list. If your word has a comma in it, it will not be ignored correctly", () => dictionary.AddWord("foo,bar"));
			AssertExceptionThrown<ArgumentNullException>("Can't add whats not a word", () => dictionary.AddWord(string.Empty));
			AssertExceptionThrown<ArgumentNullException>("Can't add whats not a word", () => dictionary.AddWord(null));
		}

		public void TestGetOrAdd_CreatesWhenNeeded()
		{
			Assert("Should newly create the dictionary when it doesn't already exist", !WordDictionary.GetOrAdd(Factory, "XXX").IsInDatabase);
		}

		public void TestGetOrAdd_GetsFromDbIfExists()
		{
			var staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff.GS_Code = "ASS";

			var dictionary = Factory.NewWithValidTestData<WordDictionary>();
			dictionary.DIC_GS_NKStaff = staff.GS_Code;
			dictionary.AddWord("G'Day");

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			Assert("Should load dictionary from db when available", WordDictionary.GetOrAdd(otherFactory, staff.GS_Code).IsInDatabase);
		}

		public void TestGetEnumerator()
		{
			var dictionary = WordDictionary.GetOrAdd(Factory, "XXX");
			((ICollection<string>)dictionary).Add("DDD");
			AssertCollectionContains("DDD", dictionary);
		}
	}
}
