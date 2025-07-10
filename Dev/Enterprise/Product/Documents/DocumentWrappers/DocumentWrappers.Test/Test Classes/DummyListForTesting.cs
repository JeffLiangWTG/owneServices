using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DummyListForTesting : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string Dumb = "Dumb";
			public const string Dumber = "Dumber";
		}

		public static class Descriptions
		{
			public const string Dumb = "Dumb It Down";
			public const string Dumber = "Dumber Then Dumb";
		}

		public DummyListForTesting()
		{
			AddPair(Codes.Dumb, Descriptions.Dumb);
			AddPair(Codes.Dumber, Descriptions.Dumber);
		}
	}
}
