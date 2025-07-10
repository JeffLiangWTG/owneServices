using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools.SpellCheck
{
	public partial class PersonalDictionaryForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public PersonalDictionaryForm()
		{
			saveButtonUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, saveButtonUserControl, isSaveButtonHidden: true);
		}

		public PersonalDictionaryForm(WordDictionary dictionary)
			: base(dictionary)
		{
			foreach (var word in dictionary.Words.OrderBy(w => w))
			{
				wordsListBox.Items.Add(word);
			}

			UpdateRemoveButton();
			saveButtonUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, saveButtonUserControl, isSaveButtonHidden: true);
		}

		void UpdateRemoveButton()
			=> removeButton.Enabled = wordsListBox.SelectedItems.Count > 0;

		void wordsListBox_SelectedIndexChanged(object sender, EventArgs e)
			=> UpdateRemoveButton();

		void removeButton_Click(object sender, EventArgs e)
		{
			foreach (var word in wordsListBox.SelectedItems.Cast<string>().ToList())
			{
				Dictionary.RemoveWord(word);
				wordsListBox.Items.Remove(word);
			}
		}

		public WordDictionary Dictionary => (WordDictionary)CurrentDataItem;
		public BusinessObjectFactory Factory => Dictionary.Factory;
	}
}
