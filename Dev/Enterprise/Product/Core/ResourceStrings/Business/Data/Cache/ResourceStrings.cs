using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;

namespace Enterprise.ResourceStrings.Business
{
	public class ResourceStrings : IResourceStrings
	{
		ResourceStrings(string language)
		{
			overrideLanguage = language;
		}

		ResourceStrings() : this(null) { }

		readonly string overrideLanguage;

		public static ResourceStrings Instance
		{
			get { return instance ?? (instance = new ResourceStrings()); }
		}
		[ThreadStatic]
		static ResourceStrings instance;

		static IResourceStringCacheBuilder GetResourceStringCacheBuilder()
		{
			return ObjectFactory.GetDesignerSafe<IResourceStringCacheBuilder>();
		}

		IResourceStrings InternalResourceStrings
		{
			get { return internalResourceStrings ?? (internalResourceStrings = new CargoWiseOne.ResourceStrings.ResourceStrings(overrideLanguage, GetResourceStringCacheBuilder)); }
		}
		IResourceStrings internalResourceStrings;

		#region Implementation of IResourceStrings

		public ResourceStringData GetData(UInt16 asmid, string resourceKey)
		{
			return InternalResourceStrings.GetData(asmid, resourceKey);
		}

		public string GetString(UInt16 asmid, string resourceKey)
		{
			return InternalResourceStrings.GetString(asmid, resourceKey);
		}

		public string GetString(UInt16 asmid, string resourceKey, string englishText, params object[] parameters)
		{
			return InternalResourceStrings.GetString(asmid, resourceKey, englishText, parameters);
		}

		public void NotifyCurrentLanguageChanged()
		{
			InternalResourceStrings.NotifyCurrentLanguageChanged();
		}

		public void ResetCache()
		{
			InternalResourceStrings.ResetCache();
		}

		public void PreBuildCaches(ResourceLanguage[] languages)
		{
			InternalResourceStrings.PreBuildCaches(languages);
		}

		public IResourceStrings GetLanguageInstance(string language)
		{
			return InternalResourceStrings.GetLanguageInstance(language);
		}

		public IDisposable TemporarilySwitchLanguage(string language)
		{
			return InternalResourceStrings.TemporarilySwitchLanguage(language);
		}

		public IDisposable HoldLanguageInstances()
		{
			return InternalResourceStrings.HoldLanguageInstances();
		}

		public string CurrentLanguage
		{
			get { return InternalResourceStrings.CurrentLanguage; }
			set
			{
				if (value != InternalResourceStrings.CurrentLanguage)
				{
					InternalResourceStrings.CurrentLanguage = value;
					NotifyCurrentLanguageChanged();
				}
			}
		}

		public IMockResourceStringCache UseMockData()
		{
			return InternalResourceStrings.UseMockData();
		}

		public event EventHandler<ResourceStringDemandedEventArgs> ResourceDemanded
		{
			add { InternalResourceStrings.ResourceDemanded += value; }
			remove { InternalResourceStrings.ResourceDemanded -= value; }
		}

		public IEnumerable<string> GetRecentlyUsedKeys()
		{
			return InternalResourceStrings.GetRecentlyUsedKeys();
		}

		#endregion
	}
}
