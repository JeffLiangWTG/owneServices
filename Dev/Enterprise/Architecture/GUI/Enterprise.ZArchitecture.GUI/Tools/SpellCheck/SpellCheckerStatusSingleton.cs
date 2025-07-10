using System;
using System.Collections.Generic;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GUI.Tools
{
	public sealed class SpellCheckerStatusSingleton
	{
		internal Dictionary<string, BooleanRegistryItem> registryCache = new Dictionary<string, BooleanRegistryItem>();

		BooleanRegistryItem GetRegistryItem(string key)
		{
			if (!registryCache.TryGetValue(key, out var value))
			{
				value = new BooleanRegistryItem(key, (NoResString)string.Empty, (NoResString)"FALSE when spellcheck disabled", (NoResString)string.Empty, RegistryStorageFlags.Company, RegistryOptions.IsHidden, true);// This registry is hidden and therefore wont be seen
				registryCache.Add(key, value);
			}
			return value;
		}

		public bool IsSpellCheckerEnabled(string key)
			=> GetRegistryItem(key).GetValueWithoutFallback(Env.CurrentUserPK, Guid.Empty, Guid.Empty);

		void SetSpellCheckerStatus(string key, bool status)
		{
			GetRegistryItem(key);
			registryCache[key].SetValue(Env.CurrentUserPK, Guid.Empty, Guid.Empty, status);
		}

		public void SyncroniseSpellCheckerStatus(string key, bool status)
		{
			SetSpellCheckerStatus(key, status);

			if (SpellCheckerInstancesDictionary.TryGetValue(key, out var instancesCollection))
			{
				foreach (var spellChecker in instancesCollection)
				{
					if (status)
					{
						spellChecker.EnableSpellCheck();
					}
					else
					{
						spellChecker.DisableSpellCheck();
					}
				}
			}
		}

		public void RemoveSpellCheckerInstanceFromDictionary(string key, SpellChecker spellChecker)
		{
			if (SpellCheckerInstancesDictionary.TryGetValue(key, out var instancesCollection))
			{
				if (instancesCollection.Remove(spellChecker) && instancesCollection.Count == 0)
				{
					SpellCheckerInstancesDictionary.Remove(key);
				}
			}
		}

		public void AddSpellCheckerInstanceIntoDictionary(string key, SpellChecker spellChecker)
		{
			SpellCheckerInstancesDictionary.TryGetValue(key, out var instancesCollection);

			if (instancesCollection == null)
			{
				instancesCollection = new List<SpellChecker>();
				SpellCheckerInstancesDictionary.Add(key, instancesCollection);
			}

			instancesCollection.Add(spellChecker);
		}

		internal Dictionary<string, List<SpellChecker>> SpellCheckerInstancesDictionary { get; } = new Dictionary<string, List<SpellChecker>>();

		public static SpellCheckerStatusSingleton Instance => instance ?? (instance = new SpellCheckerStatusSingleton());

		[ThreadStatic]
		static SpellCheckerStatusSingleton instance;
	}
}
