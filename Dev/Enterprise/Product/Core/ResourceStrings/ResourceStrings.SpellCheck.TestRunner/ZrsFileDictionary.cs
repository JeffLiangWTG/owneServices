using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using CargoWiseOne.ResourceStrings;

namespace ResourceStrings.SpellCheck.TestRunner
{
	sealed class ZrsFileDictionary
	{
		internal static ZrsFileDictionary Instance => LazyInstance.Value;

		internal IEnumerable<string> GetResourceStringKeys(string zrsFileName)
		{
			return dictionary[zrsFileName];
		}

		internal bool Contains(string zrsFileName)
		{
			return dictionary.ContainsKey(zrsFileName);
		}

		ZrsFileDictionary()
		{
			var zrsFiles = ZrsFile.GetFilesForLanguage(Res.DefaultLanguage);
			foreach (var zrsFilePath in zrsFiles)
			{
				using (var zrs = ZipFile.OpenRead(zrsFilePath))
				{
					foreach (var entry in zrs.Entries)
					{
						var resourceStringData = XmlResourceStringSource.ReadAll(entry.Open());
						var resourceStringKeyList = resourceStringData.Select(resource => resource.Key);
						dictionary[entry.Name] = resourceStringKeyList;
					}
				}
			}
		}

		static readonly Lazy<ZrsFileDictionary> LazyInstance =
			new Lazy<ZrsFileDictionary>(() => new ZrsFileDictionary());

		readonly IDictionary<string, IEnumerable<string>> dictionary = new Dictionary<string, IEnumerable<string>>();
	}
}
