using System.Collections;
using System.Collections.Generic;
using CargoWise.Common;

namespace CargoWise.BuildTools
{
	public class BuildXmlBizOEntryCollection : IEnumerable
	{
		public BuildXmlBizOEntryCollection()
		{
			hash = new Hashtable();
		}

		public BuildXmlBizOEntry this[string tableName]
		{
			get { return (tableName != null) ? (BuildXmlBizOEntry)hash[tableName] : null; }
		}

		public void Add(BuildXmlBizOEntry entry)
		{
			Argument.NotNull(entry, nameof(entry)); // Suggested By ReviewBot 
			hash.Add(entry.TableName, entry);
		}

		public void Add(string refDbCountry, string refDbType, string tableName, string solutionName, bool masterFilesReference, bool preventDelete, bool convertZStringToWesternEuropeanCharacters = false, List<BuildXmlAddInfoEntry> addInfos = null)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.NotNullOrEmpty(solutionName, nameof(solutionName));
			Add(new BuildXmlBizOEntry(refDbCountry, refDbType, tableName, solutionName, masterFilesReference, preventDelete, convertZStringToWesternEuropeanCharacters, addInfos));
		}

		public void Remove(BuildXmlBizOEntry entry)
		{
			Argument.NotNull(entry, nameof(entry)); // Suggested By ReviewBot 
			Argument.NotNullOrEmpty(entry.TableName, nameof(entry.TableName));
			Remove(entry.TableName);
		}

		public void Remove(string tableName)
		{
			Argument.NotNullOrEmpty(tableName, nameof(tableName)); // Suggested By ReviewBot 
			hash.Remove(tableName);
		}

		public bool Contains(string tableName)
		{
			return tableName != null && hash.ContainsKey(tableName);
		}

		public int Count
		{
			get { return hash.Count; }
		}

		readonly Hashtable hash;

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return hash.Values.GetEnumerator();
		}

		#endregion
	}
}
