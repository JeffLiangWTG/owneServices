using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;

namespace CargoWise.BuildTools
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	public class BuildXmlBizOEntry
	{
		public BuildXmlBizOEntry(string refDbCountry, string refDbType, string tableName, string solutionName, bool masterFileReference, bool preventDelete, bool convertZStringToWesternEuropeanCharacters, List<BuildXmlAddInfoEntry> addInfoEntries = null)
		{
			Argument.NotNull(tableName, nameof(tableName));
			Argument.NotNull(solutionName, nameof(solutionName));
			RefDbCountry = refDbCountry;
			RefDbType = refDbType;
			TableName = tableName;
			SolutionName = solutionName;
			MasterFileReference = masterFileReference;
			PreventDelete = preventDelete;
			ConvertZStringToWesternEuropeanCharacters = convertZStringToWesternEuropeanCharacters;
			AddInfoEntries = addInfoEntries?.AsReadOnly();
		}

		public readonly string RefDbCountry;
		public readonly string RefDbType;
		public string TableName { get; }
		public string SolutionName { get; }
		public readonly bool MasterFileReference;
		public readonly bool PreventDelete;
		public readonly bool ConvertZStringToWesternEuropeanCharacters;

		public IReadOnlyCollection<BuildXmlAddInfoEntry> AddInfoEntries;

		public bool LivesInZArchitecture
		{
			get
			{
				return SolutionName.ToLower() == "enterprise.zarchitecture.business" || SolutionName.ToLower() == "enterprise.zarchitecture.gui";
			}
		}

		public bool LivesInMasterFiles
		{
			get
			{
				return SolutionName.ToLower() == "masterfiles";
			}
		}

		public bool LivesInHRMFiles
		{
			get
			{
				return SolutionName.ToLower() == "hrm";
			}
		}
	}
}
