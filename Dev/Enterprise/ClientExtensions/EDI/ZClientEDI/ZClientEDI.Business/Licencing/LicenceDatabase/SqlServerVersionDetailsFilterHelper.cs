
using System;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public static class SqlServerVersionDetailsFilterHelper
	{
		public const string Blank = "Blank";

		public static CodeDescriptionPairList GetSqlEditionList()
		{
			CodeDescriptionPairList result = new SqlServerEditionList();
			result.Insert(0, GetBlankCodeDescriptionPair());
			return result;
		}

		public static CodeDescriptionPairList GetSqlVersionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			foreach (var generation in SqlServerVersionNumber.SqlGeneration.All.Reverse())
			{
				result.AddPair(generation.Name, generation.FormalName);
			}

			result.Insert(0, GetBlankCodeDescriptionPair());
			result.AddPair("Other", "Other");
			return result;
		}

		static CodeDescriptionPair GetBlankCodeDescriptionPair()
		{
			return new CodeDescriptionPair(Blank, "Not Specified");
		}
	}
}

