using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.SpellCheck;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck.Testing
{
	[TestedType(typeof(PersonalDictionaryForm))]
	public class PersonalDictionaryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var bizo = Factory.NewWithValidTestData<WordDictionary>();
			Factory.Save();
			return new PersonalDictionaryForm(bizo);
		}

		public void TestListContainsWordsInAlphabeticalOrder()
		{
			var bizo = CreateDictionary("Heer", "ziiis", "som", "werds");
			using (var form = new PersonalDictionaryForm(bizo))
			{
				var listboxWords = GetWordsListbox(form).Items.Cast<string>().ToArray();
				AssertArrayEqualsByElements("The listbox should contain the dictionary words in alphabetical order", new[] { "Heer", "som", "werds", "ziiis" }, listboxWords);
			}
		}

		public void TestRemoveButtonRemovesWords()
		{
			var bizo = CreateDictionary("Heer", "ziiis", "som", "werds");
			using (var form = new PersonalDictionaryForm(bizo))
			{
				form.Show();
				var listbox = GetWordsListbox(form);
				listbox.SelectedItem = "ziiis";
				GetRemoveButton(form).PerformClick();
				Application.DoEvents();
				AssertCollectionNotContains("After hitting remove, the word should be removed from the list", "ziiis", listbox.Items);
				AssertNotContains("The removed word should be removed from the bizo as well", "ziiis", bizo.DIC_WordsList);
			}
		}

		public void TestRemoveButtonWhenNothingIsSelected()
		{
			var bizo = CreateDictionary("Heer", "ziiis", "som", "werds");
			using (var form = new PersonalDictionaryForm(bizo))
			{
				form.Show();
				var listbox = GetWordsListbox(form);
				Assert("Remove button should be disabled when nothing is selected", !GetRemoveButton(form).Enabled);
				listbox.SelectedIndex = 0;
				Assert("Remove button should be enabled after an item is selected", GetRemoveButton(form).Enabled);
			}
		}

		WordDictionary CreateDictionary(params string[] words)
		{
			var dictionary = Factory.NewWithValidTestData<WordDictionary>();
			foreach (var word in dictionary.Words.ToList())
			{
				dictionary.RemoveWord(word);
			}

			foreach (var word in words)
			{
				dictionary.AddWord(word);
			}

			return dictionary;
		}

		ZListBox GetWordsListbox(PersonalDictionaryForm form) => (ZListBox)form.Controls.Find("wordsListBox", false).Single();
		ZButton GetRemoveButton(PersonalDictionaryForm form) => (ZButton)form.Controls.Find("removeButton", false).Single();
	}
}
