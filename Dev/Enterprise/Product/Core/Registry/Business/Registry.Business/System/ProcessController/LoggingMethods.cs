using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class LoggingMethods : CodeDescriptionPairList
	{
		public LoggingMethods()
		{
			AddPair(FSL, ResString.GetMultilingualString("afe51284-71ec-489e-a562-ea1ac6cd6a24", "File System Logging"));
			AddPair(ELK, ResString.GetMultilingualString("36ac7b58-06e1-4585-9d73-a8e07411c0a1", "Elastic Search Logging"));
			AddPair(KAF, ResString.GetMultilingualString("5715c52e-52dd-42d2-84b6-1c2d945672e3", "Kafka Logging"));
			AddPair(SYS, ResString.GetMultilingualString("9fb598d0-c5b7-4da6-bafe-45f3c78b3b60", "Syslog Logging"));
			AddPair(CFL, ResString.GetMultilingualString("621d26e2-e7db-47d7-b023-9656281b6827", "Combined File System Logging"));

			DefaultCode = FSL;
		}

		public const string FSL = "FSL";
		public const string ELK = "ELK";
		public const string KAF = "KAF";
		public const string SYS = "SYS";
		public const string CFL = "CFL";
	}
}
