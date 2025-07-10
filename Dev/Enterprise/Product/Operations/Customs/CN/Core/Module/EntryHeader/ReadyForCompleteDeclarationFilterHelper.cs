using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Module
{
	#region SuppressResourceStringsCheckRegion
	static class ReadyForCompleteDeclarationFilterHelper
	{
		public static class Options
		{
			public const string Ready = "Ready";
			public const string NotReady = "Not-ready";
			public const string All = "All";
		}

		public static CodeDescriptionPairList OptionsList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair(Options.Ready, Res.GetString("EntryHeaderFilterStrip|ReadyForCompleteDeclarationStatusList|Ready", "Ready"));
				result.AddPair(Options.NotReady, Res.GetString("EntryHeaderFilterStrip|ReadyForCompleteDeclarationStatusList|NotReady", "Not Ready"));
				result.AddPair(Options.All, Res.GetString("EntryHeaderFilterStrip|ReadyForCompleteDeclarationStatusList|All", "All"));

				return result;
			}
		}
	}
	#endregion
}
