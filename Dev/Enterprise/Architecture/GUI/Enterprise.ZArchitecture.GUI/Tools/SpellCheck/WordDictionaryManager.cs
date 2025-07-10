using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.SpellCheck;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	class WordDictionaryManager
	{
		[ThreadStatic]
		static Overridable<WordDictionaryManager> instance;

		public static WordDictionaryManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new Overridable<WordDictionaryManager>(null);
				}

				if (instance.Value == null)
				{
					instance.Value = new WordDictionaryManager();
				}

				return instance.Value;
			}
		}

		readonly BusinessObjectFactory factory;

		WordDictionaryManager()
		{
			factory = new BusinessObjectFactory();
		}

		public WordDictionary CurrentDictionary
		{
			get
			{
				var currentUser = Env.Instance.CurrentUser;
				return currentUser != null ? WordDictionary.GetOrAdd(factory, currentUser.Initials) : null;
			}
		}

		public void AddWord(string word)
		{
			if (word != null)
			{
				CurrentDictionary.AddWord(word);
				factory.Save();
			}
		}
	}
}
