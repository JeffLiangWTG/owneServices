namespace Enterprise.ZArchitecture.Core
{
	public class OnlineIndexRebuildLowPriorityAbortAfterWaitList : CodeDescriptionPairList
	{
		public OnlineIndexRebuildLowPriorityAbortAfterWaitList()
		{
			AddPair(Codes.None, Descriptions.None);
			AddPair(Codes.Self, Descriptions.Self);
			AddPair(Codes.Blockers, Descriptions.Blockers);
		}

		public static class Codes
		{
			public const string None = "NONE";
			public const string Self = "SELF";
			public const string Blockers = "BLOCKERS";
		}

		public static class Descriptions
		{
			public static MultilingualString None => SourceGenerated.ResString.GetMultilingualString("8EB1025F-97D7-4262-8A6E-5F763B1E5EAB"
				, "Continue waiting for the lock with normal (regular) priority");

			public static MultilingualString Self => SourceGenerated.ResString.GetMultilingualString("8AE7B026-9DA9-4618-A238-0216ED189C3E"
				, "Exit the on-line index rebuild DDL operation currently being executed without taking any action");

			public static MultilingualString Blockers => SourceGenerated.ResString.GetMultilingualString("16B71C33-6D69-48E6-AEA2-030C2F619FB2"
				, "Kill all user transactions that block the on-line index rebuild operation so that the operation can continue");
		}
	}
}
