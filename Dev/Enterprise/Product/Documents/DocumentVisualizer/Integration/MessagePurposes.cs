using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Integration
{
	public sealed class MessagePurposes : CodeDescriptionPairList
	{
		#region SuppressResourceStringsCheckRegion

		public static class Codes
		{
			public const string Original = "ORG";
			public const string Amendment = "AMD";
			public const string Withdrawal = "WTH";
		}

		public static class Descriptions
		{
			public const string Original = "Original";
			public const string Amendment = "Amendment";
			public const string Withdrawal = "Withdrawal";
		}

		#endregion

		public MessagePurposes()
		{
			AddPair(Codes.Original, Descriptions.Original);
			AddPair(Codes.Amendment, Descriptions.Amendment);
			AddPair(Codes.Withdrawal, Descriptions.Withdrawal);
		}
	}
}
