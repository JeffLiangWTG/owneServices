
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.Shared
{
	public class CustomsStatusCodeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Cleared = "CLR";
			public const string Held = "HLD";
			public const string Withdrawn = "WRD";
			public const string Unknown = "UNK";
		}
	}
}
