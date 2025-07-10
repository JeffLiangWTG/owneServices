using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.Licencing.Business
{
	public sealed class DatabaseStatusList : CodeDescriptionPairList
	{
		public DatabaseStatusList()
		{
			AddPair(Codes.NON, Descriptions.NON);
			AddPair(Codes.REG, Descriptions.REG);
			AddPair(Codes.Preregistered, Descriptions.Preregistered);
		}

		public static class Codes
		{
			public const string NON = "NON";
			public const string REG = "REG";
			public const string Preregistered = "PRE";
		}

		public static class Descriptions
		{
			public const string NON = "Not Registered";
			public const string REG = "Registered";
			public const string Preregistered = "Preregistered";
		}
	}
}
