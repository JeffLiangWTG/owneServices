using System.Linq;
using CargoWise.Integration;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Diagnostics;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public static class EntryStatusListHelper
	{
		public static CodeDescriptionPairList GetEntryStatusList(this CodeDescriptionPairList entryStatusList, bool forLPCOModule)
		{
			var result = new CodeDescriptionPairList();
			result.AddPairsIfNotExist(entryStatusList.Cast<ICodeDescription>().Where(x => !(forLPCOModule ^ x.Code.StartsWith(ImportLicenseStatusPrefix))));
			return result;
		}

		public static CodeDescriptionPairList GetEntryStatusList(this CodeDescriptionPairList entryStatusList, string prefix)
		{
			var result = new CodeDescriptionPairList();
			result.AddPairsIfNotExist(entryStatusList.Cast<ICodeDescription>().Where(x => x.Code.StartsWith(prefix)));
			return result;
		}

		public const string ImportLicenseStatusPrefix = "L";
		public const string ImportSiscomexEntryStatusPrefix = "S";
		public const string ExportEntryStatusPrefix = "E";
		public const string ImportEntryStatusPrefix = "I";
		public const string LPCOStatusPrefix = "P";
	}
}
