using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core
{
	public static class DatabaseSecurityModePairList
	{
		public static class Codes
		{
			public const string Indeterminate = "IND";
			public const string Locked = "LCK";
			public const string ExOpen = "OPN";
			public const string OpenMode = "O12";
		}

		public static class Descriptions
		{
			public static string Indeterminate
			{
				get { return Res.GetString("5b574857-3dbc-41a2-b31b-eff942beb27e", "Indeterminate"); }
			}
			public static string Locked
			{
				get { return Res.GetString("0e06e172-588a-4d57-b7ed-91de19f5053b", "Locked"); }
			}
			public static string ExOpen
			{
				get { return Res.GetString("3fc6e0b7-c5d1-4466-aea5-7db16731659d", "Locked (open if SQL Server < 2012)"); }
			}
			public static string OpenMode
			{
				get { return Res.GetString("4eb5716e-e3aa-48cd-b09d-08f760c4556b", "Allowed Open by agreement"); }
			}
		}
	}
}
